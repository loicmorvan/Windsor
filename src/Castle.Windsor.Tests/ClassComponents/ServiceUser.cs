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
using Castle.Windsor.Tests.Components;

namespace Castle.Windsor.Tests.ClassComponents;

public class ServiceUser
{
	public ServiceUser(A a)
	{
		ArgumentNullException.ThrowIfNull(a);
		AComponent = a;
	}

	public ServiceUser(A a, B b) : this(a)
	{
		ArgumentNullException.ThrowIfNull(b);
		BComponent = b;
	}

	public ServiceUser(A a, B b, C c) : this(a, b)
	{
		ArgumentNullException.ThrowIfNull(c);
		CComponent = c;
	}

	public A AComponent { get; }

	public B BComponent { get; }

	public C CComponent { get; }
}