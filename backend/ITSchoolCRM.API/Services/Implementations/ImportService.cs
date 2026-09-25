using System.Globalization;
using System.Text;
using System.Text.Json;
using ITSchoolCRM.API.Data;
using ITSchoolCRM.API.DTOs.Imports;
using ITSchoolCRM.API.Models;
using ITSchoolCRM.API.Services.Interfaces;
using Microsoft.EntityFrameworkCore;
using NPOI.HSSF.UserModel;
using NPOI.SS.UserModel;
using NPOI.XSSF.UserModel;

namespace ITSchoolCRM.API.Services.Implementations;

public class ImportService : IImportService
{
    private readonly CrmDbContext _context;
    private readonly IUserAccessService _accessService;
    private readonly IAuditService _auditService;

    private static readonly HashSet<string> AllowedExcelExtensions =
        new(StringComparer.OrdinalIgnoreCase)
        {
            ".xls",
            ".xlsx"
        };

    private static readonly Dictionary<string, string> MappingFields =
        new(StringComparer.OrdinalIgnoreCase)
        {
            ["UniversityName"] = "Название ВУЗа",
            ["DirectionName"] = "ИТ-направление",
            ["ProductName"] = "ИТ-продукт",
            ["Vendor"] = "Вендор",
            ["Software"] = "ПО",
            ["ContractNumber"] = "Номер договора",
            ["LicenseSignedAt"] = "Подписание лицензии",
            ["LicenseValidUntil"] = "Срок действия лицензии",
            ["TransferStatus"] = "Статус по передачи",
            ["ManagerFullName"] = "ФИО Менеджера",
            ["UniversityContactFullName"] = "Ответственные от ВУЗа",
            ["Comment"] = "Комментарий"
        };

    public ImportService(
        CrmDbContext context,
        IUserAccessService accessService,
        IAuditService auditService)
    {
        _context = context;
        _accessService = accessService;
        _auditService = auditService;
    }

    public Dictionary<string, string> GetAvailableMappingFields()
    {
        return new Dictionary<string, string>(
            MappingFields,
            StringComparer.OrdinalIgnoreCase);
    }

    public async Task<ImportBatchDto> ImportExcelAsync(
        IFormFile file,
        ImportMappingDto mapping,
        CancellationToken cancellationToken)
    {
        if (file is null || file.Length == 0)
        {
            throw new ArgumentException("Файл не выбран.");
        }

        var extension = Path.GetExtension(file.FileName);

        if (!AllowedExcelExtensions.Contains(extension))
        {
            throw new InvalidOperationException(
                "Для импорта каталогов разрешены только XLS и XLSX.");
        }

        if (mapping is null || mapping.Mapping.Count == 0)
        {
            throw new ArgumentException(
                "Необходимо указать mapping полей.");
        }

        ValidateMapping(mapping.Mapping);

        var userId = await _accessService.GetCurrentDatabaseUserIdAsync(
            cancellationToken);

        var batch = new import_batch
        {
            file_name = Path.GetFileName(file.FileName),
            uploaded_by = userId,
            status = "Processing",
            created_at = DateTime.UtcNow
        };

        _context.import_batches.Add(batch);

        await _context.SaveChangesAsync(cancellationToken);

        try
        {
            var rows = await ReadExcelAsync(
                file,
                extension,
                cancellationToken);

            var imported = 0;

            foreach (var row in rows)
            {
                cancellationToken.ThrowIfCancellationRequested();

                if (IsEmptyRow(row))
                {
                    continue;
                }

                await ImportExcelRowAsync(
                    row,
                    mapping.Mapping,
                    cancellationToken);

                imported++;
            }

            batch.status = "Completed";
            batch.completed_at = DateTime.UtcNow;
            batch.error_message = $"Импортировано строк: {imported}.";

            await _context.SaveChangesAsync(cancellationToken);

            await _auditService.WriteAsync(
                "IMPORT",
                "import_batch",
                batch.import_batches_id,
                null,
                new
                {
                    batch.import_batches_id,
                    batch.file_name,
                    batch.status,
                    imported
                },
                cancellationToken);

            return ToDto(batch);
        }
        catch (Exception ex)
        {
            batch.status = "Failed";
            batch.completed_at = DateTime.UtcNow;
            batch.error_message = ex.Message;

            await _context.SaveChangesAsync(cancellationToken);

            throw;
        }
    }

