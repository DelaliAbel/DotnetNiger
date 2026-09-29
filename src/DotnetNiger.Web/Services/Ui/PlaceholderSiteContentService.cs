namespace DotnetNiger.Web.Services.Ui;

/// <summary>
/// Implémentation de garniture : des contenus réalistes et bien remplis pour valider
/// le rendu des pages. Seront remplacés par les données de l'API en phase 2.
/// </summary>
public sealed class PlaceholderSiteContentService : ISiteContentService
{
    private static readonly ArticleVue[] Articles = new[]
    {
        new ArticleVue
        {
            Titre = ".NET 10 est disponible : ce qui change pour les développeurs africains",
            Categorie = "Nouveautés de .NET",
            Extrait = "Performance, expérience d'installation simplifiée et nouveautés d'ASP.NET Core : tour d'horizon de la dernière version majeure de la plateforme.",
            Auteur = "Salifou Maiga",
            LectureMinutes = 8,
            Slug = "dotnet-10-disponible",
            ImageUrl = "/images/img3.jpg",
        },
        new ArticleVue
        {
            Titre = "Interview : Aïcha Ousmane, développeuse MAUI à Niamey",
            Categorie = "Interviews",
            Extrait = "D'une première app météo à une application de suivi de récoltes, elle raconte comment .NET MAUI l'aider à toucher Android et iOS avec une seule base de code.",
            Auteur = "Abdoulaye Ibrahim",
            LectureMinutes = 12,
            Slug = "interview-aicha-ousmane-maui",
        },
        new ArticleVue
        {
            Titre = "DotnetNiger.Toolbox atteint son centième contributeur",
            Categorie = "Projets locaux",
            Extrait = "La boîte à outils de la communauté vient de franchir un cap : cent contributeurs et plus de trois cents corrections acceptées depuis le lancement.",
            Auteur = "Issoufou Hamadou",
            LectureMinutes = 6,
            Slug = "toolbox-centieme-contributeur",
            ImageUrl = "/images/img2.jpg",
        },
        new ArticleVue
        {
            Titre = "Blazor en production : le retour d'expérience du portail de la communauté",
            Categorie = "Projets locaux",
            Extrait = "Architecture, rendu serveur, déploiement : comment le site DotnetNiger lui-même est construit avec Blazor et ce que nous en avons appris.",
            Auteur = "Moussa Djibrilla",
            LectureMinutes = 15,
            Slug = "blazor-en-production",
        },
        new ArticleVue
        {
            Titre = "Bien démarrer avec ASP.NET Core : votre premier point de terminaison",
            Categorie = "Nouveautés de .NET",
            Extrait = "Minimal APIs, model binding, validation : un pas à pas pensé pour les étudiants et les développeurs qui arrivent à .NET par le web.",
            Auteur = "Fatima Harouna",
            LectureMinutes = 10,
            Slug = "bien-demarrer-aspnet-core",
            ImageUrl = "/images/img1.jpeg",
        },
        new ArticleVue
        {
            Titre = "CSharpei et les cartes mentales : apprendre C# autrement",
            Categorie = "Interviews",
            Extrait = "Rencontre avec la créatrice d'un projet pédagogique qui vulgarise la syntaxe C# pour les classes préparatoires de l'université de Niamey.",
            Auteur = "Abdoulaye Ibrahim",
            LectureMinutes = 9,
            Slug = "csharpei-cartes-mentales",
        },
        new ArticleVue
        {
            Titre = "Sécurité : les cinq erreurs les plus fréquentes dans les API .NET",
            Categorie = "Nouveautés de .NET",
            Extrait = "Jeton dans l'URL, secrets en dur, absence de limitation de débit : passez en revue les failles les plus courantes et comment le framework vous aide.",
            Auteur = "Issoufou Hamadou",
            LectureMinutes = 11,
            Slug = "securite-cinq-erreurs-api",
            ImageUrl = "/images/img4.jpeg",
        },
        new ArticleVue
        {
            Titre = "Du prototype à la production : 48 heures à notre premier hackathon",
            Categorie = "Projets locaux",
            Extrait = "Récit du hackathon DotnetNiger : six équipes, un même défi et des solutions concrètes pour le marché nigérien.",
            Auteur = "Moussa Djibrilla",
            LectureMinutes = 7,
            Slug = "hackathon-48-heures",
        },
        new ArticleVue
        {
            Titre = "C# moderne : de la programmation orientée objet à la programmation orientée données",
            Categorie = "Nouveautés de .NET",
            Extrait = "Records, match patterns, source generators : les évolutions récentes du langage expliquées avec des exemples du quotidien.",
            Auteur = "Zara Oumarou",
            LectureMinutes = 13,
            Slug = "csharp-moderne",
            ImageUrl = "/images/img2.jpg",
        },
        new ArticleVue
        {
            Titre = "Entretien avec Mahamadou, développeur .NET chez une banque de la place",
            Categorie = "Interviews",
            Extrait = "Couche monolithique, tests, réglementation bancaire : le quotidien d'un développeur .NET dans une institution financière ouest-africaine.",
            Auteur = "Fatima Harouna",
            LectureMinutes = 14,
            Slug = "entretien-mahamadou-banque",
        },
        new ArticleVue
        {
            Titre = "Optimiser une API : de 800 ms à 80 ms sans changer d'infrastructure",
            Categorie = "Nouveautés de .NET",
            Extrait = "Caching, pagination, ensembles de résultats, compression : les techniques qui ont décuplé la vitesse de notre API de démonstration.",
            Auteur = "Salifou Maiga",
            LectureMinutes = 16,
            Slug = "optimiser-api-800-a-80",
        },
        new ArticleVue
        {
            Titre = "Lancement du club .NET de l'université de Niamey",
            Categorie = "Projets locaux",
            Extrait = "Des enseignants et des étudiants s'associent à DotnetNiger pour créer un point de rencontre hebdomadaire sur le campus : bienvenue aux débutants.",
            Auteur = "Aïcha Ousmane",
            LectureMinutes = 5,
            Slug = "club-net-universite-niamey",
            ImageUrl = "/images/img1.jpeg",
        },
    };

