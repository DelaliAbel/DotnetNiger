namespace DotnetNiger.Gateway.Configuration;

/// <summary>Messages centralisés pour le Gateway.</summary>
public static class Messages
{
    public static class Auth
    {
        public const string OriginNotAllowed = "Origine non autorisée.";
        public const string InvalidJson = "Invalid JSON.";
        public const string AccessTokenRequired = "accessToken is required.";
        public const string TokensStored = "Tokens stockés dans des cookies sécurisés.";
        public const string TokensCleared = "Tokens supprimés.";
        public const string RefreshTokenMissing = "refresh_token manquant.";
        public const string RefreshTokenInvalid = "Refresh token invalide ou expiré.";
        public const string InvalidAuthResponse = "Réponse invalide du serveur d'authentification.";
        public const string RefreshError = "Erreur lors du rafraîchissement du token.";
    }

    public static class Proxy
    {
        public const string ServiceNotFound = "Service non trouvé ou inactif.";
        public const string UpstreamUnavailable = "Service amont indisponible.";
    }

    public static class Swagger
    {
        public const string DownstreamUnavailable = "Les documents Swagger aval sont indisponibles.";
        public const string MergeFailed = "Échec de la fusion Swagger.";
        public const string CacheCleared = "Cache Swagger vidé.";
    }

    public static class Registration
    {
        public const string InvalidJson = "Invalid JSON.";
        public const string IdAndUrlRequired = "Id et Url sont requis.";
    }

    public static class Ocelot
    {
        public const string MissingGlobalConfig = "Fichier ocelot.global.json manquant.";
        public const string InvalidGlobalJson = "JSON invalide dans ocelot.global.json.";
    }

    public static class Common
    {
        public const string InternalServerError = "Une erreur interne du serveur est survenue.";
    }
}
