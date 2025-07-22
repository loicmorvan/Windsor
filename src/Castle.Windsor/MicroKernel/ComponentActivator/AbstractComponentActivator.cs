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

using System;
using System.Collections.Generic;
using Castle.DynamicProxy;
using Castle.Windsor.Core;
using Castle.Windsor.MicroKernel.Context;

namespace Castle.Windsor.MicroKernel.ComponentActivator;

/// <summary>
///     Abstract implementation of <see cref = "IComponentActivator" />. The implementors must only override the InternalCreate and InternalDestroy methods in order to perform their creation and
///     destruction logic.
/// </summary>
[Serializable]
public abstract class AbstractComponentActivator : IComponentActivator
{
	/// <summary>Constructs an AbstractComponentActivator</summary>
	protected AbstractComponentActivator(ComponentModel model, IKernelInternal kernel, ComponentInstanceDelegate onCreation, ComponentInstanceDelegate onDestruction)
	{
		Model = model;
		Kernel = kernel;
		OnCreation = onCreation;
		OnDestruction = onDestruction;
	}

	public IKernelInternal Kernel { get; }

	public ComponentModel Model { get; }

	public ComponentInstanceDelegate OnCreation { get; }

	public ComponentInstanceDelegate OnDestruction { get; }

	public virtual object Create(CreationContext context, Burden burden)
	{
		var instance = InternalCreate(context);
		burden.SetRootInstance(instance);

		OnCreation(Model, instance);

		return instance;
	}

	public virtual void Destroy(object instance)
	{
		InternalDestroy(instance);

		OnDestruction(Model, instance);
	}

	protected abstract object InternalCreate(CreationContext context);

	protected abstract void InternalDestroy(object instance);

	protected virtual void ApplyCommissionConcerns(object instance)
	{
		if (Model.Lifecycle.HasCommissionConcerns == false) return;

		instance = ProxyUtil.GetUnproxiedInstance(instance);
		if (instance == null)
			// see http://issues.castleproject.org/issue/IOC-332 for details
			throw new NotSupportedException(
				$"Can not apply commission concerns to component {Model.Name} because it appears to be a target-less proxy. Currently those are not supported.");
		ApplyConcerns(Model.Lifecycle.CommissionConcerns, instance);
	}

	protected virtual void ApplyConcerns(IEnumerable<ICommissionConcern> steps, object instance)
	{
		foreach (var concern in steps) concern.Apply(Model, instance);
	}

	protected virtual void ApplyConcerns(IEnumerable<IDecommissionConcern> steps, object instance)
	{
		foreach (var concern in steps) concern.Apply(Model, instance);
	}

	protected virtual void ApplyDecommissionConcerns(object instance)
	{
		if (Model.Lifecycle.HasDecommissionConcerns == false) return;

		instance = ProxyUtil.GetUnproxiedInstance(instance);
		ApplyConcerns(Model.Lifecycle.DecommissionConcerns, instance);
	}
}