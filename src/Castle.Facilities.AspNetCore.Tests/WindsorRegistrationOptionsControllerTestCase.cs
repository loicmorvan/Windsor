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
using Castle.Facilities.AspNetCore.Tests.Framework;
using Castle.MicroKernel.Registration;
using Microsoft.AspNetCore.Mvc;

// ReSharper disable UnusedType.Global

namespace Castle.Facilities.AspNetCore.Tests;

public abstract class WindsorRegistrationOptionsControllerTestCase : IDisposable
{
	protected TestContext TestContext;

	public void Dispose()
	{
		TestContext.Dispose();
	}

	[Theory]
	[InlineData(typeof(OverrideController))]
	public void Should_resolve_overidden_Controllers_using_WindsorRegistrationOptions(Type optionsResolvableType)
	{
		TestContext.WindsorContainer.Resolve(optionsResolvableType);
	}

	// ReSharper disable once MemberCanBeProtected.Global
	public class OverrideController : Controller;
}

public class WindsorRegistrationOptionsForAssembliesControllerTestCase : WindsorRegistrationOptionsControllerTestCase
{
	public WindsorRegistrationOptionsForAssembliesControllerTestCase()
	{
		TestContext = TestContextFactory.Get(opts => opts
			.UseEntryAssembly(typeof(Uri).Assembly)
			.RegisterControllers(typeof(OverrideController).Assembly));
	}
}

public class WindsorRegistrationOptionsForComponentsControllerTestCase : WindsorRegistrationOptionsControllerTestCase
{
	public WindsorRegistrationOptionsForComponentsControllerTestCase()
	{
		TestContext = TestContextFactory.Get(opts => opts
			.UseEntryAssembly(typeof(Uri).Assembly)
			.RegisterControllers(Component.For<OverrideController>().LifestyleScoped().Named("controllers")));
	}
}