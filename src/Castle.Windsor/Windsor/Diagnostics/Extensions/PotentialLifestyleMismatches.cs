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
using System.Diagnostics;
using System.Linq;
using System.Text;
using Castle.Core;
using Castle.Core.Internal;
using Castle.MicroKernel;
using Castle.Windsor.Diagnostics.DebuggerViews;
using Castle.Windsor.Diagnostics.Helpers;

namespace Castle.Windsor.Diagnostics.Extensions;

public class PotentialLifestyleMismatches : AbstractContainerDebuggerExtension
{
	private const string Name = "Potential lifestyle mismatches";
	private IPotentialLifestyleMismatchesDiagnostic _diagnostic;

	public override IEnumerable<DebuggerViewItem> Attach()
	{
		var mismatches = _diagnostic.Inspect();
		if (mismatches.Length == 0) return [];

		Array.Sort(mismatches,
			(f, s) => string.Compare(f[0].ComponentModel.Name, s[0].ComponentModel.Name, StringComparison.Ordinal));
		var items = mismatches.ConvertAll(MismatchedComponentView);
		return
		[
			new DebuggerViewItem(Name, "Count = " + mismatches.Length, items)
		];
	}

	public override void Init(IKernel kernel, IDiagnosticsHost diagnosticsHost)
	{
		_diagnostic = new PotentialLifestyleMismatchesDiagnostic(kernel);
		diagnosticsHost.AddDiagnostic(_diagnostic);
	}

	private string GetKey(IHandler root)
	{
		return string.Format("\"{0}\" »{1}«", GetNameDescription(root.ComponentModel),
			root.ComponentModel.GetLifestyleDescription());
	}

	private string GetMismatchMessage(IHandler[] handlers)
	{
		var message = new StringBuilder();
		Debug.Assert(handlers.Length > 1);
		var root = handlers.First();
		var last = handlers.Last();
		message.Append(
			$"Component '{GetNameDescription(root.ComponentModel)}' with lifestyle {root.ComponentModel.GetLifestyleDescription()} ");
		message.Append(
			$"depends on '{GetNameDescription(last.ComponentModel)}' with lifestyle {last.ComponentModel.GetLifestyleDescription()}");

		for (var i = 1; i < handlers.Length - 1; i++)
		{
			var via = handlers[i];
			message.AppendLine();
			message.AppendFormat("\tvia '{0}' with lifestyle {1}", GetNameDescription(via.ComponentModel),
				via.ComponentModel.GetLifestyleDescription());
		}

		message.AppendLine();
		message.AppendFormat(
			"This kind of dependency is usually not desired and may lead to various kinds of bugs.");
		return message.ToString();
	}

	private string GetName(IHandler[] handlers, IHandler root)
	{
		var indirect = handlers.Length > 2 ? "indirectly " : string.Empty;
		return
			$"\"{GetNameDescription(root.ComponentModel)}\" »{root.ComponentModel.GetLifestyleDescription()}« {indirect}depends on";
	}

	private string GetNameDescription(ComponentModel componentModel)
	{
		return componentModel.ComponentName.SetByUser
			? componentModel.ComponentName.Name
			: componentModel.ToString();
	}

	private object MismatchedComponentView(IHandler[] handlers)
	{
		return new DebuggerViewItemWithDetails(GetName(handlers, handlers.First()),
			GetKey(handlers.Last()),
			GetMismatchMessage(handlers),
			handlers.ConvertAll(object (h) => ComponentDebuggerView.BuildFor(h)));
	}
}