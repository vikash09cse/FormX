using DbUp;
using DbUp.Engine;
using DbUp.Helpers;
using FormXDB.Utilities;
using Microsoft.Extensions.Configuration;

namespace FormXDB;

static class Program
{
    private static readonly string[] TableScriptOrder =
    [
        "tenants.sql",
        "users.sql",
        "tenant_settings.sql",
        "login_audit.sql",
        "refresh_tokens.sql",
        "global_document_templates.sql",
        "document_templates.sql",
        "roles.sql",
        "user_roles.sql",
        "role_menus.sql",
        "projects.sql",
        "user_scopes.sql",
        "states.sql",
        "districts.sql",
        "blocks.sql",
        "villages.sql",
        "user_district_scopes.sql",
        "forms.sql",
        "form_groups.sql",
        "validation_regex_presets.sql",
        "form_fields.sql",
        "form_field_options.sql",
        "form_field_parent_options.sql",
        "form_roles.sql",
        "form_submissions.sql",
        "form_submission_values.sql",
        "form_followup_configs.sql",
        "tenant_dashboard_forms.sql"
    ];

    static int Main(string[] args)
    {
        LogHelpers.LogHeader("FormX Database Migration");

        var configuration = new ConfigurationBuilder()
            .SetBasePath(AppContext.BaseDirectory)
            .AddJsonFile("appsettings.json", optional: false)
            .Build();

        var connectionString = configuration["Database:ConnectionString"];
        if (string.IsNullOrWhiteSpace(connectionString))
        {
            LogHelpers.LogError("Database connection string is not configured.");
            return 1;
        }

        EnsureDatabase.For.SqlDatabase(connectionString);

        var tablesFolder = Path.Combine(AppContext.BaseDirectory, "Schema", "Tables");
        var ok = ExecuteTableScripts(tablesFolder, connectionString)
            && ExecuteScripts(Path.Combine(AppContext.BaseDirectory, "Migrations"), connectionString)
            && ExecuteScripts(Path.Combine(AppContext.BaseDirectory, "Programmability", "Functions"), connectionString)
            && ExecuteScripts(Path.Combine(AppContext.BaseDirectory, "Programmability", "Procedures"), connectionString)
            && ExecuteScripts(Path.Combine(AppContext.BaseDirectory, "Schema", "Seed"), connectionString);

        if (ok)
        {
            LogHelpers.LogSuccess("Database migration completed successfully.");
            return 0;
        }

        LogHelpers.LogError("Database migration completed with errors.");
        return 1;
    }

    private static bool ExecuteTableScripts(string folderPath, string connectionString)
    {
        if (!Directory.Exists(folderPath))
        {
            LogHelpers.LogSkip($"Folder not found: {folderPath}");
            return true;
        }

        LogHelpers.LogInfo($"Executing scripts in {folderPath}...");

        var scripts = new List<SqlScript>();
        var order = 1;
        foreach (var fileName in TableScriptOrder)
        {
            var path = Path.Combine(folderPath, fileName);
            if (!File.Exists(path))
            {
                LogHelpers.LogSkip($"Table script not found: {fileName}");
                continue;
            }

            scripts.Add(new SqlScript($"{order:D3}_{fileName}", File.ReadAllText(path)));
            order++;
        }

        var extraScripts = Directory.GetFiles(folderPath, "*.sql")
            .Select(Path.GetFileName)
            .Where(name => name != null && !TableScriptOrder.Contains(name, StringComparer.OrdinalIgnoreCase))
            .OrderBy(name => name, StringComparer.OrdinalIgnoreCase);

        foreach (var fileName in extraScripts)
        {
            var path = Path.Combine(folderPath, fileName!);
            scripts.Add(new SqlScript($"{order:D3}_{fileName}", File.ReadAllText(path)));
            order++;
        }

        var upgrader = DeployChanges.To
            .SqlDatabase(connectionString)
            .WithScripts(scripts)
            .WithTransactionPerScript()
            .JournalTo(new NullJournal())
            .LogToConsole()
            .Build();

        var result = upgrader.PerformUpgrade();
        if (!result.Successful)
        {
            LogHelpers.LogError($"Error: {result.Error}");
            return false;
        }

        LogHelpers.LogSuccess($"Done: {folderPath}");
        return true;
    }

    private static bool ExecuteScripts(string folderPath, string connectionString)
    {
        if (!Directory.Exists(folderPath))
        {
            LogHelpers.LogSkip($"Folder not found: {folderPath}");
            return true;
        }

        LogHelpers.LogInfo($"Executing scripts in {folderPath}...");

        var upgrader = DeployChanges.To
            .SqlDatabase(connectionString)
            .WithScriptsFromFileSystem(folderPath)
            .WithTransactionPerScript()
            .JournalTo(new NullJournal())
            .LogToConsole()
            .Build();

        var result = upgrader.PerformUpgrade();
        if (!result.Successful)
        {
            LogHelpers.LogError($"Error: {result.Error}");
            return false;
        }

        LogHelpers.LogSuccess($"Done: {folderPath}");
        return true;
    }
}
