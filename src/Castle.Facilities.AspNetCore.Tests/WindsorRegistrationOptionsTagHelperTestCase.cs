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

namespace Castle.Windsor.Facilities.AspNetCore.Tests;

using System;

using Castle.Windsor.Facilities.AspNetCore.Tests.Framework;
using Castle.Windsor.MicroKernel.Registration;

using Microsoft.AspNetCore.Razor.TagHelpers;

using TestContext = Castle.Windsor.Facilities.AspNetCore.Tests.Framework.TestContext;

public abstract class WindsorRegistrationOptionsTagHelperTestCase : IDisposable
{
	protected TestContext testContext;

	public void Dispose()
	{
		testContext.Dispose();
	}

	[Theory]
	[InlineData(typeof(OverrideTagHelper))]
	public void Should_resolve_overidden_TagHelpers_using_WindsorRegistrationOptions(Type optionsResolvableType)
	{
		testContext.WindsorContainer.Resolve(optionsResolvableType);
	}

	public class OverrideTagHelper : TagHelper;
}

public class WindsorRegistrationOptionsForAssembliesTagHelperTestCase : WindsorRegistrationOptionsTagHelperTestCase
{
	public WindsorRegistrationOptionsForAssembliesTagHelperTestCase()
	{
		testContext = TestContextFactory.Get(opts => opts
			.UseEntryAssembly(typeof(Uri).Assembly)
			.RegisterTagHelpers(typeof(OverrideTagHelper).Assembly));
	}
}

public class WindsorRegistrationOptionsForComponentsTagHelperTestCase : WindsorRegistrationOptionsTagHelperTestCase
{
	public WindsorRegistrationOptionsForComponentsTagHelperTestCase()
	{
		testContext = TestContextFactory.Get(opts => opts
			.UseEntryAssembly(typeof(Uri).Assembly)
			.RegisterTagHelpers(Component.For<OverrideTagHelper>().LifestyleScoped().Named("tag-helpers")));
	}
}