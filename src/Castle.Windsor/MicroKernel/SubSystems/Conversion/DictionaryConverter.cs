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
using System.Diagnostics;
using Castle.Core.Configuration;

namespace Castle.Windsor.MicroKernel.SubSystems.Conversion;

[Serializable]
public class DictionaryConverter(ITypeConverterContext context) : AbstractTypeConverter(context)
{
    public override bool CanHandleType(Type type)
    {
        return type == typeof(IDictionary) || type == typeof(Hashtable);
    }

    public override object PerformConversion(string value, Type targetType)
    {
        throw new NotImplementedException();
    }

    public override object PerformConversion(IConfiguration configuration, Type targetType)
    {
        Debug.Assert(CanHandleType(targetType));

        var dict = new Dictionary<object, object?>();

        var keyTypeName = configuration.Attributes["keyType"];
        var defaultKeyType = typeof(string);

        var valueTypeName = configuration.Attributes["valueType"];
        var defaultValueType = typeof(string);

        if (keyTypeName != null)
        {
            defaultKeyType = Context.Composition.PerformConversion<Type>(keyTypeName);
        }

        if (valueTypeName != null)
        {
            defaultValueType = Context.Composition.PerformConversion<Type>(valueTypeName);
        }

        foreach (var itemConfig in configuration.Children)
        {
            // Preparing the key

            var keyValue = itemConfig.Attributes["key"];

            if (keyValue == null)
            {
                throw new ConverterException("You must provide a key for the dictionary entry");
            }

            var convertKeyTo = defaultKeyType;

            var itemConfigAttribute = itemConfig.Attributes["keyType"];
            if (itemConfigAttribute != null)
            {
                convertKeyTo = Context.Composition.PerformConversion<Type>(itemConfigAttribute);
            }

            var key = Context.Composition.PerformConversion(keyValue, convertKeyTo);
            if (key is null)
            {
                throw new ConverterException("Could not convert key to type " + defaultKeyType);           
            }

            // Preparing the value

            var convertValueTo = defaultValueType;

            var configAttribute = itemConfig.Attributes["valueType"];
            if (configAttribute != null)
            {
                convertValueTo = Context.Composition.PerformConversion<Type>(configAttribute);
            }

            var value = Context.Composition.PerformConversion(itemConfig.Children.Count == 0
                ? itemConfig
                : itemConfig.Children[0], convertValueTo);

            dict.Add(key, value);
        }

        return dict;
    }
}