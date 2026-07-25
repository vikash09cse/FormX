using SharedKernel.Enums;

namespace WebApi.Features.Users.Infrastructure;

public interface IUsersRepository
{
    Task<(IEnumerable<TenantUserRow> Items, int Total)> GetUsersAsync(Guid tenantId, int page, int pageSize, string? filter, CancellationToken ct);
    Task<TenantUserRow?> GetByIdAsync(Guid tenantId, Guid userId, CancellationToken ct);
    Task<bool> EmailExistsAsync(Guid tenantId, string email, Guid? excludeId, CancellationToken ct);
    Task<Guid> CreateAsync(Guid tenantId, string email, string firstName, string lastName, string? designation, byte role, string passwordHash, Guid createdBy, CancellationToken ct);
    Task UpdateAsync(Guid tenantId, Guid userId, string firstName, string lastName, string? designation, byte role, Guid updatedBy, CancellationToken ct);
    Task SetStatusAsync(Guid tenantId, Guid userId, UserStatus status, Guid updatedBy, CancellationToken ct);
    Task DeleteAsync(Guid tenantId, Guid userId, Guid updatedBy, CancellationToken ct);
    Task<IReadOnlyList<Guid>> GetRoleIdsAsync(Guid tenantId, Guid userId, CancellationToken ct);
    Task SetRolesAsync(Guid tenantId, Guid userId, IEnumerable<Guid> roleIds, Guid createdBy, CancellationToken ct);
    Task<IReadOnlyList<Guid>> GetScopeProjectIdsAsync(Guid tenantId, Guid userId, CancellationToken ct);
    Task SetScopesAsync(Guid tenantId, Guid userId, IEnumerable<Guid> projectIds, Guid createdBy, CancellationToken ct);
}
