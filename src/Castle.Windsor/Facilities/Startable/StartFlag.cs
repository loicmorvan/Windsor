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

namespace Castle.Facilities.Startable;

using System.Collections.Generic;

using Castle.MicroKernel;
using Castle.MicroKernel.Context;

public class StartFlag : IStartFlagInternal
{
	protected readonly List<IHandler> WaitList = [];
	protected StartableFacility.StartableEvents Events;

	public virtual void Signal()
	{
		Events.StartableComponentRegistered -= CacheHandler;
		StartAll();
	}

	protected void CacheHandler(IHandler handler)
	{
		WaitList.Add(handler);
	}

	protected virtual void Init()
	{
		Events.StartableComponentRegistered += CacheHandler;
	}

	protected virtual void Start(IHandler handler)
	{
		handler.Resolve(CreationContext.CreateEmpty());
	}

	protected void StartAll()
	{
		var array = WaitList.ToArray();
		WaitList.Clear();
		foreach (var handler in array)
		{
			Start(handler);
		}
	}

	void IStartFlagInternal.Init(StartableFacility.StartableEvents events)
	{
		this.Events = events;
		Init();
	}
}