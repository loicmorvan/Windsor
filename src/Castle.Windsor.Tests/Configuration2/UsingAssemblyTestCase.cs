// Copyright 2004-2010 Castle Project - http://www.castleproject.org/
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

#if FEATURE_WPF //This test requires PresentationCore.dll (specified in Configuration2/config_with_using_assembly.xml)
namespace Castle.Windsor.Tests.Configuration2
{
	using System;

	using Castle.MicroKernel;
	using Castle.MicroKernel.SubSystems.Conversion;

	

	
	public class UsingAssemblyTestCase
	{
		[Fact]
		public void Installers_by_type()
		{
			var container =
 new WindsorContainer(ConfigHelper.ResolveConfigPath("Configuration2/config_with_using_assembly.xml"));
			var manager = container.Kernel.GetSubSystem(SubSystemConstants.ConversionManagerKey) as IConversionManager;
			var type = manager.PerformConversion<Type>("BrushMappingMode");
			Assert.NotNull(type);
		}
	}
}

#endif