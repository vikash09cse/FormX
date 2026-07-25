using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.IdentityModel.Tokens;
using Microsoft.OpenApi.Models;
using SharedKernel.Services;
using SharedKernel.Settings;
using SharedKernel.Utilities;
using SharedKernel.Utilities.Helpers;
using System.Reflection;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace SharedKernel;

public class SharedKernelStartup;

public static class DependencyInjection
{
    private static readonly JsonSerializerOptions JsonOptions = new()
    {
        PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
        DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingNull
    };
    public static void InjectGlobalConfigurations(this WebApplicationBuilder builder, Assembly moduleAssembly)
    {
        builder.Services.AddControllers().ConfigureApiBehaviorOptions(o =>
        {
            o.SuppressModelStateInvalidFilter = true;
        });

        builder.Services.Configure<DatabaseSettings>(builder.Configuration.GetSection("Database"));
        builder.Services.Configure<JwtSettings>(builder.Configuration.GetSection("Jwt"));
        builder.Services.Configure<AppSettings>(builder.Configuration.GetSection("AppSettings"));
        builder.Services.Configure<SendGridSettings>(builder.Configuration.GetSection("SendGridSettings"));
        builder.Services.Configure<PhiEncryptionSettings>(builder.Configuration.GetSection("PhiEncryption"));

        builder.Services.AddSingleton<JwtHelper>();
        builder.Services.AddSingleton<PhiEncryptionHelper>();
        builder.Services.AddSingleton<DbHelper>();
        builder.Services.AddScoped<EmailService>();
        builder.Services.AddHttpContextAccessor();

        InjectBySuffix(builder.Services, moduleAssembly, "Repository");
        InjectBySuffix(builder.Services, moduleAssembly, "Service");

        builder.Services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
            .AddJwtBearer(options =>
            {
                options.RequireHttpsMetadata = false;
                options.SaveToken = true;
                options.TokenValidationParameters = builder.Services.BuildServiceProvider()
                    .GetRequiredService<JwtHelper>().GetTokenValidationParameters();
                options.Events = new JwtBearerEvents
                {
                    OnAuthenticationFailed = async ctx =>
                    {
                        ctx.NoResult();
                        var message = ctx.Exception is SecurityTokenExpiredException
                            ? "Token expired."
                            : "Invalid token.";
                        await WriteUnauthorizedResultAsync(ctx.Response, message);
                    },
                    OnChallenge = async ctx =>
                    {
                        ctx.HandleResponse();
                        if (ctx.Response.HasStarted) return;
                        var message = string.IsNullOrEmpty(ctx.ErrorDescription)
                            ? "Unauthorized access."
                            : ctx.ErrorDescription;
                        await WriteUnauthorizedResultAsync(ctx.Response, message);
                    }
                };
            });

        builder.Services.AddAuthorization();
        builder.Services.AddEndpointsApiExplorer();
        builder.Services.AddSwaggerGen(c =>
        {
            c.AddSecurityDefinition("Bearer", new OpenApiSecurityScheme
            {
                Scheme = "Bearer",
                BearerFormat = "JWT",
                In = ParameterLocation.Header,
                Name = "Authorization",
                Type = SecuritySchemeType.Http
            });
            c.AddSecurityRequirement(new OpenApiSecurityRequirement
            {
                {
                    new OpenApiSecurityScheme
                    {
                        Reference = new OpenApiReference { Id = "Bearer", Type = ReferenceType.SecurityScheme }
                    },
                    Array.Empty<string>()
                }
            });
        });

        Dapper.DefaultTypeMap.MatchNamesWithUnderscores = false;
    }

    public static void UseGlobalConfigurations(this WebApplication app)
    {
        if (app.Environment.IsDevelopment())
        {
            app.UseSwagger();
            app.UseSwaggerUI();
        }

        app.UseHttpsRedirection();
        app.UseCors("AllowAngularApps");
        app.UseStaticFiles();
        app.UseMiddleware<GlobalExceptionMiddleware>();
        app.UseAuthentication();
        app.UseAuthorization();
        app.UseMiddleware<TenantContextMiddleware>();
        app.MapControllers();
    }

    private static void InjectBySuffix(IServiceCollection services, Assembly assembly, string suffix)
    {
        foreach (var type in assembly.GetTypes().Where(t => t.Name.EndsWith(suffix) && !t.IsAbstract && t.IsClass))
        {
            var iface = type.GetInterface($"I{type.Name}");
            if (iface != null)
                services.AddScoped(iface, type);
            else
                services.AddScoped(type);
        }
    }

    private static async Task WriteUnauthorizedResultAsync(HttpResponse response, string message)
    {
        response.StatusCode = StatusCodes.Status401Unauthorized;
        response.ContentType = "application/json";
        var result = Result<object>.Fail(ErrorCode.Unauthorized, message);
        await response.WriteAsync(JsonSerializer.Serialize(result, JsonOptions));
    }
}
