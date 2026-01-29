# Guide de Contribution

Merci de votre intérêt pour contribuer à DotnetNiger ! Ce document fournit les directives et processus pour contribuer au projet.

## 🎯 Comment contribuer

Il existe plusieurs façons de contribuer :

1. **Signaler des bugs** - Ouvrir une issue pour les bugs découverts
2. **Suggérer des améliorations** - Proposer de nouvelles fonctionnalités via les issues
3. **Soumettre du code** - Via des pull requests
4. **Améliorer la documentation** - Corriger ou améliorer les docs
5. **Partager des retours** - Donner votre avis sur le projet

## 📋 Avant de commencer

### Configuration de votre environnement

```bash
# Cloner votre fork
git clone https://github.com/YOUR_USERNAME/DotnetNiger.git
cd DotnetNiger

# Ajouter le remote upstream
git remote add upstream https://github.com/ORIGINAL_OWNER/DotnetNiger.git

# Créer une branche pour votre travail
git checkout -b feature/ma-fonctionnalite
```

### Prérequis

- .NET 9 SDK
- Node.js 20+ (pour Prettier et outils de formatage)
- Visual Studio 2022 ou VS Code
- Docker (optionnel, pour tester avec docker-compose)
- Git

### Installation des dépendances

```bash
# Installer les dépendances .NET
dotnet restore

# Installer les dépendances Node.js (pour Prettier)
npm install
```

### Formatage automatique avec Prettier

Le projet utilise Prettier pour le formatage automatique des fichiers (Markdown, JSON, YAML, etc.).

```bash
# Formater tous les fichiers
npm run format

# Vérifier le formatage sans modifier les fichiers
npm run format:check
```

**Important** : Avant de soumettre une pull request, assurez-vous que tous vos fichiers sont formatés avec Prettier. Le workflow CI vérifiera automatiquement le formatage.

## 🐛 Signaler un bug

Avant de soumettre un rapport de bug :

1. **Vérifier les issues existantes** - Assurez-vous que le bug n'a pas déjà été signalé
2. **Vérifier la documentation** - Consulter les guides pour voir si c'est documenté
3. **Tester la dernière version** - Le bug pourrait être déjà corrigé

### Format du rapport de bug

```markdown
**Description du bug**
Décrivez clairement et concisément ce qui est cassé.

**Étapes pour reproduire**

1. Allez à...
2. Cliquez sur...
3. Observez le comportement...

**Comportement attendu**
Décrivez ce qui devrait se passer.

**Comportement actuel**
Décrivez ce qui se passe réellement.

**Environnement**

- OS: [ex: Windows 11]
- .NET Version: [ex: 9.0.0]
- IDE: [ex: Visual Studio 2022]

**Logs d'erreur**
```

Coller les logs pertinents ici

```

**Captures d'écran**
Si applicable, ajouter des captures d'écran.
```

## ✨ Suggérer des améliorations

### Format de suggestion

```markdown
**Description du problème**
Décrire le problème de façon claire.

**Solution proposée**
Décrire votre idée de solution.

**Alternatives considérées**
Lister les alternatives envisagées.

**Contexte supplémentaire**
Ajouter tout contexte utile.
```

## 💻 Soumettre du code

### Processus de pull request

1. **Fork le repository** sur GitHub
2. **Créer une branche** avec un nom descriptif
3. **Faire vos changements**
4. **Écrire des tests** pour le nouveau code
5. **Exécuter les tests** localement
6. **Commit avec messages clairs**
7. **Push vers votre fork**
8. **Ouvrir une pull request**

### Conventions de nommage des branches

```
feature/description-courte       # Nouvelle fonctionnalité
bugfix/description-courte        # Correction de bug
docs/description-courte          # Documentation
refactor/description-courte      # Refactorisation
test/description-courte          # Tests
chore/description-courte         # Maintenance
```

### Messages de commit

Utiliser le format Conventional Commits :

```
<type>(<scope>): <subject>

<body>

<footer>
```

Exemples :

```
feat(gateway): ajouter middleware de rate limiting
fix(identity): corriger validation du token JWT
docs(readme): mettre à jour les instructions
refactor(community): optimiser les requêtes DB
test(gateway): ajouter tests pour le cache
```

Types courants :

- `feat` - Nouvelle fonctionnalité
- `fix` - Correction de bug
- `docs` - Documentation
- `refactor` - Refactorisation sans changement fonctionnel
- `test` - Ajout ou modification de tests
- `chore` - Tâches de maintenance

### Standards de code

#### C# Coding Guidelines

