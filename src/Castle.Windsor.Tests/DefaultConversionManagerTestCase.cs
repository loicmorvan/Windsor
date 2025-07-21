// Copyright 2004-2022 Castle Project - http://www.castleproject.org/
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

namespace Castle.Windsor.Tests;

using System;
using System.Collections;
using System.Collections.Generic;
using System.Threading;

using Castle.Core.Configuration;
using Castle.Windsor.MicroKernel.SubSystems.Conversion;

public class DefaultConversionManagerTestCase
{
	private readonly DefaultConversionManager converter = new();

	[Fact]
	[Bug("IOC-314")]
	public void Converting_numbers_uses_ordinal_culture()
	{
		Assert.Equal(",", Thread.CurrentThread.CurrentCulture.NumberFormat.NumberDecimalSeparator);

		var result = converter.PerformConversion<decimal>("123.456");

		Assert.Equal(123.456m, result);
	}

	[Fact]
	public void PerformConversionInt()
	{
		Assert.Equal(100, converter.PerformConversion("100", typeof(int)));
		Assert.Equal(1234, converter.PerformConversion("1234", typeof(int)));
	}

	[Fact]
	public void PerformConversionChar()
	{
		Assert.Equal('a', converter.PerformConversion("a", typeof(char)));
	}

	[Fact]
	public void PerformConversionBool()
	{
		Assert.Equal(true, converter.PerformConversion("true", typeof(bool)));
		Assert.Equal(false, converter.PerformConversion("false", typeof(bool)));
	}

	[Fact]
	public void PerformConversionType()
	{
		Assert.Equal(typeof(DefaultConversionManagerTestCase),
			converter.PerformConversion(
				"Castle.Windsor.Tests.DefaultConversionManagerTestCase, Castle.Windsor.Tests",
				typeof(Type)));
	}

	[Fact]
	public void PerformConversionList()
	{
		var config = new MutableConfiguration("list");
		config.Attributes["type"] = "System.String";

		config.Children.Add(new MutableConfiguration("item", "first"));
		config.Children.Add(new MutableConfiguration("item", "second"));
		config.Children.Add(new MutableConfiguration("item", "third"));

		Assert.True(converter.CanHandleType(typeof(IList)));
		Assert.True(converter.CanHandleType(typeof(ArrayList)));

		var list = (IList)converter.PerformConversion(config, typeof(IList));
		Assert.NotNull(list);
		Assert.Equal("first", list[0]);
		Assert.Equal("second", list[1]);
		Assert.Equal("third", list[2]);
	}

	[Fact]
	public void Dictionary()
	{
		var config = new MutableConfiguration("dictionary");
		config.Attributes["keyType"] = "System.String";
		config.Attributes["valueType"] = "System.String";

		var firstItem = new MutableConfiguration("item", "first");
		firstItem.Attributes["key"] = "key1";
		config.Children.Add(firstItem);
		var secondItem = new MutableConfiguration("item", "second");
		secondItem.Attributes["key"] = "key2";
		config.Children.Add(secondItem);
		var thirdItem = new MutableConfiguration("item", "third");
		thirdItem.Attributes["key"] = "key3";
		config.Children.Add(thirdItem);

		var intItem = new MutableConfiguration("item", "40");
		intItem.Attributes["key"] = "4";
		intItem.Attributes["keyType"] = "System.Int32, mscorlib";
		intItem.Attributes["valueType"] = "System.Int32, mscorlib";

		config.Children.Add(intItem);

		var dateItem = new MutableConfiguration("item", "2005/12/1");
		dateItem.Attributes["key"] = "2000/1/1";
		dateItem.Attributes["keyType"] = "System.DateTime, mscorlib";
		dateItem.Attributes["valueType"] = "System.DateTime, mscorlib";

		config.Children.Add(dateItem);

		Assert.True(converter.CanHandleType(typeof(IDictionary)));
		Assert.True(converter.CanHandleType(typeof(Hashtable)));

		var dict = (IDictionary)
			converter.PerformConversion(config, typeof(IDictionary));

		Assert.NotNull(dict);

		Assert.Equal("first", dict["key1"]);
		Assert.Equal("second", dict["key2"]);
		Assert.Equal("third", dict["key3"]);
		Assert.Equal(40, dict[4]);
		Assert.Equal(new DateTime(2005, 12, 1), dict[new DateTime(2000, 1, 1)]);
	}

