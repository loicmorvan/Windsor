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
using Castle.Core.Configuration;

namespace Castle.Core;

/// <summary>
///     Represents a parameter. Usually the parameter
///     comes from the external world, ie, an external configuration.
/// </summary>
[Serializable]
public class ParameterModel
{
	/// <summary>
	///     Initializes a new instance of the <see cref="ParameterModel" /> class.
	/// </summary>
	/// <param name="name">The name.</param>
	/// <param name="value">The value.</param>
	public ParameterModel(string name, string value)
	{
		Name = name;
		Value = value;
	}

	/// <summary>
	///     Initializes a new instance of the <see cref="ParameterModel" /> class.
	/// </summary>
	/// <param name="name">The name.</param>
	/// <param name="value">The value.</param>
	public ParameterModel(string name, IConfiguration value)
	{
		Name = name;
		ConfigValue = value;
	}

	/// <summary>
	///     Gets the config value.
	/// </summary>
	/// <value>The config value.</value>
	public IConfiguration ConfigValue { get; }

	/// <summary>
	///     Gets the name.
	/// </summary>
	/// <value>The name.</value>
	public string Name { get; }

	/// <summary>
	///     Gets the value.
	/// </summary>
	/// <value>The value.</value>
	public string Value { get; }
}