using System.Collections.Generic;

namespace DependencyInjection.ContainerConfiguration.Configuration
{
    public class ServiceGroupConfiguration
    {
        public IEnumerable<ServiceConfiguration> Singleton { get; set; }
        public IEnumerable<ServiceConfiguration> Transient { get; set; }
        public IEnumerable<ServiceConfiguration> Scoped { get; set; }
    }
}