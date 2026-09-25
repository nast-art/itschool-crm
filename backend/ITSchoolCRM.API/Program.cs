using System.Security.Claims;
using System.Text.Json;
using ITSchoolCRM.API.Caching;
using StackExchange.Redis;
using ITSchoolCRM.API.Auth;
using ITSchoolCRM.API.Data;
using ITSchoolCRM.API.DTOs.Errors;
using ITSchoolCRM.API.Infrastructure;
using ITSchoolCRM.API.Integrations.LMS.Options;
using ITSchoolCRM.API.Integrations.LMS.Services;
using ITSchoolCRM.API.Integrations.Services;
using ITSchoolCRM.API.Integrations.Website.Options;
using ITSchoolCRM.API.Integrations.Website.Services;
using ITSchoolCRM.API.Middleware;
using ITSchoolCRM.API.Services.Implementations;
using ITSchoolCRM.API.Services.Interfaces;

using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using Microsoft.OpenApi;
using ITSchoolCRM.API.Storage;

AppContext.SetSwitch("Npgsql.EnableLegacyTimestampBehavior", true);
var builder = WebApplication.CreateBuilder(args);


// ============================================================
// CONTROLLERS
// ============================================================

builder.Services
    .AddControllers()
    .ConfigureApiBehaviorOptions(options =>
    {
        // Единый формат ошибок валидации под ErrorResponseDto { code, message }
        // (нефункциональное требование 3 ТЗ) — фронт читает code/message.
        // Инициализатор вместо конструктора: существующий ErrorResponseDto
        // не имеет конструктора с двумя аргументами.
        options.InvalidModelStateResponseFactory = context =>
        {
            var message = string.Join(" ",
                context.ModelState.Values
                    .SelectMany(v => v.Errors)
                    .Select(e => string.IsNullOrWhiteSpace(e.ErrorMessage)
                        ? "Ошибка валидации запроса."
                        : e.ErrorMessage));
            return new BadRequestObjectResult(
                new ErrorResponseDto
                {
                    Code = "ERR_VALIDATION",
                    Message = message,
                });
        };
    });

// Keycloak Admin API (сервисный аккаунт для управления пользователями).
// Отдельная секция "KeycloakAdmin" — основная секция "Keycloak"
// уже занята настройками JWT (Authority/Audience).
builder.Services.Configure<KeycloakOptions>(
    builder.Configuration.GetSection(KeycloakOptions.SectionName));

builder.Services.AddHttpClient<IKeycloakAdminClient, KeycloakAdminClient>();

builder.Services.AddEndpointsApiExplorer();
builder.Services.AddMemoryCache();


// ============================================================
// SWAGGER
// ============================================================

builder.Services.AddSwaggerGen(options =>
{
    const string oauthSchemeId = "oauth2";

    options.AddSecurityDefinition(
        oauthSchemeId,
        new OpenApiSecurityScheme
        {
            Type = SecuritySchemeType.OAuth2,

            Flows = new OpenApiOAuthFlows
            {
                AuthorizationCode =
                    new OpenApiOAuthFlow
                    {
                        AuthorizationUrl = new Uri(
                            "http://localhost:8080/realms/itschool/protocol/openid-connect/auth"),

                        TokenUrl = new Uri(
                            "http://localhost:8080/realms/itschool/protocol/openid-connect/token"),

                        Scopes = new Dictionary<string, string>
                        {
                            ["openid"] = "OpenID"
                        }
                    }
            }
        });

    // Применяем OAuth2 к операциям Swagger.
    // Для OAuth2 явно указываем scope openid.
    options.AddSecurityRequirement(document =>
        new OpenApiSecurityRequirement
        {
            [
                new OpenApiSecuritySchemeReference(
                    oauthSchemeId,
                    document)
            ] = ["openid"]
        });
});


// ============================================================
// DATABASE
// ============================================================

builder.Services.AddDbContext<CrmDbContext>(options =>
{
    options.UseNpgsql(
        builder.Configuration.GetConnectionString(
            "DefaultConnection"));
});


// ============================================================
// KEYDB (РАСПРЕДЕЛЁННЫЙ КЭШ)
// ============================================================
// abortConnect=false: API стартует, даже если KeyDB ещё не поднялся —
// промахи кэша прозрачно уходят в PostgreSQL.

builder.Services.AddStackExchangeRedisCache(options =>
{
    options.Configuration =
        builder.Configuration.GetConnectionString("Keydb");

    // Префикс отделяет ключи CRM, если в KeyDB появится второй сервис
    options.InstanceName = "itschoolcrm:";
});

