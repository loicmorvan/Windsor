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

using System;
using System.ComponentModel;
using System.Reflection;
using Castle.Core.Configuration;

namespace Castle.MicroKernel.SubSystems.Conversion;

/// <summary>
///     Attempts to utilize an existing <see cref="TypeConverter" /> for conversion
/// </summary>
[Serializable]
public class ComponentModelConverter : AbstractTypeConverter
{
	public override bool CanHandleType(Type type)
	{
		if (type.GetTypeInfo().IsInterface) return false;

		var converter = TypeDescriptor.GetConverter(type);
		return converter != null && converter.CanConvertFrom(typeof(string));
	}

	public override object PerformConversion(string value, Type targetType)
	{
		var converter = TypeDescriptor.GetConverter(targetType);

		try
		{
			return converter.ConvertFrom(value);
		}
		catch (Exception ex)
		{
			var message = string.Format(
				"Could not convert from '{0}' to {1}",
				value, targetType.FullName);

			throw new ConverterException(message, ex);
		}
	}

	public override object PerformConversion(IConfiguration configuration, Type targetType)
	{
		return PerformConversion(configuration.Value, targetType);
	}
}