using Microsoft.IdentityModel.Tokens;
using SharedKernel.DTOs;
using SharedKernel.Enums;
using SharedKernel.Utilities;
using SharedKernel.Utilities.Extensions;
using SharedKernel.Utilities.Helpers;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Cryptography;
using System.Text;
using WebApi.Features.Auth.Infrastructure;

namespace WebApi.Features.Auth;

public class AuthService(
    IAuthRepository repository,
    JwtHelper jwtHelper,
    IHttpContextAccessor httpContextAccessor)
{
    private const string InvalidCredentials = "Invalid email or password.";
    private const string LoginSuccessMessage = "Login successful.";

    public async Task<Result<TenantBySubdomainResponse>> GetTenantBySubdomainAsync(string subdomain, CancellationToken ct)
    {
        var tenant = await repository.GetTenantBySubdomainAsync(subdomain.ToLowerInvariant(), ct);
        if (tenant == null)
            return Result<TenantBySubdomainResponse>.Fail(ErrorCode.NotFound, "Hospital not found.");

        return Result<TenantBySubdomainResponse>.Ok(new TenantBySubdomainResponse(
            tenant.TenantId, tenant.HospitalName, tenant.Subdomain, tenant.Status,
            tenant.LogoUrl, tenant.Timezone));
    }

    public async Task<Result<LoginResponse>> PlatformLoginAsync(PlatformLoginRequest request, CancellationToken ct)
    {
        var ip = httpContextAccessor.HttpContext?.GetClientIp();
        var user = await repository.GetUserForLoginAsync(request.Email.Trim(), null, ct);

        if (user == null ||
            (user.UserType != (byte)UserType.PlatformAdmin && user.UserType != (byte)UserType.BackOfficeUser) ||
            user.Status != (byte)UserStatus.Active ||
            !PasswordHelper.Verify(request.Password, user.PasswordHash))
        {
            await repository.LogLoginAttemptAsync(null, request.Email, LoginType.Platform, false, InvalidCredentials, ip, ct);
            return Result<LoginResponse>.Fail(ErrorCode.Unauthorized, InvalidCredentials);
        }

        var userType = (UserType)user.UserType;
        var context = BuildContext(user.UserId, user.Email, $"{user.FirstName} {user.LastName}".Trim(),
            userType, user.Designation, Guid.Empty, string.Empty, string.Empty);

        jwtHelper.CreateToken(context);
        await repository.SaveRefreshTokenAsync(context.UserId, null, HashToken(context.RefreshToken), context.RefreshTokenExpiry, ct);
        await repository.LogLoginAttemptAsync(null, request.Email, LoginType.Platform, true, null, ip, ct);

        return Result<LoginResponse>.Ok(ToLoginResponse(context), LoginSuccessMessage);
    }

    public async Task<Result<LoginResponse>> TenantLoginAsync(TenantLoginRequest request, CancellationToken ct)
    {
        var ip = httpContextAccessor.HttpContext?.GetClientIp();
        var email = request.Email.Trim();
        var matches = await repository.GetTenantUsersForLoginByEmailAsync(email, ct);

        if (matches.Count == 0)
        {
            await repository.LogLoginAttemptAsync(null, email, LoginType.Tenant, false, InvalidCredentials, ip, ct);
            return Result<LoginResponse>.Fail(ErrorCode.Unauthorized, InvalidCredentials);
        }

        if (matches.Count > 1)
        {
            await repository.LogLoginAttemptAsync(null, email, LoginType.Tenant, false, "Ambiguous tenant user email", ip, ct);
            return Result<LoginResponse>.Fail(ErrorCode.Unauthorized, InvalidCredentials);
        }

        var user = matches[0];

        if (user.TenantStatus == (byte)TenantStatus.Suspended)
        {
            await repository.LogLoginAttemptAsync(user.TenantId, email, LoginType.Tenant, false, "Tenant suspended", ip, ct);
            return Result<LoginResponse>.Fail(
                ErrorCode.Unauthorized,
                "Your organization's account is currently inactive. Please contact support.");
        }

        if (user.Status != (byte)UserStatus.Active ||
            !PasswordHelper.Verify(request.Password, user.PasswordHash))
        {
            await repository.LogLoginAttemptAsync(user.TenantId, email, LoginType.Tenant, false, InvalidCredentials, ip, ct);
            return Result<LoginResponse>.Fail(ErrorCode.Unauthorized, InvalidCredentials);
        }

        var userType = (UserType)user.UserType;
        var context = BuildContext(
            user.UserId,
            user.Email,
            $"{user.FirstName} {user.LastName}".Trim(),
            userType,
            user.Designation,
            user.TenantId!.Value,
            user.HospitalName ?? string.Empty,
            user.Subdomain ?? string.Empty);

        jwtHelper.CreateToken(context);
        await repository.SaveRefreshTokenAsync(context.UserId, user.TenantId, HashToken(context.RefreshToken), context.RefreshTokenExpiry, ct);
        await repository.UpdateUserLastLoginAsync(user.UserId, ct);
        await repository.LogLoginAttemptAsync(user.TenantId, email, LoginType.Tenant, true, null, ip, ct);

        return Result<LoginResponse>.Ok(ToLoginResponse(context), LoginSuccessMessage);
    }

    public async Task<Result<LoginResponse>> RefreshTokenAsync(string bearerToken, CancellationToken ct)
    {
        try
        {
            // Allow expired access tokens within the configured refresh window.
            var context = jwtHelper.ValidateAndCreateTenantContextAllowExpired(bearerToken);
            jwtHelper.CreateToken(context);
            await repository.SaveRefreshTokenAsync(
                context.UserId,
                context.TenantId == Guid.Empty ? null : context.TenantId,
                HashToken(context.RefreshToken),
                context.RefreshTokenExpiry,
                ct);
            return Result<LoginResponse>.Ok(ToLoginResponse(context), "Token refreshed successfully.");
        }
        catch (Exception ex) when (ex is SecurityTokenException or ArgumentException)
        {
            return Result<LoginResponse>.Fail(ErrorCode.Unauthorized, "Invalid or expired token.");
        }
    }

    public async Task<Result<bool>> LogoutAsync(CancellationToken ct)
    {
        var context = httpContextAccessor.GetTenantContext();
        var ip = httpContextAccessor.HttpContext?.GetClientIp();
        await repository.LogLoginAttemptAsync(
            context.TenantId == Guid.Empty ? null : context.TenantId,
            context.Email,
            context.IsPlatformAdmin ? LoginType.Platform : LoginType.Tenant,
            true,
            "Logout",
            ip,
            ct);
        return Result<bool>.Ok(true, "Logged out successfully.");
    }

    private static TenantContext BuildContext(
        Guid userId, string email, string fullName, UserType userType, string? designation,
        Guid tenantId, string tenantName, string subdomain) => new()
    {
        UserId = userId,
        Email = email,
        UserName = email,
        FullName = fullName,
        Designation = string.IsNullOrWhiteSpace(designation) ? null : designation.Trim(),
        UserType = (byte)userType,
        Role = RoleNames.FromUserType(userType),
        TenantId = tenantId,
        TenantName = tenantName,
        Subdomain = subdomain
    };

    private static LoginResponse ToLoginResponse(TenantContext c) => new(
        c.UserId, c.Email, c.FullName, c.Role, c.UserType, c.Designation,
        c.TenantId == Guid.Empty ? null : c.TenantId,
        string.IsNullOrEmpty(c.TenantName) ? null : c.TenantName,
        string.IsNullOrEmpty(c.Subdomain) ? null : c.Subdomain,
        c.Token, c.TokenType, c.ExpiresIn, c.RefreshToken, c.RefreshTokenExpiry);

    private static string HashToken(string token)
    {
        var bytes = SHA256.HashData(Encoding.UTF8.GetBytes(token));
        return Convert.ToBase64String(bytes);
    }
}
