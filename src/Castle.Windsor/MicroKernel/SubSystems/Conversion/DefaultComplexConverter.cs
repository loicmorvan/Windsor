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
using System.Globalization;
using System.Reflection;
using Castle.Core.Configuration;
using Castle.Windsor.Core.Internal;

namespace Castle.Windsor.MicroKernel.SubSystems.Conversion;

[Serializable]
public class DefaultComplexConverter(ITypeConverterContext context) : AbstractTypeConverter(context)
{
    private IConversionManager? _conversionManager;

    /// <summary>Gets the conversion manager.</summary>
    /// <value>The conversion manager.</value>
    private IConversionManager ConversionManager => _conversionManager ??= Context.Kernel.GetConversionManager();

    /// <summary>Creates the target type instance.</summary>
    /// <param name="type">The type.</param>
    /// <param name="configuration">The configuration.</param>
    /// <returns></returns>
    private object CreateInstance(Type type, IConfiguration configuration)
    {
        type = ObtainImplementation(type, configuration);

        var constructor = ChooseConstructor(type);

        object[]? args = null;
        if (constructor != null)
        {
            args = ConvertConstructorParameters(constructor, configuration);
        }

        var instance = type.CreateInstance<object>(args);
        return instance;
    }

    private Type ObtainImplementation(Type type, IConfiguration configuration)
    {
        var typeNode = configuration.Attributes["type"];

        if (string.IsNullOrEmpty(typeNode))
        {
            return type.GetTypeInfo().IsInterface
                ? throw new ConverterException("A type attribute must be specified for interfaces")
                : type;
        }

        var implType = Context.Composition.PerformConversion<Type>(typeNode);
        if (type.IsAssignableFrom(implType))
        {
            return implType;
        }

        Debug.Assert(implType != null);
        var message = $"Type {implType.FullName} is not assignable to {type.FullName}";

        throw new ConverterException(message);
    }

    /// <summary>
    ///     Chooses the first non default constructor. Throws an exception if more than one non default constructor is
    ///     found
    /// </summary>
    /// <param name="type"></param>
    /// <returns>The chosen constructor, or <c>null</c> if none was found</returns>
    private static ConstructorInfo? ChooseConstructor(Type type)
    {
        ConstructorInfo? chosen = null;
        var constructors = type.GetConstructors();
        foreach (var candidate in constructors)
        {
            if (candidate.GetParameters().Length == 0)
            {
                continue;
            }

            if (chosen != null)
            {
                throw new ConverterException("Classes with more than one non-default constructor are not supported.");
            }

            chosen = candidate;
        }

        return chosen;
    }

    /// <summary>Converts the constructor parameters.</summary>
    /// <param name="constructor">The constructor.</param>
    /// <param name="configuration">The configuration.</param>
    /// <returns></returns>
    private object[] ConvertConstructorParameters(ConstructorInfo constructor, IConfiguration configuration)
    {
        var parameters = constructor.GetParameters();
        var parameterValues = new object[parameters.Length];

        for (int i = 0, n = parameters.Length; i < n; ++i)
        {
            var parameter = parameters[i];

            var paramConfig = FindChildIgnoreCase(configuration, parameter.Name);
            if (paramConfig == null)
            {
                throw new ConverterException($"Child '{parameter.Name}' missing in {configuration.Name} element.");
            }

            var paramType = parameter.ParameterType;
            if (!ConversionManager.CanHandleType(paramType))
            {
                throw new ConverterException(
                    $"No converter found for child '{parameter.Name}' in {configuration.Name} element (type: {paramType.Name}).");
            }

            parameterValues[i] = ConvertChildParameter(paramConfig, paramType);
        }

        return parameterValues;
    }

    /// <summary>Converts the property values.</summary>
    /// <param name="instance">The instance.</param>
    /// <param name="type">The type.</param>
    /// <param name="configuration">The configuration.</param>
    private void ConvertPropertyValues(object instance, Type type, IConfiguration configuration)
    {
        foreach (var propConfig in configuration.Children)
        {
            var property =
                type.GetProperty(propConfig.Name,
                    BindingFlags.Instance | BindingFlags.Public | BindingFlags.IgnoreCase);
            if (property == null || !property.CanWrite)
            {
                continue;
            }

            var propType = property.PropertyType;
            if (!ConversionManager.CanHandleType(propType))
            {
                throw new ConverterException(
                    $"No converter found for child '{property.Name}' in {configuration.Name} element (type: {propType.Name}).");
            }

            var val = ConvertChildParameter(propConfig, propType);
            property.SetValue(instance, val, null);
        }
    }

    private object ConvertChildParameter(IConfiguration config, Type type)
    {
        var configValue = config.Value;
        if (configValue == null && config.Children.Count != 0)
        {
            return Context.Composition.PerformConversion(config, type);
        }

        return Context.Composition.PerformConversion(configValue ?? throw new InvalidOperationException(), type);
    }

    /// <summary>Finds the child (case insensitive).</summary>
    /// <param name="config">The config.</param>
    /// <param name="name">The name.</param>
    /// <returns></returns>
    private static IConfiguration? FindChildIgnoreCase(IConfiguration config, string? name)
    {
        return config.Children.FirstOrDefault(child =>
            CultureInfo.CurrentCulture.CompareInfo.Compare(child.Name, name, CompareOptions.IgnoreCase) == 0);
    }

    #region ITypeConverter Member

    public override bool CanHandleType(Type type)
    {
        return !type.GetTypeInfo().IsPrimitive;
    }

    public override object PerformConversion(IConfiguration configuration, Type targetType)
    {
        var instance = CreateInstance(targetType, configuration);
        ConvertPropertyValues(instance, targetType, configuration);

        return instance;
    }

    public override object PerformConversion(string value, Type targetType)
    {
        throw new NotImplementedException();
    }

    #endregion
}