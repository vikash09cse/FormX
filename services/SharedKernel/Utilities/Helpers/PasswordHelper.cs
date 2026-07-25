namespace SharedKernel.Utilities.Helpers;

public static class PasswordHelper
{
    public static string Hash(string password) => BCrypt.Net.BCrypt.HashPassword(password);

    public static bool Verify(string password, string hash) => BCrypt.Net.BCrypt.Verify(password, hash);

    public static string GenerateTemporaryPassword()
    {
        const string chars = "ABCDEFGHJKLMNPQRSTUVWXYZabcdefghijkmnpqrstuvwxyz23456789!@#$";
        return new string(Enumerable.Range(0, 12).Select(_ => chars[Random.Shared.Next(chars.Length)]).ToArray()) + "1aA!";
    }
}

public static class SubdomainValidator
{
    private static readonly HashSet<string> Reserved = new(StringComparer.OrdinalIgnoreCase)
    {
        "admin", "api", "www", "app", "platform"
    };

    public static bool IsValid(string subdomain, out string? error)
    {
        error = null;
        if (string.IsNullOrWhiteSpace(subdomain))
        {
            error = "Subdomain is required.";
            return false;
        }

        if (subdomain.Length > 50)
        {
            error = "Subdomain must be 50 characters or less.";
            return false;
        }

        if (!System.Text.RegularExpressions.Regex.IsMatch(subdomain, "^[a-z0-9-]+$"))
        {
            error = "Subdomain may only contain lowercase letters, numbers, and hyphens.";
            return false;
        }

        if (Reserved.Contains(subdomain))
        {
            error = "Subdomain is reserved.";
            return false;
        }

        return true;
    }
}
