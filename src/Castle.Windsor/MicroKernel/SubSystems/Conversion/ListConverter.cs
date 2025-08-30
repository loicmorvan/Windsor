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
public class ListConverter : AbstractTypeConverter
{
    public override bool CanHandleType(Type type)
    {
        return type == typeof(IList) || type == typeof(ArrayList);
    }

    public override object PerformConversion(string? value, Type targetType)
    {
        throw new NotImplementedException();
    }

    public override object PerformConversion(IConfiguration configuration, Type targetType)
    {
        Debug.Assert(CanHandleType(targetType));

        var convertTo = GetConvertToType(configuration);

        return configuration
            .Children
            .Select(itemConfig => Context.Composition.PerformConversion(itemConfig.Value, convertTo))
            .ToList();
    }

    private Type GetConvertToType(IConfiguration configuration)
    {
        var itemType = configuration.Attributes["type"];
        var convertTo = typeof(string);
        if (itemType != null)
        {
            convertTo = Context.Composition.PerformConversion<Type>(itemType);
        }

        return convertTo;
    }
}