# Dotnet.Identity.Client

Interface statique (HTML/CSS/JS) pour piloter tous les endpoints exposés par `DotnetNiger.Identity`. Le client s'exécute côté navigateur et agit comme un mini-Postman spécialisé :

- sélection rapide des endpoints (groupés par domaine fonctionnel) avec pré-remplissage des chemins, paramètres et corps JSON ;
- gestion centralisée des secrets (Bearer token, refresh token, clé `X-API-Key`), injectés automatiquement selon le type d'endpoint ;
- support des uploads multipart pour l'avatar utilisateur ;
- visualisation des réponses (body + headers) avec temps d'exécution et taille ;
- génération d'une commande `curl` pour reproduire la requête dans un terminal (hors upload fichier).

## Prérequis

- Servir l'API Identity en local (par défaut: `http://localhost:5075`).
- Ouvrir cette UI via un serveur statique (extension VS Code « Live Server », `npx serve`, etc.). Un simple `file://` peut fonctionner mais certaines fonctionnalités (copie, fetch cross-origin) exigent `http://`.

## Démarrage rapide

```bash
cd Dotnet.Identity.Client
npx serve .
# puis visiter http://localhost:3000 (ou le port indiqué)
```

1. Renseignez la base URL, la version (`1.0`), vos tokens et/ou clé API.
2. Choisissez un endpoint dans la colonne de gauche ; les champs de la requête se mettent à jour.
3. Ajustez paramètres, headers et body si nécessaire puis cliquez sur « Envoyer la requête ».
4. Analysez la réponse dans l'onglet de droite ou copiez la commande `curl` pour vos scripts.

> ⚠️ Vérifiez la configuration CORS de l'API si vous servez l'UI sur un domaine différent.
