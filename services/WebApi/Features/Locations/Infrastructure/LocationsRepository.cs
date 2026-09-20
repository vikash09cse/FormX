using Dapper;
using SharedKernel.Utilities.Helpers;
using System.Data;

namespace WebApi.Features.Locations.Infrastructure;

public class LocationRow
{
    public Guid Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public string Code { get; set; } = string.Empty;
    public byte Status { get; set; }
    public DateTime CreatedAt { get; set; }
    public DateTime UpdatedAt { get; set; }
    public Guid? ParentId { get; set; }
    public string? ParentName { get; set; }
    public Guid? StateId { get; set; }
    public string? StateName { get; set; }
    public Guid? DistrictId { get; set; }
    public string? DistrictName { get; set; }
}

public class LocationImportSummaryRow
{
    public int Imported { get; set; }
    public int ErrorCount { get; set; }
}

public class LocationImportErrorRow
{
    public int Rownum { get; set; }
    public string Message { get; set; } = string.Empty;
}

internal class StateDbRow
{
    public Guid Stateid { get; set; }
    public string Name { get; set; } = string.Empty;
    public string Code { get; set; } = string.Empty;
    public byte Status { get; set; }
    public DateTime Createdat { get; set; }
    public DateTime Updatedat { get; set; }
}

internal class DistrictDbRow
{
    public Guid Districtid { get; set; }
    public Guid Stateid { get; set; }
    public string Statename { get; set; } = string.Empty;
    public string Name { get; set; } = string.Empty;
    public string Code { get; set; } = string.Empty;
    public byte Status { get; set; }
    public DateTime Createdat { get; set; }
    public DateTime Updatedat { get; set; }
}

internal class BlockDbRow
{
    public Guid Blockid { get; set; }
    public Guid Districtid { get; set; }
    public string Districtname { get; set; } = string.Empty;
    public Guid Stateid { get; set; }
    public string Statename { get; set; } = string.Empty;
    public string Name { get; set; } = string.Empty;
    public string Code { get; set; } = string.Empty;
    public byte Status { get; set; }
    public DateTime Createdat { get; set; }
    public DateTime Updatedat { get; set; }
}

internal class VillageDbRow
{
    public Guid Villageid { get; set; }
    public Guid Blockid { get; set; }
    public string Blockname { get; set; } = string.Empty;
    public Guid Districtid { get; set; }
    public string Districtname { get; set; } = string.Empty;
    public Guid Stateid { get; set; }
    public string Statename { get; set; } = string.Empty;
    public string Name { get; set; } = string.Empty;
    public string Code { get; set; } = string.Empty;
    public byte Status { get; set; }
    public DateTime Createdat { get; set; }
    public DateTime Updatedat { get; set; }
}

public interface ILocationsRepository
{
    Task<IEnumerable<LocationRow>> GetStatesAsync(Guid tenantId, CancellationToken ct);
    Task<LocationRow?> GetStateByIdAsync(Guid tenantId, Guid id, CancellationToken ct);
    Task<bool> StateNameExistsAsync(Guid tenantId, string name, Guid? excludeId, CancellationToken ct);
    Task<bool> StateCodeExistsAsync(Guid tenantId, string code, Guid? excludeId, CancellationToken ct);
    Task CreateStateAsync(Guid tenantId, Guid id, string name, string code, byte status, Guid userId, CancellationToken ct);
    Task UpdateStateAsync(Guid tenantId, Guid id, string name, string code, byte status, Guid userId, CancellationToken ct);
    Task DeleteStateAsync(Guid tenantId, Guid id, Guid userId, CancellationToken ct);

    Task<IEnumerable<LocationRow>> GetDistrictsAsync(Guid tenantId, Guid? stateId, CancellationToken ct);
    Task<LocationRow?> GetDistrictByIdAsync(Guid tenantId, Guid id, CancellationToken ct);
    Task<bool> DistrictNameExistsAsync(Guid tenantId, Guid stateId, string name, Guid? excludeId, CancellationToken ct);
    Task<bool> DistrictCodeExistsAsync(Guid tenantId, string code, Guid? excludeId, CancellationToken ct);
    Task CreateDistrictAsync(Guid tenantId, Guid id, Guid stateId, string name, string code, byte status, Guid userId, CancellationToken ct);
    Task UpdateDistrictAsync(Guid tenantId, Guid id, Guid stateId, string name, string code, byte status, Guid userId, CancellationToken ct);
    Task DeleteDistrictAsync(Guid tenantId, Guid id, Guid userId, CancellationToken ct);

