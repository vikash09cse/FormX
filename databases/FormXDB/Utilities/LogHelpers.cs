namespace FormXDB.Utilities;

public static class LogHelpers
{
    public static void LogHeader(string message) => Console.WriteLine($"\n========== {message} ==========");
    public static void LogInfo(string message) => Console.WriteLine($"[INFO] {message}");
    public static void LogSuccess(string message) => Console.WriteLine($"[SUCCESS] {message}");
    public static void LogWarn(string message) => Console.WriteLine($"[WARN] {message}");
    public static void LogError(string message) => Console.WriteLine($"[ERROR] {message}");
    public static void LogSkip(string message) => Console.WriteLine($"[SKIP] {message}");
}
