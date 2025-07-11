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

namespace Castle.Windsor.Diagnostics.DebuggerViews;

using System.Diagnostics;

[DebuggerDisplay("{description,nq}", Name = "{name,nq}")]
public class DebuggerViewItemWithDetails(string name, string description, string details, params object[] items)
{
	[DebuggerBrowsable(DebuggerBrowsableState.Never)]
	private readonly object description = description;

	[DebuggerBrowsable(DebuggerBrowsableState.Never)]
	private readonly string name = name;

	[DebuggerBrowsable(DebuggerBrowsableState.Never)]
	public object Description
	{
		get { return description; }
	}

	public string Details { get; private set; } = details;

	[DebuggerBrowsable(DebuggerBrowsableState.RootHidden)]
	public object[] Items { get; set; } = items;

	[DebuggerBrowsable(DebuggerBrowsableState.Never)]
	public string Name
	{
		get { return name; }
	}
}