    Task<IEnumerable<LocationRow>> GetBlocksAsync(Guid tenantId, Guid? districtId, CancellationToken ct);
    Task<LocationRow?> GetBlockByIdAsync(Guid tenantId, Guid id, CancellationToken ct);
    Task<bool> BlockNameExistsAsync(Guid tenantId, Guid districtId, string name, Guid? excludeId, CancellationToken ct);
    Task<bool> BlockCodeExistsAsync(Guid tenantId, string code, Guid? excludeId, CancellationToken ct);
    Task CreateBlockAsync(Guid tenantId, Guid id, Guid districtId, string name, string code, byte status, Guid userId, CancellationToken ct);
    Task UpdateBlockAsync(Guid tenantId, Guid id, Guid districtId, string name, string code, byte status, Guid userId, CancellationToken ct);
    Task DeleteBlockAsync(Guid tenantId, Guid id, Guid userId, CancellationToken ct);

    Task<IEnumerable<LocationRow>> GetVillagesAsync(Guid tenantId, Guid? blockId, CancellationToken ct);
    Task<LocationRow?> GetVillageByIdAsync(Guid tenantId, Guid id, CancellationToken ct);
    Task<bool> VillageNameExistsAsync(Guid tenantId, Guid blockId, string name, Guid? excludeId, CancellationToken ct);
    Task<bool> VillageCodeExistsAsync(Guid tenantId, string code, Guid? excludeId, CancellationToken ct);
    Task CreateVillageAsync(Guid tenantId, Guid id, Guid blockId, string name, string code, byte status, Guid userId, CancellationToken ct);
    Task UpdateVillageAsync(Guid tenantId, Guid id, Guid blockId, string name, string code, byte status, Guid userId, CancellationToken ct);
    Task DeleteVillageAsync(Guid tenantId, Guid id, Guid userId, CancellationToken ct);

    Task<(LocationImportSummaryRow Summary, IEnumerable<LocationImportErrorRow> Errors)> ImportAsync(
        string procedure, Guid tenantId, Guid userId, string rowsJson, CancellationToken ct);
}

public class LocationsRepository(DbHelper dbHelper) : ILocationsRepository
{
    private static LocationRow MapState(StateDbRow r) => new()
    {
        Id = r.Stateid, Name = r.Name, Code = r.Code, Status = r.Status,
        CreatedAt = r.Createdat, UpdatedAt = r.Updatedat
    };

    private static LocationRow MapDistrict(DistrictDbRow r) => new()
    {
        Id = r.Districtid, ParentId = r.Stateid, ParentName = r.Statename,
        StateId = r.Stateid, StateName = r.Statename,
        Name = r.Name, Code = r.Code, Status = r.Status,
        CreatedAt = r.Createdat, UpdatedAt = r.Updatedat
    };

    private static LocationRow MapBlock(BlockDbRow r) => new()
    {
        Id = r.Blockid, ParentId = r.Districtid, ParentName = r.Districtname,
        DistrictId = r.Districtid, DistrictName = r.Districtname,
        StateId = r.Stateid, StateName = r.Statename,
        Name = r.Name, Code = r.Code, Status = r.Status,
        CreatedAt = r.Createdat, UpdatedAt = r.Updatedat
    };

    private static LocationRow MapVillage(VillageDbRow r) => new()
    {
        Id = r.Villageid, ParentId = r.Blockid, ParentName = r.Blockname,
        DistrictId = r.Districtid, DistrictName = r.Districtname,
        StateId = r.Stateid, StateName = r.Statename,
        Name = r.Name, Code = r.Code, Status = r.Status,
        CreatedAt = r.Createdat, UpdatedAt = r.Updatedat
    };

