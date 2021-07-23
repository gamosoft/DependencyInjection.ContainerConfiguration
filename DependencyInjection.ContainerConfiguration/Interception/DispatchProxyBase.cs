using System;
using System.Linq;
using System.Reflection;

namespace DependencyInjection.ContainerConfiguration.Interception
{
    /// <summary>
    /// Abstract class used to create interception behaviors
    /// </summary>
    /// <typeparam name="T">Interface type</typeparam>
    /// <typeparam name="K">Proxy type</typeparam>
    public abstract class DispatchProxyBase<T, K> : DispatchProxy
        where K : DispatchProxy
    {
        protected T _original;

        /// <summary>
        /// Method used to create the instance of the proxy
        /// </summary>
        /// <param name="original">Decorated instance</param>
        /// <param name="services">Optional services to inject</param>
        /// <returns>Proxy class decorated with behavior</returns>
        public static T Create(T original, params object[] services)
        {
            object proxy = Create<T, K>();
            ((DispatchProxyBase<T, K>)proxy).SetParameters(original, services);
            return (T)proxy;
        }

        private void SetParameters(T original, params object[] services)
        {
            _original = original ?? throw new ArgumentNullException(nameof(original));
            this.InjectServices(services);
        }

        private void InjectServices(params object[] services)
        {
            if (services == null || services.Length == 0)
                return;

            // Need to be protected
            // Chack all variables in the behavior for matching interfaces
            var fields = this.GetType().GetFields(BindingFlags.NonPublic | BindingFlags.Instance);
            foreach (var service in services)
            {
                var interfaces = service.GetType().GetInterfaces();
                var found = fields.FirstOrDefault(f => interfaces.Any(i => i.FullName == f.FieldType.FullName));
                if (found != null)
                    found.SetValue(this, service);
            }
        }
    }
}