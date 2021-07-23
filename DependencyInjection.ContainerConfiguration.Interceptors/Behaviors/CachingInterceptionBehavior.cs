using DependencyInjection.ContainerConfiguration.Interception;
using DependencyInjection.ContainerConfiguration.Interceptors.Attributes;
using Microsoft.Extensions.Caching.Memory;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Security.Cryptography;
using System.Text;

namespace DependencyInjection.ContainerConfiguration.Interceptors.Behaviors
{
    public class CachingInterceptionBehavior<T> : DispatchProxyBase<T, CachingInterceptionBehavior<T>>
    {
        // Variable has to be protected (not private)
        protected readonly IMemoryCache _cache;

        public CachingInterceptionBehavior()
        {
            // We need at least one parameterless constructor because DispatchProxy uses it to create the proxy
        }

        public CachingInterceptionBehavior(IMemoryCache _memoryCache)
        {
            // If we want to inject other services from the container, we need this other constructor next
            _cache = _memoryCache;
        }

        protected override object Invoke(MethodInfo targetMethod, object[] args)
        {
            var attr = targetMethod.GetCustomAttributes(typeof(CachingAttribute), true).FirstOrDefault() as CachingAttribute;
            if (attr == null)
            {
                // Attribute not found, so don't use cache, invoke the method itself
                Trace("Caching attribute NOT found!", ConsoleColor.Yellow);
                return targetMethod.Invoke(_original, args);
            }
            else
            {
                var cachingAttributeKey = attr.CacheKey;
                Trace($"Caching attribute found!", ConsoleColor.Yellow);

                string cacheKey = String.IsNullOrEmpty(cachingAttributeKey) ? this.GenerateCacheKey(targetMethod, args) : cachingAttributeKey;
                Trace($"Cache key: {cacheKey}", ConsoleColor.Cyan);
                return _cache.GetOrCreate(cacheKey, entry =>
                {
                    entry.SlidingExpiration = TimeSpan.FromSeconds(100);
                    Trace($"Not found in cache", ConsoleColor.Red);
                    return targetMethod.Invoke(_original, args);
                });
            }
        }

        /// <summary>
        /// Generates a hashed cache key with the call signature (silly implementation)
        /// </summary>
        /// <param name="input">Input context (parameters, etc...)</param>
        /// <param name="args">Parameters passed</param>
        /// <returns>Cache key name</returns>
        private string GenerateCacheKey(MethodInfo input, object[] args)
        {
            StringBuilder key = new StringBuilder();
            key.Append("_" + System.AppDomain.CurrentDomain.FriendlyName);
            key.Append("_" + typeof(T).FullName + "." + input.Name);
            foreach (ParameterInfo pi in input.GetParameters())
            {
                IEnumerable en = args[pi.Position] as IEnumerable; // To handle list parameters, etc
                if (en != null)
                {
                    key.Append("_[" + pi.Name);
                    foreach (object obj in args[pi.Position] as IEnumerable)
                    {
                        // Generate a key depending on length of attributes or whatever you want, etc...
                        key.Append(obj.ToString());
                    }
                    key.Append("]");
                }
                else
                {
                    key.Append("_[" + pi.Name + args[pi.Position].ToString() + "]");
                }
            }
            SHA256Managed man = new SHA256Managed();
            return Convert.ToBase64String(man.ComputeHash(Encoding.Default.GetBytes(key.ToString())));
        }

        private void Trace(string message, ConsoleColor color)
        {
            Console.ForegroundColor = color;
            Console.WriteLine(message);
            Console.ResetColor();
        }
    }
}