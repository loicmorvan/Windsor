﻿// Copyright 2004-2022 Castle Project - http://www.castleproject.org/
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
using Castle.Core.Resource;
using Castle.Services.Logging.Log4netIntegration;
using Castle.Windsor.Tests.LoggingFacility.Classes;

namespace Castle.Windsor.Tests.LoggingFacility;

public class CustomFacilityTests
{
	[Fact]
	public void ReadCustomFacilityConfigFromXml()
	{
		using var container = new WindsorContainer();
		container.Install(
			Castle.Windsor.Installer.Configuration.FromXml(
				new StaticContentResource(
					string.Format(
						@"<castle>
<facilities>
<facility
  customLoggerFactory='{0}'
  type='{1}'/>
</facilities>
</castle>",
						typeof(CustomLog4NetFactory).AssemblyQualifiedName,
						typeof(Castle.Facilities.Logging.LoggingFacility).AssemblyQualifiedName))));
		var logger = container.Resolve<ILogger>();
		Assert.IsType<Log4netLogger>(logger);
	}
}