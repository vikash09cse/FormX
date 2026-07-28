namespace WebApi.Features.Account;

public record UserProfileResponse(
    Guid UserId,
    string Email,
    string FirstName,
    string LastName,
    string? Designation,
    string Role,
    byte RoleCode);

public record UpdateProfileRequest(
    string FirstName,
    string LastName,
    string? Designation);

public record ChangePasswordRequest(
    string CurrentPassword,
    string NewPassword);
