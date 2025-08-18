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

using Castle.Windsor.Core;
using Castle.Windsor.Core.Internal;
using Castle.Windsor.MicroKernel;
using Castle.Windsor.MicroKernel.Registration;
using Castle.Windsor.Tests.Components;

namespace Castle.Windsor.Tests;

public sealed class GraphTestCase : IDisposable
{
    private readonly DefaultKernel _kernel = new();

    public void Dispose()
    {
        _kernel.Dispose();
    }

    [Fact]
    public void TopologicalSortOnComponents()
    {
        _kernel.Register(Component.For(typeof(A)).Named("a"));
        _kernel.Register(Component.For(typeof(B)).Named("b"));
        _kernel.Register(Component.For(typeof(C)).Named("c"));

        var nodes = _kernel.GraphNodes;

        Assert.NotNull(nodes);
        Assert.Equal(3, nodes.Length);

        var vertices = TopologicalSortAlgo.Sort(nodes);

        Assert.Equal("c", ((ComponentModel)vertices[0]).Name);
        Assert.Equal("b", ((ComponentModel)vertices[1]).Name);
        Assert.Equal("a", ((ComponentModel)vertices[2]).Name);
    }
}