    private void ValidateMapping(
        Dictionary<string, string> mapping)
    {
        foreach (var pair in mapping)
        {
            if (!MappingFields.ContainsKey(pair.Value))
            {
                throw new InvalidOperationException(
                    $"Поле '{pair.Value}' отсутствует в допустимом mapping.");
            }
        }
    }

    private async Task<List<Dictionary<string, string?>>> ReadExcelAsync(
        IFormFile file,
        string extension,
        CancellationToken cancellationToken)
    {
        await using var input = file.OpenReadStream();

        IWorkbook workbook;

        if (extension.Equals(
            ".xls",
            StringComparison.OrdinalIgnoreCase))
        {
            workbook = new HSSFWorkbook(input);
        }
        else
        {
            workbook = new XSSFWorkbook(input);
        }

        var sheet = workbook.GetSheetAt(0);

        if (sheet is null)
        {
            throw new InvalidOperationException(
                "В Excel-файле отсутствует лист.");
        }

        var headerRow = sheet.GetRow(sheet.FirstRowNum);

        if (headerRow is null)
        {
            throw new InvalidOperationException(
                "В Excel-файле отсутствует строка заголовков.");
        }

        var headers = new List<string>();

        for (var i = 0; i < headerRow.LastCellNum; i++)
        {
            headers.Add(GetCellValue(headerRow.GetCell(i)));
        }

        var result = new List<Dictionary<string, string?>>();

        for (
            var rowIndex = sheet.FirstRowNum + 1;
            rowIndex <= sheet.LastRowNum;
            rowIndex++)
        {
            cancellationToken.ThrowIfCancellationRequested();

            var row = sheet.GetRow(rowIndex);

            if (row is null)
            {
                continue;
            }

            var values = new Dictionary<string, string?>(
                StringComparer.OrdinalIgnoreCase);

            for (
                var columnIndex = 0;
                columnIndex < headers.Count;
                columnIndex++)
            {
                if (string.IsNullOrWhiteSpace(headers[columnIndex]))
                {
                    continue;
                }

                values[headers[columnIndex]] =
                    GetCellValue(row.GetCell(columnIndex));
            }

            result.Add(values);
        }

        return result;
    }

    private static string GetCellValue(ICell? cell)
    {
        if (cell is null)
        {
            return string.Empty;
        }

        if (cell.CellType == CellType.Formula)
        {
            return cell.ToString().Trim();
        }

        if (cell.CellType == CellType.Numeric)
        {
            if (DateUtil.IsCellDateFormatted(cell))
            {
                return cell.DateCellValue.ToString();
            }

            return cell.NumericCellValue.ToString(
                CultureInfo.InvariantCulture);
        }

        return cell.ToString().Trim();
    }

    private static bool IsEmptyRow(
        Dictionary<string, string?> row)
    {
        return row.Values.All(string.IsNullOrWhiteSpace);
    }

