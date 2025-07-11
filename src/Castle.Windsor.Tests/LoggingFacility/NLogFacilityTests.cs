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
using Castle.MicroKernel.Registration;
using Castle.Services.Logging.NLogIntegration;
using Castle.Windsor.Tests.LoggingFacility.Classes;
using NLog;
using NLog.Targets;

namespace Castle.Windsor.Tests.LoggingFacility;

/// <summary>
///     Summary description for NLogFacilityTestts.
/// </summary>
public class NLogFacilityTests : BaseTest, IDisposable
{
	private readonly IWindsorContainer _container;

	public NLogFacilityTests()
	{
		_container = base.CreateConfiguredContainer<NLogFactory>();
	}

	public void Dispose()
	{
		if (_container != null) _container.Dispose();
	}

	[Fact]
	public void SimpleTest()
	{
		_container.Register(Component.For(typeof(SimpleLoggingComponent)).Named("component"));
		var test = _container.Resolve<SimpleLoggingComponent>("component");

		test.DoSomething();

		var expectedLogOutput = string.Format("|INFO|{0}|Hello world", typeof(SimpleLoggingComponent).FullName);
		var actualLogOutput = (LogManager.Configuration.FindTargetByName("memory") as MemoryTarget).Logs[0];
		actualLogOutput = actualLogOutput.Substring(actualLogOutput.IndexOf('|'));
		Assert.Equal(expectedLogOutput, actualLogOutput);
	}
}