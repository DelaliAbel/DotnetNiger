using Yarp.ReverseProxy.Configuration;


namespace DotnetNiger.Gateway.Infrastructure.Config
{
    public static class RouteConfiguration
    {
        public static IReadOnlyList<RouteConfig> GetRoutes()
        {
            return new[]
            {
                new RouteConfig
                {
                    RouteId = "identity-route",
                    ClusterId = "identity-cluster",
                    Match = new RouteMatch
                    {
                        Path = "/identity/{**catch-all}"
                    },
                    Transforms = new List<IReadOnlyDictionary<string, string>>
                    {
                        new Dictionary<string, string>
                        {

                            {"PathPattern", "{**catch-all}" }
                        }
                    }
                },
                 new RouteConfig
                {
                    RouteId = "identity-swagger",
                    ClusterId = "identity-cluster",
                    Match = new RouteMatch
                    {
                        Path = "swagger/identity/{**catch-all}"
                    },
                    Transforms = new List<IReadOnlyDictionary<string, string>>
                    {
                        new Dictionary<string, string>
                        {
                            {"PathPattern", "/swagger/{**catch-all}" }

                        }
                    }
                },

                new RouteConfig
                {
                    RouteId = "community-route",
                    ClusterId = "community-cluster",
                    Match = new RouteMatch
                    {
                        Path = "/community/{**catch-all}"
                    },
                    Transforms = new List<IReadOnlyDictionary<string, string>>
                    {
                        new Dictionary<string, string>
                        {

                            {"PathPattern", "{**catch-all}" }
                        }
                    }
                },
                 new RouteConfig
                {
                    RouteId = "community-swagger",
                    ClusterId = "community-cluster",
                    Match = new RouteMatch
                    {
                        Path = "swagger/community/{**catch-all}"
                    },
                    Transforms = new List<IReadOnlyDictionary<string, string>>
                    {
                        new Dictionary<string, string>
                        {
                            {"PathPattern", "/swagger/{**catch-all}" }

                        }
                    }
                },
                // Route pour les fichiers uploadés (Identity)
                new RouteConfig
                {
                    RouteId = "identity-uploads",
                    ClusterId = "identity-cluster",
                    Match = new RouteMatch
                    {
                        Path = "/uploads/{**catch-all}"
                    },
                    Transforms = new List<IReadOnlyDictionary<string, string>>
                    {
                        new Dictionary<string, string>
                        {
                            {"PathPattern", "/uploads/{**catch-all}" }
                        }
                    }
                }
            };
        }
    }
}