	[Fact]
	public void DictionaryWithDifferentValueTypes()
	{
		var config = new MutableConfiguration("dictionary");

		config.CreateChild("entry")
			.Attribute("key", "intentry")
			.Attribute("valueType", "System.Int32, mscorlib")
			.Value = "123";

		config.CreateChild("entry")
			.Attribute("key", "values")
			.Attribute("valueType", "System.Int32[], mscorlib")
			.CreateChild("array")
			.Attribute("type", "System.Int32, mscorlib")
			.CreateChild("item", "400");

		var dict =
			(IDictionary)converter.PerformConversion(config, typeof(IDictionary));

		Assert.NotNull(dict);

		Assert.Equal(123, dict["intentry"]);
		var values = (int[])dict["values"];
		Assert.NotNull(values);
		Assert.Single(values);
		Assert.Equal(400, values[0]);
	}

	[Fact]
	public void GenericPerformConversionList()
	{
		var config = new MutableConfiguration("list");
		config.Attributes["type"] = "System.Int64";

		config.Children.Add(new MutableConfiguration("item", "345"));
		config.Children.Add(new MutableConfiguration("item", "3147"));
		config.Children.Add(new MutableConfiguration("item", "997"));

		Assert.True(converter.CanHandleType(typeof(IList<double>)));
		Assert.True(converter.CanHandleType(typeof(List<string>)));

		var list = (IList<long>)converter.PerformConversion(config, typeof(IList<long>));
		Assert.NotNull(list);
		Assert.Equal(345L, list[0]);
		Assert.Equal(3147L, list[1]);
		Assert.Equal(997L, list[2]);
	}

	[Fact]
	public void ListOfLongGuessingType()
	{
		var config = new MutableConfiguration("list");

		config.Children.Add(new MutableConfiguration("item", "345"));
		config.Children.Add(new MutableConfiguration("item", "3147"));
		config.Children.Add(new MutableConfiguration("item", "997"));

		Assert.True(converter.CanHandleType(typeof(IList<double>)));
		Assert.True(converter.CanHandleType(typeof(List<string>)));

		var list = (IList<long>)converter.PerformConversion(config, typeof(IList<long>));
		Assert.NotNull(list);
		Assert.Equal(345L, list[0]);
		Assert.Equal(3147L, list[1]);
		Assert.Equal(997L, list[2]);
	}

	[Fact]
	public void GenericDictionary()
	{
		var config = new MutableConfiguration("dictionary");
		config.Attributes["keyType"] = "System.String";
		config.Attributes["valueType"] = "System.Int32";

		var firstItem = new MutableConfiguration("item", "1");
		firstItem.Attributes["key"] = "key1";
		config.Children.Add(firstItem);
		var secondItem = new MutableConfiguration("item", "2");
		secondItem.Attributes["key"] = "key2";
		config.Children.Add(secondItem);
		var thirdItem = new MutableConfiguration("item", "3");
		thirdItem.Attributes["key"] = "key3";
		config.Children.Add(thirdItem);

		Assert.True(converter.CanHandleType(typeof(IDictionary<string, string>)));
		Assert.True(converter.CanHandleType(typeof(Dictionary<string, int>)));

		var dict =
			(IDictionary<string, int>)converter.PerformConversion(config, typeof(IDictionary<string, int>));

		Assert.NotNull(dict);

		Assert.Equal(1, dict["key1"]);
		Assert.Equal(2, dict["key2"]);
		Assert.Equal(3, dict["key3"]);
	}

	[Fact]
	public void Array()
	{
		var config = new MutableConfiguration("array");

		config.Children.Add(new MutableConfiguration("item", "first"));
		config.Children.Add(new MutableConfiguration("item", "second"));
		config.Children.Add(new MutableConfiguration("item", "third"));

		Assert.True(converter.CanHandleType(typeof(string[])));

		var array = (string[])
			converter.PerformConversion(config, typeof(string[]));

		Assert.NotNull(array);

		Assert.Equal("first", array[0]);
		Assert.Equal("second", array[1]);
		Assert.Equal("third", array[2]);
	}

	[Fact]
	public void PerformConversionTimeSpan()
	{
		Assert.Equal(TimeSpan.Zero, converter.PerformConversion("0", typeof(TimeSpan)));
		Assert.Equal(TimeSpan.FromDays(14), converter.PerformConversion("14", typeof(TimeSpan)));
		Assert.Equal(new TimeSpan(0, 1, 2, 3), converter.PerformConversion("1:2:3", typeof(TimeSpan)));
		Assert.Equal(new TimeSpan(0, 0, 0, 0, 250), converter.PerformConversion("0:0:0.250", typeof(TimeSpan)));
		Assert.Equal(new TimeSpan(10, 20, 30, 40, 500),
			converter.PerformConversion("10.20:30:40.50", typeof(TimeSpan)));
	}
}