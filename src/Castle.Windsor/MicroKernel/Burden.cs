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

using System;
using System.Collections.Generic;
using System.Linq;
using Castle.Windsor.Core;
using Castle.Windsor.Core.Internal;
using Castle.Windsor.MicroKernel.LifecycleConcerns;

namespace Castle.Windsor.MicroKernel;

public class Burden
{
	private Decommission _decommission = Decommission.No;

	private List<Burden> _dependencies;

	internal Burden(IHandler handler, bool requiresDecommission, bool trackedExternally)
	{
		Handler = handler;
		TrackedExternally = trackedExternally;
		if (requiresDecommission)
		{
			_decommission = Decommission.Yes;
		}
		else if (Model.Lifecycle.HasDecommissionConcerns)
		{
			if (Model.Implementation == typeof(LateBoundComponent) && Model.Lifecycle.DecommissionConcerns.All(IsLateBound))
				_decommission = Decommission.LateBound;
			else
				_decommission = Decommission.Yes;
		}
	}

	public IHandler Handler { get; }

	public object Instance { get; private set; }

	public ComponentModel Model => Handler.ComponentModel;

	public bool RequiresDecommission
	{
		get => _decommission != Decommission.No;
		set => _decommission = value ? Decommission.Yes : Decommission.No;
	}

	/// <summary>
	///     If <c>true</c> requires release by <see cref = "IReleasePolicy" /> . If <c>false</c> , the object has a well defined, detectable end of life (web-request end, disposal of the container etc), and
	///     will be released externally.
	/// </summary>
	public bool RequiresPolicyRelease => TrackedExternally == false && RequiresDecommission;

	private bool TrackedExternally { get; }

	public void AddChild(Burden child)
	{
		_dependencies ??= new List<Burden>(Model.Dependents.Length);
		_dependencies.Add(child);

		if (child.RequiresDecommission) _decommission = Decommission.Yes;
	}

	public bool Release()
	{
		var releasing = Releasing;
		if (releasing != null) releasing(this);

		if (Handler.Release(this) == false) return false;

		var released = Released;
		if (released != null) released(this);

		_dependencies?.ForEach(c => c.Release());
		var graphReleased = GraphReleased;
		if (graphReleased != null) graphReleased(this);
		return true;
	}

	public void SetRootInstance(object instance)
	{
		ArgumentNullException.ThrowIfNull(instance);
		Instance = instance;
		if (_decommission == Decommission.LateBound)
			// TODO: this may need to be extended if we lazily provide any other decimmission concerns
			RequiresDecommission = instance is IDisposable;
	}

	private bool IsLateBound(IDecommissionConcern arg)
	{
		return arg is LateBoundConcerns<IDecommissionConcern>;
	}

	public event BurdenReleaseDelegate Released;
	public event BurdenReleaseDelegate Releasing;
	public event BurdenReleaseDelegate GraphReleased;

	private enum Decommission : byte
	{
		No,
		Yes,
		LateBound
	}
}