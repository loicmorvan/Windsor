// Copyright 2004-2009 Castle Project - http://www.castleproject.org/
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

namespace Castle.Windsor.Tests.RuntimeParameters;

public class CompA;

public class HasCustomDependency
{
	private CompA name;

	public HasCustomDependency(CompA name)
	{
		this.name = name;
	}
}

public class NeedClassWithCustomerDependency
{
	private HasCustomDependency dependency;

	public NeedClassWithCustomerDependency(HasCustomDependency dependency)
	{
		this.dependency = dependency;
	}
}

[Transient]
public class CompB
{
	public CompB(CompA ca, CompC cc, string myArgument)
	{
		Compc = cc;
		MyArgument = myArgument;
	}

	public CompC Compc { get; set; }

	public string MyArgument { get; } = string.Empty;
}

public class CompC
{
	public readonly int test;

	public CompC(int test)
	{
		this.test = test;
	}
}