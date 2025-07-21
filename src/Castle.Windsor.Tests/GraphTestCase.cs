// Copyright 2004-2011 Castle Project - http://www.castleproject.org/
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

namespace Castle.Windsor.Tests;

using System;

using Castle.Windsor.Core;
using Castle.Windsor.Core.Internal;
using Castle.Windsor.MicroKernel;
using Castle.Windsor.MicroKernel.Registration;
using Castle.Windsor.Tests.Components;

public class GraphTestCase : IDisposable
{
	private readonly IKernel kernel;

	public GraphTestCase()
	{
		kernel = new DefaultKernel();
	}

	public void Dispose()
	{
		kernel.Dispose();
	}

	[Fact]
	public void TopologicalSortOnComponents()
	{
		kernel.Register(Component.For(typeof(A)).Named("a"));
		kernel.Register(Component.For(typeof(B)).Named("b"));
		kernel.Register(Component.For(typeof(C)).Named("c"));

		var nodes = kernel.GraphNodes;

		Assert.NotNull(nodes);
		Assert.Equal(3, nodes.Length);

		var vertices = TopologicalSortAlgo.Sort(nodes);

		Assert.Equal("c", (vertices[0] as ComponentModel).Name);
		Assert.Equal("b", (vertices[1] as ComponentModel).Name);
		Assert.Equal("a", (vertices[2] as ComponentModel).Name);
	}
}