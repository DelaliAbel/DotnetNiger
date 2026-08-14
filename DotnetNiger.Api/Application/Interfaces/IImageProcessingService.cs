using System.Threading;

namespace DotnetNiger.Api.Application.Interfaces;

/// <summary>Interface du service de traitement d'images.</summary>
public interface IImageProcessingService
{
    /// <summary>Sauvegarde un fichier image.</summary>
    Task<string> SaveAsync(Stream stream, string fileName, string type, CancellationToken ct = default);
    /// <summary>Supprime un fichier image.</summary>
    bool Delete(string path);
}
