namespace DotnetNiger.Infrastructure.Services;

public interface IImageProcessingService
{
    Task<string> SaveAsync(Stream stream, string fileName, string type);
    bool Delete(string path);
}
