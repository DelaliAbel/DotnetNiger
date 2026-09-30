using DotnetNiger.Application.Abstractions;
using DotnetNiger.Domain.Entities.Community;
using DotnetNiger.Domain.Entities.Content;
using DotnetNiger.Domain.Enums;
using DotnetNiger.Infrastructure.Identity;
using DotnetNiger.Infrastructure.Persistence;
using Microsoft.Data.Sqlite;
using Microsoft.EntityFrameworkCore;

namespace DotnetNiger.IntegrationTests;

/// <summary>
/// Vérifie que le schéma créé par EF Core correspond bien à ce que le modèle métier
/// attend, et que les garde-fous du domaine sont appliqués jusqu'en base.
/// </summary>
/// <remarks>
/// Chaque test travaille sur ses propres données, identifiées par un slug unique. La
/// base est partagée par la classe, donc un test ne doit jamais compter les lignes des
/// autres : toute attente de comptage passe par un filtre.
/// </remarks>
public sealed class SchemaTests(SqliteFixture fixture) : IClassFixture<SqliteFixture>
{
    private static readonly DateTimeOffset Instant = DateTimeOffset.Parse(
        "2026-09-27T10:15:30+00:00",
        System.Globalization.CultureInfo.InvariantCulture);

    private static ApplicationUser CreerUtilisateur()
    {
        var jeton = Guid.NewGuid().ToString("N")[..8];
        var utilisateur = new ApplicationUser($"membre.{jeton}@example.ne");
        utilisateur.UpdateProfile("Awa", $"Diallo-{jeton}", city: "Niamey");

        return utilisateur;
    }

    /// <summary>Crée un article rattaché à la rubrique du contexte fourni.</summary>
    /// <param name="auteurId">Identifiant de l'auteur.</param>
    /// <param name="context">Contexte qui possède l'article et sa rubrique.</param>
    private static Article CreerArticle(Guid auteurId, DotnetNigerDbContext context)
    {
        // Le titre est unique par article : le slug en découle, et l'index unique sur le
        // slug rejetterait sinon les articles de titre identique.
        return new Article(
            auteurId,
            $"Article {Guid.NewGuid():N}",
            "Un résumé court.",
            "Le contenu de l'article.",
            RubriqueDeTest.Pour(context));
    }

    private static async Task<ApplicationUser> EnregistrerUtilisateurAsync(DotnetNigerDbContext context)
    {
        var utilisateur = CreerUtilisateur();
        context.Users.Add(utilisateur);
        await context.SaveChangesAsync();

        return utilisateur;
    }

    private static async Task<Article> EnregistrerArticleAsync(DotnetNigerDbContext context, Guid auteurId)
    {
        var article = CreerArticle(auteurId, context);
        context.Articles.Add(article);
        await context.SaveChangesAsync();

        return article;
    }

    [Fact]
    public async Task Le_schema_contient_toutes_les_tables_attendues()
    {
        string[] attendues =
        [
            "AspNetUsers",
            "AspNetRoles",
            "Articles",
            "Resources",
            "CommunityEvents",
            "EventRegistrations",
            "Projects",
            "Comments",
            "Reports",
            "Partners",
        ];

        var trouvees = new HashSet<string>(StringComparer.Ordinal);

        await fixture.Context.Database.OpenConnectionAsync();
        await using var commande = fixture.Context.Database.GetDbConnection().CreateCommand();
        commande.CommandText = "SELECT name FROM sqlite_master WHERE type = 'table'";

        await using var lecture = await commande.ExecuteReaderAsync();
        while (await lecture.ReadAsync())
        {
            trouvees.Add(lecture.GetString(0));
        }

        await fixture.Context.Database.CloseConnectionAsync();

        var manquantes = attendues.Where(table => !trouvees.Contains(table)).ToArray();

        Assert.True(
            manquantes.Length == 0,
            $"Tables manquantes : {string.Join(", ", manquantes)}.");
    }

