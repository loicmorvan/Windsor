// Copyright 2004-2022 Castle Project - http://www.castleproject.org/
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
using System.Reflection;
using Castle.Core.Internal;
using Castle.Core.Logging;
using Castle.MicroKernel;
using Castle.MicroKernel.Facilities;
using Castle.MicroKernel.Registration;
using Castle.MicroKernel.SubSystems.Conversion;

namespace Castle.Facilities.Logging;

/// <summary>A facility for logging support.</summary>
public class LoggingFacility : AbstractFacility
{
	private readonly string _customLoggerFactoryTypeName;
	private string _configFileName;

	private ITypeConverter _converter;

	private Type _loggerFactoryType;
	private LoggerLevel? _loggerLevel;
	private ILoggerFactory _loggerFactory;
	private string _logName;
	private bool _configuredExternally;

	/// <summary>Initializes a new instance of the <see cref="LoggingFacility" /> class.</summary>
	public LoggingFacility()
	{
	}

	/// <summary>Initializes a new instance of the <see cref="LoggingFacility" /> class using a custom LoggerImplementation</summary>
	/// <param name="customLoggerFactory"> The type name of the type of the custom logger factory. </param>
	/// <param name="configFile"> The configuration file that should be used by the chosen LoggerImplementation </param>
	public LoggingFacility(string customLoggerFactory, string configFile)
	{
		_customLoggerFactoryTypeName = customLoggerFactory;
		_configFileName = configFile;
	}

	public LoggingFacility LogUsing<TLoggerFactory>()
		where TLoggerFactory : ILoggerFactory
	{
		_loggerFactoryType = typeof(TLoggerFactory);
		return this;
	}

	public LoggingFacility LogUsing<TLoggerFactory>(TLoggerFactory loggerFactory)
		where TLoggerFactory : ILoggerFactory
	{
		_loggerFactoryType = typeof(TLoggerFactory);
		_loggerFactory = loggerFactory;
		return this;
	}

	public LoggingFacility ConfiguredExternally()
	{
		_configuredExternally = true;
		return this;
	}

	public LoggingFacility WithConfig(string configFile)
	{
		_configFileName = configFile ?? throw new ArgumentNullException(nameof(configFile));
		return this;
	}

	public LoggingFacility WithLevel(LoggerLevel level)
	{
		_loggerLevel = level;
		return this;
	}

	public LoggingFacility ToLog(string name)
	{
		_logName = name;
		return this;
	}

#if FEATURE_SYSTEM_CONFIGURATION
		/// <summary>
		///   loads configuration from current AppDomain's config file (aka web.config/app.config)
		/// </summary>
		/// <returns> </returns>
		public LoggingFacility WithAppConfig()
		{
			configFileName = AppDomain.CurrentDomain.SetupInformation.ConfigurationFile;
			return this;
		}
#endif

	protected override void Init()
	{
		SetUpTypeConverter();
		if (_loggerFactory == null) ReadConfigurationAndCreateLoggerFactory();
		RegisterLoggerFactory(_loggerFactory);
		RegisterDefaultILogger(_loggerFactory);
		RegisterSubResolver(_loggerFactory);
	}

	private void ReadConfigurationAndCreateLoggerFactory()
	{
		if (_loggerFactoryType == null) _loggerFactoryType = ReadCustomLoggerType();
		EnsureIsValidLoggerFactoryType();
		CreateProperLoggerFactory();
	}

	private Type ReadCustomLoggerType()
	{
		if (FacilityConfig != null)
		{
			var customLoggerType = FacilityConfig.Attributes["customLoggerFactory"];
			if (string.IsNullOrEmpty(customLoggerType) == false)
				return _converter.PerformConversion<Type>(customLoggerType);
		}

		if (_customLoggerFactoryTypeName != null)
			return _converter.PerformConversion<Type>(_customLoggerFactoryTypeName);
		return typeof(NullLogFactory);
	}

	private void EnsureIsValidLoggerFactoryType()
	{
		if (!_loggerFactoryType.Is<ILoggerFactory>())
			throw new FacilityException(
				$"The specified type '{_loggerFactoryType}' does not implement ILoggerFactory.");
	}

