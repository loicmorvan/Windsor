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

using Castle.Core.Logging;
using Castle.MicroKernel.Registration;
using Castle.Windsor.Tests.LoggingFacility.Classes;

namespace Castle.Windsor.Tests.LoggingFacility;

public class ConsoleLoggerTestCase : AbstractContainerTestCase
{
	[Fact]
	[Bug("FACILITIES-153")]
	public void Can_specify_level_at_registration_time()
	{
		Container.AddFacility<Castle.Facilities.Logging.LoggingFacility>(f =>
			f.LogUsing<ConsoleFactory>().WithLevel(LoggerLevel.Fatal));
		Container.Register(Component.For<SimpleLoggingComponent>());

		var item = Container.Resolve<SimpleLoggingComponent>();
		Assert.True(item.Logger.IsFatalEnabled);
		Assert.False(item.Logger.IsErrorEnabled);
	}
}