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

namespace Castle.Core;

using System;
using System.Reflection;

/// <summary>
///     This attribute is useful only when you want to register all components on an assembly as a batch process. By doing so, the batch register will look for this attribute to distinguish components
///     from other classes.
/// </summary>
[AttributeUsage(AttributeTargets.Class, AllowMultiple = true)]
public class CastleComponentAttribute : LifestyleAttribute
{
	public CastleComponentAttribute(string name) : this(name, null)
	{
	}

	public CastleComponentAttribute(params Type[] services)
		: this(null, services)
	{
	}

	public CastleComponentAttribute(string name, params Type[] services)
		: this(name, LifestyleType.Undefined, services)
	{
	}

	public CastleComponentAttribute(string name, LifestyleType lifestyle, params Type[] services) : base(lifestyle)
	{
		Name = name;
		Services = services ?? Type.EmptyTypes;
		ServicesSpecifiedExplicitly = Services.Length > 0;
	}

	public bool HasName => string.IsNullOrEmpty(Name) == false;

	public string Name { get; }

	public Type[] Services { get; private set; }
	public bool ServicesSpecifiedExplicitly { get; }

	public static CastleComponentAttribute GetDefaultsFor(Type type)
	{
		var attribute = (CastleComponentAttribute)type.GetTypeInfo().GetCustomAttribute(typeof(CastleComponentAttribute));
		if (attribute != null)
		{
			if (attribute.ServicesSpecifiedExplicitly == false) attribute.Services = [type];
			return attribute;
		}

		return new CastleComponentAttribute(type);
	}
}