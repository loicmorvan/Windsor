﻿// Copyright 2004-2011 Castle Project - http://www.castleproject.org/
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
using System.Reflection;
using Castle.Core;
using Castle.Core.Interceptor;
using Castle.DynamicProxy;
using Castle.MicroKernel;
using Castle.MicroKernel.Facilities;

namespace Castle.Facilities.TypedFactory.Internal;

[Transient]
public class TypedFactoryInterceptor(IKernelInternal kernel, ITypedFactoryComponentSelector componentSelector)
	: IInterceptor, IOnBehalfAware, IDisposable
{
	private readonly IReleasePolicy _scope = kernel.ReleasePolicy.CreateSubPolicy();

	private bool _disposed;
	private IDictionary<MethodInfo, FactoryMethod> _methods;

	public ITypedFactoryComponentSelector ComponentSelector { get; } = componentSelector;

	public void Dispose()
	{
		if (_disposed) return;

		_disposed = true;
		_scope.Dispose();
	}

	public void Intercept(IInvocation invocation)
	{
		// don't check whether the factory was already disposed: it may be a call to Dispose or
		// Release methods, which must remain functional after dispose as well
		FactoryMethod method;
		if (TryGetMethod(invocation, out method) == false)
			throw new InvalidOperationException(
				string.Format(
					"Can't find information about factory method {0}. This is most likely a bug. Please report it.",
					invocation.Method));
		switch (method)
		{
			case FactoryMethod.Resolve:
				Resolve(invocation);
				break;
			case FactoryMethod.Release:
				Release(invocation);
				break;
			case FactoryMethod.Dispose:
				Dispose();
				break;
		}
	}

	public void SetInterceptedComponentModel(ComponentModel target)
	{
		_methods =
			(IDictionary<MethodInfo, FactoryMethod>)target.ExtendedProperties[TypedFactoryFacility.FactoryMapCacheKey];
		if (_methods == null)
			throw new ArgumentException(
				string.Format("Component {0} is not a typed factory. {1} only works with typed factories.", target.Name,
					GetType().Name));
	}

	private void Release(IInvocation invocation)
	{
		if (_disposed) return;

		for (var i = 0; i < invocation.Arguments.Length; i++) _scope.Release(invocation.Arguments[i]);
	}

	private void Resolve(IInvocation invocation)
	{
		if (_disposed) throw new ObjectDisposedException("this", "The factory was disposed and can no longer be used.");

		var component =
			ComponentSelector.SelectComponent(invocation.Method, invocation.TargetType, invocation.Arguments);
		if (component == null)
			throw new FacilityException(
				string.Format(
					"Selector {0} didn't select any component for method {1}. This usually signifies a bug in the selector.",
					ComponentSelector,
					invocation.Method));
		invocation.ReturnValue = component(kernel, _scope);
	}

	private bool TryGetMethod(IInvocation invocation, out FactoryMethod method)
	{
		if (_methods.TryGetValue(invocation.Method, out method)) return true;
		if (invocation.Method.IsGenericMethod == false) return false;
		return _methods.TryGetValue(invocation.Method.GetGenericMethodDefinition(), out method);
	}
}