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

using Castle.Core.Configuration;
using Castle.Windsor.Core;
using Castle.Windsor.MicroKernel.Context;

namespace Castle.Windsor.MicroKernel.SubSystems.Conversion;

/// <summary>Composition of all available conversion managers</summary>
[Serializable]
public sealed class DefaultConversionManager : AbstractSubSystem, IConversionManager, ITypeConverterContext
{
    [ThreadStatic] private static Stack<Tuple<ComponentModel, CreationContext>> _slot;

    private readonly List<ITypeConverter> _converters = [];
    private readonly List<ITypeConverter> _standAloneConverters = [];

    public DefaultConversionManager()
    {
        InitDefaultConverters();
    }

    private static Stack<Tuple<ComponentModel, CreationContext>> CurrentStack =>
        _slot ??= new Stack<Tuple<ComponentModel, CreationContext>>();

    public void Add(ITypeConverter converter)
    {
        converter.Context = this;

        _converters.Add(converter);

        if (converter is not IKernelDependentConverter)
        {
            _standAloneConverters.Add(converter);
        }
    }

    public ITypeConverterContext Context
    {
        get => this;
        set => throw new NotImplementedException();
    }

    public bool CanHandleType(Type type)
    {
        return _converters.Any(converter => converter.CanHandleType(type));
    }

    public bool CanHandleType(Type type, IConfiguration configuration)
    {
        return _converters.Any(converter => converter.CanHandleType(type, configuration));
    }

    public object PerformConversion(string value, Type targetType)
    {
        foreach (var converter in _converters.Where(converter => converter.CanHandleType(targetType)))
        {
            return converter.PerformConversion(value, targetType);
        }

        var message = $"No converter registered to handle the type {targetType.FullName}";

        throw new ConverterException(message);
    }

    public object PerformConversion(IConfiguration configuration, Type targetType)
    {
        foreach (var converter in _converters.Where(converter => converter.CanHandleType(targetType, configuration)))
        {
            return converter.PerformConversion(configuration, targetType);
        }

        var message = $"No converter registered to handle the type {targetType.FullName}";

        throw new ConverterException(message);
    }

    public TTarget PerformConversion<TTarget>(string value)
    {
        return (TTarget)PerformConversion(value, typeof(TTarget));
    }

    public TTarget PerformConversion<TTarget>(IConfiguration configuration)
    {
        return (TTarget)PerformConversion(configuration, typeof(TTarget));
    }

    IKernelInternal ITypeConverterContext.Kernel => Kernel;

    public void Push(ComponentModel model, CreationContext context)
    {
        CurrentStack.Push(new Tuple<ComponentModel, CreationContext>(model, context));
    }

    public void Pop()
    {
        CurrentStack.Pop();
    }

    public ComponentModel CurrentModel => CurrentStack.Count == 0 ? null : CurrentStack.Peek().Item1;

    public CreationContext CurrentCreationContext => CurrentStack.Count == 0 ? null : CurrentStack.Peek().Item2;

    public ITypeConverter Composition => this;

    private void InitDefaultConverters()
    {
        Add(new PrimitiveConverter());
        Add(new TimeSpanConverter());
        Add(new TypeNameConverter(new TypeNameParser()));
        Add(new EnumConverter());
        Add(new ListConverter());
        Add(new DictionaryConverter());
        Add(new GenericDictionaryConverter());
        Add(new GenericListConverter());
        Add(new ArrayConverter());
        Add(new ComponentConverter());
        Add(new AttributeAwareConverter());
        Add(new ComponentModelConverter());
    }
}