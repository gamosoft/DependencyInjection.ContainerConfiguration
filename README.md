# DependencyInjection.ContainerConfiguration

This package adds extensions to **IServiceCollection** to automatically register all services defined in an external configuration _json_ file, with the possibility of injecting interception behaviors.

I was a huge fan of **Enterprise Library** and **Unity** back in the day, and as an exercise I created this package that allows you to define mappings of services/interfaces to be added to your **ServiceCollection** based on configuration.

As a bonus, this package also lets you use interception behaviors (based on **DispatchProxy**) so you can decorate and extend functionality without having to alter the original code.

Icon made by Freepik (https://www.freepik.com) from Flaticon (https://www.flaticon.com/)

## Installation
Simply add the package using your client of choice, such as the command line:

```
dotnet add package DependencyInjection.ContainerConfiguration
```

## Usage
First you need to create a _container.json_ configuration file with the mappings. Basically it's a simple file with the **Services** element, which contains **Transient**, **Singleton** and **Scoped** mappings.

Each mapping needs an **Interface** and the actual **Implementation** to be mapped to:

```json
{
  "Services": {
    "Transient": [
      {
        "Interface": "DependencyInjection.ContainerConfiguration.ConsoleApp.Interfaces.IDemoInterface, DependencyInjection.ContainerConfiguration.ConsoleApp",
        "Implementation": "DependencyInjection.ContainerConfiguration.ConsoleApp.Managers.DemoManager, DependencyInjection.ContainerConfiguration.ConsoleApp"
      }
    ],
    "Singleton": [],
    "Scoped": []
  }
}
```

Next make sure you register the services by calling the *AddContainerConfiguration* extension:

```csharp
var serviceCollection = new ServiceCollection()
    // Do other common things here...
    .AddMemoryCache()
    // Register services based on configuration file
    .AddContainerConfiguration();
```

And that's pretty much it, just get the service you want and invoke it!

```csharp
var mgr = _serviceProvider.GetService<IDemoInterface>();
Console.WriteLine($"Value returned: {mgr?.Run(2, new List<int> { 1, 2, 3 })}");
```

> _Note: Just resolve how you would typically would do, in the sample source I have added a helper class_

You can also call the _AddContainerConfiguration_ extension method with the path to a different container file (instead of _container.json_ in the running folder) or even with an existing **IConfiguration** object that has to adhere to the schema above.

## Extending with interception behaviors
If you want to add cross cutting concerns to your services on a configuration based form, you can also use this package.

The approach used is based on the **Decorator** pattern using **DispatchProxy**. Create your class and make it implement the abstract **DispatchProxyBase** class.

Next ensure the *Invoke* method does what you want (you may choose not to call the next method in the pipeline, etc):

```csharp
public class MyCustomBehavior<T> : DispatchProxyBase<T, MyCustomBehavior<T>>
{
    protected override object Invoke(MethodInfo targetMethod, object[] args)
    {
        // ######################
        // Do something before...
        // ######################

        // Actual method invocation (or delegate to next interceptor)
        var result = targetMethod.Invoke(_original, args);

        // #####################
        // Do something after...
        // #####################

        return result; // Return original result, or something else, etc...
    }
}
```

Finally going back to the configuration file, after each registered service simply add an array of **InterceptionBehaviors**. The order matters as they will be invoked the way they are found in the collection.


```json
{
  "Services": {
    "Transient": [
      {
        "Interface": "DependencyInjection.ContainerConfiguration.ConsoleApp.Interfaces.IDemoInterface, DependencyInjection.ContainerConfiguration.ConsoleApp",
        "Implementation": "DependencyInjection.ContainerConfiguration.ConsoleApp.Managers.DemoManager, DependencyInjection.ContainerConfiguration.ConsoleApp",
        "InterceptionBehaviors": [
          "DependencyInjection.ContainerConfiguration.Interceptors.Behaviors.ConsoleLoggingInterceptionBehavior`1, DependencyInjection.ContainerConfiguration.Interceptors",
          "DependencyInjection.ContainerConfiguration.Interceptors.Behaviors.CachingInterceptionBehavior`1, DependencyInjection.ContainerConfiguration.Interceptors"
        ]
      }
    ],
    "Singleton": [],
    "Scoped": []
  }
}
```

> _Please note the class name with the **Generics** notation:
(DependencyInjection.ContainerConfiguration.Interceptors.Behaviors.ConsoleLoggingInterceptionBehavior`1_)

> _There are a couple of behaviors included in the source (caching and logging) to give you an example of what you can do._

## Notes
This may not be the most efficient way but it was a lot of fun developing and serves the purposes for most of the scenarios. Also, full source code is included so feel free to modify/extend at your heart's desire. ;-)

Due to the nature of **DispatchProxy**, if you wish to inject services from the DI container into an interception behavior you have to create an empty constructor (used by **DispatchProxy**) and **AFTERWARDS** a constructor with the parameters to be injected, like in the example for the caching behavior:

```csharp
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
        ...
```

I hope you enjoy using this!