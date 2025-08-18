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

using System.Diagnostics;

namespace Castle.Windsor.Core;

/// <summary>Represents a collection of ordered lifecycle concerns.</summary>
[Serializable]
public class LifecycleConcernsCollection
{
    [DebuggerBrowsable(DebuggerBrowsableState.Never)]
    private List<ICommissionConcern> _commission;

    [DebuggerBrowsable(DebuggerBrowsableState.Never)]
    private List<IDecommissionConcern> _decommission;

    /// <summary>Returns all concerns for the commission phase</summary>
    /// <value></value>
    public IEnumerable<ICommissionConcern> CommissionConcerns => !HasCommissionConcerns ? [] : _commission;

    /// <summary>Returns all concerns for the decommission phase</summary>
    /// <value></value>
    public IEnumerable<IDecommissionConcern> DecommissionConcerns =>
        !HasDecommissionConcerns ? [] : _decommission;

    /// <summary>Gets a value indicating whether this instance has commission steps.</summary>
    /// <value><c>true</c> if this instance has commission steps; otherwise, <c>false</c>.</value>
    [DebuggerBrowsable(DebuggerBrowsableState.Never)]
    public bool HasCommissionConcerns => _commission != null && _commission.Count != 0;

    /// <summary>Gets a value indicating whether this instance has decommission steps.</summary>
    /// <value><c>true</c> if this instance has decommission steps; otherwise, <c>false</c>.</value>
    [DebuggerBrowsable(DebuggerBrowsableState.Never)]
    public bool HasDecommissionConcerns => _decommission != null && _decommission.Count != 0;

    [DebuggerBrowsable(DebuggerBrowsableState.Never)]
    private List<ICommissionConcern> Commission => _commission ??= [];

    [DebuggerBrowsable(DebuggerBrowsableState.Never)]
    private List<IDecommissionConcern> Decommission => _decommission ??= [];

    public void Add(ICommissionConcern concern)
    {
        ArgumentNullException.ThrowIfNull(concern);
        Commission.Add(concern);
    }

    public void Add(IDecommissionConcern concern)
    {
        ArgumentNullException.ThrowIfNull(concern);
        Decommission.Add(concern);
    }

    public void AddFirst(ICommissionConcern concern)
    {
        ArgumentNullException.ThrowIfNull(concern);
        Commission.Insert(0, concern);
    }

    public void AddFirst(IDecommissionConcern concern)
    {
        ArgumentNullException.ThrowIfNull(concern);
        Decommission.Insert(0, concern);
    }

    public void Remove(ICommissionConcern concern)
    {
        ArgumentNullException.ThrowIfNull(concern);
        Commission.Remove(concern);
    }

    public void Remove(IDecommissionConcern concern)
    {
        ArgumentNullException.ThrowIfNull(concern);
        Decommission.Remove(concern);
    }
}