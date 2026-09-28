namespace DotnetNiger.Infrastructure.Notifications;

/// <summary>
/// Réglages du relais d'expédition des courriels.
/// </summary>
public sealed class OutboxOptions
{
    /// <summary>Nom de la section de configuration.</summary>
    public const string SectionName = "Outbox";

    /// <summary>
    /// Nombre de messages expédiés par tour de boucle. Une valeur basse garde la mémoire
    /// et la durée de la boucle sous contrôle, au prix d'un balayage plus fréquent.
    /// </summary>
    public int BatchSize { get; set; } = 50;

    /// <summary>
    /// Nombre de tentatives avant abandon définitif d'un message.
    /// </summary>
    /// <remarks>
    /// Le délai entre deux tentatives croît exponentiellement, de 30 secondes à une heure.
    /// Sept tentatives couvrent ainsi un peu plus d'une heure de panne.
    /// </remarks>
    public int MaxAttempts { get; set; } = 7;

    /// <summary>
    /// Temps d'attente entre deux tours de boucle lorsque la file était vide.
    /// </summary>
    public TimeSpan IdleInterval { get; set; } = TimeSpan.FromSeconds(15);

    /// <summary>
    /// Indique si le relais doit fonctionner. Désactivé, aucun message ne part.
    /// </summary>
    /// <remarks>
    /// Utile pendant les tests d'intégration, qui vérifient le contenu de la file sans
    /// laisser une tâche de fond/consommer les messages qu'ils viennent d'écrire.
    /// </remarks>
    public bool Enabled { get; set; } = true;
}
