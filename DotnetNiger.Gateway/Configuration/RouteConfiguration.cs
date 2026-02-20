using Yarp.ReverseProxy.Configuration;


namespace DotnetNiger.Gateway.Configuration
{
    public static class RouteConfiguration
    {
        public static IReadOnlyList<RouteConfig> GetRoutes()
        {
            return new[]
            {
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
                }
            };
        }
    }
}
