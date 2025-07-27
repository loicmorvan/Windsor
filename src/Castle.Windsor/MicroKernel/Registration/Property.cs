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

namespace Castle.Windsor.MicroKernel.Registration;

/// <summary>Represents a key/value pair.</summary>
public class Property
{
	public Property(object key, object value)
	{
		Key = key;
		Value = value;
	}

	/// <summary>Gets the property key.</summary>
	public object Key { get; }

	/// <summary>Gets the property value.</summary>
	public object Value { get; }

	/// <summary>Create a <see cref = "PropertyKey" /> with key.</summary>
	/// <param name = "key">The property key.</param>
	/// <returns>The new <see cref = "PropertyKey" /></returns>
	public static PropertyKey ForKey(string key)
	{
		return new PropertyKey(key);
	}

	/// <summary>Create a <see cref = "PropertyKey" /> with key.</summary>
	/// <param name = "key">The property key.</param>
	/// <returns>The new <see cref = "PropertyKey" /></returns>
	public static PropertyKey ForKey(Type key)
	{
		return new PropertyKey(key);
	}

	/// <summary>Create a <see cref = "PropertyKey" /> with key.</summary>
	/// <param key = "key">The property key.</param>
	/// <returns>The new <see cref = "PropertyKey" /></returns>
	public static PropertyKey ForKey<TKey>()
	{
		return new PropertyKey(typeof(TKey));
	}

	public static implicit operator Dependency(Property item)
	{
		return item == null ? null : new Dependency(item);
	}
}