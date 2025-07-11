﻿// Copyright 2004-2011 Castle Project - http://www.castleproject.org/
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
using Castle.MicroKernel.Registration;
using Castle.MicroKernel.SubSystems.Configuration;
using Castle.Windsor.Configuration;

namespace Castle.Windsor.Installer;

/// <summary>
///     Delegate to provide environment name.
/// </summary>
/// <returns>The environment name.</returns>
public delegate string EnvironmentDelegate();

public class ConfigurationInstaller : IWindsorInstaller
{
	private readonly IConfigurationInterpreter _interpreter;
	private EnvironmentDelegate _environment;

	/// <summary>
	///     Initializes a new instance of the ConfigurationInstaller class.
	/// </summary>
	public ConfigurationInstaller(IConfigurationInterpreter interpreter)
	{
		ArgumentNullException.ThrowIfNull(interpreter);
		_interpreter = interpreter;
	}

	void IWindsorInstaller.Install(IWindsorContainer container, IConfigurationStore store)
	{
		if (_environment != null) _interpreter.EnvironmentName = _environment();

		_interpreter.ProcessResource(_interpreter.Source, store, container.Kernel);
	}

	/// <summary>
	///     Sets the configuration environment name.
	/// </summary>
	/// <param name="environmentName">The environment name.</param>
	/// <returns></returns>
	public ConfigurationInstaller Environment(string environmentName)
	{
		return Environment(() => environmentName);
	}

	/// <summary>
	///     Set the configuration environment strategy.
	/// </summary>
	/// <param name="environment">The environment strategy.</param>
	/// <returns></returns>
	public ConfigurationInstaller Environment(EnvironmentDelegate environment)
	{
		_environment = environment;
		return this;
	}
}