    private static readonly EvenementVue[] Evenements = new[]
    {
        new EvenementVue
        {
            Titre = "Meetup DotnetNiger : découvrir .NET AI",
            Type = "Meetup",
            Lieu = "Niamey, en présentiel",
            Description = "Une soirée pour découvrir les API d'intelligence artificielle de .NET avec des démonstrations de génération augmentée sur des données locales.",
            ImageUrl = "https://picsum.photos/seed/dotnetai/800/450",
        },
        new EvenementVue
        {
            Titre = "Conférence en ligne : performance en ASP.NET Core",
            Type = "Conférence",
            Lieu = "En ligne",
            Description = "Caching, middleware, allocations et profiling : un atelier de deux heures pour rendre vos API plus rapides, avec les questions du public.",
            ImageUrl = "https://picsum.photos/seed/perfconf/800/450",
        },
        new EvenementVue
        {
            Titre = "Hackathon DotnetNiger #1 : une application pour l'agriculture en 48 heures",
            Type = "Hackathon",
            Lieu = "Niamey, en présentiel",
            Description = "Six équipes, vingt-quatre participants et un défi : tirer parti de .NET et de l'intelligence artificielle pour un projet agricole ouest-africain.",
            ImageUrl = "https://picsum.photos/seed/hackathon1/800/450",
        },
        new EvenementVue
        {
            Titre = "Atelier Blazor : construire votre premier composant réutilisable",
            Type = "Atelier",
            Lieu = "En ligne",
            Description = "Un atelier pratique pour apprendre les bases de la création de composants : paramètres, événements, cycles de vie et factorisation.",
            ImageUrl = "https://picsum.photos/seed/atelierblazor/800/450",
        },
        new EvenementVue
        {
            Titre = "Tech Talk : .NET MAUI et le déploiement mobile en Afrique",
            Type = "Conférence",
            Lieu = "En ligne",
            Description = "Retour d'expérience sur le déploiement d'applications mobiles .NET MAUI à faible coût et hors connexion pour les zones rurales.",
            ImageUrl = "https://picsum.photos/seed/maumobile/800/450",
        },
        new EvenementVue
        {
            Titre = "Table ronde : le métier de développeur .NET au Niger",
            Type = "Meetup",
            Lieu = "Niamey, en présentiel",
            Description = "Développeurs, recruteurs et étudiants échangent sur les parcours, les compétences recherchées et les débouchés de la filière .NET dans la sous-région.",
            ImageUrl = "https://picsum.photos/seed/tableronde/800/450",
        },
    };

