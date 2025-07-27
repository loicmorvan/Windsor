// Copyright 2004-2022 Castle Project - http://www.castleproject.org/
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
using Castle.Windsor.Core;
using Castle.Windsor.Core.Internal;
using Castle.Windsor.MicroKernel;
using Castle.Windsor.Windsor.Diagnostics.DebuggerViews;

namespace Castle.Windsor.Windsor.Diagnostics.Extensions;

public class DuplicatedDependenciesDebuggerExtension : AbstractContainerDebuggerExtension
{
	private const string Name = "Components with potentially duplicated dependencies";

	private DuplicatedDependenciesDiagnostic _diagnostic;

	public override IEnumerable<DebuggerViewItem> Attach()
	{
		var result = _diagnostic.Inspect();
		if (result.Length == 0) return [];
		var items = BuildItems(result);
		return
		[
			new DebuggerViewItem(Name, "Count = " + items.Length, items)
		];
	}

	public override void Init(IKernel kernel, IDiagnosticsHost diagnosticsHost)
	{
		_diagnostic = new DuplicatedDependenciesDiagnostic(kernel);
		diagnosticsHost.AddDiagnostic<IDuplicatedDependenciesDiagnostic>(_diagnostic);
	}

	private ComponentDebuggerView[] BuildItems(Tuple<IHandler, DependencyDuplicate[]>[] results)
	{
		return results.ConvertAll(ComponentWithDuplicateDependenciesView);
	}

	private ComponentDebuggerView ComponentWithDuplicateDependenciesView(Tuple<IHandler, DependencyDuplicate[]> input)
	{
		var handler = input.Item1;
		var mismatches = input.Item2;
		var items = mismatches.ConvertAll(MismatchView);
		Array.Sort(items, (c1, c2) => string.Compare(c1.Name, c2.Name, StringComparison.Ordinal));
		return ComponentDebuggerView.BuildRawFor(handler, "Count = " + mismatches.Length, items);
	}

	private DebuggerViewItemWithDetails MismatchView(DependencyDuplicate input)
	{
		return new DebuggerViewItemWithDetails(Description(input.Dependency1), Description(input.Dependency2),
			_diagnostic.GetDetails(input));
	}

	private static string Description(DependencyModel dependencyModel)
	{
		return dependencyModel.TargetItemType.ToCSharpString() + " " + dependencyModel.DependencyKey;
	}
}