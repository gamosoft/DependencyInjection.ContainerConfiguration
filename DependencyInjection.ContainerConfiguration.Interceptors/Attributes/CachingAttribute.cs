using System;

namespace DependencyInjection.ContainerConfiguration.Interceptors.Attributes
{
    /// <summary>
    /// Caching attribute to be used declaratively in the methods
    /// This attribute needs to be applied to method declaration on the interface
    /// </summary>
    [AttributeUsage(AttributeTargets.Method, AllowMultiple = false)]
    public class CachingAttribute : Attribute
    {
        #region "Properties"

        /// <summary>
        /// Optional cache key. If present, will override automatic generation of cache key in the behavior
        /// </summary>
        public string CacheKey { get; set; }

        #endregion "Properties"
    }
}