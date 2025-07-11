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

public class ExtendedNLogFacilityTests : BaseTest, IDisposable
{
	private readonly IWindsorContainer _container;

	public ExtendedNLogFacilityTests()
	{
		_container = base.CreateConfiguredContainer<ExtendedNLogFactory>();
	}

	public void Dispose()
	{
		_container.Dispose();
	}

	[Fact]
	public void SimpleTest()
	{
		_container.Register(Component.For(typeof(SimpleLoggingComponent)).Named("component1"));
		var test = _container.Resolve<SimpleLoggingComponent>("component1");

		test.DoSomething();

		var expectedLogOutput = string.Format("|INFO|{0}|Hello world", typeof(SimpleLoggingComponent).FullName);
		var actualLogOutput = (LogManager.Configuration.FindTargetByName("memory") as MemoryTarget).Logs[0];
		actualLogOutput = actualLogOutput.Substring(actualLogOutput.IndexOf('|'));

		Assert.Equal(expectedLogOutput, actualLogOutput);

		_container.Register(Component.For(typeof(SmtpServer)).Named("component2"));
		var smtpServer = _container.Resolve<ISmtpServer>("component2");

		smtpServer.Start();
		smtpServer.InternalSend("rbellamy@pteradigm.com", "jobs@castlestronghold.com",
			"We're looking for a few good porgrammars.");
		smtpServer.Stop();

		expectedLogOutput =
			string.Format(
				"|INFO|{0}|InternalSend rbellamy@pteradigm.com jobs@castlestronghold.com We're looking for a few good porgrammars.",
				typeof(SmtpServer).FullName);
		actualLogOutput = (LogManager.Configuration.FindTargetByName("memory") as MemoryTarget).Logs[1];
		actualLogOutput = actualLogOutput.Substring(actualLogOutput.IndexOf('|'));

		Assert.Equal(expectedLogOutput, actualLogOutput);
	}

	[Fact]
	public void ContextTest()
	{
		_container.Register(Component.For(typeof(ComplexLoggingComponent)).Named("component1"));
		var complexLoggingComponent = _container.Resolve<ComplexLoggingComponent>("component1");

		complexLoggingComponent.DoSomeContextual();

		var expectedLogOutput = string.Format("|DEBUG|{0}|flam|bar|Outside Inside0|Bim, bam boom.",
			typeof(ComplexLoggingComponent).FullName);
		var actualLogOutput = (LogManager.Configuration.FindTargetByName("memory1") as MemoryTarget).Logs[0];
		actualLogOutput = actualLogOutput.Substring(actualLogOutput.IndexOf('|'));

		Assert.Equal(expectedLogOutput, actualLogOutput);
	}
}