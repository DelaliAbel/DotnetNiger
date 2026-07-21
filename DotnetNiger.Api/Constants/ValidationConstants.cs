namespace DotnetNiger.Api.Constants;

public static class ValidationConstants
{
    public const int MaxPageSize = 100;
    public const int MinPageSize = 1;
    public const int DefaultPageSize = 10;
    public const int MaxNameLength = 100;
    public const int MaxEmailLength = 256;
    public const int MaxSlugLength = 200;
    public const int MaxTitleLength = 200;
    public const int MaxContentLength = 10000;
    public const int MaxUploadSize = 4 * 1024 * 1024;
}
