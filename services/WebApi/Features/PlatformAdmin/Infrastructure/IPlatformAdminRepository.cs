using SharedKernel.Enums;
using WebApi.Features.PlatformAdmin;

namespace WebApi.Features.PlatformAdmin.Infrastructure;

public interface IPlatformAdminRepository
{
    Task<IEnumerable<TenantDashboardRow>> GetTenantsAsync(CancellationToken ct);
    Task<PlatformDashboardRow> GetPlatformDashboardAsync(CancellationToken ct);
    Task<TenantDetailResponse?> GetTenantByIdAsync(Guid id, CancellationToken ct);
    Task<bool> SubdomainExistsAsync(string subdomain, CancellationToken ct);
    Task<bool> UserEmailExistsAsync(string email, Guid? excludeUserId, CancellationToken ct);
    Task<(IEnumerable<PortalUserRow> Items, int Total)> GetPortalUsersAsync(int page, int pageSize, CancellationToken ct);
    Task<Guid> CreateTenantAsync(CreateTenantRequest req, string passwordHash, CancellationToken ct);
    Task UpdateTenantAsync(Guid id, UpdateTenantRequest req, CancellationToken ct);
    Task SetTenantStatusAsync(Guid id, TenantStatus status, CancellationToken ct);
    Task<(IEnumerable<BackOfficeUserRow> Items, int Total)> GetBackOfficeUsersAsync(int page, int pageSize, CancellationToken ct);
    Task<Guid> CreateBackOfficeUserAsync(CreateBackOfficeUserRequest req, string passwordHash, CancellationToken ct);
    Task UpdateBackOfficeUserAsync(Guid id, UpdateBackOfficeUserRequest req, CancellationToken ct);
    Task SetBackOfficeUserStatusAsync(Guid id, UserStatus status, CancellationToken ct);
    Task<bool> DeleteBackOfficeUserAsync(Guid id, CancellationToken ct);
    Task<bool> BackOfficeEmailExistsAsync(string email, Guid? excludeId, CancellationToken ct);
}
