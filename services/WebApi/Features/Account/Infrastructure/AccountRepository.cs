using Dapper;
using SharedKernel.Utilities.Helpers;
using System.Data;

namespace WebApi.Features.Account.Infrastructure;

public class AccountRepository(DbHelper dbHelper) : IAccountRepository
{
    public async Task<UserProfileRow?> GetProfileAsync(Guid tenantId, Guid userId, CancellationToken ct)
    {
        using var conn = dbHelper.GetConnection();
        return await conn.QueryFirstOrDefaultAsync<UserProfileRow>(
            "dbo.sp_get_current_user_profile",
            new { tenantid = tenantId, userid = userId },
            commandType: CommandType.StoredProcedure);
    }

    public async Task UpdateProfileAsync(
        Guid tenantId, Guid userId, string firstName, string lastName, string? designation, Guid updatedBy, CancellationToken ct)
    {
        using var conn = dbHelper.GetConnection();
        await conn.ExecuteAsync(
            "dbo.sp_update_current_user_profile",
            new
            {
                tenantid = tenantId,
                userid = userId,
                firstname = firstName,
                lastname = lastName,
                designation,
                updatedby = updatedBy
            },
            commandType: CommandType.StoredProcedure);
    }

    public async Task<string?> GetPasswordHashAsync(Guid tenantId, Guid userId, CancellationToken ct)
    {
        using var conn = dbHelper.GetConnection();
        return await conn.QueryFirstOrDefaultAsync<string>(
            "dbo.sp_get_user_password_hash",
            new { tenantid = tenantId, userid = userId },
            commandType: CommandType.StoredProcedure);
    }

    public async Task UpdatePasswordAsync(Guid tenantId, Guid userId, string passwordHash, Guid updatedBy, CancellationToken ct)
    {
        using var conn = dbHelper.GetConnection();
        await conn.ExecuteAsync(
            "dbo.sp_update_user_password",
            new
            {
                tenantid = tenantId,
                userid = userId,
                passwordhash = passwordHash,
                updatedby = updatedBy
            },
            commandType: CommandType.StoredProcedure);
    }
}
