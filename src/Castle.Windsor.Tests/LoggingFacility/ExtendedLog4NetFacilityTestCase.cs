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
using System.IO;
using Castle.MicroKernel.Registration;
using Castle.Services.Logging.Log4netIntegration;
using Castle.Windsor.Tests.LoggingFacility.Classes;
using log4net;
using log4net.Appender;
using log4net.Layout;
using log4net.Repository.Hierarchy;

namespace Castle.Windsor.Tests.LoggingFacility;

/// <summary>Summary description for ExtendedLog4NetFacilityTests.</summary>
public class ExtendedLog4NetFacilityTestCase : BaseTest, IDisposable
{
	private readonly IWindsorContainer _container;

	public ExtendedLog4NetFacilityTestCase()
	{
		_container = base.CreateConfiguredContainer<ExtendedLog4netFactory>();
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

		var expectedLogOutput = string.Format("[INFO ] [{0}] - Hello world" + Environment.NewLine,
			typeof(SimpleLoggingComponent).FullName);
		var memoryAppender =
			Assert.IsType<MemoryAppender>(((Hierarchy)LogManager.GetRepository()).Root.GetAppender("memory"));
		TextWriter actualLogOutput = new StringWriter();
		var patternLayout = new PatternLayout("[%-5level] [%logger] - %message%newline");
		patternLayout.Format(actualLogOutput, memoryAppender.GetEvents()[0]);

		Assert.Equal(expectedLogOutput, actualLogOutput.ToString());

		_container.Register(Component.For(typeof(SmtpServer)).Named("component2"));
		var smtpServer = _container.Resolve<ISmtpServer>("component2");

		smtpServer.Start();
		smtpServer.InternalSend("rbellamy@pteradigm.com", "jobs@castlestronghold.com",
			"We're looking for a few good porgrammars.");
		smtpServer.Stop();

		expectedLogOutput =
			string.Format(
				"[DEBUG] [Castle.Facilities.Logging.Tests.Classes.SmtpServer] - Stopped" + Environment.NewLine,
				typeof(SimpleLoggingComponent).FullName);
		memoryAppender =
			Assert.IsType<MemoryAppender>(((Hierarchy)LogManager.GetRepository()).Root.GetAppender("memory"));
		actualLogOutput = new StringWriter();
		patternLayout = new PatternLayout("[%-5level] [%logger] - %message%newline");

		Assert.Equal(4, memoryAppender.GetEvents().Length);

		patternLayout.Format(actualLogOutput, memoryAppender.GetEvents()[3]);

		Assert.Equal(expectedLogOutput, actualLogOutput.ToString());
	}

	[Fact]
	public void ContextTest()
	{
		_container.Register(Component.For<ComplexLoggingComponent>().Named("component1"));
		var complexLoggingComponent = _container.Resolve<ComplexLoggingComponent>("component1");

		complexLoggingComponent.DoSomeContextual();

		var expectedLogOutput = string.Format(
			"[DEBUG] [Castle.Facilities.Logging.Tests.Classes.ComplexLoggingComponent] [Outside Inside0] [bar] [flam] - Bim, bam boom." +
			Environment.NewLine,
			typeof(SimpleLoggingComponent).FullName);
		var memoryAppender =
			Assert.IsType<MemoryAppender>(((Hierarchy)LogManager.GetRepository()).Root.GetAppender("memory"));
		var actualLogOutput = new StringWriter();
		var patternLayout =
			new PatternLayout(
				"[%-5level] [%logger] [%properties{NDC}] [%properties{foo}] [%properties{flim}] - %message%newline");
		patternLayout.Format(actualLogOutput, memoryAppender.GetEvents()[0]);

		Assert.Equal(expectedLogOutput, actualLogOutput.ToString());
	}
}