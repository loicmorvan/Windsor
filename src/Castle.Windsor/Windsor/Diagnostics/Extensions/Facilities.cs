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

using System.Collections.Generic;
using Castle.MicroKernel;
using Castle.Windsor.Diagnostics.DebuggerViews;

namespace Castle.Windsor.Diagnostics.Extensions;

public class Facilities : IContainerDebuggerExtension
{
	private IKernel _kernel;

	public IEnumerable<DebuggerViewItem> Attach()
	{
		var facilities = _kernel.GetFacilities();
		if (facilities.Length == 0) yield break;
		yield return new DebuggerViewItem("Facilities", "Count = " + facilities.Length, facilities);
	}

	public void Init(IKernel kernel, IDiagnosticsHost diagnosticsHost)
	{
		_kernel = kernel;
	}
}