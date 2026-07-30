# Security

## Signalement d'une vulnérabilité

Si vous découvrez une faille de sécurité, **ne créez pas d'issue publique**.  
Contactez directement les maintainers par email à l'adresse de support.

Nous nous engageons à :
- Accuser réception sous 48 heures
- Fournir une estimation de correction
- Publier un correctif dans les plus brefs délais
- Mentionner le rapporteur dans les notes de version (sauf demande contraire)

## Pratiques de sécurité applicatives

### Authentification
- Authentification par **JWT** (JSON Web Tokens) avec tokens d'accès (60 min) et refresh tokens (7 jours)
- **OAuth2** via Google, GitHub et Microsoft pour les connexions sociales
- Mots de passe hashés avec **PBKDF2** (via ASP.NET Core Identity)
- Rate limiting strict sur les endpoints d'authentification (5 requêtes/min)

### Protection des données
- **CORS** restreint en production (domaines explicitement autorisés)
- **HTTPS** obligatoire en production
- **SQL Server** avec paramétrisation des requêtes (EF Core)
- **Uploads** : validation du type MIME et de la taille des fichiers
- Les emails transactionnels ne contiennent jamais de mots de passe en clair

### Configuration
- **Secrets jamais commit** : les mots de passe, clés API et chaînes de connexion sont exclus via `.gitignore`
- Variables d'environnement utilisées pour les secrets en production
- Fichier `appsettings.Production.json` et `.env` exclus du versionnement

### Infrastructure
- **Rate limiting** sur tous les endpoints (30 req/min par défaut, 10 pour les endpoints sensibles)
- **Validation** stricte des entrées utilisateur (DTOs avec Data Annotations)
- **Headers de sécurité** : anti-clickjacking, CSP, X-Content-Type-Options (via middleware)

## Bonnes pratiques pour les contributeurs

- Ne jamais commit de secrets, tokens, ou mots de passe
- Ne pas exposer d'informations sensibles dans les logs
- Utiliser les User Secrets .NET (`dotnet user-secrets`) pour le développement local
- Signer les commits avec GPG si possible
- Ouvrir les PR uniquement vers `dev`, jamais directement vers `master`
