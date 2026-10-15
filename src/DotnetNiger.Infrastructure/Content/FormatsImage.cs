namespace DotnetNiger.Infrastructure.Content;

/// <summary>
/// Reconnaissance du type réel d'une image à partir de ses octets.
/// </summary>
/// <remarks>
/// Les marqueurs de début et de fin sont vérifiés : le type annoncé par le client ne prouve
/// rien — n'importe quel fichier peut être envoyé en se déclarant <c>image/jpeg</c> — et un
/// en-tête valide se recopie en trois octets sans garantir que le fichier est une image
/// complète. C'est le contenu qui décide de ce que le navigateur affichera, donc c'est lui
/// qu'on annonce dans l'en-tête de réponse.
/// </remarks>
internal static class FormatsImage
{
    /// <summary>
    /// Reconnait le type MIME d'une image à partir de ses marqueurs.
    /// </summary>
    /// <param name="contenu">Octets du fichier.</param>
    /// <returns>Le type MIME reconnu, ou <see langword="null"/> si l'image est inconnue.</returns>
    public static string? LireTypeReel(byte[] contenu)
    {
        if (contenu.Length >= 3
            && contenu[0] == 0xFF
            && contenu[1] == 0xD8
            && contenu[2] == 0xFF)
        {
            return EstJpegTermine(contenu) ? "image/jpeg" : null;
        }

        if (contenu.Length >= 8
            && contenu[0] is 0x89 && contenu[1] == 0x50 && contenu[2] == 0x4E && contenu[3] == 0x47
            && contenu[4] is 0x0D && contenu[5] == 0x0A && contenu[6] is 0x1A && contenu[7] == 0x0A)
        {
            return "image/png";
        }

        if (contenu.Length >= 12
            && contenu[0] == 0x52 && contenu[1] == 0x49 && contenu[2] == 0x46 && contenu[3] == 0x46
            && contenu[8] == 0x57 && contenu[9] == 0x45 && contenu[10] == 0x42 && contenu[11] == 0x50)
        {
            // RIFF : la taille annoncée aux octets 4 à 7 doit correspondre à la taille
            // réelle du fichier, sans quoi le conteneur est tronqué ou trafiqué.
            var tailleAnnoncee = BitConverter.ToUInt32(contenu, 4);

            return tailleAnnoncee + 8 == (uint)contenu.Length ? "image/webp" : null;
        }

        return null;
    }

    /// <summary>
    /// Vérifie qu'une image JPEG est bien terminée.
    /// </summary>
    /// <remarks>
    /// L'en-tête seul ne prouve rien : n'importe quel fichier peut commencer par les trois
    /// octets <c>FF D8 FF</c>. Un fichier de quelques centaines d'octets composé de bruit
    /// après un en-tête valide passait le contrôle et était ensuite servi comme une image
    /// cassée. Le marqueur de fin <c>FF D9</c> est ce qui distingue un JPEG complet d'un
    /// fichier tronqué ou fabriqué.
    /// </remarks>
    /// <param name="contenu">Octets du fichier.</param>
    /// <returns><see langword="true"/> si le fichier se termine correctement.</returns>
    private static bool EstJpegTermine(byte[] contenu)
    {
        for (var i = contenu.Length - 2; i >= 0; i--)
        {
            // Les octets 0xFF se répètent en fin de flux JPEG : on saute les remplissages.
            if (contenu[i] == 0xFF && contenu[i + 1] == 0xD9)
            {
                return true;
            }

            if (contenu[i] != 0xFF)
            {
                return false;
            }
        }

        return false;
    }
}