// Мультиплексор нужен для INCR по счётчикам версий (инвалидация
// групп ключей без SCAN). Один на приложение — Singleton.
builder.Services.AddSingleton<IConnectionMultiplexer>(
    _ => ConnectionMultiplexer.Connect(
        builder.Configuration.GetConnectionString("Keydb")!));

builder.Services.AddSingleton<ICacheService, CacheService>();

builder.Services.AddSingleton(new CacheOptions
{
    CatalogTtl = TimeSpan.FromSeconds(
        builder.Configuration.GetValue<int>("Cache:CatalogTtlSeconds", 600)),
    InteractionTtl = TimeSpan.FromSeconds(
        builder.Configuration.GetValue<int>("Cache:InteractionTtlSeconds", 300)),
    StatisticsTtl = TimeSpan.FromSeconds(
        builder.Configuration.GetValue<int>("Cache:StatisticsTtlSeconds", 60)),
});

// Health-check для KeyDB — используется эндпоинтом /health/ready
builder.Services
    .AddHealthChecks()
    .AddRedis(
        builder.Configuration.GetConnectionString("Keydb")!,
        name: "keydb",
        tags: new[] { "ready" });

// ============================================================
// HTTP CONTEXT
// ============================================================

builder.Services.AddHttpContextAccessor();

builder.Services.AddHttpClient("Keycloak");
// ============================================================
// AUTHENTICATION
// ============================================================

builder.Services
    .AddAuthentication(
        JwtBearerDefaults.AuthenticationScheme)
    .AddJwtBearer(options =>
    {
        options.Authority =
            builder.Configuration[
                "Keycloak:Authority"];

        options.Audience =
            builder.Configuration[
                "Keycloak:Audience"];

        options.RequireHttpsMetadata =
            builder.Configuration.GetValue<bool>(
                "Keycloak:RequireHttpsMetadata");

        // Keycloak claims оставляем в исходном виде.
        options.MapInboundClaims = false;

        options.TokenValidationParameters =
            new TokenValidationParameters
            {
                // Проверяем issuer.
                ValidateIssuer = true,

                ValidIssuer =
                    builder.Configuration[
                        "Keycloak:Authority"],

                // Проверяем audience.
                ValidateAudience = true,

                ValidAudience =
                    builder.Configuration[
                        "Keycloak:Audience"],

                // Проверяем срок действия JWT.
                ValidateLifetime = true,

                // Проверяем подпись JWT.
                ValidateIssuerSigningKey = true,

                // Имя пользователя берём из Keycloak.
                NameClaimType =
                    "preferred_username",

                // Роли после обработки ниже будут
                // находиться в ClaimTypes.Role.
                RoleClaimType =
                    ClaimTypes.Role
            };

        // ====================================================
        // KEYCLOAK ROLES
        // ====================================================

        options.Events = new JwtBearerEvents
        {
            OnTokenValidated = context =>
            {
                if (context.Principal?.Identity
                    is not ClaimsIdentity identity)
                {
                    return Task.CompletedTask;
                }

                // Keycloak обычно хранит realm roles
                // внутри realm_access.roles.
                var realmAccessClaim =
                    context.Principal.FindFirst(
                        "realm_access");

                if (realmAccessClaim is null)
                {
                    return Task.CompletedTask;
                }

                try
                {
                    using var document =
                        JsonDocument.Parse(
                            realmAccessClaim.Value);

                    if (!document.RootElement.TryGetProperty(
                        "roles",
                        out var rolesElement))
                    {
                        return Task.CompletedTask;
                    }

                    foreach (var roleElement
                             in rolesElement.EnumerateArray())
                    {
                        var role =
                            roleElement.GetString();

                        if (string.IsNullOrWhiteSpace(role))
                        {
                            continue;
                        }

                        // Не добавляем одну и ту же роль
                        // несколько раз.
                        var alreadyExists =
                            identity.Claims.Any(
                                claim =>
                                    claim.Type ==
                                    ClaimTypes.Role
                                    &&
                                    claim.Value ==
                                    role);

                        if (!alreadyExists)
                        {
                            identity.AddClaim(
                                new Claim(
                                    ClaimTypes.Role,
                                    role));
                        }
                    }
                }
                catch (JsonException)
                {
                    context.Fail(
                        "Invalid realm_access claim.");
                }

                return Task.CompletedTask;
            }
        };
    });


// ============================================================
// AUTHORIZATION
// ============================================================

