namespace DotnetNiger.Api.Services.General;

public interface IImageProcessingService
{
    Task<string> SaveAsync(Stream stream, string fileName, string type);
    bool Delete(string path);
}
