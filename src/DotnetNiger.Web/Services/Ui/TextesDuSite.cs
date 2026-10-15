namespace DotnetNiger.Web.Services.Ui;

/// <summary>
/// Textes propres au site : ce que l'association dit d'elle-même.
/// </summary>
/// <remarks>
/// <para>
/// Ces contenus ne sont ni des articles ni des événements : ils ne se publient, ne
/// s'archivent et ne se modèrent pas. La mission, les valeurs et les jalons de l'histoire
/// ne changent pas au rythme des parutions, et les faire passer par la table des contenus
/// les ferait disparaître au premier incident d'affichage.
/// </para>
/// <para>
/// Les canaux relèvent de la même logique : ils changent avec les coordonnées
/// officielles du site, pas avec les publications. Les regrouper ici laisse une adresse
/// Discord modifiable en un seul endroit lisible, et non dans une ligne de base.
/// </para>
/// <para>
/// Les photos de l'équipe restent dans <c>wwwroot/images/equipe/</c>, choisies par
/// convention de nom de fichier : cela évite une colonne de plus pour une information
/// qui n'appartient qu'à la présentation.
/// </para>
/// </remarks>
internal static class TextesDuSite
{
    /// <summary>Présentation de l'association, en tête de la page À propos.</summary>
    public const string Presentation = "DotnetNiger est plus qu'une simple communauté de développeurs : c'est un catalyseur de la transformation numérique au Sahel. Fondée par des passionnés de l'écosystème Microsoft, notre organisation s'est donnée pour mission de démocratiser l'accès aux technologies de pointe et de former une nouvelle génération d'ingénieurs capables de relever les défis les plus complexes de notre époque.";

    /// <summary>Mission, en une phrase.</summary>
    public const string Mission = "Propulser le talent local au sommet de la tech mondiale.";

    /// <summary>Mission développée, en un paragraphe.</summary>
    public const string MissionDetail = "Nous croyons que le Niger possède un potentiel technologique inexploité. Notre mission est de fournir les outils, les connaissances et le réseau nécessaires pour que chaque développeur .NET puisse exceller, innover et contribuer au progrès numérique de notre nation.";

