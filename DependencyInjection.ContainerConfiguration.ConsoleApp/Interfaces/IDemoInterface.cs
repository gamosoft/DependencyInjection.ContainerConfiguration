using DependencyInjection.ContainerConfiguration.Interceptors.Attributes;
using System.Collections.Generic;

namespace DependencyInjection.ContainerConfiguration.ConsoleApp.Interfaces
{
    public interface IDemoInterface
    {
        [Caching(CacheKey = "someKey")]
        int Run(int value, List<int> more);
    }
}