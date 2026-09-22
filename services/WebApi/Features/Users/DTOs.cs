namespace WebApi.Features.Users;

public record CreateTenantUserRequest(
    string Email,
    string FirstName,
    string LastName,
    byte Role,
    string? TemporaryPassword,
    string? Designation,
    IReadOnlyList<Guid>? RoleIds,
    IReadOnlyList<Guid>? ProjectScopeIds,
    IReadOnlyList<Guid>? DistrictScopeIds);

public record UpdateTenantUserRequest(
    string FirstName,
    string LastName,
    byte Role,
    string? Designation,
    string? TemporaryPassword,
    IReadOnlyList<Guid>? RoleIds,
    IReadOnlyList<Guid>? ProjectScopeIds,
    IReadOnlyList<Guid>? DistrictScopeIds);

public record TenantUserResponse(
    Guid Id,
    string Email,
    string FirstName,
    string LastName,
    string? Designation,
    string Role,
    byte RoleCode,
    string Status,
    byte StatusCode,
    DateTime? LastLoginAt,
    DateTime CreatedAt,
    string? Password,
    IReadOnlyList<Guid> RoleIds,
    IReadOnlyList<Guid> ProjectScopeIds,
    IReadOnlyList<Guid> DistrictScopeIds);

public record TenantUserListResponse(
    IEnumerable<TenantUserResponse> Items,
    int TotalCount,
    int Page,
    int PageSize);
