using DependencyInjection.ContainerConfiguration.Extensions;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DependencyInjection.ContainerConfiguration.ConsoleApp.Helpers
{
    public class ServiceCollectionHelper
    {
        #region "Variables"

        private static ServiceCollectionHelper _instance;
        private static readonly object _lockObject = new object();
        private static ServiceProvider _serviceProvider;

        #endregion "Variables"

        protected ServiceCollectionHelper()
        {
        }

        public IConfiguration Configuration { get; private set; }

        public static ServiceCollectionHelper Instance
        {
            get
            {
                if (_instance == null)
                {
                    lock (_lockObject)
                    {
                        if (_instance == null)
                            _instance = new ServiceCollectionHelper();
                    }
                }
                return _instance;
            }
        }

        /// <summary>
        /// Register all services here
        /// </summary>
        public void RegisterServices()
        {
            var serviceCollection = new ServiceCollection()
                // Do other common things here...
                .AddMemoryCache()
                // Register services based on configuration file
                .AddContainerConfiguration("./container.json");

            _serviceProvider = serviceCollection.BuildServiceProvider();
        }

        /// <summary>
        /// Method help get service from service collection
        /// </summary>
        /// <typeparam name="T">Service you want to get</typeparam>
        /// <returns>Service or NULL if not found</returns>
        public T GetService<T>()
        {
            return _serviceProvider.GetService<T>();
        }

        /// <summary>
        /// Dispose
        /// </summary>
        public void Dispose()
        {
            if (_serviceProvider is IDisposable)
            {
                ((IDisposable)_serviceProvider).Dispose();
            }
        }
    }
}