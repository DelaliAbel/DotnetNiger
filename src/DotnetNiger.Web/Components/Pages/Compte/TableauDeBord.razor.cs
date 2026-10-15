using System.Globalization;
using DotnetNiger.Web.Services.EspaceMembre;
using Microsoft.AspNetCore.Components;

namespace DotnetNiger.Web.Components.Pages.Compte;

/// <summary>
/// Accueil de l'espace membre : ce que le membre a partagé et ses rencontres.
/// </summary>
/// <remarks>
/// La page ne lit rien elle-même : elle passe par <see cref="ServiceEspaceMembre"/>, comme
/// la mise en page qui l'encadre. Deux lectures parallèles du même membre sur le même
/// contexte de base sont refusées par EF, et c'est ce qui faisait tomber le tableau de
/// bord en erreur 500.
/// <para>
/// Le service ne rend que le contenu ; les libellés qui le présentent restent ici.
/// </para>
/// </remarks>
public partial class TableauDeBord
{
    private static readonly CultureInfo Culture = CultureInfo.GetCultureInfo("fr-FR");

    /// <summary>Catégories proposées, alignées sur celles du formulaire de dépôt.</summary>
    private static readonly string[] Categories = ["Guide", "Training", "Tutorial", "Library", "Tool"];

    private EspaceMembreVue? _espace;

    /// <summary>Message bref affiché après une action de connexion.</summary>
    [SupplyParameterFromQuery(Name = "confirmation")]
    public string? Confirmation { get; set; }

    /// <summary>Message d'erreur transmis par une autre page.</summary>
    [SupplyParameterFromQuery(Name = "erreur")]
    public string? Erreur { get; set; }

    /// <summary>Alerte éventuellement affichée en tête de contenu.</summary>
    private string? _Alerte;

    private bool _AlerteEstErreur;

    /// <inheritdoc />
    protected override async Task OnParametersSetAsync()
    {
        _AlerteEstErreur = Erreur is not null;
        _Alerte = Erreur switch
        {
            "depot-invalide" => "Vérifiez le titre et la description, puis réessayez.",
            "format_invalide" => "La ressource n'a pas pu être publiée : son adresse ou son contenu est invalide.",
            "taille_invalide" => "La ressource dépasse la taille autorisée.",
            null when Confirmation == "ressource-deposee" => "Ressource publiée : elle est maintenant en ligne.",
            null when Confirmation == "ressource-corrigee" => "Ressource corrigée.",
            null when Confirmation == "ressource-retiree" => "Ressource retirée.",
            "ressource-pas-a-vous" => "Cette ressource n'est pas la vôtre.",
            "ressource-introuvable" => "Cette ressource n'existe plus.",
            null => Confirmation,
            _ => "Le dépôt n'a pas abouti. Vérifiez les champs et réessayez.",
        };

        _espace = await Espace.LireAsync();
    }
}