```csharp
// ✅ BON
public class UserService
{
    private readonly ILogger<UserService> _logger;

    public UserService(ILogger<UserService> logger)
    {
        _logger = logger;
    }

    /// <summary>
    /// Récupère un utilisateur par ID
    /// </summary>
    /// <param name="userId">L'ID de l'utilisateur</param>
    /// <returns>L'utilisateur ou null</returns>
    public async Task<User?> GetUserByIdAsync(int userId)
    {
        try
        {
            _logger.LogInformation("Récupération de l'utilisateur: {UserId}", userId);
            return await _context.Users.FindAsync(userId);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Erreur lors de la récupération de l'utilisateur");
            throw;
        }
    }
}

// ❌ MAUVAIS
public class userservice
{
    public user GetUser(int id)
    {
        return _context.users.find(id);
    }
}
```

#### Directives générales

- **Nommage** : PascalCase pour classes, méthodes publiques ; camelCase pour variables locales
- **Documentation XML** : Ajouter pour tous les membres publics
- **Async/Await** : Utiliser `async/await` plutôt que `.Result`
- **LINQ** : Préférer les expressions LINQ aux boucles
- **Null checking** : Utiliser les null-conditional operators (`?.`)
- **Logging** : Utiliser Serilog avec contextes appropriés
- **Erreurs** : Créer des exceptions personnalisées en héritage
- **Dépendances** : Injecter via constructeur (Dependency Injection)

### Tests

Chaque PR doit inclure des tests :

```csharp
[TestClass]
public class UserServiceTests
{
    private IUserRepository _userRepository;
    private UserService _userService;

    [TestInitialize]
    public void Setup()
    {
        _userRepository = new Mock<IUserRepository>().Object;
        _userService = new UserService(_userRepository);
    }

    [TestMethod]
    public async Task GetUserById_WithValidId_ReturnsUser()
    {
        // Arrange
        var userId = 1;
        var expectedUser = new User { Id = userId, Name = "John Doe" };

        // Act
        var result = await _userService.GetUserByIdAsync(userId);

        // Assert
        Assert.IsNotNull(result);
        Assert.AreEqual(expectedUser.Id, result.Id);
    }
}
```

### Documentation du code

```csharp
/// <summary>
/// Valide un token JWT et retourne les claims
/// </summary>
/// <param name="token">Le token JWT à valider</param>
/// <returns>Les claims du token si valide</returns>
/// <exception cref="UnauthorizedException">Si le token est invalide</exception>
/// <remarks>
/// Cette méthode vérifie l'expiration et la signature du token.
/// </remarks>
public async Task<ClaimsPrincipal> ValidateTokenAsync(string token)
{
    // Implementation
}
```

## 🔄 Processus de revue

Chaque PR sera examinée par au moins un mainteneur :

1. **Vérification de la qualité du code**
2. **Tests et couverture**
3. **Documentation
   **
4. **Conformité avec les guidelines**
5. **Performance et sécurité**

### Feedback

Les commentaires de revue auront pour objectif d'être :

- ✅ Constructifs et respectueux
- ✅ Spécifiques et actionnables
- ✅ Éducatifs quand approprié
- ❌ Pas personnels ou dérogatoires

## 📚 Documentation

Toute nouvelle fonctionnalité doit être documentée :

1. **Code comments** - Expliquer le "pourquoi", pas le "quoi"
2. **XML Documentation** - Pour l'API publique
3. **README updates** - Si ça affecte l'utilisation
4. **Architecture docs** - Si c'est une change majeure

## ✅ Checklist avant de soumettre

- [ ] Mon code suit les style guidelines du projet
- [ ] J'ai mis à jour la documentation correspondante
- [ ] J'ai ajouté des tests pour mes changements
- [ ] Tous les tests passent localement
- [ ] Je n'ai pas introduit de régression
- [ ] Mon code n'a pas de warnings
- [ ] Mon message de commit est clair et descriptif

## 🎓 Ressources utiles

- [Documentation .NET](https://docs.microsoft.com/dotnet/)
- [C# Coding Guidelines](https://docs.microsoft.com/en-us/dotnet/fundamentals/coding-style)
- [YARP Documentation](https://microsoft.github.io/reverse-proxy/)
- [Conventional Commits](https://www.conventionalcommits.org/)

## 💬 Questions ?

- Ouvrir une issue avec le label `question`
- Consulter les discussions existantes
- Contacter l'équipe maintainers

## 📝 Licence

En contribuant à DotnetNiger, vous acceptez que vos contributions soient sous licence MIT.

---

Merci de contribuer à DotnetNiger ! 🚀
