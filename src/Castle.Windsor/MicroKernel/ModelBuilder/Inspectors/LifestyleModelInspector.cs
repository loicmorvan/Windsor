// Copyright 2004-2012 Castle Project - http://www.castleproject.org/
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

using System.Reflection;
using Castle.Windsor.Core;
using Castle.Windsor.Core.Internal;
using Castle.Windsor.MicroKernel.Lifestyle.Scoped;
using Castle.Windsor.MicroKernel.SubSystems.Conversion;

namespace Castle.Windsor.MicroKernel.ModelBuilder.Inspectors;

/// <summary>
///     Inspects the component configuration and the type looking for a definition of lifestyle type. The
///     configuration preceeds whatever is defined in the component.
/// </summary>
/// <remarks>
///     This inspector is not guarantee to always set up an lifestyle type. If nothing could be found it wont touch the
///     model. In this case is up to the kernel to establish a default lifestyle for
///     components.
/// </remarks>
[Serializable]
public class LifestyleModelInspector : IContributeComponentModelConstruction
{
    private readonly IConversionManager _converter;

    public LifestyleModelInspector(IConversionManager converter)
    {
        _converter = converter;
    }

    /// <summary>
    ///     Searches for the lifestyle in the configuration and, if unsuccessful look for the lifestyle attribute in the
    ///     implementation type.
    /// </summary>
    public virtual void ProcessModel(IKernel kernel, ComponentModel model)
    {
        if (!ReadLifestyleFromConfiguration(model))
        {
            ReadLifestyleFromType(model);
        }
    }

    /// <summary>
    ///     Reads the attribute "lifestyle" associated with the component configuration and tries to convert to
    ///     <see cref="LifestyleType" /> enum type.
    /// </summary>
    protected virtual bool ReadLifestyleFromConfiguration(ComponentModel model)
    {
        if (model.Configuration == null)
        {
            return false;
        }

        var lifestyleRaw = model.Configuration.Attributes["lifestyle"];
        if (lifestyleRaw != null)
        {
            var lifestyleType = _converter.PerformConversion<LifestyleType>(lifestyleRaw);
            model.LifestyleType = lifestyleType;
            switch (lifestyleType)
            {
                case LifestyleType.Singleton:
                case LifestyleType.Transient:
                case LifestyleType.Thread:
                    return true;
                case LifestyleType.Pooled:
                    ExtractPoolConfig(model);
                    return true;
                case LifestyleType.Custom:
                    var lifestyle = GetMandatoryTypeFromAttribute(model, "customLifestyleType", lifestyleType);
                    ValidateTypeFromAttribute(lifestyle, typeof(ILifestyleManager), "customLifestyleType");
                    model.CustomLifestyle = lifestyle;

                    return true;
                case LifestyleType.Scoped:
                    var scopeAccessorType = GetTypeFromAttribute(model, "scopeAccessorType");
                    if (scopeAccessorType == null)
                    {
                        return true;
                    }

                    ValidateTypeFromAttribute(scopeAccessorType, typeof(IScopeAccessor), "scopeAccessorType");
                    model.ExtendedProperties[Constants.ScopeAccessorType] = scopeAccessorType;

                    return true;
                case LifestyleType.Bound:
                    var binderType = GetTypeFromAttribute(model, "scopeRootBinderType");
                    if (binderType == null)
                    {
                        return true;
                    }

                    var binder = ExtractBinder(binderType, model.Name);
                    model.ExtendedProperties[Constants.ScopeRootSelector] = binder;

                    return true;
                default:
                    throw new InvalidOperationException(
                        $"Component {model.Name} has {lifestyleType} lifestyle. This is not a valid value.");
            }
        }

        {
            // type was not present, but we might figure out the lifestyle based on presence of some attributes related to some lifestyles
            var binderType = GetTypeFromAttribute(model, "scopeRootBinderType");
            if (binderType != null)
            {
                var binder = ExtractBinder(binderType, model.Name);
                model.ExtendedProperties[Constants.ScopeRootSelector] = binder;
                model.LifestyleType = LifestyleType.Bound;
                return true;
            }

            var scopeAccessorType = GetTypeFromAttribute(model, "scopeAccessorType");
            if (scopeAccessorType != null)
            {
                ValidateTypeFromAttribute(scopeAccessorType, typeof(IScopeAccessor), "scopeAccessorType");
                model.ExtendedProperties[Constants.ScopeAccessorType] = scopeAccessorType;
                model.LifestyleType = LifestyleType.Scoped;
                return true;
            }

            var customLifestyleType = GetTypeFromAttribute(model, "customLifestyleType");
            if (customLifestyleType == null)
            {
                return false;
            }

            ValidateTypeFromAttribute(customLifestyleType, typeof(ILifestyleManager), "customLifestyleType");
            model.CustomLifestyle = customLifestyleType;
            model.LifestyleType = LifestyleType.Custom;
            return true;
        }
    }