    public async Task<IEnumerable<LocationRow>> GetStatesAsync(Guid tenantId, CancellationToken ct)
    {
        using var conn = dbHelper.GetConnection();
        var rows = await conn.QueryAsync<StateDbRow>("dbo.sp_state_get_list", new { tenantid = tenantId }, commandType: CommandType.StoredProcedure);
        return rows.Select(MapState);
    }

    public async Task<LocationRow?> GetStateByIdAsync(Guid tenantId, Guid id, CancellationToken ct)
    {
        using var conn = dbHelper.GetConnection();
        var r = await conn.QueryFirstOrDefaultAsync<StateDbRow>("dbo.sp_state_get_by_id", new { tenantid = tenantId, stateid = id }, commandType: CommandType.StoredProcedure);
        return r == null ? null : MapState(r);
    }

    public async Task<bool> StateNameExistsAsync(Guid tenantId, string name, Guid? excludeId, CancellationToken ct)
    {
        using var conn = dbHelper.GetConnection();
        return await conn.ExecuteScalarAsync<int>("dbo.sp_state_name_exists", new { tenantid = tenantId, name, excludeid = excludeId }, commandType: CommandType.StoredProcedure) > 0;
    }

    public async Task<bool> StateCodeExistsAsync(Guid tenantId, string code, Guid? excludeId, CancellationToken ct)
    {
        using var conn = dbHelper.GetConnection();
        return await conn.ExecuteScalarAsync<int>("dbo.sp_state_code_exists", new { tenantid = tenantId, code, excludeid = excludeId }, commandType: CommandType.StoredProcedure) > 0;
    }

    public async Task CreateStateAsync(Guid tenantId, Guid id, string name, string code, byte status, Guid userId, CancellationToken ct)
    {
        using var conn = dbHelper.GetConnection();
        await conn.ExecuteAsync("dbo.sp_state_create", new { stateid = id, tenantid = tenantId, name, code, status, createdby = userId }, commandType: CommandType.StoredProcedure);
    }

    public async Task UpdateStateAsync(Guid tenantId, Guid id, string name, string code, byte status, Guid userId, CancellationToken ct)
    {
        using var conn = dbHelper.GetConnection();
        await conn.ExecuteAsync("dbo.sp_state_update", new { tenantid = tenantId, stateid = id, name, code, status, updatedby = userId }, commandType: CommandType.StoredProcedure);
    }

    public async Task DeleteStateAsync(Guid tenantId, Guid id, Guid userId, CancellationToken ct)
    {
        using var conn = dbHelper.GetConnection();
        await conn.ExecuteAsync("dbo.sp_state_delete", new { tenantid = tenantId, stateid = id, updatedby = userId }, commandType: CommandType.StoredProcedure);
    }

    public async Task<IEnumerable<LocationRow>> GetDistrictsAsync(Guid tenantId, Guid? stateId, CancellationToken ct)
    {
        using var conn = dbHelper.GetConnection();
        var rows = await conn.QueryAsync<DistrictDbRow>("dbo.sp_district_get_list", new { tenantid = tenantId, stateid = stateId }, commandType: CommandType.StoredProcedure);
        return rows.Select(MapDistrict);
    }

    public async Task<LocationRow?> GetDistrictByIdAsync(Guid tenantId, Guid id, CancellationToken ct)
    {
        using var conn = dbHelper.GetConnection();
        var r = await conn.QueryFirstOrDefaultAsync<DistrictDbRow>("dbo.sp_district_get_by_id", new { tenantid = tenantId, districtid = id }, commandType: CommandType.StoredProcedure);
        return r == null ? null : MapDistrict(r);
    }

    public async Task<bool> DistrictNameExistsAsync(Guid tenantId, Guid stateId, string name, Guid? excludeId, CancellationToken ct)
    {
        using var conn = dbHelper.GetConnection();
        return await conn.ExecuteScalarAsync<int>("dbo.sp_district_name_exists", new { tenantid = tenantId, stateid = stateId, name, excludeid = excludeId }, commandType: CommandType.StoredProcedure) > 0;
    }

