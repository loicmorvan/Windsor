// Copyright 2004-2011 Castle Project - http://www.castleproject.org/
// 
// Licensed under the Apache License, Version 2.0 (the "License");
// you may not use this file except in compliance with the License.
// You may obtain a copy of the License at
// 
//     http://www.apache.org/licenses/LICENSE-2.0
// 
// Unless required by applicable law or agreed to in writing, software
// distributed under the License is distributed on an "AS IS" BASIS,
// WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied.
// See the License for the specific language governing permissions and
// limitations under the License.

using System.Diagnostics;
using System.Reflection;
using Castle.Core.Configuration;
using Castle.Windsor.Core;
using Castle.Windsor.Core.Internal;
using Castle.Windsor.MicroKernel;
using Castle.Windsor.MicroKernel.Registration;
using Castle.Windsor.MicroKernel.SubSystems.Configuration;
using Castle.Windsor.MicroKernel.SubSystems.Conversion;

namespace Castle.Windsor.Windsor.Installer;

/// <summary>Default <see cref="IComponentsInstaller" /> implementation.</summary>
public sealed class DefaultComponentInstaller : IComponentsInstaller
{
    private string? _assemblyName;

    /// <summary>Perform installation.</summary>
    /// <param name="container">Target container</param>
    /// <param name="store">Configuration store</param>
    public void SetUp(IWindsorContainer container, IConfigurationStore store)
    {
        var converter = (IConversionManager)container.Kernel.GetSubSystem(SubSystemConstants.ConversionManagerKey);
        SetUpInstallers(store.GetInstallers(), container, converter);
        SetUpFacilities(store.GetFacilities(), container, converter);
        SetUpComponents(store.GetComponents(), container, converter);
    }

    private void SetUpInstallers(IConfiguration[] installers, IWindsorContainer container,
        IConversionManager converter)
    {
        var instances = new Dictionary<Type, IWindsorInstaller>();
        var assemblies = new HashSet<Assembly>();
        foreach (var installer in installers)
        {
            AddInstaller(installer, instances, converter, assemblies);
        }

        if (instances.Count != 0)
        {
            container.Install(instances.Values.ToArray());
        }
    }

    private void AddInstaller(IConfiguration installer, Dictionary<Type, IWindsorInstaller> cache,
        IConversionManager conversionManager, ICollection<Assembly> assemblies)
    {
        var typeName = installer.Attributes["type"];
        if (!string.IsNullOrEmpty(typeName))
        {
            var type = conversionManager.PerformConversion<Type>(typeName);
            AddInstaller(cache, type);
            return;
        }

        _assemblyName = installer.Attributes["assembly"];
        if (!string.IsNullOrEmpty(_assemblyName))
        {
            var assembly = ReflectionUtil.GetAssemblyNamed(_assemblyName);
            if (assemblies.Contains(assembly))
            {
                return;
            }

            assemblies.Add(assembly);

            GetAssemblyInstallers(cache, assembly);
            return;
        }

        var directory = installer.Attributes["directory"];
        var mask = installer.Attributes["fileMask"];
        var token = installer.Attributes["publicKeyToken"];
        Debug.Assert(directory != null);
        var assemblyFilter = new AssemblyFilter(directory, mask);
        if (token != null)
        {
            assemblyFilter.WithKeyToken(token);
        }

        foreach (var assembly in ReflectionUtil.GetAssemblies(assemblyFilter))
        {
            if (assemblies.Contains(assembly))
            {
                continue;
            }

            assemblies.Add(assembly);
            GetAssemblyInstallers(cache, assembly);
        }
    }

    private static void GetAssemblyInstallers(Dictionary<Type, IWindsorInstaller> cache, Assembly assembly)
    {
        var types = assembly.GetAvailableTypes();
        foreach (var type in InstallerTypes(types))
        {
            AddInstaller(cache, type);
        }
    }

    private static IEnumerable<Type> InstallerTypes(IEnumerable<Type> types)
    {
        return types.Where(IsInstaller);
    }

    private static bool IsInstaller(Type type)
    {
        return type.GetTypeInfo().IsClass &&
               !type.GetTypeInfo().IsAbstract &&
               !type.GetTypeInfo().IsGenericTypeDefinition &&
               type.Is<IWindsorInstaller>();
    }

    private static void AddInstaller(Dictionary<Type, IWindsorInstaller> cache, Type type)
    {
        if (cache.ContainsKey(type))
        {
            return;
        }

        var installerInstance = type.CreateInstance<IWindsorInstaller>();
        cache.Add(type, installerInstance);
    }

    private static void SetUpFacilities(IConfiguration[] configurations, IWindsorContainer container,
        IConversionManager converter)
    {
        foreach (var facility in configurations)
        {
            var type = converter.PerformConversion<Type>(facility.Attributes["type"]);
            var facilityInstance = type.CreateInstance<IFacility>();
            Debug.Assert(facilityInstance != null);

            container.AddFacility(facilityInstance);
        }
    }

    private static void AssertImplementsService(IConfiguration id, Type service, Type implementation)
    {
        if (service == null)
        {
            return;
        }

        if (service.GetTypeInfo().IsGenericTypeDefinition)
        {
            implementation = implementation.MakeGenericType(service.GetGenericArguments());
        }

        if (service.IsAssignableFrom(implementation))
        {
            return;
        }

        Debug.Assert(implementation != null);
        var message =
            $"Could not set up component '{id.Attributes["id"]}'. Type '{implementation.AssemblyQualifiedName}' does not implement service '{service.AssemblyQualifiedName}'";
        throw new ComponentRegistrationException(message);
    }

    private static void SetUpComponents(IConfiguration[] configurations, IWindsorContainer container,
        IConversionManager converter)
    {
        foreach (var component in configurations)
        {
            var implementation = GetType(converter, component.Attributes["type"]);
            var firstService = GetType(converter, component.Attributes["service"]);
            var services = new HashSet<Type>();
            if (firstService != null)
            {
                services.Add(firstService);
            }

            CollectAdditionalServices(component, converter, services);

            var name = default(string);
            if (implementation != null)
            {
                AssertImplementsService(component, firstService, implementation);
                var defaults = CastleComponentAttribute.GetDefaultsFor(implementation);
                if (defaults.ServicesSpecifiedExplicitly && services.Count == 0)
                {
                    defaults.Services.ForEach(s => services.Add(s));
                }

                name = GetName(defaults, component);
            }

            if (services.Count == 0 && implementation == null)
            {
                continue;
            }

            container.Register(Component.For(services).ImplementedBy(implementation).Named(name));
        }
    }

    private static string GetName(CastleComponentAttribute defaults, IConfiguration component)
    {
        return component.Attributes["id-automatic"] != bool.TrueString
            ? component.Attributes["id"]
            : defaults.Name;
    }

    private static Type GetType(IConversionManager converter, string typeName)
    {
        return typeName == null
            ? null
            : converter.PerformConversion<Type>(typeName);
    }

    private static void CollectAdditionalServices(IConfiguration component, IConversionManager converter,
        ICollection<Type> services)
    {
        var forwardedTypes = component.Children["forwardedTypes"];
        if (forwardedTypes == null)
        {
            return;
        }

        foreach (var forwardedServiceTypeName in forwardedTypes.Children.Select(forwardedType =>
                     forwardedType.Attributes["service"]))
        {
            try
            {
                services.Add(converter.PerformConversion<Type>(forwardedServiceTypeName));
            }
            catch (ConverterException e)
            {
                throw new ComponentRegistrationException(
                    $"Component {component.Attributes["id"]} defines invalid forwarded type.", e);
            }
        }
    }
}