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
using Castle.Facilities.AspNetCore.Tests.Framework;
using Castle.Windsor.MicroKernel.Registration;
using Microsoft.AspNetCore.Razor.TagHelpers;

namespace Castle.Facilities.AspNetCore.Tests;

public abstract class WindsorRegistrationOptionsTagHelperTestCase
{
	[Theory]
	[InlineData(typeof(OverrideTagHelper))]
	public void WindsorRegistrationOptionsForAssembliesTagHelperTestCase(Type optionsResolvableType)
	{
		using var testContext = TestContextFactory.Get(opts => opts
			.UseEntryAssembly(typeof(Uri).Assembly)
			.RegisterTagHelpers(typeof(OverrideTagHelper).Assembly));
		testContext.WindsorContainer.Resolve(optionsResolvableType);
	}

	[Theory]
	[InlineData(typeof(OverrideTagHelper))]
	public void WindsorRegistrationOptionsForComponentsTagHelperTestCase(Type optionsResolvableType)
	{
		using var testContext = TestContextFactory.Get(opts => opts
			.UseEntryAssembly(typeof(Uri).Assembly)
			.RegisterTagHelpers(Component.For<OverrideTagHelper>().LifestyleScoped().Named("tag-helpers")));
		testContext.WindsorContainer.Resolve(optionsResolvableType);
	}

	private class OverrideTagHelper : TagHelper;
}