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

using System.Collections;
using Castle.Core.Configuration;
using Castle.Windsor.Core;
using Castle.Windsor.Core.Internal;
using Castle.Windsor.MicroKernel.Registration;
using Castle.Windsor.MicroKernel.Util;

namespace Castle.Windsor.MicroKernel.ModelBuilder.Descriptors;

public class ServiceOverrideDescriptor : AbstractPropertyDescriptor
{
    private readonly object _value;

    public ServiceOverrideDescriptor(params ServiceOverride[] overrides)
    {
        _value = overrides;
    }

    public ServiceOverrideDescriptor(IDictionary dictionary)
    {
        _value = dictionary;
    }

    public override void BuildComponentModel(IKernel kernel, ComponentModel model)
    {
        if (_value is IDictionary dictionary)
        {
            foreach (DictionaryEntry property in dictionary)
            {
                Apply(model, property.Key, property.Value, null);
            }
        }

        if (_value is ServiceOverride[] overrides)
        {
            overrides.ForEach(o => Apply(model, o.DependencyKey, o.Value, o));
        }
    }

    private void Apply(ComponentModel model, object dependencyKey, object dependencyValue, ServiceOverride @override)
    {
        if (dependencyValue is string value)
        {
            ApplySimpleReference(model, dependencyKey, value);
        }
        else if (dependencyValue is IEnumerable<string> enumerable)
        {
            ApplyReferenceList(model, dependencyKey, enumerable, @override);
        }
        else if (dependencyValue is Type type)
        {
            ApplySimpleReference(model, dependencyKey, ComponentName.DefaultNameFor(type));
        }
        else if (dependencyValue is IEnumerable<Type> types)
        {
            ApplyReferenceList(model, dependencyKey, types.Select(ComponentName.DefaultNameFor), @override);
        }
    }

    private void ApplyReferenceList(ComponentModel model, object name, IEnumerable<string> items,
        ServiceOverride serviceOverride)
    {
        var list = new MutableConfiguration("list");

        if (serviceOverride is { Type: not null })
        {
            list.Attributes.Add("type", serviceOverride.Type.AssemblyQualifiedName);
        }

        foreach (var item in items)
        {
            var reference = ReferenceExpressionUtil.BuildReference(item);
            list.Children.Add(new MutableConfiguration("item", reference));
        }

        AddParameter(model, GetNameString(name), list);
    }

    private void ApplySimpleReference(ComponentModel model, object dependencyName, string componentKey)
    {
        var reference = ReferenceExpressionUtil.BuildReference(componentKey);
        AddParameter(model, GetNameString(dependencyName), reference);
    }

    private string GetNameString(object key)
    {
        if (key is Type type)
        {
            return type.AssemblyQualifiedName;
        }

        return key.ToString();
    }
}