    public async Task<bool> DistrictCodeExistsAsync(Guid tenantId, string code, Guid? excludeId, CancellationToken ct)
    {
        using var conn = dbHelper.GetConnection();
        return await conn.ExecuteScalarAsync<int>("dbo.sp_district_code_exists", new { tenantid = tenantId, code, excludeid = excludeId }, commandType: CommandType.StoredProcedure) > 0;
    }

    public async Task CreateDistrictAsync(Guid tenantId, Guid id, Guid stateId, string name, string code, byte status, Guid userId, CancellationToken ct)
    {
        using var conn = dbHelper.GetConnection();
        await conn.ExecuteAsync("dbo.sp_district_create", new { districtid = id, tenantid = tenantId, stateid = stateId, name, code, status, createdby = userId }, commandType: CommandType.StoredProcedure);
    }

    public async Task UpdateDistrictAsync(Guid tenantId, Guid id, Guid stateId, string name, string code, byte status, Guid userId, CancellationToken ct)
    {
        using var conn = dbHelper.GetConnection();
        await conn.ExecuteAsync("dbo.sp_district_update", new { tenantid = tenantId, districtid = id, stateid = stateId, name, code, status, updatedby = userId }, commandType: CommandType.StoredProcedure);
    }

    public async Task DeleteDistrictAsync(Guid tenantId, Guid id, Guid userId, CancellationToken ct)
    {
        using var conn = dbHelper.GetConnection();
        await conn.ExecuteAsync("dbo.sp_district_delete", new { tenantid = tenantId, districtid = id, updatedby = userId }, commandType: CommandType.StoredProcedure);
    }

    public async Task<IEnumerable<LocationRow>> GetBlocksAsync(Guid tenantId, Guid? districtId, CancellationToken ct)
    {
        using var conn = dbHelper.GetConnection();
        var rows = await conn.QueryAsync<BlockDbRow>("dbo.sp_block_get_list", new { tenantid = tenantId, districtid = districtId }, commandType: CommandType.StoredProcedure);
        return rows.Select(MapBlock);
    }

    public async Task<LocationRow?> GetBlockByIdAsync(Guid tenantId, Guid id, CancellationToken ct)
    {
        using var conn = dbHelper.GetConnection();
        var r = await conn.QueryFirstOrDefaultAsync<BlockDbRow>("dbo.sp_block_get_by_id", new { tenantid = tenantId, blockid = id }, commandType: CommandType.StoredProcedure);
        return r == null ? null : MapBlock(r);
    }

    public async Task<bool> BlockNameExistsAsync(Guid tenantId, Guid districtId, string name, Guid? excludeId, CancellationToken ct)
    {
        using var conn = dbHelper.GetConnection();
        return await conn.ExecuteScalarAsync<int>("dbo.sp_block_name_exists", new { tenantid = tenantId, districtid = districtId, name, excludeid = excludeId }, commandType: CommandType.StoredProcedure) > 0;
    }

    public async Task<bool> BlockCodeExistsAsync(Guid tenantId, string code, Guid? excludeId, CancellationToken ct)
    {
        using var conn = dbHelper.GetConnection();
        return await conn.ExecuteScalarAsync<int>("dbo.sp_block_code_exists", new { tenantid = tenantId, code, excludeid = excludeId }, commandType: CommandType.StoredProcedure) > 0;
    }

    public async Task CreateBlockAsync(Guid tenantId, Guid id, Guid districtId, string name, string code, byte status, Guid userId, CancellationToken ct)
    {
        using var conn = dbHelper.GetConnection();
        await conn.ExecuteAsync("dbo.sp_block_create", new { blockid = id, tenantid = tenantId, districtid = districtId, name, code, status, createdby = userId }, commandType: CommandType.StoredProcedure);
    }

    public async Task UpdateBlockAsync(Guid tenantId, Guid id, Guid districtId, string name, string code, byte status, Guid userId, CancellationToken ct)
    {
        using var conn = dbHelper.GetConnection();
        await conn.ExecuteAsync("dbo.sp_block_update", new { tenantid = tenantId, blockid = id, districtid = districtId, name, code, status, updatedby = userId }, commandType: CommandType.StoredProcedure);
    }

