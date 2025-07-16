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

namespace Castle.MicroKernel.Tests;

using System;

using Castle.Core;
using Castle.MicroKernel.Registration;

public class TransientMultiConstructorTestCase
{
	[Fact]
	public void TransientMultiConstructorTest()
	{
		var container = new DefaultKernel();
		((IKernel)container).Register(Component.For(typeof(AnyClass)).Named("AnyClass"));

		var arguments1 = new Arguments();
		arguments1.Add("integer", 1);

		var arguments2 = new Arguments();
		arguments2.Add("datetime", DateTime.Now.AddDays(1));

		var a = container.Resolve(typeof(AnyClass), arguments1);
		var b = container.Resolve(typeof(AnyClass), arguments2);

		Assert.NotSame(a, b);
	}

	[Fact]
	public void TransientMultipleConstructorNonValueTypeTest()
	{
		var container = new DefaultKernel();
		((IKernel)container).Register(Component.For(typeof(AnyClassWithReference)).Named("AnyClass"));
		var one = new Tester1("AnyString");
		var two = new Tester2(1);

		var arguments1 = new Arguments();
		arguments1.Add("test1", one);

		var arguments2 = new Arguments();
		arguments2.Add("test2", two);

		var a = container.Resolve(typeof(AnyClassWithReference), arguments1);
		var b = container.Resolve(typeof(AnyClassWithReference), arguments2);

		Assert.NotSame(a, b);

		// multi resolve test

		a = container.Resolve(typeof(AnyClassWithReference), arguments1);
		b = container.Resolve(typeof(AnyClassWithReference), arguments2);

		Assert.NotSame(a, b);
	}
}

[Transient]
public class AnyClass
{
	public AnyClass(int integer)
	{
	}

	public AnyClass(DateTime datetime)
	{
	}
}

public class Tester1
{
	public string bar;

	public Tester1(string bar)
	{
		this.bar = bar;
	}
}

public class Tester2
{
	public int foo;

	public Tester2(int foo)
	{
		this.foo = foo;
	}
}

[Transient]
public class AnyClassWithReference
{
	public AnyClassWithReference(Tester1 test1)
	{
	}

	public AnyClassWithReference(Tester2 test2)
	{
	}
}