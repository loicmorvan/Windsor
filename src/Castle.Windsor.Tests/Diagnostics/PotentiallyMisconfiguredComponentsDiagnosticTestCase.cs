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

using Castle.Windsor.MicroKernel;
using Castle.Windsor.MicroKernel.Registration;
using Castle.Windsor.Tests.Components;
using Castle.Windsor.Windsor.Diagnostics;

namespace Castle.Windsor.Tests.Diagnostics;

public class PotentiallyMisconfiguredComponentsDiagnosticTestCase : AbstractContainerTestCase
{
    private IPotentiallyMisconfiguredComponentsDiagnostic _diagnostic;

    protected override void AfterContainerCreated()
    {
        var host = Kernel.GetSubSystem(SubSystemConstants.DiagnosticsKey) as IDiagnosticsHost;
        _diagnostic = host.GetDiagnostic<IPotentiallyMisconfiguredComponentsDiagnostic>();
    }

    [Fact]
    public void Empty_when_all_components_healthy()
    {
        Container.Register(Component.For<A>(), Component.For<B>(), Component.For<C>());

        var handlers = _diagnostic.Inspect();

        Assert.Empty(handlers);
    }

    [Fact]
    public void Has_all_components_with_missing_or_waiting_dependencies()
    {
        Container.Register(Component.For<B>(), Component.For<C>());

        var handlers = _diagnostic.Inspect();

        Assert.Equal(2, handlers.Length);
    }
}