    public async Task DeleteBlockAsync(Guid tenantId, Guid id, Guid userId, CancellationToken ct)
    {
        using var conn = dbHelper.GetConnection();
        await conn.ExecuteAsync("dbo.sp_block_delete", new { tenantid = tenantId, blockid = id, updatedby = userId }, commandType: CommandType.StoredProcedure);
    }

    public async Task<IEnumerable<LocationRow>> GetVillagesAsync(Guid tenantId, Guid? blockId, CancellationToken ct)
    {
        using var conn = dbHelper.GetConnection();
        var rows = await conn.QueryAsync<VillageDbRow>("dbo.sp_village_get_list", new { tenantid = tenantId, blockid = blockId }, commandType: CommandType.StoredProcedure);
        return rows.Select(MapVillage);
    }

    public async Task<LocationRow?> GetVillageByIdAsync(Guid tenantId, Guid id, CancellationToken ct)
    {
        using var conn = dbHelper.GetConnection();
        var r = await conn.QueryFirstOrDefaultAsync<VillageDbRow>("dbo.sp_village_get_by_id", new { tenantid = tenantId, villageid = id }, commandType: CommandType.StoredProcedure);
        return r == null ? null : MapVillage(r);
    }

    public async Task<bool> VillageNameExistsAsync(Guid tenantId, Guid blockId, string name, Guid? excludeId, CancellationToken ct)
    {
        using var conn = dbHelper.GetConnection();
        return await conn.ExecuteScalarAsync<int>("dbo.sp_village_name_exists", new { tenantid = tenantId, blockid = blockId, name, excludeid = excludeId }, commandType: CommandType.StoredProcedure) > 0;
    }

    public async Task<bool> VillageCodeExistsAsync(Guid tenantId, string code, Guid? excludeId, CancellationToken ct)
    {
        using var conn = dbHelper.GetConnection();
        return await conn.ExecuteScalarAsync<int>("dbo.sp_village_code_exists", new { tenantid = tenantId, code, excludeid = excludeId }, commandType: CommandType.StoredProcedure) > 0;
    }

    public async Task CreateVillageAsync(Guid tenantId, Guid id, Guid blockId, string name, string code, byte status, Guid userId, CancellationToken ct)
    {
        using var conn = dbHelper.GetConnection();
        await conn.ExecuteAsync("dbo.sp_village_create", new { villageid = id, tenantid = tenantId, blockid = blockId, name, code, status, createdby = userId }, commandType: CommandType.StoredProcedure);
    }

    public async Task UpdateVillageAsync(Guid tenantId, Guid id, Guid blockId, string name, string code, byte status, Guid userId, CancellationToken ct)
    {
        using var conn = dbHelper.GetConnection();
        await conn.ExecuteAsync("dbo.sp_village_update", new { tenantid = tenantId, villageid = id, blockid = blockId, name, code, status, updatedby = userId }, commandType: CommandType.StoredProcedure);
    }

    public async Task DeleteVillageAsync(Guid tenantId, Guid id, Guid userId, CancellationToken ct)
    {
        using var conn = dbHelper.GetConnection();
        await conn.ExecuteAsync("dbo.sp_village_delete", new { tenantid = tenantId, villageid = id, updatedby = userId }, commandType: CommandType.StoredProcedure);
    }

    public async Task<(LocationImportSummaryRow Summary, IEnumerable<LocationImportErrorRow> Errors)> ImportAsync(
        string procedure, Guid tenantId, Guid userId, string rowsJson, CancellationToken ct)
    {
        using var conn = dbHelper.GetConnection();
        using var multi = await conn.QueryMultipleAsync(procedure, new { tenantid = tenantId, userid = userId, rowsjson = rowsJson }, commandType: CommandType.StoredProcedure);
        var summary = await multi.ReadSingleAsync<LocationImportSummaryRow>();
        var errors = (await multi.ReadAsync<LocationImportErrorRow>()).ToList();
        return (summary, errors);
    }
}