    private async Task ImportExcelRowAsync(
        Dictionary<string, string?> row,
        Dictionary<string, string> mapping,
        CancellationToken cancellationToken)
    {
        var universityName = GetMappedValue(
            row,
            mapping,
            "UniversityName");

        var directionName = GetMappedValue(
            row,
            mapping,
            "DirectionName");

        var productName = GetMappedValue(
            row,
            mapping,
            "ProductName");

        if (string.IsNullOrWhiteSpace(universityName))
        {
            throw new InvalidOperationException(
                "Для строки отсутствует название ВУЗа.");
        }

        universityName = universityName.Trim();
        directionName = directionName?.Trim();
        productName = productName?.Trim();

        var university = await _context.universities
            .FirstOrDefaultAsync(
                x =>
                    x.name != null &&
                    x.name.ToLower() == universityName.ToLower(),
                cancellationToken);

        if (university is null)
        {
            university = new university
            {
                name = universityName,
                is_active = true,
                created_at = DateTime.UtcNow
            };

            _context.universities.Add(university);

            await _context.SaveChangesAsync(cancellationToken);
        }

        it_direction? direction = null;

        if (!string.IsNullOrWhiteSpace(directionName))
        {
            direction = await _context.it_directions
                .FirstOrDefaultAsync(
                    x =>
                        x.name != null &&
                        x.name.ToLower() == directionName.ToLower(),
                    cancellationToken);

            if (direction is null)
            {
                direction = new it_direction
                {
                    name = directionName,
                    is_active = true,
                    created_at = DateTime.UtcNow
                };

                _context.it_directions.Add(direction);

                await _context.SaveChangesAsync(cancellationToken);
            }
        }

        it_product? product = null;

        if (!string.IsNullOrWhiteSpace(productName))
        {
            product = await _context.it_products
                .FirstOrDefaultAsync(
                    x =>
                        x.name != null &&
                        x.name.ToLower() == productName.ToLower(),
                    cancellationToken);

            if (product is null)
            {
                product = new it_product
                {
                    name = productName,
                    vendor = GetMappedValue(
                        row,
                        mapping,
                        "Vendor"),
                    description = GetMappedValue(
                        row,
                        mapping,
                        "Software"),
                    is_active = true,
                    created_at = DateTime.UtcNow
                };

                _context.it_products.Add(product);

                await _context.SaveChangesAsync(cancellationToken);
            }
            else
            {
                var vendor = GetMappedValue(
                    row,
                    mapping,
                    "Vendor");

                var software = GetMappedValue(
                    row,
                    mapping,
                    "Software");

                if (!string.IsNullOrWhiteSpace(vendor))
                {
                    product.vendor = vendor;
                }

                if (!string.IsNullOrWhiteSpace(software))
                {
                    product.description = software;
                }

                product.updated_at = DateTime.UtcNow;
            }
        }

        var managerFullName = GetMappedValue(
            row,
            mapping,
            "ManagerFullName");

        user? manager = null;

        if (!string.IsNullOrWhiteSpace(managerFullName))
        {
            // Колонки full_name нет: сопоставляем «ФИО Менеджера» из Excel
            // со сборкой частей ФИО (в памяти — менеджеров немного).
            var managers = await _context.users
                .AsNoTracking()
                .ToListAsync(cancellationToken);

            var target = managerFullName.Trim();

            manager = managers.FirstOrDefault(x =>
                string.Join(' ',
                    new[]
                    {
                        x.last_name,
                        x.first_name,
                        x.middle_name
                    }.Where(s => !string.IsNullOrWhiteSpace(s)))
                .Equals(target, StringComparison.OrdinalIgnoreCase));
        }

        var contactFullName = GetMappedValue(
            row,
            mapping,
            "UniversityContactFullName");

        university_contact? contact = null;

        if (!string.IsNullOrWhiteSpace(contactFullName))
        {
            contact = await _context.university_contacts
                .FirstOrDefaultAsync(
                    x =>
                        x.university_id == university.universities_id &&
                        x.full_name != null &&
                        x.full_name.ToLower() == contactFullName.ToLower(),
                    cancellationToken);

            if (contact is null)
            {
                contact = new university_contact
                {
                    university_id = university.universities_id,
                    full_name = contactFullName,
                    is_active = true
                };

                _context.university_contacts.Add(contact);
            }
        }

        var contractNumber = GetMappedValue(
            row,
            mapping,
            "ContractNumber");

        contract? contract = null;

        if (!string.IsNullOrWhiteSpace(contractNumber))
        {
            contract = await _context.contracts
                .FirstOrDefaultAsync(
                    x => x.contract_number == contractNumber,
                    cancellationToken);

            if (contract is null)
            {
                contract = new contract
                {
                    contract_number = contractNumber,
                    created_at = DateTime.UtcNow
                };

                _context.contracts.Add(contract);
            }

            contract.status = GetMappedValue(
                row,
                mapping,
                "TransferStatus");

            contract.comment = GetMappedValue(
                row,
                mapping,
                "Comment");
        }

        var validUntilText = GetMappedValue(
            row,
            mapping,
            "LicenseValidUntil");

        var signedAtText = GetMappedValue(
            row,
            mapping,
            "LicenseSignedAt");

        license? license = null;

        if (!string.IsNullOrWhiteSpace(validUntilText) ||
            !string.IsNullOrWhiteSpace(signedAtText))
        {
            license = new license
            {
                signed_at = ParseDate(signedAtText),
                valid_until = ParseDate(validUntilText),
                transfer_status = GetMappedValue(
                    row,
                    mapping,
                    "TransferStatus"),
                comment = GetMappedValue(
                    row,
                    mapping,
                    "Comment"),
                created_at = DateTime.UtcNow
            };

            _context.licenses.Add(license);
        }

        await _context.SaveChangesAsync(cancellationToken);

        if (manager is not null)
        {
            var managerExists = await _context.university_managers
                .AnyAsync(
                    x =>
                        x.university_id == university.universities_id &&
                        x.user_id == manager.users_id,
                    cancellationToken);

            if (!managerExists)
            {
                _context.university_managers.Add(
                    new university_manager
                    {
                        university_id = university.universities_id,
                        user_id = manager.users_id,
                        assigned_at = DateTime.UtcNow
                    });
            }
        }

        if (direction is not null && product is not null)
        {
            var program = await _context.it_programs
                .FirstOrDefaultAsync(
                    x =>
                        x.direction_id == direction.it_directions_id &&
                        x.name != null &&
                        x.name.ToLower() == product.name!.ToLower(),
                    cancellationToken);

            if (program is null)
            {
                program = new it_program
                {
                    direction_id = direction.it_directions_id,
                    name = product.name,
                    description = product.description,
                    is_active = true,
                    created_at = DateTime.UtcNow
                };

                _context.it_programs.Add(program);

                await _context.SaveChangesAsync(cancellationToken);
            }

            var programProductExists = await _context.program_products
                .AnyAsync(
                    x =>
                        x.program_id == program.it_programs_id &&
                        x.product_id == product.it_products_id,
                    cancellationToken);

            if (!programProductExists)
            {
                _context.program_products.Add(
                    new program_product
                    {
                        program_id = program.it_programs_id,
                        product_id = product.it_products_id
                    });
            }
        }

        await _context.SaveChangesAsync(cancellationToken);
    }