    public static readonly MembreVue[] Membres = new[]
        {
            new MembreVue
            {
                Nom = "Abdoulaye Ibrahim",
                Role = "Président de la communauté",
                Teinte = "violet",
                ImageUrl = "/images/equipe/01-abdoulaye-ibrahim.png",
                LienGithub = "https://github.com/abdoulaye-ibrahim",
                LienLinkedin = "https://www.linkedin.com/in/abdoulaye-ibrahim",
                LienX = "https://x.com/abdoulaye_ib",
            },
            new MembreVue
            {
                Nom = "Aïcha Ousmane",
                Role = "Vice-présidente — événements",
                Teinte = "vert",
                ImageUrl = "/images/equipe/02-aicha-ousmane.png",
                LienGithub = "https://github.com/aicha-ousmane",
                LienLinkedin = "https://www.linkedin.com/in/aicha-ousmane",
                LienX = "https://x.com/aicha_ousmane",
            },
            new MembreVue
            {
                Nom = "Moussa Djibrilla",
                Role = "Trésorier — partenariats",
                Teinte = "orange",
                ImageUrl = "/images/equipe/03-moussa-djibrilla.png",
                LienGithub = "https://github.com/moussa-djibrilla",
                LienLinkedin = "https://www.linkedin.com/in/moussa-djibrilla",
                LienX = "https://x.com/moussa_dj",
            },
            new MembreVue
            {
                Nom = "Salifou Maiga",
                Role = "Lead développeur",
                Teinte = "ink",
                ImageUrl = "/images/equipe/04-salifou-maiga.png",
                LienGithub = "https://github.com/salifou-maiga",
                LienLinkedin = "https://www.linkedin.com/in/salifou-maiga",
                LienX = "https://x.com/salifou_maiga",
            },
            new MembreVue
            {
                Nom = "Fatima Harouna",
                Role = "Communication et communauté",
                Teinte = "violet",
                ImageUrl = "/images/equipe/05-fatima-harouna.png",
                LienGithub = "https://github.com/fatima-harouna",
                LienLinkedin = "https://www.linkedin.com/in/fatima-harouna",
                LienX = "https://x.com/fatima_h",
            },
            new MembreVue
            {
                Nom = "Issoufou Hamadou",
                Role = "Développeur backend",
                Teinte = "vert",
                ImageUrl = "/images/equipe/06-issoufou-hamadou.png",
                LienGithub = "https://github.com/issoufou-hamadou",
                LienLinkedin = "https://www.linkedin.com/in/issoufou-hamadou",
                LienX = "https://x.com/issoufou_h",
            },
            new MembreVue
            {
                Nom = "Zara Oumarou",
                Role = "Formatrice C# et responsable pédagogie",
                Teinte = "orange",
                ImageUrl = "/images/equipe/07-zara-oumarou.png",
                LienGithub = "https://github.com/zara-oumarou",
                LienLinkedin = "https://www.linkedin.com/in/zara-oumarou",
                LienX = "https://x.com/zara_oumarou",
            },
            new MembreVue
            {
                Nom = "Mahamadou Kane",
                Role = "Développeur .NET — secteur bancaire",
                Teinte = "ink",
                ImageUrl = "/images/equipe/08-mahamadou-kane.png",
                LienGithub = "https://github.com/mahamadou-kane",
                LienLinkedin = "https://www.linkedin.com/in/mahamadou-kane",
                LienX = "https://x.com/mahamadou_k",
            },
            new MembreVue
            {
                Nom = "Rokia Diallo",
                Role = "Chargée des partenariats académiques",
                Teinte = "violet",
                ImageUrl = "/images/equipe/09-rokia-diallo.png",
                LienGithub = "https://github.com/rokia-diallo",
                LienLinkedin = "https://www.linkedin.com/in/rokia-diallo",
                LienX = "https://x.com/rokia_diallo",
            },
            new MembreVue
            {
                Nom = "Hassan Adamou",
                Role = "Mainteneur DotnetNiger.Toolbox",
                Teinte = "vert",
                ImageUrl = "/images/equipe/10-hassan-adamou.png",
                LienGithub = "https://github.com/hassan-adamou",
                LienLinkedin = "https://www.linkedin.com/in/hassan-adamou",
            },
            new MembreVue
            {
                Nom = "Mariama Seydou",
                Role = "Designer d'interface",
                Teinte = "orange",
                ImageUrl = "/images/equipe/11-mariama-seydou.png",
                LienGithub = "https://github.com/mariama-seydou",
                LienLinkedin = "https://www.linkedin.com/in/mariama-seydou",
                LienX = "https://x.com/mariama_sey",
            },
            new MembreVue
            {
                Nom = "Ibrahim Cissé",
                Role = "Responsable des hackathons",
                Teinte = "ink",
                ImageUrl = "/images/equipe/12-ibrahim-cisse.png",
                LienGithub = "https://github.com/ibrahim-cisse",
                LienLinkedin = "https://www.linkedin.com/in/ibrahim-cisse",
                LienX = "https://x.com/ibrahim_cisse",
            },
        };

    public static readonly ValeurVue[] Valeurs = new[]
        {
            new ValeurVue
            {
                Titre = "Collaboration",
                Description = "L'esprit de partage est au cœur de notre ADN. Ensemble, nous apprenons et grandissons à la fois.",
                Icone = "users",
            },
            new ValeurVue
            {
                Titre = "Innovation",
                Description = "Repousser les limites du possible avec .NET pour résoudre les défis locaux par le code.",
                Icone = "sparkles",
            },
            new ValeurVue
            {
                Titre = "Excellence",
                Description = "Nous visons la qualité technique et l'éthique professionnelle dans tous nos projets communautaires.",
                Icone = "star",
            },
        };

    public static readonly JalonVue[] Jalons = new[]
        {
            new JalonVue
            {
                Periode = "2024",
                Titre = "La Fondation",
                Description = "Un petit groupe de passionnés se réunit à Niamey pour poser les jalons de l'écosystème .NET au Niger, avec l'ambition de créer un pont vers l'innovation mondiale.",
            },
            new JalonVue
            {
                Periode = "2025",
                Titre = "L'Expansion",
                Description = "Lancement du premier grand meetup communautaire réunissant plus de cent développeurs et étudiants, marquant le début d'une dynamique de partage sans précédent.",
            },
            new JalonVue
            {
                Periode = "2026",
                Titre = "La Reconnaissance",
                Description = "Signature de partenariats stratégiques avec des institutions locales et intégration officielle dans le réseau global des communautés .NET.",
            },
        };
}
