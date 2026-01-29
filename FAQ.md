# FAQ - Questions Fréquemment Posées

## 🚀 Démarrage

### Q: Comment démarrer le projet rapidement ?
**A:** Suivez [SETUP.md](./SETUP.md) pour les instructions détaillées. En résumé :
```bash
git clone <repo>
cd DotnetNiger
dotnet restore
docker-compose up
```

### Q: Quelle version de .NET est requise ?
**A:** .NET 9 SDK ou supérieur. Vérifiez avec `dotnet --version`.

### Q: Comment accéder à la documentation API ?
**A:** Après démarrage, consultez :
- Gateway: http://localhost:5000/swagger
- Identity: http://localhost:5075/swagger
- Community: http://localhost:5269/swagger

## 🔐 Authentification

### Q: Comment obtenir un token JWT ?
**A:** 
1. Créer un utilisateur via `/auth/register`
2. Se connecter via `/auth/login` pour obtenir le token
3. Utiliser le token dans l'header `Authorization: Bearer <token>`

### Q: Quelle est la durée de validité du token ?
**A:** Par défaut 60 minutes. Configurable dans `appsettings.json`.

### Q: Comment utiliser les refresh tokens ?
**A:** Appeler `/auth/refresh` avec le refresh token pour obtenir un nouveau access token.

## 📊 Base de données

### Q: Comment appliquer les migrations ?
**A:** 
```bash
dotnet ef database update
```

### Q: Comment créer une nouvelle migration ?
**A:**
```bash
dotnet ef migrations add DescriptionDeMigration
dotnet ef database update
```

### Q: Où sont stockées les données ?
**A:** SQL Server (localhost:1433 ou Docker). Configuration dans `appsettings.json`.

## 🐳 Docker

### Q: Docker n'est pas en cours d'exécution ?
**A:** 
- Installer [Docker Desktop](https://www.docker.com/products/docker-desktop)
- Démarrer Docker Desktop
- Réessayer `docker-compose up`

### Q: Erreur : "Port 5000 already in use" ?
**A:**
```bash
# Trouver le processus
netstat -ano | findstr :5000

# Le terminer
taskkill /PID <PID> /F

# Ou modifier le port dans docker-compose.yml
```

### Q: Comment voir les logs du conteneur ?
**A:**
```bash
docker-compose logs -f gateway
```

## 💾 Cache

### Q: Comment vider le cache Redis ?
**A:**
```bash
docker-compose exec redis redis-cli FLUSHALL
```

### Q: Où se fait le cache ?
**A:** 
- L1: Application Memory (MemoryCache)
- L2: Redis (distributed)

## 🧪 Tests

### Q: Comment exécuter les tests ?
**A:**
```bash
dotnet test
dotnet test DotnetNiger.Gateway
```

### Q: Comment ajouter des tests ?
**A:** Créer une classe `.Tests.cs` en suivant les patterns xUnit/MSTest.

## 🔍 Débogage

### Q: Comment déboguer en Visual Studio ?
**A:**
1. Mettre des breakpoints
2. Appuyer F5
3. Naviguer sur l'endpoint

### Q: Comment voir les logs détaillés ?
**A:** Modifier `appsettings.Development.json`:
```json
"LogLevel": {
  "Default": "Debug"
}
```

## 🚀 Déploiement

### Q: Où déployer ?
**A:** Azure, AWS, GCP ou on-premise. Voir [DEPLOYMENT.md](./DEPLOYMENT.md).

### Q: Comment déployer en production ?
**A:**
```bash
docker-compose -f docker-compose.prod.yml up -d
```

### Q: Est-ce que les certificats SSL sont inclus ?
**A:** Non, utiliser Let's Encrypt ou Azure Certificates.

## 👥 Contribution

### Q: Comment puis-je contribuer ?
**A:** Voir [CONTRIBUTING.md](./CONTRIBUTING.md) pour les directives détaillées.

### Q: Quelle est la politique de code review ?
**A:** 2 approvals minimales + tests + pas de warning.

### Q: Puis-je modifier la structure des dossiers ?
**A:** Consultez l'équipe avant des changements majeurs.

## 📈 Performance

### Q: Comment optimiser les performances ?
**A:**
- Utiliser le cache Redis
- Indexer les colonnes BD fréquentes
- Async/await patterns
- Query optimization (EF Core)

### Q: Pourquoi c'est lent ?
**A:** Vérifier les métriques Prometheus, les logs, et les requêtes BD.

### Q: Comment monitorer les performances ?
**A:** 
- Prometheus: http://localhost:5000/metrics
- Grafana: Créer un dashboard
- ELK: Logs centralisés

## 🔒 Sécurité

### Q: Comment signaler une vulnérabilité ?
**A:** Voir [SECURITY.md](./SECURITY.md).

### Q: Où stocker les secrets ?
**A:** Utiliser un gestionnaire de secrets, pas en dur dans le code.

### Q: HTTPS est obligatoire ?
**A:** En production oui. En dev optionnel.

## 📚 Documentation

### Q: Où est la documentation API complète ?
**A:** Voir [API.md](./API.md).

### Q: Où est l'architecture expliquée ?
**A:** Voir [ARCHITECTURE.md](./ARCHITECTURE.md).

### Q: Comment contribuer à la documentation ?
**A:** Modifier les fichiers .md et soumettre une PR.

## 🆘 Support

### Q: Où obtenir de l'aide ?
**A:**
1. Consulter la FAQ (ce fichier)
2. Lire la documentation pertinente
3. Ouvrir une issue sur GitHub
4. Contacter l'équipe support

### Q: Comment signaler un bug ?
**A:** Ouvrir une issue avec titre clair, description, steps to reproduce et logs.

### Q: Quelle est la SLA de support ?
**A:** Community support. Pour support professionnel, contactez l'équipe.

## 📝 Changelog

### Q: Où voir les changements récents ?
**A:** Voir [CHANGELOG.md](./CHANGELOG.md).

### Q: Quelle est la politique de versioning ?
**A:** [Semantic Versioning](https://semver.org/) - MAJOR.MINOR.PATCH.

---

## Besoin d'aide ?

Si votre question n'est pas répondue ici :

1. **Chercher** dans les issues GitHub existantes
2. **Consulter** la documentation pertinente
3. **Ouvrir** une nouvelle issue avec le label `question`
4. **Contacter** l'équipe de support

---

**Dernière mise à jour**: 29 Janvier 2026
