﻿// Copyright 2004-2012 Castle Project - http://www.castleproject.org/
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

using System;
using System.Collections.Generic;
using Castle.Core.Internal;
using Castle.MicroKernel;
using Castle.Windsor.Diagnostics.DebuggerViews;

namespace Castle.Windsor.Diagnostics.Extensions;

public class PotentiallyMisconfiguredComponents : AbstractContainerDebuggerExtension
{
	private const string Name = "Potentially misconfigured components";
	private IPotentiallyMisconfiguredComponentsDiagnostic _diagnostic;

	public override IEnumerable<DebuggerViewItem> Attach()
	{
		var handlers = _diagnostic.Inspect();
		if (handlers.Length == 0) return [];

		Array.Sort(handlers,
			(f, s) => string.Compare(f.ComponentModel.Name, s.ComponentModel.Name, StringComparison.Ordinal));
		var items = handlers.ConvertAll(DefaultComponentView);
		return
		[
			new DebuggerViewItem(Name, "Count = " + items.Length, items)
		];
	}

	public override void Init(IKernel kernel, IDiagnosticsHost diagnosticsHost)
	{
		_diagnostic = new PotentiallyMisconfiguredComponentsDiagnostic(kernel);
		diagnosticsHost.AddDiagnostic(_diagnostic);
	}
}