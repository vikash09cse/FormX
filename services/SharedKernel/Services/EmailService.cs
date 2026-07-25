using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using SharedKernel.Settings;

namespace SharedKernel.Services;

public class EmailService(
    IOptions<AppSettings> appSettings,
    ILogger<EmailService> logger)
{
    public Task SendWelcomeEmailAsync(string toEmail, string hospitalName, string subdomain, string tempPassword)
    {
        var loginUrl = $"{appSettings.Value.TenantPortalUrl.TrimEnd('/')}/{subdomain}";
        logger.LogInformation(
            "Welcome email queued for {Email}. Hospital: {Hospital}. Login URL: {Url}. Temp password: {Password}",
            toEmail, hospitalName, loginUrl, tempPassword);
        return Task.CompletedTask;
    }

    public Task SendPlatformUserEmailAsync(string toEmail, string tempPassword)
    {
        logger.LogInformation(
            "Platform user email queued for {Email}. Login: {Url}. Temp password: {Password}",
            toEmail, appSettings.Value.SuperAdminUrl, tempPassword);
        return Task.CompletedTask;
    }

    public Task SendFormFieldNotificationAsync(string toEmail, string formName, string fieldLabel)
    {
        logger.LogInformation(
            "Form field notification queued for {Email}. Form: {Form}. Field: {Field}",
            toEmail, formName, fieldLabel);
        return Task.CompletedTask;
    }
}
