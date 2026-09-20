namespace WebApi.Features.Locations;

public record LocationItemResponse(
    Guid Id,
    string Name,
    string Code,
    string Status,
    byte StatusCode,
    DateTime CreatedAt,
    DateTime UpdatedAt,
    Guid? ParentId = null,
    string? ParentName = null,
    Guid? StateId = null,
    string? StateName = null,
    Guid? DistrictId = null,
    string? DistrictName = null);

public record SaveStateRequest(string Name, string Code, byte Status);

public record SaveDistrictRequest(Guid StateId, string Name, string Code, byte Status);

public record SaveBlockRequest(Guid DistrictId, string Name, string Code, byte Status);

public record SaveVillageRequest(Guid BlockId, string Name, string Code, byte Status);

public record LocationImportResult(
    int Imported,
    int ErrorCount,
    IReadOnlyList<LocationImportError> Errors);

public record LocationImportError(int RowNumber, string Message);
