// Copyright 2020 Castle Project - http://www.castleproject.org/
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

// ReSharper disable UnusedParameter.Local

using Castle.Windsor.MicroKernel;
using Castle.Windsor.MicroKernel.Handlers;
using Castle.Windsor.MicroKernel.Registration;
using JetBrains.Annotations;

namespace Castle.Windsor.Tests;

public class SystemNullableTestCase : AbstractContainerTestCase
{
    [Fact]
    public void Null_may_be_specified_for_non_optional_System_Nullable_constructor_parameter()
    {
        Container.Register(
            Component.For<DependencyFromContainer>(),
            Component.For<ComponentWithNonOptionalNullableParameter>());

        Container.Resolve<ComponentWithNonOptionalNullableParameter>(
            Arguments.FromProperties(new { nonOptionalNullableParameter = (int?)null }));
    }

    [Fact]
    public void Non_optional_System_Nullable_constructor_parameter_is_still_required()
    {
        Container.Register(
            Component.For<DependencyFromContainer>(),
            Component.For<ComponentWithNonOptionalNullableParameter>());

        var exception =
            Assert.Throws<HandlerException>(() => Container.Resolve<ComponentWithNonOptionalNullableParameter>());
        Assert.Equal(
            $"""
                 Can't create component '{typeof(ComponentWithNonOptionalNullableParameter)}' as it has dependencies to be satisfied.

                 '{typeof(ComponentWithNonOptionalNullableParameter)}' is waiting for the following dependencies:
                 - Parameter 'nonOptionalNullableParameter' which was not provided. Did you forget to set the dependency?

                 """.ConvertToEnvironmentLineEndings(),
            exception.Message);
    }

    [UsedImplicitly]
    public sealed class DependencyFromContainer;

    private sealed class ComponentWithNonOptionalNullableParameter
    {
        public ComponentWithNonOptionalNullableParameter(int? nonOptionalNullableParameter,
            DependencyFromContainer dependencyFromContainer)
        {
        }
    }
}