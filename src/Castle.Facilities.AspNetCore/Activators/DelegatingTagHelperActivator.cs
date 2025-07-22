// Copyright 2004-2018 Castle Project - http://www.castleproject.org/
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
using Microsoft.AspNetCore.Mvc.Razor;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.AspNetCore.Razor.TagHelpers;

namespace Castle.Facilities.AspNetCore.Activators;

internal sealed class DelegatingTagHelperActivator : ITagHelperActivator
{
	private readonly Predicate<Type> _customCreatorSelector;
	private readonly Func<Type, object> _customTagHelperCreator;
	private readonly ITagHelperActivator _defaultTagHelperActivator;

	public DelegatingTagHelperActivator(
		Predicate<Type> customCreatorSelector,
		Func<Type, object> customTagHelperCreator,
		ITagHelperActivator defaultTagHelperActivator)
	{
		_customCreatorSelector =
			customCreatorSelector ?? throw new ArgumentNullException(nameof(customCreatorSelector));
		_customTagHelperCreator =
			customTagHelperCreator ?? throw new ArgumentNullException(nameof(customTagHelperCreator));
		_defaultTagHelperActivator = defaultTagHelperActivator ??
		                             throw new ArgumentNullException(nameof(defaultTagHelperActivator));
	}

	public TTagHelper Create<TTagHelper>(ViewContext context) where TTagHelper : ITagHelper
	{
		return _customCreatorSelector(typeof(TTagHelper))
			? (TTagHelper)_customTagHelperCreator(typeof(TTagHelper))
			: _defaultTagHelperActivator.Create<TTagHelper>(context);
	}
}