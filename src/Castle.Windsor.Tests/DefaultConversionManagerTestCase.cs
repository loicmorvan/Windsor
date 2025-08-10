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

using System.Collections;
using Castle.Core.Configuration;
using Castle.Windsor.MicroKernel.SubSystems.Conversion;

namespace Castle.Windsor.Tests;

public class DefaultConversionManagerTestCase
{
    private readonly DefaultConversionManager _converter = new();

    [Fact]
    [Bug("IOC-314")]
    public void Converting_numbers_uses_ordinal_culture()
    {
        Assert.Equal(",", Thread.CurrentThread.CurrentCulture.NumberFormat.NumberDecimalSeparator);

        var result = _converter.PerformConversion<decimal>("123.456");

        Assert.Equal(123.456m, result);
    }

    [Fact]
    public void PerformConversionInt()
    {
        Assert.Equal(100, _converter.PerformConversion<int>("100"));
        Assert.Equal(1234, _converter.PerformConversion<int>("1234"));
    }

    [Fact]
    public void PerformConversionChar()
    {
        Assert.Equal('a', _converter.PerformConversion<char>("a"));
    }

    [Fact]
    public void PerformConversionBool()
    {
        Assert.True(_converter.PerformConversion<bool>("true"));
        Assert.False(_converter.PerformConversion<bool>("false"));
    }

    [Fact]
    public void PerformConversionType()
    {
        Assert.Equal(typeof(DefaultConversionManagerTestCase),
            _converter.PerformConversion<Type>(
                "Castle.Windsor.Tests.DefaultConversionManagerTestCase, Castle.Windsor.Tests"));
    }

    [Fact]
    public void PerformConversionList()
    {
        var config = new MutableConfiguration("list")
        {
            Attributes =
            {
                ["type"] = "System.String"
            }
        };

        config.Children.Add(new MutableConfiguration("item", "first"));
        config.Children.Add(new MutableConfiguration("item", "second"));
        config.Children.Add(new MutableConfiguration("item", "third"));

        Assert.True(_converter.CanHandleType(typeof(IList)));
        Assert.True(_converter.CanHandleType(typeof(ArrayList)));

        var list = _converter.PerformConversion<IList>(config);
        Assert.NotNull(list);
        Assert.Equal("first", list[0]);
        Assert.Equal("second", list[1]);
        Assert.Equal("third", list[2]);
    }

    [Fact]
    public void Dictionary()
    {
        var config = new MutableConfiguration("dictionary")
        {
            Attributes =
            {
                ["keyType"] = "System.String",
                ["valueType"] = "System.String"
            }
        };

        var firstItem = new MutableConfiguration("item", "first")
        {
            Attributes =
            {
                ["key"] = "key1"
            }
        };
        config.Children.Add(firstItem);
        var secondItem = new MutableConfiguration("item", "second")
        {
            Attributes =
            {
                ["key"] = "key2"
            }
        };
        config.Children.Add(secondItem);
        var thirdItem = new MutableConfiguration("item", "third")
        {
            Attributes =
            {
                ["key"] = "key3"
            }
        };
        config.Children.Add(thirdItem);

        var intItem = new MutableConfiguration("item", "40")
        {
            Attributes =
            {
                ["key"] = "4",
                ["keyType"] = "System.Int32, mscorlib",
                ["valueType"] = "System.Int32, mscorlib"
            }
        };

        config.Children.Add(intItem);

        var dateItem = new MutableConfiguration("item", "2005/12/1")
        {
            Attributes =
            {
                ["key"] = "2000/1/1",
                ["keyType"] = "System.DateTime, mscorlib",
                ["valueType"] = "System.DateTime, mscorlib"
            }
        };

        config.Children.Add(dateItem);

        Assert.True(_converter.CanHandleType(typeof(IDictionary)));
        Assert.True(_converter.CanHandleType(typeof(Hashtable)));

        var dict = _converter.PerformConversion<IDictionary>(config);

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
            _converter.PerformConversion<IDictionary>(config);

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
        var config = new MutableConfiguration("list")
        {
            Attributes =
            {
                ["type"] = "System.Int64"
            }
        };

        config.Children.Add(new MutableConfiguration("item", "345"));
        config.Children.Add(new MutableConfiguration("item", "3147"));
        config.Children.Add(new MutableConfiguration("item", "997"));

        Assert.True(_converter.CanHandleType(typeof(IList<double>)));
        Assert.True(_converter.CanHandleType(typeof(List<string>)));

        var list = _converter.PerformConversion<IList<long>>(config);
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

        Assert.True(_converter.CanHandleType(typeof(IList<double>)));
        Assert.True(_converter.CanHandleType(typeof(List<string>)));

        var list = _converter.PerformConversion<IList<long>>(config);
        Assert.NotNull(list);
        Assert.Equal(345L, list[0]);
        Assert.Equal(3147L, list[1]);
        Assert.Equal(997L, list[2]);
    }

    [Fact]
    public void GenericDictionary()
    {
        var config = new MutableConfiguration("dictionary")
        {
            Attributes =
            {
                ["keyType"] = "System.String",
                ["valueType"] = "System.Int32"
            }
        };

        var firstItem = new MutableConfiguration("item", "1")
        {
            Attributes =
            {
                ["key"] = "key1"
            }
        };
        config.Children.Add(firstItem);
        var secondItem = new MutableConfiguration("item", "2")
        {
            Attributes =
            {
                ["key"] = "key2"
            }
        };
        config.Children.Add(secondItem);
        var thirdItem = new MutableConfiguration("item", "3")
        {
            Attributes =
            {
                ["key"] = "key3"
            }
        };
        config.Children.Add(thirdItem);

        Assert.True(_converter.CanHandleType(typeof(IDictionary<string, string>)));
        Assert.True(_converter.CanHandleType(typeof(Dictionary<string, int>)));

        var dict =
            _converter.PerformConversion<IDictionary<string, int>>(config);

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

        Assert.True(_converter.CanHandleType(typeof(string[])));

        var array = _converter.PerformConversion<string[]>(config);

        Assert.NotNull(array);

        Assert.Equal("first", array[0]);
        Assert.Equal("second", array[1]);
        Assert.Equal("third", array[2]);
    }

    [Fact]
    public void PerformConversionTimeSpan()
    {
        Assert.Equal(TimeSpan.Zero, _converter.PerformConversion<TimeSpan>("0"));
        Assert.Equal(TimeSpan.FromDays(14), _converter.PerformConversion<TimeSpan>("14"));
        Assert.Equal(new TimeSpan(0, 1, 2, 3), _converter.PerformConversion<TimeSpan>("1:2:3"));
        Assert.Equal(new TimeSpan(0, 0, 0, 0, 250), _converter.PerformConversion<TimeSpan>("0:0:0.250"));
        Assert.Equal(new TimeSpan(10, 20, 30, 40, 500),
            _converter.PerformConversion<TimeSpan>("10.20:30:40.50"));
    }
}