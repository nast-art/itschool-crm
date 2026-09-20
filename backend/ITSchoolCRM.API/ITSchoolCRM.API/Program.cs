using System.Security.Claims;
using System.Text.Json;

using ITSchoolCRM.API.Auth;
using ITSchoolCRM.API.Data;
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
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using Microsoft.OpenApi;

AppContext.SetSwitch("Npgsql.EnableLegacyTimestampBehavior", true);
var builder = WebApplication.CreateBuilder(args);


// ============================================================
// CONTROLLERS
// ============================================================

builder.Services
    .AddControllers()
    .AddJsonOptions(options =>
        options.JsonSerializerOptions.Converters
            .Add(new UtcDateTimeConverter()));

builder.Services.AddEndpointsApiExplorer();



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

app.Run();