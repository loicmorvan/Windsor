// Copyright 2004-2012 Castle Project - http://www.castleproject.org/
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

using System.Diagnostics;
using Castle.Windsor.MicroKernel;
using Castle.Windsor.Windsor.Diagnostics.DebuggerViews;

namespace Castle.Windsor.Windsor.Diagnostics;

[DebuggerDisplay("")]
internal class KernelDebuggerProxy(IKernel kernel)
{
    private readonly IEnumerable<IContainerDebuggerExtension> _extensions = (IEnumerable<IContainerDebuggerExtension>?)kernel.GetSubSystem<IContainerDebuggerExtensionHost>(SubSystemConstants.DiagnosticsKey) ?? [];

    public KernelDebuggerProxy(IWindsorContainer container) : this(container.Kernel)
    {
    }

    [DebuggerDisplay("")]
    [DebuggerBrowsable(DebuggerBrowsableState.RootHidden)]
    public DebuggerViewItem[] Extensions
    {
        get { return _extensions.SelectMany(e => e.Attach()).ToArray(); }
    }
}