    /// <summary>Check if the type expose one of the lifestyle attributes defined in Castle.Model namespace.</summary>
    protected virtual void ReadLifestyleFromType(ComponentModel model)
    {
        var attributes = model.Implementation.GetAttributes<LifestyleAttribute>(true);
        if (attributes.Length == 0)
        {
            return;
        }

        var attribute = attributes[0];
        model.LifestyleType = attribute.Lifestyle;

        switch (model.LifestyleType)
        {
            case LifestyleType.Custom:
            {
                var custom = (CustomLifestyleAttribute)attribute;
                ValidateTypeFromAttribute(custom.CustomLifestyleType, typeof(ILifestyleManager), "CustomLifestyleType");
                model.CustomLifestyle = custom.CustomLifestyleType;
                break;
            }
            case LifestyleType.Pooled:
            {
                var pooled = (PooledAttribute)attribute;
                model.ExtendedProperties[ExtendedPropertiesConstants.PoolInitialPoolSize] = pooled.InitialPoolSize;
                model.ExtendedProperties[ExtendedPropertiesConstants.PoolMaxPoolSize] = pooled.MaxPoolSize;
                break;
            }
            case LifestyleType.Bound:
            {
                var binder = ExtractBinder(((BoundToAttribute)attribute).ScopeRootBinderType, model.Name);
                model.ExtendedProperties[Constants.ScopeRootSelector] = binder;
                break;
            }
            case LifestyleType.Scoped:
            {
                var scoped = (ScopedAttribute)attribute;
                if (scoped.ScopeAccessorType == null)
                {
                    return;
                }

                ValidateTypeFromAttribute(scoped.ScopeAccessorType, typeof(IScopeAccessor), "ScopeAccessorType");
                model.ExtendedProperties[Constants.ScopeAccessorType] = scoped.ScopeAccessorType;
                break;
            }
        }
    }

    protected virtual void ValidateTypeFromAttribute(Type typeFromAttribute, Type expectedInterface, string attribute)
    {
        if (expectedInterface.IsAssignableFrom(typeFromAttribute))
        {
            return;
        }

        throw new InvalidOperationException(
            $"The Type '{typeFromAttribute?.FullName}' specified in the '{attribute}' attribute must implement {expectedInterface.FullName}");
    }

    private Func<IHandler[], IHandler> ExtractBinder(Type scopeRootBinderType, string name)
    {
        var filterMethod =
            scopeRootBinderType.GetTypeInfo().FindMembers(MemberTypes.Method,
                    BindingFlags.Instance | BindingFlags.Public, IsBindMethod, null)
                .FirstOrDefault();
        if (filterMethod == null)
        {
            throw new InvalidOperationException(
                string.Format(
                    "Type {0} which was designated as 'scopeRootBinderType' for component {1} does not have any public instance method matching signature of 'IHandler Method(IHandler[] pickOne)' and can not be used as scope root binder.",
                    scopeRootBinderType.Name, name));
        }

        var instance = scopeRootBinderType.CreateInstance<object>();

        var methodInfo = (MethodInfo)filterMethod;
        return methodInfo.CreateDelegate<Func<IHandler[], IHandler>>(instance);
    }

    private void ExtractPoolConfig(ComponentModel model)
    {
        var initialRaw = model.Configuration.Attributes["initialPoolSize"];
        var maxRaw = model.Configuration.Attributes["maxPoolSize"];

        if (initialRaw != null)
        {
            var initial = _converter.PerformConversion<int>(initialRaw);
            model.ExtendedProperties[ExtendedPropertiesConstants.PoolInitialPoolSize] = initial;
        }

        if (maxRaw == null)
        {
            return;
        }

        var max = _converter.PerformConversion<int>(maxRaw);
        model.ExtendedProperties[ExtendedPropertiesConstants.PoolMaxPoolSize] = max;
    }

    private Type GetMandatoryTypeFromAttribute(ComponentModel model, string attribute, LifestyleType lifestyleType)
    {
        var rawAttribute = model.Configuration.Attributes[attribute];
        if (rawAttribute == null)
        {
            throw new InvalidOperationException(
                $"Component {model.Name} has {lifestyleType} lifestyle, but its configuration doesn't have mandatory '{attribute}' attribute.");
        }

        return _converter.PerformConversion<Type>(rawAttribute);
    }

    private Type GetTypeFromAttribute(ComponentModel model, string attribute)
    {
        var rawAttribute = model.Configuration.Attributes[attribute];
        if (rawAttribute == null)
        {
            return null;
        }

        return _converter.PerformConversion<Type>(rawAttribute);
    }

    private bool IsBindMethod(MemberInfo methodMember, object _)
    {
        var method = (MethodInfo)methodMember;
        if (method.ReturnType != typeof(IHandler))
        {
            return false;
        }

        var parameters = method.GetParameters();
        if (parameters.Length != 1)
        {
            return false;
        }

        if (parameters[0].ParameterType != typeof(IHandler[]))
        {
            return false;
        }

        return true;
    }
}