    private static string? GetMappedValue(
        Dictionary<string, string?> row,
        Dictionary<string, string> mapping,
        string targetField)
    {
        var source = mapping.FirstOrDefault(
            x =>
                x.Value.Equals(
                    targetField,
                    StringComparison.OrdinalIgnoreCase));

        if (string.IsNullOrWhiteSpace(source.Key))
        {
            return null;
        }

        if (!row.TryGetValue(source.Key, out var value))
        {
            return null;
        }

        return string.IsNullOrWhiteSpace(value)
            ? null
            : value.Trim();
    }

    private static DateTime? ParseDate(string? value)
    {
        if (string.IsNullOrWhiteSpace(value))
        {
            return null;
        }

        if (DateTime.TryParse(
            value,
            CultureInfo.InvariantCulture,
            DateTimeStyles.None,
            out var result))
        {
            return result;
        }

        if (DateTime.TryParse(
            value,
            new CultureInfo("ru-RU"),
            DateTimeStyles.None,
            out result))
        {
            return result;
        }

        throw new InvalidOperationException(
            $"Не удалось распознать дату: {value}");
    }

    public async Task<ImportBatchDto> ImportJsonAsync(
        IFormFile file,
        CancellationToken cancellationToken)
    {
        if (file is null || file.Length == 0)
        {
            throw new ArgumentException(
                "JSON-файл не выбран.");
        }

        var extension = Path.GetExtension(file.FileName);

        if (!extension.Equals(
            ".json",
            StringComparison.OrdinalIgnoreCase))
        {
            throw new InvalidOperationException(
                "Для JSON-импорта разрешён только формат JSON.");
        }

        var userId = await _accessService.GetCurrentDatabaseUserIdAsync(
            cancellationToken);

        var batch = new import_batch
        {
            file_name = Path.GetFileName(file.FileName),
            uploaded_by = userId,
            status = "Processing",
            created_at = DateTime.UtcNow
        };

        _context.import_batches.Add(batch);

        await _context.SaveChangesAsync(cancellationToken);

        try
        {
            string json;

            await using (var stream = file.OpenReadStream())
            {
                using var reader = new StreamReader(
                    stream,
                    Encoding.UTF8,
                    detectEncodingFromByteOrderMarks: true);

                json = await reader.ReadToEndAsync(cancellationToken);
            }

            var options = new JsonSerializerOptions
            {
                PropertyNameCaseInsensitive = true
            };

            var request = JsonSerializer.Deserialize<JsonImportRequestDto>(
                json,
                options);

            if (request is null)
            {
                throw new InvalidOperationException(
                    "JSON не содержит корректных данных.");
            }

            foreach (var item in request.Items)
            {
                cancellationToken.ThrowIfCancellationRequested();

                await ImportJsonItemAsync(
                    item,
                    cancellationToken);
            }

            batch.status = "Completed";
            batch.completed_at = DateTime.UtcNow;
            batch.error_message =
                $"Импортировано объектов: {request.Items.Count}.";

            await _context.SaveChangesAsync(cancellationToken);

            await _auditService.WriteAsync(
                "IMPORT_JSON",
                "import_batch",
                batch.import_batches_id,
                null,
                new
                {
                    batch.import_batches_id,
                    batch.file_name,
                    batch.status,
                    count = request.Items.Count
                },
                cancellationToken);

            return ToDto(batch);
        }
        catch (Exception ex)
        {
            batch.status = "Failed";
            batch.completed_at = DateTime.UtcNow;
            batch.error_message = ex.Message;

            await _context.SaveChangesAsync(cancellationToken);

            throw;
        }
    }

