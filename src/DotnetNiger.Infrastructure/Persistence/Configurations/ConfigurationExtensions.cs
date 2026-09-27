using DotnetNiger.Domain.Common;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace DotnetNiger.Infrastructure.Persistence.Configurations;

/// <summary>
/// Aides communes aux configurations d'entités.
/// </summary>
internal static class ConfigurationExtensions
{
    /// <summary>
    /// Stocke une énumération sous forme de texte.
    /// </summary>
    /// <typeparam name="TEnum">Type de l'énumération.</typeparam>
    /// <param name="builder">Constructeur de la propriété.</param>
    /// <param name="maxLength">Longueur maximale de la valeur texte.</param>
    /// <returns>Le même constructeur, pour permettre le chaînage.</returns>
    /// <remarks>
    /// Le stockage textuel garde le schéma lisible et rend les requêtes SQL directes
    /// dans un client comme SQL Server Management Studio. Le fichier de migrations doit
    /// être regénéré si ce choix change.
    /// </remarks>
    public static PropertyBuilder<TEnum> AsEnumText<TEnum>(
        this PropertyBuilder<TEnum> builder,
        int maxLength = 32)
        where TEnum : struct, Enum
        => builder.HasConversion<string>().HasMaxLength(maxLength);

    /// <summary>
    /// Masque par défaut les entités retirées, tout en laissant un filtre nommé
    /// permettant de toutes les lire.
    /// </summary>
    /// <typeparam name="T">Type de l'entité.</typeparam>
    /// <param name="builder">Constructeur de l'entité.</param>
    /// <returns>Le même constructeur, pour permettre le chaînage.</returns>
    /// <remarks>
    /// Le filtre est appliqué par le moteur et non par chaque requête : impossible
    /// d'oublier la condition. Un second filtre nommé <c>AllContent</c> est défini pour
    /// permettre au tableau de bord d'administration de lire les entités retirées.
    /// </remarks>
    public static EntityTypeBuilder<T> WithSoftDeleteFilter<T>(this EntityTypeBuilder<T> builder)
        where T : class, ISoftDeletable
    {
        builder.HasQueryFilter(entity => !entity.IsDeleted);
        return builder;
    }
}
