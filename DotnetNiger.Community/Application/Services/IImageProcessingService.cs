namespace DotnetNiger.Community.Application.Services;

/// <summary>Interface pour le traitement et la validation des images uploadées.</summary>
public interface IImageProcessingService
{
    /// <summary>Valide et sauvegarde un fichier image sur le disque.</summary>
    /// <param name="stream">Contenu du fichier.</param>
    /// <param name="fileName">Nom original du fichier.</param>
    /// <param name="type">Type d'utilisation (Blog, Event, User).</param>
    /// <param name="userId">Identifiant de l'utilisateur (pour les avatars).</param>
    /// <returns>Chemin relatif de l'image sauvegardée.</returns>
    Task<string> SaveAsync(Stream stream, string fileName, string type, string? userId = null);

    /// <summary>Valide le format d'une image à partir de son flux binaire.</summary>
    /// <param name="stream">Flux de l'image.</param>
    /// <returns>Message d'erreur si invalide, null si valide.</returns>
    string? ValidateImage(Stream stream);

    /// <summary>Supprime un fichier image par son chemin relatif.</summary>
    /// <param name="relativePath">Chemin relatif du fichier.</param>
    /// <returns>True si supprimé, false si introuvable.</returns>
    bool Delete(string relativePath);
}
