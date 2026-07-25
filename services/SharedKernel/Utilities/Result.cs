namespace SharedKernel.Utilities;

public enum ErrorCode
{
    None = 0,
    Validation = 100,
    BadRequest = 101,
    Unauthorized = 401,
    Forbidden = 403,
    NotFound = 300,
    AlreadyExists = 400,
    InternalError = 500
}

public class Result<T>
{
    public bool Success { get; set; }
    public string Message { get; set; } = string.Empty;
    public T? Data { get; set; }
    public ErrorCode ErrorCode { get; set; }
    public List<AppError> Errors { get; set; } = [];

    public static Result<T> Ok(T data, string message = "Success") =>
        new() { Success = true, Message = message, Data = data, ErrorCode = ErrorCode.None };

    public static Result<T> Fail(ErrorCode code, string message) =>
        new()
        {
            Success = false,
            Message = message,
            ErrorCode = code,
            Errors = [new AppError(code, message)]
        };
}

public record AppError(ErrorCode Code, string Error);
