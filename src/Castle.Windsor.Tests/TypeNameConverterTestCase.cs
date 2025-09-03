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

using System.Diagnostics;
using System.Reflection;
using Castle.DynamicProxy;
using Castle.Windsor.MicroKernel.SubSystems.Conversion;
using Castle.Windsor.Tests.ClassComponents;
using Castle.Windsor.Tests.Components;
using JetBrains.Annotations;
using Moq;

namespace Castle.Windsor.Tests;

public class TypeNameConverterTestCase
{
    private readonly Mock<ITypeConverterContext> _contextMock = new();
    private readonly TypeNameConverter _converter;

    public TypeNameConverterTestCase()
    {
        _converter = new TypeNameConverter(_contextMock.Object,new TypeNameParser());
    }

    [Fact]
    public void Can_handle_generic_of_generics_properly()
    {
        var type = typeof(IGeneric<IGeneric<ICustomer>>);
        var name = type.FullName;
        var result = _converter.PerformConversion(name, typeof(Type));
        Assert.Equal(type, result);
    }

    [Fact]
    public void Can_handle_multi_generic_with_generic_of_generics_properly()
    {
        var type = typeof(IDoubleGeneric<ICustomer, IGeneric<ICustomer>>);
        var name = type.Name + "[[" +
                   nameof(ICustomer) + "],[" +
                   typeof(IGeneric<>).Name + "[[" + nameof(ICustomer) + "]]"
                   + "]]";
        var result = _converter.PerformConversion(name, typeof(Type));
        Assert.Equal(type, result);
    }

    [Fact]
    public void Can_handle_multi_generic_with_multi_generic_of_generics_properly_1()
    {
        var type = typeof(IDoubleGeneric<IDoubleGeneric<ICustomer, IEmptyService>, ICustomer>);
        var name = typeof(IDoubleGeneric<,>).Name
                   + "[[" +
                   typeof(IDoubleGeneric<,>).Name +
                   "[[" +
                   nameof(ICustomer) + "],[" +
                   nameof(IEmptyService)
                   + "]]"
                   + "],[" +
                   nameof(ICustomer) +
                   "]]";
        var result = _converter.PerformConversion(name, typeof(Type));
        Assert.Equal(type, result);
    }

    [Fact]
    public void Can_handle_multi_generic_with_multi_generic_of_generics_properly_2()
    {
        var type = typeof(IDoubleGeneric<ICustomer, IDoubleGeneric<ICustomer, IEmptyService>>);
        var name = typeof(IDoubleGeneric<,>).Name
                   + "[[" +
                   nameof(ICustomer) + "],[" +
                   typeof(IDoubleGeneric<,>).Name +
                   "[[" +
                   nameof(ICustomer) + "],[" +
                   nameof(IEmptyService)
                   + "]]"
                   + "]]";
        var result = _converter.PerformConversion(name, typeof(Type));
        Assert.Equal(type, result);
    }

    [Fact]
    public void Can_load_closed_generic_type_by_Name_single_generic_parameter()
    {
        var type = typeof(IGeneric<ICustomer>);
        var name = type.Name + "[[" + nameof(ICustomer) + "]]";
        var result = _converter.PerformConversion(name, typeof(Type));
        Assert.Equal(result, type);
    }

    [Fact]
    public void Can_load_closed_generic_type_by_Name_two_generic_parameters()
    {
        var type = typeof(IDoubleGeneric<ICustomer, ISpecification>);
        var name = type.Name + "[[" + nameof(ICustomer) + "],[" + typeof(ISpecification) + "]]";
        var result = _converter.PerformConversion(name, typeof(Type));
        Assert.Equal(result, type);
    }

    [Fact]
    public void Can_load_open_generic_type_by_name()
    {
        var type = typeof(IGeneric<>);
        var name = type.Name;
        var result = _converter.PerformConversion(name, typeof(Type));
        Assert.Equal(type, result);
    }

