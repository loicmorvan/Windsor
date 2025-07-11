// Copyright 2004-2014 Castle Project - http://www.castleproject.org/
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
using Castle.MicroKernel;
using Castle.MicroKernel.Context;

namespace Castle.Facilities.Startable;

public partial class StartableFacility
{
	private class DeferredStartFlag : StartFlag
	{
		public override void Signal()
		{
			StartAll();
		}

		protected override void Init()
		{
			base.Init();
			Events.Kernel.RegistrationCompleted += delegate { Signal(); };
		}
	}

	private class DeferredTryStartFlag : DeferredStartFlag
	{
		protected override void Start(IHandler handler)
		{
			if (TryStart(handler) == false) CacheHandler(handler);
		}

		private bool TryStart(IHandler handler)
		{
			return handler.TryResolve(CreationContext.CreateEmpty()) != null;
		}
	}

	private class LegacyStartFlag : StartFlag
	{
		/// <remarks>Don't check the waiting list while this flag is set as this could result in duplicate singletons.</remarks>
		private bool _inStart;

		public override void Signal()
		{
			StartAll();
		}

		protected override void Init()
		{
			base.Init();
			Events.Kernel.ComponentRegistered += delegate
			{
				if (_inStart == false) Signal();
			};
		}

		protected override void Start(IHandler handler)
		{
			if (TryStart(handler) == false) CacheHandler(handler);
		}

		/// <summary>Request the component instance</summary>
		/// <param name="handler"></param>
		private bool TryStart(IHandler handler)
		{
			try
			{
				_inStart = true;
				return handler.TryResolve(CreationContext.CreateEmpty()) != null;
			}
			finally
			{
				_inStart = false;
			}
		}
	}

	public class StartableEvents
	{
		public StartableEvents(IKernelEvents kernel)
		{
			Kernel = kernel;
			kernel.ComponentRegistered += (_, handler) =>
			{
				if (IsStartable(handler)) StartableComponentRegistered(handler);
			};
		}

		public IKernelEvents Kernel { get; }

		public event Action<IHandler> StartableComponentRegistered = delegate { };
	}
}