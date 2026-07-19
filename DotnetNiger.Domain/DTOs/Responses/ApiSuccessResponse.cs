namespace DotnetNiger.Domain.DTOs.Responses;

public class ApiSuccessResponse<T>
{
    public bool Success { get; set; } = true;
    public string? Message { get; set; }
    public T? Data { get; set; }
}
