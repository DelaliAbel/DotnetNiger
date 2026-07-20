namespace DotnetNiger.Domain.Email;

public class SmtpOptions
{
    public string Host { get; set; } = "";
    public int Port { get; set; } = 587;
    public string Username { get; set; } = "";
    public string Password { get; set; } = "";
    public string FromEmail { get; set; } = "noreply@dotnetniger.com";
    public string FromName { get; set; } = "DotnetNiger Community";
    public string AppName { get; set; } = "DotnetNiger Community";
    public string AppSubtitle { get; set; } = "";
    public string AppBaseUrl { get; set; } = "";
    public string FrontendBaseUrl { get; set; } = "";
    public string SupportEmail { get; set; } = "";
}
