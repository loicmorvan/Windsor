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

// ReSharper disable NotAccessedField.Local
// ReSharper disable UnusedMember.Local

namespace Castle.Windsor.Windsor.Diagnostics.DebuggerViews;

public class MasterDetailsDebuggerViewItem(object master, string masterDescription, string masterName, object?[] details)
{
    [DebuggerBrowsable(DebuggerBrowsableState.Never)]
    private readonly string _masterDescription = masterDescription;

    [DebuggerBrowsable(DebuggerBrowsableState.Never)]
    private readonly string _masterName = masterName;

    /// <summary>
    ///     Stupid name, but debugger views in Visual Studio display items in alphabetical order so if we want to have that
    ///     item on top its name must be alphabetically before <see cref="Details" />
    /// </summary>
    [DebuggerDisplay("{masterDescription,nq}", Name = "{masterName,nq}")]
    // ReSharper disable once UnusedAutoPropertyAccessor.Global
    // ReSharper disable once UnusedMember.Global
    public object AMaster { get; } = master;

    [DebuggerBrowsable(DebuggerBrowsableState.RootHidden)]
    public object?[] Details { get; } = details;
}