builder.Services.AddAuthorization(options =>
{
    // Пользовательский доступ.
    // Разрешён user, manager и admin.
    options.AddPolicy(
        Policies.UserAccess,
        policy =>
        {
            policy.RequireAuthenticatedUser();

            policy.RequireRole(
                Roles.User,
                Roles.Manager,
                Roles.Admin);
        });

    // Доступ менеджера.
    // Разрешён manager и admin.
    options.AddPolicy(
        Policies.ManagerAccess,
        policy =>
        {
            policy.RequireAuthenticatedUser();

            policy.RequireRole(
                Roles.Manager,
                Roles.Admin);
        });

    // Только администратор.
    options.AddPolicy(
        Policies.AdminAccess,
        policy =>
        {
            policy.RequireAuthenticatedUser();

            policy.RequireRole(
                Roles.Admin);
        });
});



// ============================================================
// APPLICATION SERVICES
// ============================================================

builder.Services.AddScoped<
    IDirectionService,
    DirectionService>();

builder.Services.AddScoped<
    IProductService,
    ProductService>();

builder.Services.AddScoped<
    IProgramService,
    ProgramService>();

builder.Services.AddScoped<
    IUniversityService,
    UniversityService>();

builder.Services.AddScoped<
    ICurrentUserService,
    CurrentUserService>();

builder.Services.AddScoped<
    IUserAccessService,
    UserAccessService>();

builder.Services.AddScoped<IWorkflowService, WorkflowService>();

builder.Services.AddScoped<
    IWorkflowStatusService,
    WorkflowStatusService>();

builder.Services.AddScoped<
    IWorkflowTransitionService,
    WorkflowTransitionService>();

builder.Services.AddScoped<
    IInteractionService,
    InteractionService>();

builder.Services.AddScoped<
    IAuditService,
    AuditService>();

builder.Services.AddScoped<
IAttachmentService,
AttachmentService>();

builder.Services.AddScoped<
    IImportService,
    ImportService>();

builder.Services.AddScoped<
    IReportService,
    ReportService>();

builder.Services.AddScoped<
ILmsIntegrationService,
MockLmsIntegrationService>();

builder.Services.AddScoped<
    IWebsiteIntegrationService,
    MockWebsiteIntegrationService>();



builder.Services.AddScoped<IAuthService, AuthService>();

builder.Services.AddScoped<IUserSyncService, UserSyncService>();

builder.Services.AddScoped<IUserService, UserService>();
builder.Services.AddScoped<IContractService, ContractService>();
builder.Services.AddScoped<ILicenseService, LicenseService>();
builder.Services.AddScoped<INotificationService, NotificationService>();
builder.Services.AddScoped<IResponsiblesService, ResponsiblesService>();
builder.Services.AddSingleton<IFileStorage, S3FileStorage>();

builder.Services.AddHostedService<
    IntegrationSyncBackgroundService>();


// ============================================================
// CORS
// ============================================================

builder.Services.AddCors(options =>
{
    options.AddPolicy(
        "ReactClient",
        policy =>
        {
            policy
                .WithOrigins(
                    "http://localhost:5173")
                .AllowAnyHeader()
                .AllowAnyMethod();
        });
});

QuestPDF.Settings.License =
    QuestPDF.Infrastructure.LicenseType.Community;


builder.Services
.Configure<LmsIntegrationOptions>(
    builder.Configuration.GetSection(
        LmsIntegrationOptions.SectionName));

builder.Services
    .Configure<WebsiteIntegrationOptions>(
        builder.Configuration.GetSection(
            WebsiteIntegrationOptions.SectionName));

builder.Services.AddHttpClient("LmsIntegration");
builder.Services.AddHttpClient("WebsiteIntegration");


var app = builder.Build();

app.UseGlobalExceptionHandling();

// ============================================================
// SWAGGER MIDDLEWARE
// ============================================================

if (app.Environment.IsDevelopment())
{
    app.UseSwagger();

    app.UseSwaggerUI(options =>
    {
        // Keycloak frontend client.
        options.OAuthClientId(
            "itschool-crm-frontend");

        // Используем Authorization Code + PKCE.
        options.OAuthUsePkce();
    });
}


// ============================================================
// HTTP PIPELINE
// ============================================================

app.UseHttpsRedirection();

app.UseCors("ReactClient");

app.UseAuthentication();

app.UseAuthorization();

app.MapControllers();

// Эндпоинт готовности: 200, когда живы БД и KeyDB.
// Удобно показывать на защите и мониторить в эксплуатации.
app.MapHealthChecks("/health/ready");

app.Run();