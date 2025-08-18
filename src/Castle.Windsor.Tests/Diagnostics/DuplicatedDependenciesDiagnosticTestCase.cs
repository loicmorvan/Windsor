// Copyright 2004-2012 Castle Project - http://www.castleproject.org/
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

using Castle.Windsor.MicroKernel;
using Castle.Windsor.MicroKernel.Registration;
using Castle.Windsor.Tests.ClassComponents;
using Castle.Windsor.Tests.Components;
using Castle.Windsor.Windsor.Diagnostics;

namespace Castle.Windsor.Tests.Diagnostics;

public class DuplicatedDependenciesDiagnosticTestCase : AbstractContainerTestCase
{
    private IDuplicatedDependenciesDiagnostic _diagnostic;

    protected override void AfterContainerCreated()
    {
        var host = (IDiagnosticsHost)Kernel.GetSubSystem(SubSystemConstants.DiagnosticsKey);
        _diagnostic = host.GetDiagnostic<IDuplicatedDependenciesDiagnostic>();
    }

    [Fact]
    public void Can_detect_components_having_duplicated_dependencies_same_name_different_type()
    {
        Container.Register(Component.For<HasObjectPropertyAndTypedCtorParameterWithSameName>());

        var result = _diagnostic.Inspect();
        Assert.NotEmpty(result);
    }

    [Fact]
    public void Can_detect_components_having_duplicated_dependencies_same_type_and_name()
    {
        Container.Register(Component.For<HasTwoConstructors>());

        var result = _diagnostic.Inspect();
        Assert.NotEmpty(result);
    }

    [Fact]
    public void Can_detect_components_having_duplicated_dependencies_same_type_different_name()
    {
        Container.Register(Component.For<HasPropertyAndCtorParameterSameTypeDifferentName>());
        var result = _diagnostic.Inspect();
        Assert.NotEmpty(result);
    }

    [Fact]
    public void Can_detect_components_having_duplicated_dependencies_same_type_via_constructor()
    {
        Container.Register(Component.For<TwoEmptyServiceDependenciesConstructor>());
        var result = _diagnostic.Inspect();
        Assert.NotEmpty(result);
    }

    [Fact]
    public void Can_detect_components_having_duplicated_dependencies_same_type_via_properties()
    {
        Container.Register(Component.For<TwoEmptyServiceDependenciesProperty>());
        var result = _diagnostic.Inspect();
        Assert.NotEmpty(result);
    }

    [Fact]
    public void Can_detect_components_having_duplicated_dependencies_via_service_override()
    {
        Container.Register(Component.For<HasObjectPropertyAndTypedCtorParameterDifferentName>()
            .DependsOn(Dependency.OnComponent<object, EmptyService2Impl1>(),
                Dependency.OnComponent<IEmptyService, EmptyService2Impl1>()));
        var result = _diagnostic.Inspect();
        Assert.NotEmpty(result);
    }

    [Fact]
    public void Can_detect_multiple_dependencies_between_properties_and_constructors()
    {
        Container.Register(Component.For<ThreeEmptyServiceDependenciesPropertyAndManyCtors>());
        var result = _diagnostic.Inspect();
        Assert.NotEmpty(result);
    }
}