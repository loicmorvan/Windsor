﻿// Copyright 2004-2018 Castle Project - http://www.castleproject.org/
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

// ReSharper disable ParameterOnlyUsedForPreconditionCheck.Local

namespace Castle.Facilities.AspNetCore.Tests.Fakes;

public class CompositeController
{
	public CompositeController(
		ControllerCrossWired crossWiredController,
		ControllerServiceProviderOnly serviceProviderOnlyController,
		ControllerWindsorOnly windsorOnlyController)
	{
		ArgumentNullException.ThrowIfNull(crossWiredController);
		ArgumentNullException.ThrowIfNull(serviceProviderOnlyController);
		ArgumentNullException.ThrowIfNull(windsorOnlyController);
	}
}

public class CompositeTagHelper
{
	public CompositeTagHelper(
		TagHelperCrossWired crossWiredTagHelper,
		TagHelperServiceProviderOnly serviceProviderOnlyTagHelper,
		TagHelperWindsorOnly windsorOnlyTagHelper)
	{
		ArgumentNullException.ThrowIfNull(crossWiredTagHelper);
		ArgumentNullException.ThrowIfNull(serviceProviderOnlyTagHelper);
		ArgumentNullException.ThrowIfNull(windsorOnlyTagHelper);
	}
}

public class CompositeViewComponent
{
	public CompositeViewComponent(
		ViewComponentCrossWired crossWiredViewComponent,
		ViewComponentServiceProviderOnly serviceProviderOnlyViewComponent,
		ViewComponentWindsorOnly windsorOnlyViewComponent)
	{
		ArgumentNullException.ThrowIfNull(crossWiredViewComponent);
		ArgumentNullException.ThrowIfNull(serviceProviderOnlyViewComponent);
		ArgumentNullException.ThrowIfNull(windsorOnlyViewComponent);
	}
}