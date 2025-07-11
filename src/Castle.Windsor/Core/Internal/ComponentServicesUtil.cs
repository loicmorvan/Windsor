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
using System.Collections.Generic;
using System.Reflection;

namespace Castle.Core.Internal;

public class ComponentServicesUtil
{
	private static readonly TypeByInheritanceDepthMostSpecificFirstComparer Comparer = new();

	public static void AddService(IList<Type> existingServices, HashSet<Type> lookup, Type newService)
	{
		if (lookup.Contains(newService)) return;
		if (newService.GetTypeInfo().IsInterface)
		{
			existingServices.Add(newService);
			lookup.Add(newService);
			return;
		}

		if (newService.GetTypeInfo().IsClass == false)
			throw new ArgumentException(
				string.Format("Type {0} is not a class nor an interface, and those are the only values allowed.",
					newService));
		var count = existingServices.Count;
		for (var i = 0; i < count; i++)
		{
			if (existingServices[i].GetTypeInfo().IsInterface)
			{
				existingServices.Insert(i, newService);
				lookup.Add(newService);
			}

			var result = Comparer.Compare(newService, existingServices[i]);
			if (result < 0)
			{
				existingServices.Insert(i, newService);
				lookup.Add(newService);
				return;
			}

			if (result == 0) return;
		}

		lookup.Add(newService);
		existingServices.Add(newService);
	}
}