	private void CreateProperLoggerFactory()
	{
		Debug.Assert(_loggerFactoryType != null);

		var ctorArgs = GetLoggingFactoryArguments();
		_loggerFactory = _loggerFactoryType.CreateInstance<ILoggerFactory>(ctorArgs);
	}

	private string GetConfigFile()
	{
		if (_configFileName != null) return _configFileName;

		if (FacilityConfig != null) return FacilityConfig.Attributes["configFile"];
		return null;
	}

	private object[] GetLoggingFactoryArguments()
	{
		const BindingFlags flags = BindingFlags.Instance | BindingFlags.Public;

		ConstructorInfo ctor;
		if (IsConfiguredExternally())
		{
			ctor = _loggerFactoryType.GetConstructor(flags, null, [typeof(bool)], null);
			if (ctor != null) return [true];
		}

		var configFile = GetConfigFile();
		if (configFile != null)
		{
			ctor = _loggerFactoryType.GetConstructor(flags, null, [typeof(string)], null);
			if (ctor != null) return [configFile];
		}

		var level = GetLoggingLevel();
		if (level != null)
		{
			ctor = _loggerFactoryType.GetConstructor(flags, null, [typeof(LoggerLevel)], null);
			if (ctor != null) return [level.Value];
		}

		ctor = _loggerFactoryType.GetConstructor(flags, null, Type.EmptyTypes, null);
		if (ctor != null) return [];
		throw new FacilityException($"No support constructor found for logging type '{_loggerFactoryType}'");
	}

	private bool IsConfiguredExternally()
	{
		if (_configuredExternally) return true;
		if (FacilityConfig != null)
		{
			var value = FacilityConfig.Attributes["configuredExternally"];
			if (value != null) return _converter.PerformConversion<bool>(value);
		}

		return false;
	}

	private LoggerLevel? GetLoggingLevel()
	{
		if (_loggerLevel.HasValue) return _loggerLevel;
		if (FacilityConfig != null)
		{
			var level = FacilityConfig.Attributes["loggerLevel"];
			if (level != null) return _converter.PerformConversion<LoggerLevel>(level);
		}

		return null;
	}

	private void RegisterDefaultILogger(ILoggerFactory factory)
	{
		if (factory is IExtendedLoggerFactory loggerFactory)
		{
			var defaultLogger = loggerFactory.Create(_logName ?? "Default");
			Kernel.Register(
				Component.For<IExtendedLogger>().NamedAutomatically("ilogger.default").Instance(defaultLogger),
				Component.For<ILogger>().NamedAutomatically("ilogger.default.base").Instance(defaultLogger));
		}
		else
		{
			Kernel.Register(Component.For<ILogger>().NamedAutomatically("ilogger.default")
				.Instance(factory.Create(_logName ?? "Default")));
		}
	}

	private void RegisterLoggerFactory(ILoggerFactory factory)
	{
		if (factory is IExtendedLoggerFactory loggerFactory)
			Kernel.Register(
				Component.For<IExtendedLoggerFactory>().NamedAutomatically("iloggerfactory").Instance(loggerFactory),
				Component.For<ILoggerFactory>().NamedAutomatically("iloggerfactory.base").Instance(loggerFactory));
		else
			Kernel.Register(Component.For<ILoggerFactory>().NamedAutomatically("iloggerfactory").Instance(factory));
	}

	private void RegisterSubResolver(ILoggerFactory loggerFactory)
	{
		if (loggerFactory is not IExtendedLoggerFactory extendedLoggerFactory)
		{
			Kernel.Resolver.AddSubResolver(new LoggerResolver(loggerFactory, _logName));
			return;
		}

		Kernel.Resolver.AddSubResolver(new LoggerResolver(extendedLoggerFactory, _logName));
	}

	private void SetUpTypeConverter()
	{
		_converter = Kernel.GetSubSystem(SubSystemConstants.ConversionManagerKey) as IConversionManager;
	}
}