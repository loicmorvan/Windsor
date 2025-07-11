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
using System.Linq;
using Castle.Core.Internal;
using Castle.Windsor.Tests.Components;

namespace Castle.Windsor.Tests;

public class TypeComparerTestCase
{
	private readonly TypeByInheritanceDepthMostSpecificFirstComparer _comparer = new();

	[Fact]
	public void More_specific_type_goes_first()
	{
		var set1 = new SimpleSortedSet<Type>(_comparer) { typeof(JohnChild), typeof(JohnParent) };
		var set2 = new SimpleSortedSet<Type>(_comparer) { typeof(JohnParent), typeof(JohnChild) };

		Assert.Equal(typeof(JohnChild), set1.First());
		Assert.Equal(typeof(JohnChild), set2.First());
	}

	[Fact]
	public void More_specific_type_goes_first_three_classes()
	{
		var set1 = new SimpleSortedSet<Type>(_comparer)
			{ typeof(JohnChild), typeof(JohnParent), typeof(JohnGrandparent) };
		var set2 = new SimpleSortedSet<Type>(_comparer)
			{ typeof(JohnChild), typeof(JohnGrandparent), typeof(JohnParent) };
		var set3 = new SimpleSortedSet<Type>(_comparer)
			{ typeof(JohnParent), typeof(JohnChild), typeof(JohnGrandparent) };
		var set4 = new SimpleSortedSet<Type>(_comparer)
			{ typeof(JohnParent), typeof(JohnGrandparent), typeof(JohnChild) };
		var set5 = new SimpleSortedSet<Type>(_comparer)
			{ typeof(JohnGrandparent), typeof(JohnParent), typeof(JohnChild) };
		var set6 = new SimpleSortedSet<Type>(_comparer)
			{ typeof(JohnGrandparent), typeof(JohnChild), typeof(JohnParent) };

		Assert.Equal(typeof(JohnChild), set1.First());
		Assert.Equal(typeof(JohnChild), set2.First());
		Assert.Equal(typeof(JohnChild), set3.First());
		Assert.Equal(typeof(JohnChild), set4.First());
		Assert.Equal(typeof(JohnChild), set5.First());
		Assert.Equal(typeof(JohnChild), set6.First());

		Assert.Equal(typeof(JohnGrandparent), set1.Last());
		Assert.Equal(typeof(JohnGrandparent), set2.Last());
		Assert.Equal(typeof(JohnGrandparent), set3.Last());
		Assert.Equal(typeof(JohnGrandparent), set4.Last());
		Assert.Equal(typeof(JohnGrandparent), set5.Last());
		Assert.Equal(typeof(JohnGrandparent), set6.Last());
	}
}