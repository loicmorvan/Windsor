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
using System.IO;
using Castle.Core.Logging;
using Castle.MicroKernel.Registration;
using Castle.Windsor.Tests.LoggingFacility.Classes;

namespace Castle.Windsor.Tests.LoggingFacility;

public class ConsoleFacilityTestCase : BaseTest, IDisposable
{
	private readonly IWindsorContainer _container;
	private readonly StringWriter _errorWriter = new();
	private readonly StringWriter _outWriter = new();

	public ConsoleFacilityTestCase()
	{
		_container = base.CreateConfiguredContainer<ConsoleFactory>();

		_outWriter.GetStringBuilder().Length = 0;
		_errorWriter.GetStringBuilder().Length = 0;

		Console.SetOut(_outWriter);
		Console.SetError(_errorWriter);
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

		var expectedLogOutput = string.Format("[Info] '{0}' Hello world" + Environment.NewLine,
			typeof(SimpleLoggingComponent).FullName);
		var actualLogOutput = "";

		test.DoSomething();

		actualLogOutput = _outWriter.GetStringBuilder().ToString();
		Assert.Equal(expectedLogOutput, actualLogOutput);
	}
}