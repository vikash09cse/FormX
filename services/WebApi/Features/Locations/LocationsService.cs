using System.Text.Json;
using ClosedXML.Excel;
using SharedKernel.Enums;
using SharedKernel.Utilities;
using SharedKernel.Utilities.Extensions;
using WebApi.Features.Locations.Infrastructure;
using WebApi.Features.Permissions;

namespace WebApi.Features.Locations;

public class LocationsService(
    ILocationsRepository repository,
    MenuAccessService menuAccess,
    IHttpContextAccessor httpContextAccessor)
{
    private Result<T>? RequireTenant<T>()
    {
        var ctx = httpContextAccessor.HttpContext?.TryGetTenantContext();
        if (ctx == null || !ctx.IsValidForTenantScope())
            return Result<T>.Fail(ErrorCode.Forbidden, "Tenant context is required.");
        return null;
    }

    public async Task<Result<bool>> EnsureLocationAdminAsync(CancellationToken ct) =>
        await menuAccess.RequireMenuAsync(MenuKeys.Location, ct);

    private Guid TenantId => httpContextAccessor.GetTenantContext().TenantId;
    private Guid UserId => httpContextAccessor.GetTenantContext().UserId;

    private static LocationItemResponse Map(LocationRow row) => new(
        row.Id,
        row.Name,
        row.Code,
        row.Status == (byte)ActiveInactiveStatus.Active ? "Active" : "Inactive",
        row.Status,
        row.CreatedAt,
        row.UpdatedAt,
        row.ParentId,
        row.ParentName,
        row.StateId,
        row.StateName,
        row.DistrictId,
        row.DistrictName);

    private static Result<T>? ValidateNameCodeStatus<T>(string name, string code, byte status)
    {
        if (string.IsNullOrWhiteSpace(name) || name.Trim().Length < 2)
            return Result<T>.Fail(ErrorCode.Validation, "Name must be at least 2 characters.");
        if (string.IsNullOrWhiteSpace(code))
            return Result<T>.Fail(ErrorCode.Validation, "Code is required.");
        if (status is not ((byte)ActiveInactiveStatus.Active) and not ((byte)ActiveInactiveStatus.Inactive))
            return Result<T>.Fail(ErrorCode.Validation, "Invalid status.");
        return null;
    }

    // ---- States ----
    public async Task<Result<IEnumerable<LocationItemResponse>>> GetStatesAsync(CancellationToken ct)
    {
        var err = RequireTenant<IEnumerable<LocationItemResponse>>();
        if (err != null) return err;
        var rows = await repository.GetStatesAsync(TenantId, ct);
        return Result<IEnumerable<LocationItemResponse>>.Ok(rows.Select(Map));
    }

    public async Task<Result<LocationItemResponse>> CreateStateAsync(SaveStateRequest request, CancellationToken ct)
    {
        var access = await EnsureLocationAdminAsync(ct);
        if (!access.Success) return Result<LocationItemResponse>.Fail(access.ErrorCode, access.Message);
        var err = RequireTenant<LocationItemResponse>() ?? ValidateNameCodeStatus<LocationItemResponse>(request.Name, request.Code, request.Status);
        if (err != null) return err;

        var name = request.Name.Trim();
        var code = request.Code.Trim();
        if (await repository.StateNameExistsAsync(TenantId, name, null, ct))
            return Result<LocationItemResponse>.Fail(ErrorCode.AlreadyExists, "A state with this name already exists.");
        if (await repository.StateCodeExistsAsync(TenantId, code, null, ct))
            return Result<LocationItemResponse>.Fail(ErrorCode.AlreadyExists, "A state with this code already exists.");

        var id = Guid.NewGuid();
        await repository.CreateStateAsync(TenantId, id, name, code, request.Status, UserId, ct);
        return Result<LocationItemResponse>.Ok(Map((await repository.GetStateByIdAsync(TenantId, id, ct))!), "State created successfully.");
    }

    public async Task<Result<LocationItemResponse>> UpdateStateAsync(Guid id, SaveStateRequest request, CancellationToken ct)
    {
        var access = await EnsureLocationAdminAsync(ct);
        if (!access.Success) return Result<LocationItemResponse>.Fail(access.ErrorCode, access.Message);
        var err = RequireTenant<LocationItemResponse>() ?? ValidateNameCodeStatus<LocationItemResponse>(request.Name, request.Code, request.Status);
        if (err != null) return err;
        if (await repository.GetStateByIdAsync(TenantId, id, ct) == null)
            return Result<LocationItemResponse>.Fail(ErrorCode.NotFound, "State not found.");

        var name = request.Name.Trim();
        var code = request.Code.Trim();
        if (await repository.StateNameExistsAsync(TenantId, name, id, ct))
            return Result<LocationItemResponse>.Fail(ErrorCode.AlreadyExists, "A state with this name already exists.");
        if (await repository.StateCodeExistsAsync(TenantId, code, id, ct))
            return Result<LocationItemResponse>.Fail(ErrorCode.AlreadyExists, "A state with this code already exists.");

        await repository.UpdateStateAsync(TenantId, id, name, code, request.Status, UserId, ct);
        return Result<LocationItemResponse>.Ok(Map((await repository.GetStateByIdAsync(TenantId, id, ct))!), "State updated successfully.");
    }

    public async Task<Result<bool>> DeleteStateAsync(Guid id, CancellationToken ct)
    {
        var access = await EnsureLocationAdminAsync(ct);
        if (!access.Success) return access;
        var err = RequireTenant<bool>();
        if (err != null) return err;
        if (await repository.GetStateByIdAsync(TenantId, id, ct) == null)
            return Result<bool>.Fail(ErrorCode.NotFound, "State not found.");
        await repository.DeleteStateAsync(TenantId, id, UserId, ct);
        return Result<bool>.Ok(true, "State deleted successfully.");
    }

    // ---- Districts ----
    public async Task<Result<IEnumerable<LocationItemResponse>>> GetDistrictsAsync(Guid? stateId, CancellationToken ct)
    {
        var err = RequireTenant<IEnumerable<LocationItemResponse>>();
        if (err != null) return err;
        var rows = await repository.GetDistrictsAsync(TenantId, stateId, ct);
        return Result<IEnumerable<LocationItemResponse>>.Ok(rows.Select(Map));
    }

    public async Task<Result<LocationItemResponse>> CreateDistrictAsync(SaveDistrictRequest request, CancellationToken ct)
    {
        var access = await EnsureLocationAdminAsync(ct);
        if (!access.Success) return Result<LocationItemResponse>.Fail(access.ErrorCode, access.Message);
        var err = RequireTenant<LocationItemResponse>() ?? ValidateNameCodeStatus<LocationItemResponse>(request.Name, request.Code, request.Status);
        if (err != null) return err;
        if (request.StateId == Guid.Empty)
            return Result<LocationItemResponse>.Fail(ErrorCode.Validation, "State is required.");

        var name = request.Name.Trim();
        var code = request.Code.Trim();
        if (await repository.DistrictNameExistsAsync(TenantId, request.StateId, name, null, ct))
            return Result<LocationItemResponse>.Fail(ErrorCode.AlreadyExists, "A district with this name already exists in the state.");
        if (await repository.DistrictCodeExistsAsync(TenantId, code, null, ct))
            return Result<LocationItemResponse>.Fail(ErrorCode.AlreadyExists, "A district with this code already exists.");

        var id = Guid.NewGuid();
        await repository.CreateDistrictAsync(TenantId, id, request.StateId, name, code, request.Status, UserId, ct);
        return Result<LocationItemResponse>.Ok(Map((await repository.GetDistrictByIdAsync(TenantId, id, ct))!), "District created successfully.");
    }

    public async Task<Result<LocationItemResponse>> UpdateDistrictAsync(Guid id, SaveDistrictRequest request, CancellationToken ct)
    {
        var access = await EnsureLocationAdminAsync(ct);
        if (!access.Success) return Result<LocationItemResponse>.Fail(access.ErrorCode, access.Message);
        var err = RequireTenant<LocationItemResponse>() ?? ValidateNameCodeStatus<LocationItemResponse>(request.Name, request.Code, request.Status);
        if (err != null) return err;
        if (await repository.GetDistrictByIdAsync(TenantId, id, ct) == null)
            return Result<LocationItemResponse>.Fail(ErrorCode.NotFound, "District not found.");

        var name = request.Name.Trim();
        var code = request.Code.Trim();
        if (await repository.DistrictNameExistsAsync(TenantId, request.StateId, name, id, ct))
            return Result<LocationItemResponse>.Fail(ErrorCode.AlreadyExists, "A district with this name already exists in the state.");
        if (await repository.DistrictCodeExistsAsync(TenantId, code, id, ct))
            return Result<LocationItemResponse>.Fail(ErrorCode.AlreadyExists, "A district with this code already exists.");

        await repository.UpdateDistrictAsync(TenantId, id, request.StateId, name, code, request.Status, UserId, ct);
        return Result<LocationItemResponse>.Ok(Map((await repository.GetDistrictByIdAsync(TenantId, id, ct))!), "District updated successfully.");
    }

    public async Task<Result<bool>> DeleteDistrictAsync(Guid id, CancellationToken ct)
    {
        var access = await EnsureLocationAdminAsync(ct);
        if (!access.Success) return access;
        var err = RequireTenant<bool>();
        if (err != null) return err;
        if (await repository.GetDistrictByIdAsync(TenantId, id, ct) == null)
            return Result<bool>.Fail(ErrorCode.NotFound, "District not found.");
        await repository.DeleteDistrictAsync(TenantId, id, UserId, ct);
        return Result<bool>.Ok(true, "District deleted successfully.");
    }

    // ---- Blocks ----
    public async Task<Result<IEnumerable<LocationItemResponse>>> GetBlocksAsync(Guid? districtId, CancellationToken ct)
    {
        var err = RequireTenant<IEnumerable<LocationItemResponse>>();
        if (err != null) return err;
        var rows = await repository.GetBlocksAsync(TenantId, districtId, ct);
        return Result<IEnumerable<LocationItemResponse>>.Ok(rows.Select(Map));
    }

    public async Task<Result<LocationItemResponse>> CreateBlockAsync(SaveBlockRequest request, CancellationToken ct)
    {
        var access = await EnsureLocationAdminAsync(ct);
        if (!access.Success) return Result<LocationItemResponse>.Fail(access.ErrorCode, access.Message);
        var err = RequireTenant<LocationItemResponse>() ?? ValidateNameCodeStatus<LocationItemResponse>(request.Name, request.Code, request.Status);
        if (err != null) return err;
        if (request.DistrictId == Guid.Empty)
            return Result<LocationItemResponse>.Fail(ErrorCode.Validation, "District is required.");

        var name = request.Name.Trim();
        var code = request.Code.Trim();
        if (await repository.BlockNameExistsAsync(TenantId, request.DistrictId, name, null, ct))
            return Result<LocationItemResponse>.Fail(ErrorCode.AlreadyExists, "A block with this name already exists in the district.");
        if (await repository.BlockCodeExistsAsync(TenantId, code, null, ct))
            return Result<LocationItemResponse>.Fail(ErrorCode.AlreadyExists, "A block with this code already exists.");

        var id = Guid.NewGuid();
        await repository.CreateBlockAsync(TenantId, id, request.DistrictId, name, code, request.Status, UserId, ct);
        return Result<LocationItemResponse>.Ok(Map((await repository.GetBlockByIdAsync(TenantId, id, ct))!), "Block created successfully.");
    }

    public async Task<Result<LocationItemResponse>> UpdateBlockAsync(Guid id, SaveBlockRequest request, CancellationToken ct)
    {
        var access = await EnsureLocationAdminAsync(ct);
        if (!access.Success) return Result<LocationItemResponse>.Fail(access.ErrorCode, access.Message);
        var err = RequireTenant<LocationItemResponse>() ?? ValidateNameCodeStatus<LocationItemResponse>(request.Name, request.Code, request.Status);
        if (err != null) return err;
        if (await repository.GetBlockByIdAsync(TenantId, id, ct) == null)
            return Result<LocationItemResponse>.Fail(ErrorCode.NotFound, "Block not found.");

        var name = request.Name.Trim();
        var code = request.Code.Trim();
        if (await repository.BlockNameExistsAsync(TenantId, request.DistrictId, name, id, ct))
            return Result<LocationItemResponse>.Fail(ErrorCode.AlreadyExists, "A block with this name already exists in the district.");
        if (await repository.BlockCodeExistsAsync(TenantId, code, id, ct))
            return Result<LocationItemResponse>.Fail(ErrorCode.AlreadyExists, "A block with this code already exists.");

        await repository.UpdateBlockAsync(TenantId, id, request.DistrictId, name, code, request.Status, UserId, ct);
        return Result<LocationItemResponse>.Ok(Map((await repository.GetBlockByIdAsync(TenantId, id, ct))!), "Block updated successfully.");
    }

    public async Task<Result<bool>> DeleteBlockAsync(Guid id, CancellationToken ct)
    {
        var access = await EnsureLocationAdminAsync(ct);
        if (!access.Success) return access;
        var err = RequireTenant<bool>();
        if (err != null) return err;
        if (await repository.GetBlockByIdAsync(TenantId, id, ct) == null)
            return Result<bool>.Fail(ErrorCode.NotFound, "Block not found.");
        await repository.DeleteBlockAsync(TenantId, id, UserId, ct);
        return Result<bool>.Ok(true, "Block deleted successfully.");
    }

    // ---- Villages ----
    public async Task<Result<IEnumerable<LocationItemResponse>>> GetVillagesAsync(Guid? blockId, CancellationToken ct)
    {
        var err = RequireTenant<IEnumerable<LocationItemResponse>>();
        if (err != null) return err;
        var rows = await repository.GetVillagesAsync(TenantId, blockId, ct);
        return Result<IEnumerable<LocationItemResponse>>.Ok(rows.Select(Map));
    }

    public async Task<Result<LocationItemResponse>> CreateVillageAsync(SaveVillageRequest request, CancellationToken ct)
    {
        var access = await EnsureLocationAdminAsync(ct);
        if (!access.Success) return Result<LocationItemResponse>.Fail(access.ErrorCode, access.Message);
        var err = RequireTenant<LocationItemResponse>() ?? ValidateNameCodeStatus<LocationItemResponse>(request.Name, request.Code, request.Status);
        if (err != null) return err;
        if (request.BlockId == Guid.Empty)
            return Result<LocationItemResponse>.Fail(ErrorCode.Validation, "Block is required.");

        var name = request.Name.Trim();
        var code = request.Code.Trim();
        if (await repository.VillageNameExistsAsync(TenantId, request.BlockId, name, null, ct))
            return Result<LocationItemResponse>.Fail(ErrorCode.AlreadyExists, "A village with this name already exists in the block.");
        if (await repository.VillageCodeExistsAsync(TenantId, code, null, ct))
            return Result<LocationItemResponse>.Fail(ErrorCode.AlreadyExists, "A village with this code already exists.");

        var id = Guid.NewGuid();
        await repository.CreateVillageAsync(TenantId, id, request.BlockId, name, code, request.Status, UserId, ct);
        return Result<LocationItemResponse>.Ok(Map((await repository.GetVillageByIdAsync(TenantId, id, ct))!), "Village created successfully.");
    }

    public async Task<Result<LocationItemResponse>> UpdateVillageAsync(Guid id, SaveVillageRequest request, CancellationToken ct)
    {
        var access = await EnsureLocationAdminAsync(ct);
        if (!access.Success) return Result<LocationItemResponse>.Fail(access.ErrorCode, access.Message);
        var err = RequireTenant<LocationItemResponse>() ?? ValidateNameCodeStatus<LocationItemResponse>(request.Name, request.Code, request.Status);
        if (err != null) return err;
        if (await repository.GetVillageByIdAsync(TenantId, id, ct) == null)
            return Result<LocationItemResponse>.Fail(ErrorCode.NotFound, "Village not found.");

        var name = request.Name.Trim();
        var code = request.Code.Trim();
        if (await repository.VillageNameExistsAsync(TenantId, request.BlockId, name, id, ct))
            return Result<LocationItemResponse>.Fail(ErrorCode.AlreadyExists, "A village with this name already exists in the block.");
        if (await repository.VillageCodeExistsAsync(TenantId, code, id, ct))
            return Result<LocationItemResponse>.Fail(ErrorCode.AlreadyExists, "A village with this code already exists.");

        await repository.UpdateVillageAsync(TenantId, id, request.BlockId, name, code, request.Status, UserId, ct);
        return Result<LocationItemResponse>.Ok(Map((await repository.GetVillageByIdAsync(TenantId, id, ct))!), "Village updated successfully.");
    }

    public async Task<Result<bool>> DeleteVillageAsync(Guid id, CancellationToken ct)
    {
        var access = await EnsureLocationAdminAsync(ct);
        if (!access.Success) return access;
        var err = RequireTenant<bool>();
        if (err != null) return err;
        if (await repository.GetVillageByIdAsync(TenantId, id, ct) == null)
            return Result<bool>.Fail(ErrorCode.NotFound, "Village not found.");
        await repository.DeleteVillageAsync(TenantId, id, UserId, ct);
        return Result<bool>.Ok(true, "Village deleted successfully.");
    }

    public byte[] BuildImportTemplate()
    {
        using var wb = new XLWorkbook();
        var states = wb.AddWorksheet("States");
        states.Cell(1, 1).Value = "Code";
        states.Cell(1, 2).Value = "Name";
        states.Cell(1, 3).Value = "Status";
        states.Cell(2, 1).Value = "MH";
        states.Cell(2, 2).Value = "Maharashtra";
        states.Cell(2, 3).Value = 1;

        var districts = wb.AddWorksheet("Districts");
        districts.Cell(1, 1).Value = "Code";
        districts.Cell(1, 2).Value = "Name";
        districts.Cell(1, 3).Value = "ParentCode";
        districts.Cell(1, 4).Value = "Status";
        districts.Cell(2, 1).Value = "MH-PUN";
        districts.Cell(2, 2).Value = "Pune";
        districts.Cell(2, 3).Value = "MH";
        districts.Cell(2, 4).Value = 1;

        var blocks = wb.AddWorksheet("Blocks");
        blocks.Cell(1, 1).Value = "Code";
        blocks.Cell(1, 2).Value = "Name";
        blocks.Cell(1, 3).Value = "ParentCode";
        blocks.Cell(1, 4).Value = "Status";
        blocks.Cell(2, 1).Value = "MH-PUN-HAV";
        blocks.Cell(2, 2).Value = "Haveli";
        blocks.Cell(2, 3).Value = "MH-PUN";
        blocks.Cell(2, 4).Value = 1;

        var villages = wb.AddWorksheet("Villages");
        villages.Cell(1, 1).Value = "Code";
        villages.Cell(1, 2).Value = "Name";
        villages.Cell(1, 3).Value = "ParentCode";
        villages.Cell(1, 4).Value = "Status";
        villages.Cell(2, 1).Value = "MH-PUN-HAV-001";
        villages.Cell(2, 2).Value = "Sample Village";
        villages.Cell(2, 3).Value = "MH-PUN-HAV";
        villages.Cell(2, 4).Value = 1;

        using var stream = new MemoryStream();
        wb.SaveAs(stream);
        return stream.ToArray();
    }

    public async Task<Result<LocationImportResult>> ImportAsync(Stream fileStream, CancellationToken ct)
    {
        var access = await EnsureLocationAdminAsync(ct);
        if (!access.Success) return Result<LocationImportResult>.Fail(access.ErrorCode, access.Message);
        var err = RequireTenant<LocationImportResult>();
        if (err != null) return err;

        using var wb = new XLWorkbook(fileStream);
        var allErrors = new List<LocationImportError>();
        var totalImported = 0;

        async Task RunSheet(string sheetName, string procedure, bool needsParent)
        {
            if (!wb.Worksheets.TryGetWorksheet(sheetName, out var ws))
                return;

            var rows = new List<object>();
            var lastRow = ws.LastRowUsed()?.RowNumber() ?? 1;
            for (var r = 2; r <= lastRow; r++)
            {
                var code = ws.Cell(r, 1).GetString().Trim();
                var name = ws.Cell(r, 2).GetString().Trim();
                if (string.IsNullOrWhiteSpace(code) && string.IsNullOrWhiteSpace(name))
                    continue;

                if (needsParent)
                {
                    var parentCode = ws.Cell(r, 3).GetString().Trim();
                    var statusCell = ws.Cell(r, 4);
                    var status = statusCell.IsEmpty() ? 1 : (byte)(statusCell.TryGetValue(out double d) ? (int)d : 1);
                    rows.Add(new { code, name, parentCode, status });
                }
                else
                {
                    var statusCell = ws.Cell(r, 3);
                    var status = statusCell.IsEmpty() ? 1 : (byte)(statusCell.TryGetValue(out double d) ? (int)d : 1);
                    rows.Add(new { code, name, status });
                }
            }

            if (rows.Count == 0) return;

            var json = JsonSerializer.Serialize(rows);
            var (summary, errors) = await repository.ImportAsync(procedure, TenantId, UserId, json, ct);
            totalImported += summary.Imported;
            allErrors.AddRange(errors.Select(e => new LocationImportError(e.Rownum, $"{sheetName} row {e.Rownum}: {e.Message}")));
        }

        await RunSheet("States", "dbo.sp_location_import_states", false);
        await RunSheet("Districts", "dbo.sp_location_import_districts", true);
        await RunSheet("Blocks", "dbo.sp_location_import_blocks", true);
        await RunSheet("Villages", "dbo.sp_location_import_villages", true);

        return Result<LocationImportResult>.Ok(
            new LocationImportResult(totalImported, allErrors.Count, allErrors),
            allErrors.Count == 0 ? "Import completed successfully." : "Import completed with errors.");
    }
}