    private async Task ImportJsonItemAsync(
        JsonImportItemDto item,
        CancellationToken cancellationToken)
    {
        university? university = null;

        if (item.UniversityId.HasValue)
        {
            university = await _context.universities
                .FirstOrDefaultAsync(
                    x =>
                        x.universities_id ==
                        item.UniversityId.Value,
                    cancellationToken);
        }

        if (university is null &&
            !string.IsNullOrWhiteSpace(item.UniversityName))
        {
            university = await _context.universities
                .FirstOrDefaultAsync(
                    x =>
                        x.name != null &&
                        x.name.ToLower() ==
                        item.UniversityName.Trim().ToLower(),
                    cancellationToken);
        }

        if (university is null)
        {
            throw new InvalidOperationException(
                "В JSON не найден ВУЗ.");
        }

        var workflow = await ResolveWorkflowAsync(
            item,
            cancellationToken);

        var status = await ResolveStatusAsync(
            item,
            workflow.workflows_id,
            cancellationToken);

        if (item.InteractionId.HasValue)
        {
            var existingInteraction =
                await _context.interactions
                    .FirstOrDefaultAsync(
                        x =>
                            x.interactions_id ==
                            item.InteractionId.Value,
                        cancellationToken);

            if (existingInteraction is null)
            {
                throw new InvalidOperationException(
                    $"Interaction {item.InteractionId.Value} не найден.");
            }

            existingInteraction.workflow_id =
                workflow.workflows_id;

            existingInteraction.current_status_id =
                status.workflow_statuses_id;

            existingInteraction.updated_at =
                DateTime.UtcNow;

            await _context.SaveChangesAsync(
                cancellationToken);

            await _auditService.WriteAsync(
                "IMPORT_JSON_INTERACTION_UPDATE",
                "interaction",
                existingInteraction.interactions_id,
                null,
                new
                {
                    existingInteraction.interactions_id,
                    existingInteraction.workflow_id,
                    existingInteraction.current_status_id
                },
                cancellationToken);

            return;
        }

        it_direction? direction = null;

        if (item.DirectionId.HasValue)
        {
            direction = await _context.it_directions
                .FirstOrDefaultAsync(
                    x =>
                        x.it_directions_id ==
                        item.DirectionId.Value,
                    cancellationToken);
        }

        if (direction is null &&
            !string.IsNullOrWhiteSpace(item.DirectionName))
        {
            direction = await _context.it_directions
                .FirstOrDefaultAsync(
                    x =>
                        x.name != null &&
                        x.name.ToLower() ==
                        item.DirectionName.Trim().ToLower(),
                    cancellationToken);
        }

        it_program? program = null;

        if (item.ProgramId.HasValue)
        {
            program = await _context.it_programs
                .FirstOrDefaultAsync(
                    x =>
                        x.it_programs_id ==
                        item.ProgramId.Value,
                    cancellationToken);
        }

        if (program is null &&
            !string.IsNullOrWhiteSpace(item.ProgramName))
        {
            program = await _context.it_programs
                .FirstOrDefaultAsync(
                    x =>
                        x.name != null &&
                        x.name.ToLower() ==
                        item.ProgramName.Trim().ToLower(),
                    cancellationToken);
        }

        it_product? product = null;

        if (item.ProductId.HasValue)
        {
            product = await _context.it_products
                .FirstOrDefaultAsync(
                    x =>
                        x.it_products_id ==
                        item.ProductId.Value,
                    cancellationToken);
        }

        if (product is null &&
            !string.IsNullOrWhiteSpace(item.ProductName))
        {
            product = await _context.it_products
                .FirstOrDefaultAsync(
                    x =>
                        x.name != null &&
                        x.name.ToLower() ==
                        item.ProductName.Trim().ToLower(),
                    cancellationToken);
        }

        user? manager = null;

        if (item.ManagerId.HasValue)
        {
            manager = await _context.users
                .FirstOrDefaultAsync(
                    x =>
                        x.users_id ==
                        item.ManagerId.Value,
                    cancellationToken);
        }

        if (manager is null &&
            !string.IsNullOrWhiteSpace(item.ManagerFullName))
        {
            // Колонки full_name нет: сопоставляем строку со сборкой
            // частей ФИО (в памяти — менеджеров немного).
            var managers = await _context.users
                .AsNoTracking()
                .ToListAsync(cancellationToken);

            var target = item.ManagerFullName.Trim();

            manager = managers.FirstOrDefault(x =>
                string.Join(' ',
                    new[]
                    {
                        x.last_name,
                        x.first_name,
                        x.middle_name
                    }.Where(s => !string.IsNullOrWhiteSpace(s)))
                .Equals(target, StringComparison.OrdinalIgnoreCase));
        }

        university_contact? contact = null;

        if (item.UniversityContactId.HasValue)
        {
            contact = await _context.university_contacts
                .FirstOrDefaultAsync(
                    x =>
                        x.university_contacts_id ==
                        item.UniversityContactId.Value,
                    cancellationToken);
        }

        if (contact is null &&
            !string.IsNullOrWhiteSpace(
                item.UniversityContactFullName))
        {
            contact = await _context.university_contacts
                .FirstOrDefaultAsync(
                    x =>
                        x.university_id ==
                        university.universities_id &&
                        x.full_name != null &&
                        x.full_name.ToLower() ==
                        item.UniversityContactFullName
                            .Trim()
                            .ToLower(),
                    cancellationToken);
        }

        if (contact is null &&
            !string.IsNullOrWhiteSpace(
                item.UniversityContactFullName))
        {
            contact = new university_contact
            {
                university_id =
                    university.universities_id,

                full_name =
                    item.UniversityContactFullName,

                position =
                    item.UniversityContactPosition,

                email =
                    item.UniversityContactEmail,

                phone =
                    item.UniversityContactPhone,

                is_active = true
            };

            _context.university_contacts.Add(contact);

            await _context.SaveChangesAsync(
                cancellationToken);
        }

        var interaction = new interaction
        {
            university_id =
                university.universities_id,

            program_id =
                program?.it_programs_id,

            product_id =
                product?.it_products_id,

            manager_id =
                manager?.users_id,

            university_contact_id =
                contact?.university_contacts_id,

            workflow_id =
                workflow.workflows_id,

            current_status_id =
                status.workflow_statuses_id,

            created_at =
                DateTime.UtcNow,

            updated_at =
                DateTime.UtcNow
        };

        _context.interactions.Add(interaction);

        await _context.SaveChangesAsync(
            cancellationToken);

        await _auditService.WriteAsync(
            "IMPORT_JSON_INTERACTION",
            "interaction",
            interaction.interactions_id,
            null,
            new
            {
                interaction.interactions_id,
                interaction.university_id,
                interaction.program_id,
                interaction.product_id,
                interaction.manager_id,
                interaction.workflow_id,
                interaction.current_status_id
            },
            cancellationToken);
    }

