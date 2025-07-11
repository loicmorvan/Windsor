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
using Castle.Core.Internal;
using Castle.MicroKernel;

namespace Castle.Facilities.TypedFactory;

public class TypedFactoryConfiguration
{
	private readonly string _defaultComponentSelectorKey;
	private IReference<ITypedFactoryComponentSelector> _selectorReference;

	public TypedFactoryConfiguration(string defaultComponentSelectorKey, Type factoryType)
	{
		_defaultComponentSelectorKey = defaultComponentSelectorKey;
		var attributes = factoryType.GetAttributes<FactoryAttribute>(true);
		if (attributes.Length > 0)
		{
			var defaults = attributes[0];
			if (defaults.SelectorComponentName != null)
				SelectedWith(defaults.SelectorComponentName);
			else if (defaults.SelectorComponentType != null)
				SelectedWith(defaults.SelectorComponentType);
			else if (defaults.SelectorType != null)
				SelectedWith(defaults.SelectorType.CreateInstance<ITypedFactoryComponentSelector>());
		}
	}

	internal IReference<ITypedFactoryComponentSelector> Reference
	{
		get
		{
			if (_selectorReference == null) SelectedWith(_defaultComponentSelectorKey);

			return _selectorReference;
		}
	}

	public void SelectedWith(string selectorComponentName)
	{
		_selectorReference = new ComponentReference<ITypedFactoryComponentSelector>(selectorComponentName);
	}

	public void SelectedWith<TSelectorComponent>() where TSelectorComponent : ITypedFactoryComponentSelector
	{
		SelectedWith(typeof(TSelectorComponent));
	}

	public void SelectedWith(Type selectorComponentType)
	{
		_selectorReference = new ComponentReference<ITypedFactoryComponentSelector>(selectorComponentType);
	}

	public void SelectedWith(ITypedFactoryComponentSelector selector)
	{
		ArgumentNullException.ThrowIfNull(selector);

		_selectorReference = new InstanceReference<ITypedFactoryComponentSelector>(selector);
	}
}