    private static readonly RessourceVue[] Ressources = new[]
    {
        new RessourceVue
        {
            Titre = "Parcours Microsoft Learn — Premiers pas avec .NET",
            Categorie = "Formations et tutoriels",
            Sujet = "Fondamentaux .NET",
            Description = "Une série de modules officiels, en français, pour découvrir .NET, C# et les outils de développement pas à pas.",
            Lien = "https://learn.microsoft.com/training/paths/build-first-app/",
            Externe = true,
            Auteur = "Microsoft Learn",
        },
        new RessourceVue
        {
            Titre = "Tutoriel C# par la pratique : 40 exercices corrigés",
            Categorie = "Formations et tutoriels",
            Sujet = "C#",
            Description = "Écrit par des membres de la communauté, ce tutoriel accompagne chaque notion du C# avec des exercices corrigés.",
            Lien = "/ressources/tutoriel-csharp-40-exercices",
            Externe = false,
            Auteur = "Zara Oumarou",
        },
        new RessourceVue
        {
            Titre = "Formation en ligne : ASP.NET Core de A à Z",
            Categorie = "Formations et tutoriels",
            Sujet = "ASP.NET Core",
            Description = "Du premier point de terminaison au déploiement : un parcours vidéo complet couvrant les fondamentaux d'ASP.NET Core.",
            Lien = "https://dotnet.microsoft.com/learn/videos",
            Externe = true,
            Auteur = "Microsoft",
        },
        new RessourceVue
        {
            Titre = "Collections et API de Microsoft Learn pour mobile",
            Categorie = "Formations et tutoriels",
            Sujet = ".NET MAUI",
            Description = "Les apprentissages officiels pour démarrer .NET MAUI, identifier les performances et publier sur les stores.",
            Lien = "https://learn.microsoft.com/dotnet/maui/",
            Externe = true,
            Auteur = "Microsoft Learn",
        },
        new RessourceVue
        {
            Titre = "Guide : architecture propre pour vos applications Blazor",
            Categorie = "Articles de blog et guides",
            Sujet = "Architecture",
            Description = "Séparation des responsabilités, dépendances inversées, tests : un guide pour garder votre application Blazor maintenable à long terme.",
            Lien = "/ressources/guide-architecture-blazor",
            Externe = false,
            Auteur = "Issoufou Hamadou",
        },
        new RessourceVue
        {
            Titre = "Guide : sécuriser votre API avec ASP.NET Core Identity",
            Categorie = "Articles de blog et guides",
            Sujet = "Sécurité",
            Description = "Inscription, connexion, rôles et cookies : les bons réflexes pour protéger votre API sans réinventer la roue.",
            Lien = "/ressources/guide-securiser-api-identity",
            Externe = false,
            Auteur = "Salifou Maiga",
        },
        new RessourceVue
        {
            Titre = "Dépendance d'injection en C# : comprendre enfin le sujet",
            Categorie = "Articles de blog et guides",
            Sujet = "C# avancé",
            Description = "Pourquoi, comment, et dans quel ordre : la DI expliquée avec des analogies du quotidien et des exemples exécutables.",
            Lien = "/ressources/dependance-injection-expliquee",
            Externe = false,
            Auteur = "Abdoulaye Ibrahim",
        },
        new RessourceVue
        {
            Titre = "Guide : optimisez les performances d'une API ASP.NET Core",
            Categorie = "Articles de blog et guides",
            Sujet = "Performance",
            Description = "Mise en cache, minimisation des allocations, pagination : les leviers mesurables pour une API qui tient la charge.",
            Lien = "/ressources/guide-performance-api-aspnet",
            Externe = false,
            Auteur = "Issoufou Hamadou",
        },
        new RessourceVue
        {
            Titre = "Boîte à outils : 12 bibliothèques .NET à connaître",
            Categorie = "Bibliothèques et outils",
            Sujet = "Sélection",
            Description = "Sérialisation, journalisation, tests, analyse : la sélection de la communauté pour développer plus vite et plus sûrement.",
            Lien = "/ressources/12-bibliotheques-dotnet",
            Externe = false,
            Auteur = "Moussa Djibrilla",
        },
        new RessourceVue
        {
            Titre = "FluentValidation : des validations déclaratives expressives",
            Categorie = "Bibliothèques et outils",
            Sujet = "Validation",
            Description = "Remplacez les blocs verbeux de validation par des règles lisibles, testables et indépendantes de vos modèles.",
            Lien = "https://docs.fluentvalidation.net/",
            Externe = true,
            Auteur = "Zara Oumarou",
        },
        new RessourceVue
        {
            Titre = "BenchmarkDotNet : mesurez la performance de votre code",
            Categorie = "Bibliothèques et outils",
            Sujet = "Benchmark",
            Description = "Des benchmarks rigoureux, des rapports lisibles et des conclusions fiables pour guider vos micro-optimisations.",
            Lien = "https://benchmarkdotnet.org/",
            Externe = true,
            Auteur = "Hassan Adamou",
        },
        new RessourceVue
        {
            Titre = "Serilog : une journalisation structurée pour la production",
            Categorie = "Bibliothèques et outils",
            Sujet = "Journalisation",
            Description = "Des journaux structurés, interrogeables et faciles à dépouiller pour comprendre ce qui se passe en production.",
            Lien = "https://serilog.net/",
            Externe = true,
            Auteur = "Fatima Ousmane",
        },
        new RessourceVue
        {
            Titre = "Cours gratuit : C# pour les débutants",
            Categorie = "Formations et tutoriels",
            Sujet = "C#",
            Description = "Le parcours officiel de Microsoft pour découvrir la syntaxe du langage, les types et les premiers programmes, avec des exercices guidés.",
            Lien = "https://learn.microsoft.com/training/",
            Externe = true,
            Auteur = "Microsoft",
        },
        new RessourceVue
        {
            Titre = "Programmation orientée objet en C# : le parcours complet",
            Categorie = "Formations et tutoriels",
            Sujet = "C# avancé",
            Description = "Classes, interfaces, héritage, génériques et délégations : un parcours pas à pas pour structurer vos applications.",
            Lien = "/ressources/parcours-poo-csharp",
            Externe = false,
            Auteur = "Hassan Adamou",
        },
        new RessourceVue
        {
            Titre = "Entity Framework Core : apprenez avec un projet concret",
            Categorie = "Formations et tutoriels",
            Sujet = "EF Core",
            Description = "De la première migration au code de la couche d'accès aux données, en passant par les jointures et le chargement des relations.",
            Lien = "/ressources/parcours-ef-core",
            Externe = false,
            Auteur = "Mahamadou Kane",
        },
        new RessourceVue
        {
            Titre = "Blazor pour les débutants : la série complète en vidéo",
            Categorie = "Formations et tutoriels",
            Sujet = "Blazor",
            Description = "Composants, données, formulaires et intégration avec une API : une série vidéo qui vous emmène de zéro à une application fonctionnelle.",
            Lien = "https://www.youtube.com/@dotnetniger",
            Externe = true,
            Auteur = "Issoufou Hamadou",
        },
        new RessourceVue
        {
            Titre = "Sécuriser vos APIs ASP.NET Core : le parcours pratique",
            Categorie = "Formations et tutoriels",
            Sujet = "Sécurité",
            Description = "JWT, cookies, rôles et protections contre les failles courantes : les gestes essentiels pour protéger vos points de terminaison.",
            Lien = "/ressources/parcours-securite-api",
            Externe = false,
            Auteur = "Fatima Harouna",
        },
        new RessourceVue
        {
            Titre = "xUnit : testez votre code avec confiance",
            Categorie = "Bibliothèques et outils",
            Sujet = "Tests",
            Description = "Théories, données paramétrées, mocks et analyse de couverture : le réflexe des tests qui protège vos refactorisations.",
            Lien = "https://xunit.net/",
            Externe = true,
            Auteur = "Abdoulaye Ibrahim",
        },
        new RessourceVue
        {
            Titre = "Dapper : des requêtes SQL simples et performantes",
            Categorie = "Bibliothèques et outils",
            Sujet = "Accès aux données",
            Description = "Un micro-ORM minimaliste, transparent et très rapide pour les projets qui préfèrent un contrôle total sur le SQL.",
            Lien = "https://www.learndapper.com/",
            Externe = true,
            Auteur = "Moussa Djibrilla",
        },
        new RessourceVue
        {
            Titre = "Polly : rendez vos appels HTTP résilients",
            Categorie = "Bibliothèques et outils",
            Sujet = "Résilience",
            Description = "Réessais, disjoncteurs, délais et dégradation gracieuse : gardez votre application debout quand un service tiers vacille.",
            Lien = "https://www.thepollyproject.org/",
            Externe = true,
            Auteur = "Salifou Maiga",
        },
        new RessourceVue
        {
            Titre = "Hangfire : des tâches planifiées en arrière-plan",
            Categorie = "Bibliothèques et outils",
            Sujet = "Tâches de fond",
            Description = "E-mails, rapports, synchronisations : planifiez et suivez vos travaux de fond depuis une interface simple et fiable.",
            Lien = "https://www.hangfire.io/",
            Externe = true,
            Auteur = "Aïcha Ousmane",
        },
        new RessourceVue
        {
            Titre = "Moq : simulez vos dépendances dans les tests",
            Categorie = "Bibliothèques et outils",
            Sujet = "Tests",
            Description = "Fabriquez des doublures expressives pour isoler vos classes et tester les comportements sans toucher à la vraie infrastructure.",
            Lien = "https://github.com/devlooped/moq",
            Externe = true,
            Auteur = "Hassan Adamou",
        },
        new RessourceVue
        {
            Titre = "Minimal APIs ou contrôleurs : comment choisir",
            Categorie = "Articles de blog et guides",
            Sujet = "Architecture",
            Description = "Deux styles, deux philosophies : un comparatif honnête pour trancher selon la taille et l'ambition de votre projet.",
            Lien = "/ressources/minimal-apis-ou-controleurs",
            Externe = false,
            Auteur = "Salifou Maiga",
        },
        new RessourceVue
        {
            Titre = "Le pipeline middleware ASP.NET Core expliqué avec des schémas",
            Categorie = "Articles de blog et guides",
            Sujet = "ASP.NET Core",
            Description = "Ordre, branchement et responsabilités : comprenez visuellement ce qui se passe avant, pendant et après votre code.",
            Lien = "/ressources/pipeline-middleware-aspnet",
            Externe = false,
            Auteur = "Fatima Harouna",
        },
        new RessourceVue
        {
            Titre = "Comprendre async/await sans mal de tête",
            Categorie = "Articles de blog et guides",
            Sujet = "C# avancé",
            Description = "Le mot clé async, l'état de la machine, l'exception bloquante : la mécanique expliquée simplement, avec des exemples sans jargon.",
            Lien = "/ressources/async-await-explique-simplement",
            Externe = false,
            Auteur = "Zara Oumarou",
        },
        new RessourceVue
        {
            Titre = "MemoryCache : les pièges à éviter en production",
            Categorie = "Articles de blog et guides",
            Sujet = "Performance",
            Description = "Évictions, courses et invalidation : comment tirer le meilleur du cache en mémoire sans laisser les données périmées s'installer.",
            Lien = "/ressources/memorycache-pieges",
            Externe = false,
            Auteur = "Mahamadou Kane",
        },
        new RessourceVue
        {
            Titre = "Tests d'intégration avec une vraie base de données",
            Categorie = "Articles de blog et guides",
            Sujet = "Tests",
            Description = "Containers, isolation et régénération des données : tester contre SQL Server localement comme en CI, sans fragile.",
            Lien = "/ressources/tests-integration-base-donnees",
            Externe = false,
            Auteur = "Moussa Djibrilla",
        },
        new RessourceVue
        {
            Titre = "Razor Pages ou Blazor : quelle approche pour votre projet ?",
            Categorie = "Articles de blog et guides",
            Sujet = "Blazor",
            Description = "Rendu serveur classique ou composants riches : les critères de choix selon l'équipe, le délai et l'expérience recherchée.",
            Lien = "/ressources/razor-pages-ou-blazor",
            Externe = false,
            Auteur = "Abdoulaye Ibrahim",
        },
        new RessourceVue
        {
            Titre = "Docker pour les développeurs .NET : conteneuriser une API",
            Categorie = "Formations et tutoriels",
            Sujet = "Déploiement",
            Description = "Multi-étapes, images légères et fichiers compose : empaqueter votre application .NET pour qu'elle s'exécute partout de la même façon.",
            Lien = "/ressources/parcours-docker-dotnet",
            Externe = false,
            Auteur = "Mahamadou Kane",
        },
        new RessourceVue
        {
            Titre = "SignalR pas à pas : ajouter du temps réel à votre application",
            Categorie = "Formations et tutoriels",
            Sujet = "Temps réel",
            Description = "Hubs, groupes et reconnexion : construisez une messagerie et des notifications en direct, du serveur jusqu'au client.",
            Lien = "/ressources/parcours-signalr",
            Externe = false,
            Auteur = "Aïcha Ousmane",
        },
        new RessourceVue
        {
            Titre = "Tests unitaires en C# : de zéro à la couverture",
            Categorie = "Formations et tutoriels",
            Sujet = "Tests",
            Description = "Choisir xUnit, isoler une dépendance, écrire des théories et mesurer la couverture : les bases solides du développement piloté par les tests.",
            Lien = "https://learn.microsoft.com/dotnet/core/testing/",
            Externe = true,
            Auteur = "Microsoft Learn",
        },
        new RessourceVue
        {
            Titre = "FluentAssertions : des tests qui se lisent comme des phrases",
            Categorie = "Bibliothèques et outils",
            Sujet = "Tests",
            Description = "Des assertions expressives et des messages d'échec limpides pour écrire des tests que vos collègues comprennent au premier regard.",
            Lien = "https://fluentassertions.com/",
            Externe = true,
            Auteur = "Fatima Ousmane",
        },
        new RessourceVue
        {
            Titre = "MediatR : découpler vos cas d'usage",
            Categorie = "Bibliothèques et outils",
            Sujet = "Architecture",
            Description = "Requêtes, commandes et notifications : une manière simple de réduire le couplage et de centraliser les comportements transverses.",
            Lien = "https://github.com/jbogard/MediatR",
            Externe = true,
            Auteur = "Issoufou Hamadou",
        },
        new RessourceVue
        {
            Titre = "Roslynator : des analyseurs de code pour C#",
            Categorie = "Bibliothèques et outils",
            Sujet = "Qualité",
            Description = "Des centaines de règles et de corrections automatiques pour uniformiser le style et traquer les petites erreurs avant la revue.",
            Lien = "https://josefpihrt.github.io/docs/roslynator/",
            Externe = true,
            Auteur = "Zara Oumarou",
        },
    };

