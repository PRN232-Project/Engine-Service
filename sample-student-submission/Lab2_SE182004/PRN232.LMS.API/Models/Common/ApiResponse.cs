namespace PRN232.LMS.API.Models.Common;

public class ApiResponse<T>
{
    public bool Success { get; set; }
    public string Message { get; set; } = null!;
    public T? Data { get; set; }
    public object? Errors { get; set; }

    public static ApiResponse<T> Ok(T data, string message = "Request processed successfully")
        => new() { Success = true, Message = message, Data = data, Errors = null };

    public static ApiResponse<T> Fail(string message, object? errors = null)
        => new() { Success = false, Message = message, Data = default, Errors = errors };

    public static ApiResponse<T> ValidationFail(object errors, string message = "Validation failed")
        => new() { Success = false, Message = message, Data = default, Errors = errors };

    public static ApiResponse<T> InternalError(string message = "Internal server error")
        => new() { Success = false, Message = message, Data = default, Errors = null };
}