    private async Task<workflow> ResolveWorkflowAsync(
        JsonImportItemDto item,
        CancellationToken cancellationToken)
    {
        workflow? workflow = null;

        if (item.WorkflowId.HasValue)
        {
            workflow = await _context.workflows
                .FirstOrDefaultAsync(
                    x =>
                        x.workflows_id ==
                        item.WorkflowId.Value,
                    cancellationToken);
        }

        if (workflow is null &&
            !string.IsNullOrWhiteSpace(item.WorkflowName))
        {
            workflow = await _context.workflows
                .FirstOrDefaultAsync(
                    x =>
                        x.name != null &&
                        x.name.ToLower() ==
                        item.WorkflowName.Trim().ToLower(),
                    cancellationToken);
        }

        if (workflow is null)
        {
            throw new InvalidOperationException(
                "Workflow не найден.");
        }

        return workflow;
    }

    private async Task<workflow_status> ResolveStatusAsync(
        JsonImportItemDto item,
        int workflowId,
        CancellationToken cancellationToken)
    {
        workflow_status? status = null;

        if (item.StatusId.HasValue)
        {
            status = await _context.workflow_statuses
                .FirstOrDefaultAsync(
                    x =>
                        x.workflow_statuses_id ==
                        item.StatusId.Value &&
                        x.workflow_id ==
                        workflowId,
                    cancellationToken);
        }

        if (status is null &&
            !string.IsNullOrWhiteSpace(item.StatusName))
        {
            status = await _context.workflow_statuses
                .FirstOrDefaultAsync(
                    x =>
                        x.workflow_id ==
                        workflowId &&
                        x.name != null &&
                        x.name.ToLower() ==
                        item.StatusName.Trim().ToLower(),
                    cancellationToken);
        }

        if (status is null)
        {
            status = await _context.workflow_statuses
                .FirstOrDefaultAsync(
                    x =>
                        x.workflow_id ==
                        workflowId &&
                        x.is_initial == true,
                    cancellationToken);
        }

        if (status is null)
        {
            throw new InvalidOperationException(
                "Для workflow не найден статус.");
        }

        return status;
    }