    private static readonly ProjetVue[] Projets = new[]
    {
        new ProjetVue
        {
            Nom = "DotnetNiger.Toolbox",
            Description = "Une collection de petits outils en ligne de commande pour automatiser les tâches courantes des développeurs .NET.",
            Technologies = new[] { ".NET", "C#" },
            Etoiles = 421,
            Contributeurs = 84,
            Lien = "https://github.com/dotnetniger/toolbox",
        },
        new ProjetVue
        {
            Nom = "DotnetNiger.Web",
            Description = "Le site de la communauté lui-même : contenu, événements, communauté et API, construit avec ASP.NET Core et Blazor.",
            Technologies = new[] { "ASP.NET Core", "Blazor" },
            Etoiles = 148,
            Contributeurs = 26,
            Lien = "https://github.com/dotnetniger/dotnetniger",
        },
        new ProjetVue
        {
            Nom = "Sagesse.Recipes",
            Description = "Une bibliothèque de recettes culinaires ouest-africaines avec recherche plein texte et exportation PDF, portée par un membre de la communauté.",
            Technologies = new[] { "C#", "EF Core" },
            Etoiles = 96,
            Contributeurs = 11,
            Lien = "https://github.com/dotnetniger/sagesse-recipes",
        },
        new ProjetVue
        {
            Nom = "MeteoMani",
            Description = "Une application .NET MAUI de prévisions météorologiques pour les villes du Sahel, optimiste pour les réseaux lents.",
            Technologies = new[] { ".NET MAUI", "C#" },
            Etoiles = 75,
            Contributeurs = 9,
            Lien = "https://github.com/dotnetniger/meteomani",
        },
        new ProjetVue
        {
            Nom = "BlazorValidate",
            Description = "Components de validation Blazor prêts à l'emploi : champs reflétés, messages d'erreur accessibles et localisation en français.",
            Technologies = new[] { "Blazor", "C#" },
            Etoiles = 132,
            Contributeurs = 15,
            Lien = "https://github.com/dotnetniger/blazorvalidate",
        },
        new ProjetVue
        {
            Nom = "NigerPay.SDK",
            Description = "Kit de démarrage pour intégrer les moyens de paiement mobiles ouest-africains dans vos applications .NET.",
            Technologies = new[] { ".NET", "C#", "EF Core" },
            Etoiles = 203,
            Contributeurs = 18,
            Lien = "https://github.com/dotnetniger/nigerpay-sdk",
        },
    };

