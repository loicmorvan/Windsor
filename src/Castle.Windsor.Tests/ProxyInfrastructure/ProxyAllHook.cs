// Copyright 2004-2011 Castle Project - http://www.castleproject.org/
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
using System.Reflection;
using Castle.DynamicProxy;

namespace Castle.Windsor.Tests.ProxyInfrastructure;

public class ProxyAllHook : IProxyGenerationHook
{
	public static int Instances;

	public ProxyAllHook()
	{
		Instances++;
	}

	public void MethodsInspected()
	{
	}

	public void NonProxyableMemberNotification(Type type, MemberInfo memberInfo)
	{
	}

	public bool ShouldInterceptMethod(Type type, MethodInfo memberInfo)
	{
		return true;
	}
}