    public async Task<List<ImportBatchDto>> GetBatchesAsync(
        CancellationToken cancellationToken)
    {
        return await _context.import_batches
            .AsNoTracking()
            .OrderByDescending(x => x.created_at)
            .Select(x => new ImportBatchDto
            {
                Id = x.import_batches_id,
                FileName = x.file_name,
                UploadedBy = x.uploaded_by,
                Status = x.status,
                CreatedAt = x.created_at,
                CompletedAt = x.completed_at,
                ErrorMessage = x.error_message
            })
            .ToListAsync(cancellationToken);
    }

    public async Task<ImportBatchDto?> GetBatchByIdAsync(
        int id,
        CancellationToken cancellationToken)
    {
        return await _context.import_batches
            .AsNoTracking()
            .Where(x => x.import_batches_id == id)
            .Select(x => new ImportBatchDto
            {
                Id = x.import_batches_id,
                FileName = x.file_name,
                UploadedBy = x.uploaded_by,
                Status = x.status,
                CreatedAt = x.created_at,
                CompletedAt = x.completed_at,
                ErrorMessage = x.error_message
            })
            .FirstOrDefaultAsync(cancellationToken);
    }

    private static ImportBatchDto ToDto(
        import_batch batch)
    {
        return new ImportBatchDto
        {
            Id = batch.import_batches_id,
            FileName = batch.file_name,
            UploadedBy = batch.uploaded_by,
            Status = batch.status,
            CreatedAt = batch.created_at,
            CompletedAt = batch.completed_at,
            ErrorMessage = batch.error_message
        };
    }
}