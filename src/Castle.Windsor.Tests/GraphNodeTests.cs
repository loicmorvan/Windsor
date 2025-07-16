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

namespace Castle.Core.Tests;

using Castle.Core.Internal;

public class GraphTestCase
{
	[Fact]
	public void SimpleUsage()
	{
		var parent = new GraphNode();
		var child = new GraphNode();

		parent.AddDependent(child);

		Assert.Same(child, parent.Dependents[0]);
	}

	[Fact]
	public void TopologicalSortOneElement()
	{
		GraphNode alone = new TestGraphNode("alone");

		var nodes = TopologicalSortAlgo.Sort(new[] { alone });

		Assert.Same(alone, nodes[0]);
	}

	[Fact]
	public void TopologicalSortSimple()
	{
		GraphNode alone = new TestGraphNode("alone");
		GraphNode first = new TestGraphNode("first");
		GraphNode second = new TestGraphNode("second");
		GraphNode third = new TestGraphNode("third");

		first.AddDependent(second);
		second.AddDependent(third);

		var nodes =
			TopologicalSortAlgo.Sort(new[] { alone, second, first, third });

		Assert.Same(first, nodes[0]);
		Assert.Same(second, nodes[1]);
		Assert.Same(third, nodes[2]);
		Assert.Same(alone, nodes[3]);
	}

	[Fact]
	public void ComplexDag()
	{
		GraphNode shirt = new TestGraphNode("shirt");
		GraphNode tie = new TestGraphNode("tie");
		GraphNode jacket = new TestGraphNode("jacket");
		GraphNode belt = new TestGraphNode("belt");
		GraphNode watch = new TestGraphNode("watch");
		GraphNode undershorts = new TestGraphNode("undershorts");
		GraphNode pants = new TestGraphNode("pants");
		GraphNode shoes = new TestGraphNode("shoes");
		GraphNode socks = new TestGraphNode("socks");

		shirt.AddDependent(belt);
		shirt.AddDependent(tie);

		tie.AddDependent(jacket);

		pants.AddDependent(belt);
		pants.AddDependent(shoes);

		undershorts.AddDependent(pants);
		undershorts.AddDependent(shoes);

		socks.AddDependent(shoes);
		belt.AddDependent(jacket);

		var nodes =
			TopologicalSortAlgo.Sort(
				new[]
					{ shirt, tie, jacket, belt, watch, undershorts, pants, shoes, socks });

		Assert.Same(socks, nodes[0]);
		Assert.Same(undershorts, nodes[1]);
		Assert.Same(pants, nodes[2]);
		Assert.Same(shoes, nodes[3]);
		Assert.Same(watch, nodes[4]);
		Assert.Same(shirt, nodes[5]);
		Assert.Same(tie, nodes[6]);
		Assert.Same(belt, nodes[7]);
		Assert.Same(jacket, nodes[8]);
	}
}

public class TestGraphNode : GraphNode
{
	public TestGraphNode(string name)
	{
		Name = name;
	}

	public string Name { get; }
}