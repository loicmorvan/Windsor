// Copyright 2004-2009 Castle Project - http://www.castleproject.org/
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

using Castle.MicroKernel.Registration;

namespace Castle.Windsor.Tests.Bugs;

public class IoC102
{
	[Fact]
	public void ComponentResolutionOrderForKernelAndDpendencyResolverIsTheSame()
	{
		var container = new WindsorContainer()
			.Register(
				Component.For<IReader>()
					.ImplementedBy<AlphaReader>(),
				Component.For<IReader>()
					.ImplementedBy<BetaReader>(),
				Component.For<Consumer>()
			);

		var consumer = container.Resolve<Consumer>();
		var reader2 = container.Resolve<IReader>();
		Assert.Same(reader2, consumer.Reader);
	}

	public interface IReader
	{
		string Read();
	}

	public class AlphaReader : IReader
	{
		public string Read()
		{
			return "Alpha";
		}
	}

	public class BetaReader : IReader
	{
		public string Read()
		{
			return "Beta";
		}
	}

	public class Consumer(IReader reader)
	{
		public readonly IReader Reader = reader;
	}
}