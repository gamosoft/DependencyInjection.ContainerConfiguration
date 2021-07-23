using DependencyInjection.ContainerConfiguration.Configuration;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using System;
using System.IO;
using System.Linq;
using System.Reflection;

namespace DependencyInjection.ContainerConfiguration.Extensions
{
    public static class ServiceCollectionExtensions
    {
        private static readonly string _defaultContainerFileName = "container.json";
        private static readonly string _servicesSection = "Services";
        private static readonly string _dispatchProxyCreateMethodName = "Create";

        /// <summary>
        /// Add service configuration from a specified file
        /// </summary>
        /// <param name="services">Service collection</param>
        /// <param name="containerFileName">Optional path to the container information file, otherwise use the default 'container.json' file</param>
        /// <returns>Service collection</returns>
        public static IServiceCollection AddContainerConfiguration(this IServiceCollection services, string containerFileName = null)
        {
            var configuration = new ConfigurationBuilder()
                .AddJsonFile(containerFileName ?? _defaultContainerFileName, optional: true)
                .Build();

            return services
                .AddContainerConfiguration(configuration);
        }

        /// <summary>
        /// Add service configuration from a specified configuration
        /// </summary>
        /// <param name="services">Service collection</param>
        /// <param name="configuration">Services configuration</param>
        /// <returns>Service collection</returns>
        public static IServiceCollection AddContainerConfiguration(this IServiceCollection services, IConfiguration configuration)
        {
            var servicesConfiguration = configuration.GetSection(_servicesSection).Get<ServiceGroupConfiguration>();
            servicesConfiguration?.Singleton?.ToList().ForEach(service => services.AddSingleton(Type.GetType(service.Interface), (sp) => RegistrationFactoryWithInterception(service, sp)));
            servicesConfiguration?.Transient?.ToList().ForEach(service => services.AddTransient(Type.GetType(service.Interface), (sp) => RegistrationFactoryWithInterception(service, sp)));
            servicesConfiguration?.Scoped?.ToList().ForEach(service => services.AddScoped(Type.GetType(service.Interface), (sp) => RegistrationFactoryWithInterception(service, sp)));
            return services;
        }

        private static object RegistrationFactoryWithInterception(ServiceConfiguration service, IServiceProvider sp)
        {
            var myInterface = Type.GetType(service.Interface);
            var myType = Type.GetType(service.Implementation);
            var myConst = myType.GetConstructors().First();
            // Figure out parameters of the DI constructor for the concrete class
            var injectedServices = myConst.GetParameters().Select(p => sp.GetRequiredService(p.ParameterType)).ToArray();
            var instance = ActivatorUtilities.CreateInstance(sp, myType, injectedServices);
            service.InterceptionBehaviors?.ToList().ForEach(b =>
            {
                var genericBehavior = Type.GetType(b);
                var concreteBehavior = genericBehavior.MakeGenericType(myInterface);
                var behaviorConst = concreteBehavior.GetConstructors().Last(); // Get the parameterized one (if any)
                // Get services to inject in the behavior
                var behaviorServices = behaviorConst.GetParameters().Select(p => sp.GetRequiredService(p.ParameterType)).ToArray();
                var concreteInstance = ActivatorUtilities.CreateInstance(sp, concreteBehavior, behaviorServices);
                // Find Create() method and invoke
                MethodInfo createMethod = GetCreateMethod(concreteBehavior);
                instance = createMethod.Invoke(concreteInstance, new[] { instance, behaviorServices });
            });
            return instance;
        }

        private static MethodInfo GetCreateMethod(Type type)
        {
            // Can be declared in the same class or anywhere up in the hierarchy
            var result = type.GetMethod(_dispatchProxyCreateMethodName);
            if (result == null && type.BaseType != null)
                result = GetCreateMethod(type.BaseType);
            if (result == null)
                throw new Exception($"Interception behavior can't locate '{_dispatchProxyCreateMethodName}' method. Make sure it implements 'DispatchProxyBase' class.");
            return result;
        }
    }
}