    [Fact]
    public void Can_load_type_from_loaded_assembly_by_just_name()
    {
        var type = typeof(ICustomer);
        var name = type.Name;
        var result = _converter.PerformConversion(name, typeof(Type));
        Assert.Equal(type, result);
    }

    [Fact]
    public void Can_load_type_from_loaded_assembly_by_name_with_namespace()
    {
        var type = typeof(IService); // notice we have multiple types 'IService in various namespaces'
        var name = type.FullName;
        var result = _converter.PerformConversion(name, typeof(Type));
        Assert.Equal(type, result);
    }

    [Fact]
    public void Throws_when_inner_generic_type_not_unique()
    {
        var type = typeof(IGeneric<IService2>);
        var name = type.Name + "[[" + nameof(IService2) + "]]";

        var exception =
            Assert.Throws<ConverterException>(() =>
                _converter.PerformConversion(name, typeof(Type)));
        Assert.StartsWith("Could not uniquely identify type for 'IService2'.", exception.Message);
    }

    [Fact]
    public void Throws_when_type_not_unique()
    {
        var type = typeof(IService2);
        var name = type.Name;

        var exception =
            Assert.Throws<ConverterException>(() =>
                _converter.PerformConversion(name, typeof(Type)));
        Assert.StartsWith("Could not uniquely identify type for 'IService2'.", exception.Message);
    }

    [Fact]
    public void Throws_helpful_exception_when_assembly_found_but_not_type()
    {
        var assemblyName = typeof(IInterceptor).GetTypeInfo().Assembly.FullName;
        var type = typeof(IService2).FullName + ", " + assemblyName;

        var exception = Assert.Throws<ConverterException>(() => _converter.PerformConversion(type, typeof(Type)));

        var message =
            $"Could not convert string '{type}' to a type. Assembly {assemblyName} was matched, but it doesn't contain the type. Make sure that the type name was not mistyped.";

        Assert.Equal(message, exception.Message);
    }

    [Fact]
    public void Throws_helpful_exception_when_assembly_specified_but_not_found()
    {
        var assemblyFullName = typeof(IInterceptor).GetTypeInfo().Assembly.FullName;
        Debug.Assert(assemblyFullName is not null);

        var assemblyName = assemblyFullName.Replace("Castle.Core", "Castle.Core42");
        var type = typeof(IService2).FullName + ", " + assemblyName;

        var exception = Assert.Throws<ConverterException>(() => _converter.PerformConversion(type, typeof(Type)));

        var message =
            $"Could not convert string '{type}' to a type. Assembly was not found. Make sure it was deployed and the name was not mistyped.";

        Assert.Equal(message, exception.Message);
    }

    [Fact]
    public void Throws_helpful_exception_when_only_type_specified_but_not_found()
    {
        const string type = "Some.Assembly.AndThen.Type+NestedEven";

        var exception = Assert.Throws<ConverterException>(() => _converter.PerformConversion(type, typeof(Type)));

        const string message =
            $"Could not convert string '{type}' to a type. Make sure assembly containing the type has been loaded into the process, or consider specifying assembly qualified name of the type.";

        Assert.Equal(message, exception.Message);
    }

    [Fact]
    public void Can_resolve_exact_match_if_two_classes_exist_that_differ_only_by_case()
    {
        var type = typeof(IGeneric<TestCaseSensitivity>);
        var name = type.AssemblyQualifiedName;
        var result = _converter.PerformConversion(name, typeof(Type));
        Assert.Equal(type, result);

        var type2 = typeof(IGeneric<TESTCASESENSITIVITY>);
        var name2 = type2.AssemblyQualifiedName;
        var result2 = _converter.PerformConversion(name2, typeof(Type));
        Assert.Equal(type2, result2);
    }

    [UsedImplicitly]
    private class TestCaseSensitivity;

    // ReSharper disable once InconsistentNaming
    [UsedImplicitly]
    private class TESTCASESENSITIVITY;
}