    private static readonly PartenaireVue[] Partenaires = new[]
    {
        new PartenaireVue
        {
            Nom = "Campus C2D Numérique",
            Type = "Partenaire",
            Description = "Pôle d'innovation qui héberge nos meetups à Niamey et met à disposition des salles et un amphithéâtre.",
            LogoUrl = "https://placehold.co/240x96/512BD4/ffffff?text=C2D+Numerique",
            Lien = "https://example.org/c2d",
        },
        new PartenaireVue
        {
            Nom = "Université de Niamey",
            Type = "Partenaire",
            Description = "Partenaire académique : co-organisation du club .NET et accueil des ateliers étudiants.",
            Lien = "https://example.org/universite-niamey",
        },
        new PartenaireVue
        {
            Nom = "Orange Niger",
            Type = "Sponsor",
            Description = "Sponsor du hackathon : connexions, hébergement cloud et support aux équipes gagnantes.",
            Lien = "https://example.org/orange-niger",
        },
        new PartenaireVue
        {
            Nom = "Sonatel",
            Type = "Partenaire",
            Description = "Groupe télécom d'Afrique de l'Ouest qui soutient la diffusion des contenus en ligne de la communauté.",
            Lien = "https://example.org/sonatel",
        },
        new PartenaireVue
        {
            Nom = "Microsoft",
            Type = "Sponsor",
            Description = "Apporte le support de Microsoft Learn, des crédits Azure et des orateurs pour nos conférences annuelles.",
            Lien = "https://developer.microsoft.com",
        },
        new PartenaireVue
        {
            Nom = "Niamey Tech Hub",
            Type = "Partenaire",
            Description = "Réseau d'acteurs de la tech à Niamey : co-organisation d'événements croisés et mise en relation.",
            Lien = "https://example.org/niamey-tech-hub",
        },
        new PartenaireVue
        {
            Nom = "L'École Supérieure d'Informatique",
            Type = "Partenaire",
            Description = "Établissement d'enseignement supérieur qui propose des modules .NET à ses étudiants.",
            Lien = "https://example.org/esi",
        },
        new PartenaireVue
        {
            Nom = "Horizons Numériques",
            Type = "Sponsor",
            Description = "Agence de communication qui assure la captation vidéo et le résumé de nos événements.",
            Lien = "https://example.org/horizons",
        },
    };

