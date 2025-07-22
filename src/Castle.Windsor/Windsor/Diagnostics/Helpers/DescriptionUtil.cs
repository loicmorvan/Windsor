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

using System.ComponentModel;
using Castle.Core.Internal;
using Castle.Windsor.Core;
using Castle.Windsor.Core.Internal;
using Castle.Windsor.MicroKernel;

namespace Castle.Windsor.Windsor.Diagnostics.Helpers;

public static class DescriptionUtil
{
	public static string GetComponentName(this IHandler handler)
	{
		var componentName = handler.ComponentModel.ComponentName;
		if (componentName.SetByUser) return $"\"{componentName.Name}\" {handler.GetServicesDescription()}";
		return handler.GetServicesDescription();
	}

	public static string GetLifestyleDescription(this ComponentModel componentModel)
	{
		if (componentModel.LifestyleType == LifestyleType.Undefined) return $"{LifestyleType.Singleton}*";
		if (componentModel.LifestyleType != LifestyleType.Custom) return componentModel.LifestyleType.ToString();
		return componentModel.CustomLifestyle.Name;
	}

	public static string GetLifestyleDescriptionLong(this ComponentModel componentModel)
	{
		if (componentModel.LifestyleType == LifestyleType.Undefined)
			return
				$"{componentModel.LifestyleType} (default lifestyle {LifestyleType.Singleton} will be used)";
		if (componentModel.LifestyleType == LifestyleType.Scoped)
		{
			var accessorType = componentModel.GetScopeAccessorType();
			if (accessorType == null) return "Scoped explicitly";
			var description = accessorType.GetAttribute<DescriptionAttribute>();
			if (description != null) return "Scoped " + description.Description;
			return "Scoped via " + accessorType.ToCSharpString();
		}

		if (componentModel.LifestyleType != LifestyleType.Custom) return componentModel.LifestyleType.ToString();
		return "Custom: " + componentModel.CustomLifestyle.Name;
	}

	public static string GetServicesDescription(this IHandler handler)
	{
		return handler.ComponentModel.ToString();
	}
}