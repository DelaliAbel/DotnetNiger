using DotnetNiger.Domain.Entities.Community;
using DotnetNiger.Domain.Entities.Comments;
using DotnetNiger.Domain.Entities.Content;
using DotnetNiger.Domain.Entities.Moderation;
using DotnetNiger.Domain.Entities.Identity;
using DotnetNiger.Domain.Entities.Notifications;
using DotnetNiger.Domain.Entities.Partners;
using DotnetNiger.Domain.Entities.Projects;
using DotnetNiger.Infrastructure.Identity;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;

namespace DotnetNiger.Infrastructure.Persistence;

/// <summary>
/// Contexte de persistance unique de DotnetNiger : contenu éditorial, comptes ASP.NET
/// Identity, inscriptions et modération.
/// </summary>
public sealed class DotnetNigerDbContext
    : IdentityDbContext<ApplicationUser, ApplicationRole, Guid>
{
    /// <summary>
    /// Initialise un nouveau contexte.
    /// </summary>
    /// <param name="options">Options de configuration, notamment le provider.</param>
    public DotnetNigerDbContext(DbContextOptions<DotnetNigerDbContext> options)
        : base(options)
    {
    }

    /// <summary>
    /// Obtient les articles de blog.
    /// </summary>
    public DbSet<Article> Articles => Set<Article>();

    /// <summary>Rubriques éditoriales du blog ; données, et non constantes.</summary>
    public DbSet<Rubrique> Rubriques => Set<Rubrique>();

    /// <summary>
    /// Obtient les ressources.
    /// </summary>
    public DbSet<Resource> Resources => Set<Resource>();

    /// <summary>
    /// Obtient les événements de la communauté.
    /// </summary>
    public DbSet<CommunityEvent> CommunityEvents => Set<CommunityEvent>();

    /// <summary>
    /// Obtient les inscriptions aux événements.
    /// </summary>
    public DbSet<EventRegistration> EventRegistrations => Set<EventRegistration>();

    /// <summary>
    /// Obtient les projets open source.
    /// </summary>
    public DbSet<Project> Projects => Set<Project>();

    /// <summary>
    /// Obtient les commentaires.
    /// </summary>
    public DbSet<Comment> Comments => Set<Comment>();

    /// <summary>
    /// Obtient les signalements.
    /// </summary>
    public DbSet<Report> Reports => Set<Report>();

    /// <summary>
    /// Obtient les partenaires et sponsors.
    /// </summary>
    public DbSet<Partner> Partners => Set<Partner>();

    /// <summary>
    /// Obtient les courriels en attente d'expédition.
    /// </summary>
    public DbSet<ContactMessage> ContactMessages => Set<ContactMessage>();
    public DbSet<NewsletterSubscription> NewsletterSubscriptions => Set<NewsletterSubscription>();
    public DbSet<OutboxMessage> OutboxMessages => Set<OutboxMessage>();

    /// <summary>
    /// Obtient les photos de profil déposées par les membres.
    /// </summary>
    public DbSet<ProfilePhoto> ProfilePhotos => Set<ProfilePhoto>();

    /// <summary>Images déposées pour le contenu du site (couvertures, illustrations).</summary>
    public DbSet<ContentImage> ContentImages => Set<ContentImage>();

    /// <summary>Liens externes affichés sur les profils des membres.</summary>
    public DbSet<MemberLink> MemberLinks => Set<MemberLink>();

    /// <inheritdoc />
    protected override void OnModelCreating(ModelBuilder builder)
    {
        ArgumentNullException.ThrowIfNull(builder);

        base.OnModelCreating(builder);

        // Chaque table est décrite dans son propre fichier de configuration :
        // ajouter une entité n'oblige pas à éditer cette méthode.
        builder.ApplyConfigurationsFromAssembly(typeof(DotnetNigerDbContext).Assembly);
    }
}
