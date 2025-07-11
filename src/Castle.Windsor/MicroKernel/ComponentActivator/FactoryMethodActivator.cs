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
using Castle.Core;
using Castle.DynamicProxy;
using Castle.MicroKernel.Context;

namespace Castle.MicroKernel.ComponentActivator;

public class FactoryMethodActivator<T> : DefaultComponentActivator, IDependencyAwareActivator
{
	protected readonly Func<IKernel, ComponentModel, CreationContext, T> Creator;
	protected readonly bool ManagedExternally;

	public FactoryMethodActivator(ComponentModel model, IKernelInternal kernel, ComponentInstanceDelegate onCreation,
		ComponentInstanceDelegate onDestruction)
		: base(model, kernel, onCreation, onDestruction)
	{
		Creator =
			Model.ExtendedProperties["factoryMethodDelegate"] as Func<IKernel, ComponentModel, CreationContext, T>;
		ManagedExternally = (Model.ExtendedProperties["factory.managedExternally"] as bool?).GetValueOrDefault();
		if (Creator == null)
			throw new ComponentActivatorException(
				string.Format(
					"{0} received misconfigured component model for {1}. Are you sure you registered this component with 'UsingFactoryMethod'?",
					GetType().Name, Model.Name), Model);
	}

	public bool CanProvideRequiredDependencies(ComponentModel component)
	{
		// the factory will take care of providing all dependencies.
		return true;
	}

	public bool IsManagedExternally(ComponentModel component)
	{
		return ManagedExternally;
	}

	protected override void ApplyCommissionConcerns(object instance)
	{
		if (ManagedExternally) return;
		base.ApplyCommissionConcerns(instance);
	}

	protected override void ApplyDecommissionConcerns(object instance)
	{
		if (ManagedExternally) return;
		base.ApplyDecommissionConcerns(instance);
	}

	protected override object Instantiate(CreationContext context)
	{
		object instance = Creator(Kernel, Model, context);
		if (ShouldCreateProxy(instance)) instance = Kernel.ProxyFactory.Create(Kernel, instance, Model, context);
		if (instance == null)
			throw new ComponentActivatorException(
				string.Format(
					"Factory method creating instances of component '{0}' returned null. This is not allowed and most likely a bug in the factory method.",
					Model.Name), Model);
		return instance;
	}

	protected override void SetUpProperties(object instance, CreationContext context)
	{
		// we don't
	}

	private bool ShouldCreateProxy(object instance)
	{
		if (instance == null) return false;
		if (Kernel.ProxyFactory.ShouldCreateProxy(Model) == false) return false;
		return ProxyUtil.IsProxy(instance) == false;
	}
}