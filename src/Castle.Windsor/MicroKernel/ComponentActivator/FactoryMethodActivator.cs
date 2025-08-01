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

using Castle.DynamicProxy;
using Castle.Windsor.Core;
using Castle.Windsor.MicroKernel.Context;

namespace Castle.Windsor.MicroKernel.ComponentActivator;

public class FactoryMethodActivator<T> : DefaultComponentActivator, IDependencyAwareActivator
{
    private readonly Func<IKernel, ComponentModel, CreationContext, T> _creator;
    private readonly bool _managedExternally;

    public FactoryMethodActivator(ComponentModel model, IKernelInternal kernel, ComponentInstanceDelegate onCreation,
        ComponentInstanceDelegate onDestruction)
        : base(model, kernel, onCreation, onDestruction)
    {
        _creator =
            Model.ExtendedProperties["factoryMethodDelegate"] as Func<IKernel, ComponentModel, CreationContext, T>;
        _managedExternally = (Model.ExtendedProperties["factory.managedExternally"] as bool?).GetValueOrDefault();
        if (_creator == null)
        {
            throw new ComponentActivatorException(
                $"{GetType().Name} received misconfigured component model for {Model.Name}. Are you sure you registered this component with 'UsingFactoryMethod'?",
                Model);
        }
    }

    public bool CanProvideRequiredDependencies(ComponentModel component)
    {
        // the factory will take care of providing all dependencies.
        return true;
    }

    public bool IsManagedExternally(ComponentModel component)
    {
        return _managedExternally;
    }

    protected override void ApplyCommissionConcerns(object instance)
    {
        if (_managedExternally)
        {
            return;
        }

        base.ApplyCommissionConcerns(instance);
    }

    protected override void ApplyDecommissionConcerns(object instance)
    {
        if (_managedExternally)
        {
            return;
        }

        base.ApplyDecommissionConcerns(instance);
    }

    protected override object Instantiate(CreationContext context)
    {
        object instance = _creator(Kernel, Model, context);
        if (ShouldCreateProxy(instance))
        {
            instance = Kernel.ProxyFactory.Create(Kernel, instance, Model, context);
        }

        if (instance == null)
        {
            throw new ComponentActivatorException(
                $"Factory method creating instances of component '{Model.Name}' returned null. This is not allowed and most likely a bug in the factory method.",
                Model);
        }

        return instance;
    }

    protected override void SetUpProperties(object instance, CreationContext context)
    {
        // we don't
    }

    private bool ShouldCreateProxy(object instance)
    {
        if (instance == null)
        {
            return false;
        }

        if (Kernel.ProxyFactory.ShouldCreateProxy(Model) == false)
        {
            return false;
        }

        return ProxyUtil.IsProxy(instance) == false;
    }
}