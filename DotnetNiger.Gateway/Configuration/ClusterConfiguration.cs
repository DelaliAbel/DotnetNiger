using DotnetNiger.Gateway.Infrastructure.HttpClients;
using Yarp.ReverseProxy.Configuration;

namespace DotnetNiger.Gateway.Configuration
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
