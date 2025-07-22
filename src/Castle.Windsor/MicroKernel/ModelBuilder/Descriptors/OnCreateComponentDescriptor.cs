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

using Castle.Windsor.Core;
using Castle.Windsor.MicroKernel.LifecycleConcerns;

namespace Castle.Windsor.MicroKernel.ModelBuilder.Descriptors;

/// <summary>Adds the actions to ExtendedProperties.</summary>
/// <typeparam name = "TS"></typeparam>
public class OnCreateComponentDescriptor<TS>(LifecycleActionDelegate<TS> action)
	: IComponentModelDescriptor, IMetaComponentModelDescriptor
	where TS : class
{
	public void BuildComponentModel(IKernel kernel, ComponentModel model)
	{
	}

	public void ConfigureComponentModel(IKernel kernel, ComponentModel model)
	{
		model.Lifecycle.AddFirst(new OnCreatedConcern<TS>(action, kernel));
	}
}