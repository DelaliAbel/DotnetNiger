
using Yarp.ReverseProxy.Configuration;

namespace DotnetNiger.Gateway.Infrastructure.Config
{
    public static class ClusterConfiguration
    {
        public static IReadOnlyList<ClusterConfig> GetClusters()
        {
            return new[]
            {
                new ClusterConfig
                {
                    ClusterId = "identity-cluster",
                    Destinations = new Dictionary<string, DestinationConfig>
                    {
                        {"identity-destination", new DestinationConfig {Address = "http://localhost:5075" } }
                    }
                },

                new ClusterConfig
                {
                    ClusterId = "community-cluster",
                    Destinations = new Dictionary<string, DestinationConfig>
                    {
                        {"community-destination", new DestinationConfig {Address = "http://localhost:5269" } }
                    }
                }
            };
        }
    }
}
