namespace DotnetNiger.Api.DTOs.Requests;

public record SubscribeRequest(string Email, string Name);

public record UnsubscribeRequest(string Email, string Token);
