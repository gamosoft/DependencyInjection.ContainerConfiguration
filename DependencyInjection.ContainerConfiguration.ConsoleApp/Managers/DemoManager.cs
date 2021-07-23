using DependencyInjection.ContainerConfiguration.ConsoleApp.Interfaces;
using System;
using System.Collections.Generic;

namespace DependencyInjection.ContainerConfiguration.ConsoleApp.Managers
{
    public class DemoManager : IDemoInterface
    {
        public int Run(int value, List<int> more)
        {
            Console.WriteLine("Actual work");
            return 15 * value;
        }
    }
}