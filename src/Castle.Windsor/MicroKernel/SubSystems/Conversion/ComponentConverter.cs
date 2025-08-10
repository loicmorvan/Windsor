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

using Castle.Core.Configuration;
using Castle.Windsor.MicroKernel.Context;
using Castle.Windsor.MicroKernel.Util;

namespace Castle.Windsor.MicroKernel.SubSystems.Conversion;

[Serializable]
public class ComponentConverter : AbstractTypeConverter, IKernelDependentConverter
{
    public override bool CanHandleType(Type type, IConfiguration configuration)
    {
        return configuration.Value != null && ReferenceExpressionUtil.IsReference(configuration.Value);
    }

    public override bool CanHandleType(Type type)
    {
        return Context.Kernel != null && Context.Kernel.HasComponent(type);
    }

    public override object PerformConversion(string value, Type targetType)
    {
        var componentName = ReferenceExpressionUtil.ExtractComponentName(value);
        if (componentName == null)
        {
            throw new ConverterException(
                $"Could not convert expression '{value}' to type '{targetType.FullName}'. Expecting a reference override like ${{some key}}");
        }

        var handler = Context.Kernel.LoadHandlerByName(componentName, targetType, null);
        if (handler == null)
        {
            throw new ConverterException(
                $"Component '{componentName}' was not found in the container.");
        }

        return handler.Resolve(Context.CurrentCreationContext ?? CreationContext.CreateEmpty());
    }

    public override object PerformConversion(IConfiguration configuration, Type targetType)
    {
        return PerformConversion(configuration.Value, targetType);
    }
}