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

namespace Castle.Facilities.Logging.Tests;

using System;
using System.IO;

using Castle.Facilities.Logging.Tests.Classes;
using Castle.MicroKernel.Registration;
using Castle.Services.Logging.Log4netIntegration;
using Castle.Windsor;

using log4net;
using log4net.Appender;
using log4net.Layout;
using log4net.Repository.Hierarchy;

public class Log4NetFacilityLognameOverrideTests : OverrideLoggerTest, IDisposable
{
	public Log4NetFacilityLognameOverrideTests()
	{
		container = base.CreateConfiguredContainer<ExtendedLog4netFactory>("Override");
	}

	public void Dispose()
	{
		container.Dispose();
	}

	private IWindsorContainer container;

	[Fact]
	public void OverrideTest()
	{
		container.Register(Component.For(typeof(SimpleLoggingComponent)).Named("component"));
		var test = container.Resolve<SimpleLoggingComponent>("component");

		test.DoSomething();

		var expectedLogOutput = string.Format("[INFO ] [Override.{0}] - Hello world" + Environment.NewLine, typeof(SimpleLoggingComponent).FullName);
		var memoryAppender = ((Hierarchy)LogManager.GetRepository()).Root.GetAppender("memory") as MemoryAppender;
		TextWriter actualLogOutput = new StringWriter();
		var patternLayout = new PatternLayout("[%-5level] [%logger] - %message%newline");
		patternLayout.Format(actualLogOutput, memoryAppender.GetEvents()[0]);

		Assert.Equal(expectedLogOutput, actualLogOutput.ToString());
	}
}