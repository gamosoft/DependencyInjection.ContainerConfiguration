using DependencyInjection.ContainerConfiguration.Interception;
using System;
using System.Diagnostics;
using System.Reflection;

namespace DependencyInjection.ContainerConfiguration.Interceptors.Behaviors
{
    public class ConsoleLoggingInterceptionBehavior<T> : DispatchProxyBase<T, ConsoleLoggingInterceptionBehavior<T>>
    {
        protected override object Invoke(MethodInfo targetMethod, object[] args)
        {
            try
            {
                Stopwatch sw = Stopwatch.StartNew();
                // BEFORE the target method execution
                Trace($"[{this.GetType().Name}:Invoke]", ConsoleColor.DarkGreen);
                Trace($"{targetMethod.Name}", ConsoleColor.DarkGreen);

                // Actual method invocation (or delegate to next interceptor)
                var result = targetMethod.Invoke(_original, args);

                // AFTER the target method execution
                Trace($"Successfully finished {targetMethod.Name}", ConsoleColor.Cyan);
                sw.Stop();
                Trace($"Total running time: {sw.Elapsed}", ConsoleColor.White);
                return result;
            }
            catch (Exception ex) when (ex is TargetInvocationException)
            {
                Trace($"Finished {targetMethod.Name} with exception {ex.GetType()}: {ex.Message}", ConsoleColor.Red);
                throw ex.InnerException ?? ex;
            }
        }

        private void Trace(string message, ConsoleColor color)
        {
            Console.ForegroundColor = color;
            Console.WriteLine(message);
            Console.ResetColor();
        }
    }
}