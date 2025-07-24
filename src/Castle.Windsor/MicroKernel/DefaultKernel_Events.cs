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
using Castle.Windsor.Core;

namespace Castle.Windsor.MicroKernel;

/// <summary>Default implementation of <see cref = "IKernel" />. This implementation is complete and also support a kernel hierarchy (sub containers).</summary>
public sealed partial class DefaultKernel
{
	private readonly object _handlersChangedLock = new();
	private bool _handlersChanged;
	private volatile bool _handlersChangedDeferred;


	public IDisposable OptimizeDependencyResolution()
	{
		if (_handlersChangedDeferred) return null;

		_handlersChangedDeferred = true;

		return new OptimizeDependencyResolutionDisposable(this);
	}

	private void RaiseAddedAsChildKernel()
	{
		AddedAsChildKernel(this, EventArgs.Empty);
	}

	private void RaiseComponentCreated(ComponentModel model, object instance)
	{
		ComponentCreated(model, instance);
	}

	private void RaiseComponentDestroyed(ComponentModel model, object instance)
	{
		ComponentDestroyed(model, instance);
	}

	private void RaiseComponentModelCreated(ComponentModel model)
	{
		ComponentModelCreated(model);
	}

	private void RaiseComponentRegistered(string key, IHandler handler)
	{
		ComponentRegistered(key, handler);
	}

	private void RaiseDependencyResolving(ComponentModel client, DependencyModel model, object dependency)
	{
		DependencyResolving(client, model, dependency);
	}

	private void RaiseHandlerRegistered(IHandler handler)
	{
		var stateChanged = true;
		while (stateChanged)
		{
			stateChanged = false;
			HandlerRegistered(handler, ref stateChanged);
		}
	}

	private void RaiseHandlersChanged()
	{
		if (_handlersChangedDeferred)
		{
			lock (_handlersChangedLock)
			{
				_handlersChanged = true;
			}

			return;
		}

		DoActualRaisingOfHandlersChanged();
	}

	private void RaiseRegistrationCompleted()
	{
		RegistrationCompleted(this, EventArgs.Empty);
	}

	private void RaiseRemovedAsChildKernel()
	{
		RemovedAsChildKernel(this, EventArgs.Empty);
	}

	private void DoActualRaisingOfHandlersChanged()
	{
		var stateChanged = true;
		while (stateChanged)
		{
			stateChanged = false;
			HandlersChanged(ref stateChanged);
		}
	}

	public event HandlerDelegate HandlerRegistered = delegate { };

	public event HandlersChangedDelegate HandlersChanged = delegate { };

	public event ComponentDataDelegate ComponentRegistered = delegate { };

	public event ComponentInstanceDelegate ComponentCreated = delegate { };

	public event ComponentInstanceDelegate ComponentDestroyed = delegate { };

	public event EventHandler AddedAsChildKernel = delegate { };

	public event EventHandler RegistrationCompleted = delegate { };

	public event EventHandler RemovedAsChildKernel = delegate { };

	public event ComponentModelDelegate ComponentModelCreated = delegate { };

	public event DependencyDelegate DependencyResolving = delegate { };

	public event ServiceDelegate EmptyCollectionResolving = delegate { };

	private class OptimizeDependencyResolutionDisposable(DefaultKernel kernel) : IDisposable
	{
		public void Dispose()
		{
			lock (kernel._handlersChangedLock)
			{
				try
				{
					if (kernel._handlersChanged == false) return;

					kernel.DoActualRaisingOfHandlersChanged();
					kernel.RaiseRegistrationCompleted();
					kernel._handlersChanged = false;
				}
				finally
				{
					kernel._handlersChangedDeferred = false;
				}
			}
		}
	}
}