    private static readonly MembreVue[] Membres = new[]
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

    private static readonly CanalVue[] Canaux = new[]
    {
        new CanalVue { Nom = "Discord", Url = "https://discord.gg/dotnetniger", Icône = "discord" },
        new CanalVue { Nom = "GitHub", Url = "https://github.com/dotnetniger", Icône = "github" },
        new CanalVue { Nom = "LinkedIn", Url = "https://www.linkedin.com/company/dotnetniger", Icône = "linkedin" },
        new CanalVue { Nom = "X (Twitter)", Url = "https://x.com/dotnetniger", Icône = "x" },
        new CanalVue { Nom = "YouTube", Url = "https://www.youtube.com/@dotnetniger", Icône = "youtube" },
        new CanalVue { Nom = "Facebook", Url = "https://www.facebook.com/dotnetniger", Icône = "facebook" },
    };

    private const string Presentation = "DotnetNiger est plus qu'une simple communauté de développeurs : c'est un catalyseur de la transformation numérique au Sahel. Fondée par des passionnés de l'écosystème Microsoft, notre organisation s'est donnée pour mission de démocratiser l'accès aux technologies de pointe et de former une nouvelle génération d'ingénieurs capables de relever les défis les plus complexes de notre époque.";

    private const string Mission = "Propulser le talent local au sommet de la tech mondiale.";

