// Copyright 2004-2017 Castle Project - http://www.castleproject.org/
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
using System.Diagnostics;
using Castle.Core;
using Castle.Core.Logging;
using Castle.MicroKernel;
using Castle.MicroKernel.Context;

namespace Castle.Facilities.Logging;

/// <summary>
///     Custom resolver used by Windsor. It gives
///     us some contextual information that we use to set up a logging
///     before satisfying the dependency
/// </summary>
public class LoggerResolver : ISubDependencyResolver
{
	private readonly IExtendedLoggerFactory _extendedLoggerFactory;
	private readonly ILoggerFactory _loggerFactory;
	private readonly string _logName;

	public LoggerResolver(ILoggerFactory loggerFactory)
	{
		if (loggerFactory == null) throw new ArgumentNullException(nameof(loggerFactory));

		_loggerFactory = loggerFactory;
	}

	public LoggerResolver(IExtendedLoggerFactory extendedLoggerFactory)
	{
		if (extendedLoggerFactory == null) throw new ArgumentNullException(nameof(extendedLoggerFactory));

		_extendedLoggerFactory = extendedLoggerFactory;
	}

	public LoggerResolver(ILoggerFactory loggerFactory, string name) : this(loggerFactory)
	{
		_logName = name;
	}

	public LoggerResolver(IExtendedLoggerFactory extendedLoggerFactory, string name) : this(extendedLoggerFactory)
	{
		_logName = name;
	}

	public bool CanResolve(CreationContext context, ISubDependencyResolver parentResolver, ComponentModel model,
		DependencyModel dependency)
	{
		return dependency.TargetType == typeof(ILogger) || dependency.TargetType == typeof(IExtendedLogger);
	}

	public object Resolve(CreationContext context, ISubDependencyResolver parentResolver, ComponentModel model,
		DependencyModel dependency)
	{
		Debug.Assert(CanResolve(context, parentResolver, model, dependency));
		if (_extendedLoggerFactory != null)
			return string.IsNullOrEmpty(_logName)
				? _extendedLoggerFactory.Create(model.Implementation)
				: _extendedLoggerFactory.Create(_logName).CreateChildLogger(model.Implementation.FullName);

		Debug.Assert(_loggerFactory != null);
		return string.IsNullOrEmpty(_logName)
			? _loggerFactory.Create(model.Implementation)
			: _loggerFactory.Create(_logName).CreateChildLogger(model.Implementation.FullName);
	}
}