    [Fact]
    public async Task Les_cles_etrangeres_sont_actives_Sur_Sqlite()
    {
        await fixture.Context.Database.OpenConnectionAsync();
        await using var commande = fixture.Context.Database.GetDbConnection().CreateCommand();
        commande.CommandText = "PRAGMA foreign_keys";

        var resultat = await commande.ExecuteScalarAsync();
        await fixture.Context.Database.CloseConnectionAsync();

        Assert.Equal(1L, Convert.ToInt64(resultat, System.Globalization.CultureInfo.InvariantCulture));
    }

    [Fact]
    public async Task Un_article_ne_peut_pas_referencer_un_auteur_inexistant()
    {
        var auteurInconnu = Guid.NewGuid();
        var context = fixture.CreateContextFor(new HorlogeTest(Instant), UtilisateurFictif.Anonyme);

        await context.Articles.AddAsync(CreerArticle(auteurInconnu, context));

        await Assert.ThrowsAsync<DbUpdateException>(() => context.SaveChangesAsync());
    }

    [Theory]
    [InlineData(false, false)]
    [InlineData(true, true)]
    public async Task Un_commentaire_doit_viser_exactement_une_cible(bool avecArticle, bool avecEvenement)
    {
        var context = fixture.CreateContextFor(new HorlogeTest(Instant), UtilisateurFictif.Anonyme);
        var auteur = await EnregistrerUtilisateurAsync(context);
        var article = await EnregistrerArticleAsync(context, auteur.Id);

        var evenement = new CommunityEvent(
            auteur.Id,
            $"Meetup {Guid.NewGuid():N}",
            "Un résumé.",
            "Le programme.",
            CommunityEventFormat.InPerson,
            Instant.UtcDateTime.AddDays(7),
            Instant.UtcDateTime.AddDays(7).AddHours(3));

        context.CommunityEvents.Add(evenement);
        await context.SaveChangesAsync();

        // L'entité refuse de fabriquer un commentaire sans cible ou avec deux cibles :
        // seul le SQL brut permet d'atteindre la contrainte de la base.
        object[] parametres =
        [
            Guid.NewGuid().ToString(),
            "Un commentaire invalide",
            CommentStatus.Published.ToString(),
            auteur.Id.ToString(),
            avecArticle ? article.Id.ToString() : null!,
            avecEvenement ? evenement.Id.ToString() : null!,
            Instant.UtcDateTime,
        ];

        var exception = await Assert.ThrowsAsync<SqliteException>(
            () => context.Database.ExecuteSqlRawAsync(
                """
                INSERT INTO "Comments"
                    ("Id", "Body", "Status", "AuthorId", "ArticleId", "CommunityEventId",
                     "ParentCommentId", "CreatedAtUtc", "UpdatedAtUtc", "IsDeleted",
                     "DeletedAtUtc", "DeletedById")
                VALUES ({0}, {1}, {2}, {3}, {4}, {5}, NULL, {6}, NULL, 0, NULL, NULL)
                """,
                parametres));

        Assert.Contains("CK_Comments_ExactlyOneTarget", exception.Message, StringComparison.Ordinal);
    }

    [Fact]
    public async Task Deux_articles_de_meme_titre_produisent_le_meme_slug_et_le_second_est_refuse()
    {
        var context = fixture.CreateContextFor(new HorlogeTest(Instant), UtilisateurFictif.Anonyme);
        var auteur = await EnregistrerUtilisateurAsync(context);

        var titre = $"Article {Guid.NewGuid():N}";
        var premier = new Article(auteur.Id, titre, "Résumé", "Contenu", RubriqueDeTest.Pour(context));
        var second = new Article(auteur.Id, titre, "Résumé", "Contenu", RubriqueDeTest.Pour(context));

        Assert.Equal(premier.Slug, second.Slug);

        context.Articles.Add(premier);
        await context.SaveChangesAsync();

        context.Articles.Add(second);
        await Assert.ThrowsAsync<DbUpdateException>(() => context.SaveChangesAsync());
    }

