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

public class AllComponents : AbstractContainerDebuggerExtension
{
	private const string Name = "All components";

	private AllComponentsDiagnostic _diagnostic;

	public override IEnumerable<DebuggerViewItem> Attach()
	{
		var handlers = _diagnostic.Inspect();

		var items = handlers.ConvertAll(DefaultComponentView);
		Array.Sort(items, (c1, c2) => string.Compare(c1.Name, c2.Name, StringComparison.Ordinal));
		return
		[
			new DebuggerViewItem(Name, "Count = " + items.Length, items)
		];
	}

	public override void Init(IKernel kernel, IDiagnosticsHost diagnosticsHost)
	{
		_diagnostic = new AllComponentsDiagnostic(kernel);
		diagnosticsHost.AddDiagnostic(_diagnostic);
	}
}