namespace DotnetNiger.Domain.Exceptions;

/// <summary>
/// Erreur métier : la requête est syntaxiquement valide mais viole une règle du domaine.
/// </summary>
/// <remarks>
/// Une <see cref="DomainException"/> n'est pas un bug : elle est attendue et doit être
/// traduite en réponse HTTP 4xx par la couche de présentation.
/// </remarks>
public class DomainException : Exception
{
    /// <summary>
    /// Initialise une nouvelle instance de <see cref="DomainException"/>.
    /// </summary>
    /// <param name="code">Code métier stable, indépendant de la langue (ex. <c>event_capacity_reached</c>).</param>
    /// <param name="message">Message destiné au developpeur, en francais.</param>
    public DomainException(string code, string message)
        : base(message)
    {
        Code = code;
    }

    /// <summary>
    /// Initialise une nouvelle instance de <see cref="DomainException"/> avec une cause.
    /// </summary>
    /// <param name="code">Code métier stable.</param>
    /// <param name="message">Message destiné au developpeur, en francais.</param>
    /// <param name="innerException">Exception d'origine.</param>
    public DomainException(string code, string message, Exception innerException)
        : base(message, innerException)
    {
        Code = code;
    }

    /// <summary>
    /// Obtient le code métier stable de l'erreur.
    /// </summary>
    public string Code { get; }
}