    [Fact]
    public async Task L_intercepteur_horodate_La_creation_puis_la_modification()
    {
        var context = fixture.CreateContextFor(new HorlogeTest(Instant), UtilisateurFictif.Anonyme);
        var auteur = await EnregistrerUtilisateurAsync(context);
        var article = await EnregistrerArticleAsync(context, auteur.Id);

        Assert.Equal(Instant.UtcDateTime, article.CreatedAtUtc);
        Assert.Null(article.UpdatedAtUtc);

        var plusTard = Instant.AddHours(3);
        using var contextPlusTard = fixture.CreateContextFor(new HorlogeTest(plusTard), UtilisateurFictif.Membre(auteur.Id));

        // L'entité doit être modifiée via le contexte qui va l'écrire : la muter
        // directement ne change rien pour un contexte qui ne la suit pas.
        var slug = article.Slug;
        var aModifier = await contextPlusTard.Articles
            .Include(candidate => candidate.Rubrique)
            .SingleAsync(candidate => candidate.Slug == slug);
        aModifier.UpdateDetails(aModifier.Title, "Un résumé révisé.", aModifier.Content, aModifier.Rubrique);
        await contextPlusTard.SaveChangesAsync();

        var relu = await contextPlusTard.Articles.SingleAsync(candidate => candidate.Slug == slug);

        Assert.Equal(plusTard.UtcDateTime, relu.UpdatedAtUtc);
        Assert.Equal(Instant.UtcDateTime, relu.CreatedAtUtc);
    }

    [Fact]
    public async Task Un_article_retire_logiquement_disparait_des_requetes()
    {
        var context = fixture.CreateContextFor(new HorlogeTest(Instant), UtilisateurFictif.Anonyme);
        var auteur = await EnregistrerUtilisateurAsync(context);
        var article = await EnregistrerArticleAsync(context, auteur.Id);
        var slug = article.Slug;

        Assert.True(await context.Articles.AnyAsync(candidate => candidate.Slug == slug));

        article.SoftDelete(Instant.UtcDateTime, auteur.Id);
        await context.SaveChangesAsync();

        Assert.False(await context.Articles.AnyAsync(candidate => candidate.Slug == slug));
        Assert.True(await context.Articles.IgnoreQueryFilters().AnyAsync(candidate => candidate.Slug == slug));
    }

    [Fact]
    public async Task Une_suppression_physique_est_transformee_en_retrait_logique()
    {
        var contextePreparation = fixture.CreateContextFor(new HorlogeTest(Instant), UtilisateurFictif.Anonyme);
        var auteur = await EnregistrerUtilisateurAsync(contextePreparation);

        // La suppression est demandée par un membre connecté : c'est lui qui est horodaté.
        var context = fixture.CreateContextFor(new HorlogeTest(Instant), UtilisateurFictif.Membre(auteur.Id));

        var article = new Article(auteur.Id, $"Article {Guid.NewGuid():N}", "Résumé", "Contenu", RubriqueDeTest.Pour(context));
        context.Articles.Add(article);
        await context.SaveChangesAsync();

        // Le slug est calculé par l'entité à partir du titre : c'est lui, et non le
        // titre, qui sert d'index unique.
        var slugAttendu = article.Slug;

        // Même si un appelant réclame une suppression physique, l'intercepteur la
        // transforme en retrait logique.
        context.Articles.Remove(article);
        await context.SaveChangesAsync();

        var relu = await context.Articles.IgnoreQueryFilters().SingleAsync(candidate => candidate.Slug == slugAttendu);

        Assert.True(relu.IsDeleted);
        Assert.Equal(Instant.UtcDateTime, relu.DeletedAtUtc);
        Assert.Equal(auteur.Id, relu.DeletedById);
    }

    private sealed class HorlogeTest(DateTimeOffset instant) : TimeProvider
    {
        public override DateTimeOffset GetUtcNow() => instant;
    }
}
