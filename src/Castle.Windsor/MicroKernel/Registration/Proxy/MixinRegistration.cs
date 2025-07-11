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
using System.Collections;
using System.Collections.Generic;

namespace Castle.MicroKernel.Registration.Proxy;

public class MixinRegistration : IEnumerable<IReference<object>>
{
	private readonly IList<IReference<object>> _items = new List<IReference<object>>();

	IEnumerator IEnumerable.GetEnumerator()
	{
		return _items.GetEnumerator();
	}

	IEnumerator<IReference<object>> IEnumerable<IReference<object>>.GetEnumerator()
	{
		return _items.GetEnumerator();
	}

	public MixinRegistration Component<TService>()
	{
		return Component(typeof(TService));
	}

	public MixinRegistration Component(Type serviceType)
	{
		ArgumentNullException.ThrowIfNull(serviceType);
		_items.Add(new ComponentReference<object>(serviceType));
		return this;
	}

	public MixinRegistration Component(string name)
	{
		ArgumentNullException.ThrowIfNull(name);
		_items.Add(new ComponentReference<object>(name));
		return this;
	}

	public MixinRegistration Objects(params object[] objects)
	{
		foreach (var item in objects) _items.Add(new InstanceReference<object>(item));
		return this;
	}
}