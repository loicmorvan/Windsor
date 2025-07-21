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

namespace Castle.Windsor.Facilities.TypedFactory;

using System;
using System.ComponentModel;
using System.Reflection;

/// <summary>Legacy class from old impl. of the facility. Do not use it.</summary>
[EditorBrowsable(EditorBrowsableState.Never)]
public class FactoryEntry
{
	public FactoryEntry(string id, Type factoryInterface, string creationMethod, string destructionMethod)
	{
		if (string.IsNullOrEmpty(id)) throw new ArgumentNullException(nameof(id));
		if (factoryInterface == null) throw new ArgumentNullException(nameof(factoryInterface));
		if (!factoryInterface.GetTypeInfo().IsInterface) throw new ArgumentException("factoryInterface must be an interface");
		if (string.IsNullOrEmpty(creationMethod)) throw new ArgumentNullException(nameof(creationMethod));

		Id = id;
		FactoryInterface = factoryInterface;
		CreationMethod = creationMethod;
		DestructionMethod = destructionMethod;
	}

	public string CreationMethod { get; }

	public string DestructionMethod { get; }

	public Type FactoryInterface { get; }

	public string Id { get; }
}