    private const string MissionDetail = "Nous croyons que le Niger possède un potentiel technologique inexploité. Notre mission est de fournir les outils, les connaissances et le réseau nécessaires pour que chaque développeur .NET puisse exceller, innover et contribuer au progrès numérique de notre nation.";

    private static readonly ValeurVue[] Valeurs = new[]
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

    private static readonly JalonVue[] Jalons = new[]
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

    public Task<AccueilVue> ChargerAccueilAsync(CancellationToken annulation = default) =>
        Task.FromResult(new AccueilVue
        {
            ArticlesRecents = Articles.Take(4).ToArray(),
            EvenementsAVenir = Evenements.Take(3).ToArray(),
            RessourcesPopulaires = Ressources.Take(6).ToArray(),
            Canaux = Canaux.ToArray(),
            NombreMembres = 840,
            NombreProjets = Projets.Length,
            NombreRessources = Ressources.Length,
            NombreEvenements = Evenements.Length,
        });

    public Task<IReadOnlyList<ArticleVue>> ChargerArticlesAsync(CancellationToken annulation = default) =>
        Task.FromResult<IReadOnlyList<ArticleVue>>(Articles);

    public Task<IReadOnlyList<EvenementVue>> ChargerEvenementsAsync(CancellationToken annulation = default) =>
        Task.FromResult<IReadOnlyList<EvenementVue>>(Evenements);

    public Task<IReadOnlyList<RessourceVue>> ChargerRessourcesAsync(CancellationToken annulation = default) =>
        Task.FromResult<IReadOnlyList<RessourceVue>>(Ressources);

    public Task<IReadOnlyList<ProjetVue>> ChargerProjetsAsync(CancellationToken annulation = default) =>
        Task.FromResult<IReadOnlyList<ProjetVue>>(Projets);

    public Task<IReadOnlyList<PartenaireVue>> ChargerPartenairesAsync(CancellationToken annulation = default) =>
        Task.FromResult<IReadOnlyList<PartenaireVue>>(Partenaires);

    public Task<IReadOnlyList<MembreVue>> ChargerMembresAsync(CancellationToken annulation = default) =>
        Task.FromResult<IReadOnlyList<MembreVue>>(Membres);

    public Task<AProposVue> ChargerAProposAsync(CancellationToken annulation = default) =>
        Task.FromResult(new AProposVue
        {
            Presentation = Presentation,
            Mission = Mission,
            MissionDetail = MissionDetail,
            Valeurs = Valeurs.ToArray(),
            Histoire = Jalons.ToArray(),
            Equipe = Membres.ToArray(),
        });
}