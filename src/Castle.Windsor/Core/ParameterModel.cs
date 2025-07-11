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

using Castle.Core.Configuration;

/// <summary>
///   Represents a parameter. Usually the parameter
///   comes from the external world, ie, an external configuration.
/// </summary>
[Serializable]
public class ParameterModel
{
	private readonly IConfiguration _configValue;
	private readonly String _name;
	private readonly String _value;

	/// <summary>
	///   Initializes a new instance of the <see cref = "ParameterModel" /> class.
	/// </summary>
	/// <param name = "name">The name.</param>
	/// <param name = "value">The value.</param>
	public ParameterModel(String name, String value)
	{
		this._name = name;
		this._value = value;
	}

	/// <summary>
	///   Initializes a new instance of the <see cref = "ParameterModel" /> class.
	/// </summary>
	/// <param name = "name">The name.</param>
	/// <param name = "value">The value.</param>
	public ParameterModel(String name, IConfiguration value)
	{
		this._name = name;
		_configValue = value;
	}

	/// <summary>
	///   Gets the config value.
	/// </summary>
	/// <value>The config value.</value>
	public IConfiguration ConfigValue
	{
		get { return _configValue; }
	}

	/// <summary>
	///   Gets the name.
	/// </summary>
	/// <value>The name.</value>
	public String Name
	{
		get { return _name; }
	}

	/// <summary>
	///   Gets the value.
	/// </summary>
	/// <value>The value.</value>
	public String Value
	{
		get { return _value; }
	}
}