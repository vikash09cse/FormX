using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Options;
using Microsoft.IdentityModel.Tokens;
using SharedKernel.DTOs;
using SharedKernel.Settings;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Security.Cryptography;
using System.Text;

namespace SharedKernel.Utilities.Helpers;

public class JwtHelper
{
    private readonly SymmetricSecurityKey _signingKey;
    private readonly TokenValidationParameters _validationParameters;
    private readonly JwtSettings _settings;
    private readonly ILogger<JwtHelper> _logger;

    public JwtHelper(IConfiguration configuration, ILogger<JwtHelper> logger)
    {
        _settings = configuration.GetSection("Jwt").Get<JwtSettings>()
            ?? throw new InvalidOperationException("Jwt settings not configured.");
        if (string.IsNullOrWhiteSpace(_settings.SecretKey))
            throw new InvalidOperationException("Jwt SecretKey not configured.");

        _signingKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(_settings.SecretKey));
        _validationParameters = new TokenValidationParameters
        {
            ValidateIssuerSigningKey = true,
            IssuerSigningKey = _signingKey,
            ValidateIssuer = true,
            ValidIssuer = _settings.Issuer,
            ValidateAudience = true,
            ValidAudience = _settings.Audience,
            ValidateLifetime = true,
            ClockSkew = TimeSpan.Zero
        };
        _logger = logger;
    }

    public void CreateToken(TenantContext context)
    {
        var claims = new List<Claim>
        {
            new("user_id", context.UserId.ToString()),
            new(ClaimTypes.Role, context.Role),
            new("user_name", context.UserName),
            new("email", context.Email),
            new("full_name", context.FullName),
            new("user_type", context.UserType.ToString()),
            new(JwtRegisteredClaimNames.Jti, Guid.NewGuid().ToString())
        };

        if (!string.IsNullOrWhiteSpace(context.Designation))
            claims.Add(new Claim("designation", context.Designation));

        if (context.TenantId != Guid.Empty)
        {
            claims.Add(new Claim("tenant_id", context.TenantId.ToString()));
            claims.Add(new Claim("tenant_name", context.TenantName));
            claims.Add(new Claim("subdomain", context.Subdomain));
        }

        var token = new JwtSecurityToken(
            issuer: _settings.Issuer,
            audience: _settings.Audience,
            claims: claims,
            expires: DateTime.UtcNow.AddHours(_settings.ExpirationHours),
            signingCredentials: new SigningCredentials(_signingKey, SecurityAlgorithms.HmacSha256));

        context.Token = new JwtSecurityTokenHandler().WriteToken(token);
        context.TokenType = "Bearer";
        context.ExpiresIn = _settings.ExpirationHours * 3600;
        context.RefreshToken = Convert.ToBase64String(RandomNumberGenerator.GetBytes(64));
        context.RefreshTokenExpiry = DateTime.UtcNow.AddDays(_settings.RefreshTokenExpirationDays);
    }

    public TenantContext ValidateAndCreateTenantContext(string token)
    {
        var handler = new JwtSecurityTokenHandler();
        handler.ValidateToken(token, _validationParameters, out var validated);
        var jwt = (JwtSecurityToken)validated;
        return TenantContext.FromJwtClaims(jwt.Claims.ToDictionary(c => c.Type, c => c.Value));
    }

    /// <summary>
    /// Validates signature/issuer/audience but allows recently expired access tokens
    /// so refresh can succeed without forcing the user through a full login.
    /// </summary>
    public TenantContext ValidateAndCreateTenantContextAllowExpired(string token)
    {
        var parameters = new TokenValidationParameters
        {
            ValidateIssuerSigningKey = true,
            IssuerSigningKey = _signingKey,
            ValidateIssuer = true,
            ValidIssuer = _settings.Issuer,
            ValidateAudience = true,
            ValidAudience = _settings.Audience,
            ValidateLifetime = false,
            ClockSkew = TimeSpan.Zero
        };

        var handler = new JwtSecurityTokenHandler();
        handler.ValidateToken(token, parameters, out var validated);
        var jwt = (JwtSecurityToken)validated;

        var maxTokenAge = TimeSpan.FromDays(Math.Max(1, _settings.RefreshTokenExpirationDays));
        var issued = jwt.ValidFrom != DateTime.MinValue
            ? jwt.ValidFrom
            : jwt.ValidTo.AddHours(-Math.Max(1, _settings.ExpirationHours));

        if (DateTime.UtcNow - issued > maxTokenAge)
            throw new SecurityTokenExpiredException("Token is too old to refresh.");

        return TenantContext.FromJwtClaims(jwt.Claims.ToDictionary(c => c.Type, c => c.Value));
    }

    public TokenValidationParameters GetTokenValidationParameters() => _validationParameters;
}
