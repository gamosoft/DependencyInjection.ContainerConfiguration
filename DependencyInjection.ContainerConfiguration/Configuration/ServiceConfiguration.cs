using System.Collections.Generic;

namespace DependencyInjection.ContainerConfiguration.Configuration
{
    public class ServiceConfiguration
    {
        public string Interface { get; set; }
        public string Implementation { get; set; }
        public IEnumerable<string> InterceptionBehaviors { get; set; }
    }
}