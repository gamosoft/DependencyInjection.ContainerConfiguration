using DependencyInjection.ContainerConfiguration.ConsoleApp.Helpers;
using DependencyInjection.ContainerConfiguration.ConsoleApp.Interfaces;
using System;
using System.Collections.Generic;

namespace DependencyInjection.ContainerConfiguration.ConsoleApp
{
    public class Program
    {
        public static void Main(string[] args)
        {
            // Register all DI configuration first
            ServiceCollectionHelper.Instance.RegisterServices();

            // Get the service and run it
            var demo = ServiceCollectionHelper.Instance.GetService<IDemoInterface>();
            Console.WriteLine($"Value returned: {demo?.Run(2, new List<int> { 1, 2, 3 })}");
            Console.WriteLine($"Value returned: {demo?.Run(2, new List<int> { 1, 2, 3 })}");
        }
    }
}
