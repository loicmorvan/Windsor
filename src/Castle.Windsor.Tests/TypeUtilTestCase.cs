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

namespace CastleTests;

using System;

using Castle.ClassComponents;
using Castle.Core.Internal;
using Castle.MicroKernel.Tests.ClassComponents;

using CastleTests.ClassComponents;
using CastleTests.Components;

public class TypeUtilTestCase
{
	[Fact]
	public void Closed_generic_double_type()
	{
		var name = typeof(IDoubleGeneric<A, A2>).ToCSharpString();
		Assert.Equal("IDoubleGeneric<A, A2>", name);
	}

	[Fact]
	public void Closed_generic_on_generic_double_type()
	{
		var name = typeof(IDoubleGeneric<GenericImpl1<A>, A2>).ToCSharpString();
		Assert.Equal("IDoubleGeneric<GenericImpl1<A>, A2>", name);
	}

	[Fact]
	public void Closed_generic_on_generic_simple_type()
	{
		var name = typeof(GenericImpl1<GenericImpl2<A>>).ToCSharpString();
		Assert.Equal("GenericImpl1<GenericImpl2<A>>", name);
	}

	[Fact]
	public void Closed_generic_simple_type()
	{
		var name = typeof(GenericImpl1<A>).ToCSharpString();
		Assert.Equal("GenericImpl1<A>", name);
	}

	[Fact]
	public void Closed_generic_nested_generic_on_generic_double_type()
	{
		var name = typeof(GenericHasNested<A2>.NestedGeneric<Tuple<int, bool>>).ToCSharpString();
		Assert.Equal("GenericHasNested<A2>.NestedGeneric<Tuple<Int32, Boolean>>", name);
	}

	[Fact]
	public void Generic_nested_generic_typeArray_multi_dimensional()
	{
		var name = typeof(GenericHasNested<A2>.NestedGeneric<AProp>[,,]).ToCSharpString();
		Assert.Equal("GenericHasNested<A2>.NestedGeneric<AProp>[,,]", name);
	}

	[Fact]
	public void Generic_nested_generic_typeArray()
	{
		var name = typeof(GenericHasNested<A2>.NestedGeneric<AProp>[]).ToCSharpString();
		Assert.Equal("GenericHasNested<A2>.NestedGeneric<AProp>[]", name);
	}

	[Fact]
	public void Generic_nested_generic_type()
	{
		var name = typeof(GenericHasNested<A2>.NestedGeneric<AProp>).ToCSharpString();
		Assert.Equal("GenericHasNested<A2>.NestedGeneric<AProp>", name);
	}

	[Fact]
	public void Generic_nested_type_array()
	{
		var name = typeof(GenericHasNested<A2>.Nested[]).ToCSharpString();
		Assert.Equal("GenericHasNested<A2>.Nested[]", name);
	}

	[Fact]
	public void Generic_nested_type()
	{
		var name = typeof(GenericHasNested<A2>.Nested).ToCSharpString();
		Assert.Equal("GenericHasNested<A2>.Nested", name);
	}

	[Fact]
	public void Non_generic_nested_type()
	{
		var name = typeof(HasNestedType.Nested).ToCSharpString();
		Assert.Equal("HasNestedType.Nested", name);
	}

	[Fact]
	public void Non_generic_nested_type_array()
	{
		var name = typeof(HasNestedType.Nested[]).ToCSharpString();
		Assert.Equal("HasNestedType.Nested[]", name);
	}

	[Fact]
	public void Non_generic_simple_type()
	{
		var name = typeof(APropCtor).ToCSharpString();
		Assert.Equal("APropCtor", name);
	}

	[Fact]
	public void Non_generic_simple_type_array()
	{
		var name = typeof(APropCtor[]).ToCSharpString();
		Assert.Equal("APropCtor[]", name);
	}

	[Fact]
	public void Open_generic_double_type()
	{
		var name = typeof(IDoubleGeneric<,>).ToCSharpString();
		Assert.Equal("IDoubleGeneric<TOne, TTwo>", name);
	}

	[Fact]
	public void Open_generic_simple_type()
	{
		var name = typeof(GenericImpl1<>).ToCSharpString();
		Assert.Equal("GenericImpl1<T>", name);
	}
}