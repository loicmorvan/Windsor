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

using System.Globalization;
using Castle.Core.Configuration;

namespace Castle.Windsor.MicroKernel.SubSystems.Conversion;

/// <summary>Implements all standard conversions.</summary>
[Serializable]
public class PrimitiveConverter : AbstractTypeConverter
{
    private readonly Type[] _types =
    [
        typeof(char),
        typeof(DateTime),
        typeof(decimal),
        typeof(bool),
        typeof(short),
        typeof(int),
        typeof(long),
        typeof(ushort),
        typeof(uint),
        typeof(ulong),
        typeof(byte),
        typeof(sbyte),
        typeof(float),
        typeof(double),
        typeof(string)
    ];

    public override bool CanHandleType(Type type)
    {
        return Array.IndexOf(_types, type) != -1;
    }

    public override object PerformConversion(string value, Type targetType)
    {
        if (targetType == typeof(string))
        {
            return value;
        }

        try
        {
            return Convert.ChangeType(value, targetType, CultureInfo.InvariantCulture);
        }
        catch (Exception ex)
        {
            var message = $"Could not convert from '{value}' to {targetType.FullName}";

            throw new ConverterException(message, ex);
        }
    }

    public override object PerformConversion(IConfiguration configuration, Type targetType)
    {
        return PerformConversion(configuration.Value, targetType);
    }
}