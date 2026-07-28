namespace WebApi.Features.Account.Infrastructure;

public interface IAccountRepository
{
    Task<UserProfileRow?> GetProfileAsync(Guid tenantId, Guid userId, CancellationToken ct);
    Task UpdateProfileAsync(Guid tenantId, Guid userId, string firstName, string lastName, string? designation, Guid updatedBy, CancellationToken ct);
    Task<string?> GetPasswordHashAsync(Guid tenantId, Guid userId, CancellationToken ct);
    Task UpdatePasswordAsync(Guid tenantId, Guid userId, string passwordHash, Guid updatedBy, CancellationToken ct);
}

public class UserProfileRow
{
    public Guid UserId { get; set; }
    public string Email { get; set; } = string.Empty;
    public string FirstName { get; set; } = string.Empty;
    public string LastName { get; set; } = string.Empty;
    public string? Designation { get; set; }
    public byte Role { get; set; }
}
