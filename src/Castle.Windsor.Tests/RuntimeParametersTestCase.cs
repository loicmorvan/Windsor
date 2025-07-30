// Copyright 2004-2013 Castle Project - http://www.castleproject.org/
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
using Castle.Windsor.MicroKernel.Handlers;
using Castle.Windsor.MicroKernel.Registration;
using Castle.Windsor.MicroKernel.Resolvers;
using Castle.Windsor.Tests.RuntimeParameters;
using Castle.Windsor.Windsor.Extensions;

namespace Castle.Windsor.Tests;

public class RuntimeParametersTestCase : AbstractContainerTestCase
{
    private readonly Dictionary<string, object> _dependencies = new()
        { { "cc", new CompC(12) }, { "myArgument", "ernst" } };

    private void AssertDependencies(CompB compb)
    {
        Assert.NotNull(compb);

        Assert.NotNull(compb.Compc);
        Assert.True(compb.MyArgument != string.Empty, "MyArgument property should not be empty");

        Assert.Same(_dependencies["cc"], compb.Compc);
        Assert.True(
            "ernst".Equals(compb.MyArgument),
            $"The MyArgument property of compb should be equal to ernst, found {compb.MyArgument}");
    }

    [Fact]
    public void AddingDependencyToServiceWithCustomDependency()
    {
        var kernel = new DefaultKernel();
        kernel.Register(Component.For<NeedClassWithCustomerDependency>(),
            Component.For<HasCustomDependency>().DependsOn(new Dictionary<object, object> { { "name", new CompA() } }));

        Assert.Equal(HandlerState.Valid, kernel.GetHandler(typeof(HasCustomDependency)).CurrentState);
        Assert.NotNull(kernel.Resolve<NeedClassWithCustomerDependency>());
    }

    [Fact]
    public void Missing_service_is_correctly_detected()
    {
        Container.Register(Component.For<CompA>().Named("compa"),
            Component.For<CompB>().Named("compb"));

        var exception = Assert.Throws<DependencyResolverException>(() =>
            Container.Resolve<CompB>(new Arguments().AddNamed("myArgument", 123)));
        Assert.Equal(
            string.Format(
                "Missing dependency.{0}Component compb has a dependency on Castle.Windsor.Tests.RuntimeParameters.CompC, which could not be resolved.{0}Make sure the dependency is correctly registered in the container as a service, or provided as inline argument.",
                Environment.NewLine),
            exception.Message);
    }

    [Fact]
    public void Parameter_takes_precedence_over_registered_service()
    {
        Container.Register(Component.For<CompA>(),
            Component.For<CompB>().DependsOn(Dependency.OnValue<string>("some string")),
            Component.For<CompC>().Instance(new CompC(0)));

        var c2 = new CompC(42);
        var args = new Arguments().AddTyped(c2);
        var b = Container.Resolve<CompB>(args);

        Assert.Same(c2, b.Compc);
    }

    [Fact]
    public void ParametersPrecedence()
    {
        Container.Register(Component.For<CompA>().Named("compa"),
            Component.For<CompB>().Named("compb").DependsOn(_dependencies));

        var instanceWithModel = Container.Resolve<CompB>();
        Assert.Same(_dependencies["cc"], instanceWithModel.Compc);

        var deps2 = new Dictionary<string, object> { { "cc", new CompC(12) }, { "myArgument", "ayende" } };

        var instanceWithArgs = Container.Resolve<CompB>(deps2);

        Assert.Same(deps2["cc"], instanceWithArgs.Compc);
        Assert.Equal("ayende", instanceWithArgs.MyArgument);
    }

    [Fact]
    public void ResolveUsingParameters()
    {
        Container.Register(Component.For<CompA>().Named("compa"),
            Component.For<CompB>().Named("compb"));
        var compb = Container.Resolve<CompB>(_dependencies);

        AssertDependencies(compb);
    }

    [Fact]
    public void ResolveUsingParametersWithinTheHandler()
    {
        Container.Register(Component.For<CompA>().Named("compa"),
            Component.For<CompB>().Named("compb").DependsOn(_dependencies));

        var compb = Container.Resolve<CompB>();

        AssertDependencies(compb);
    }

    [Fact]
    public void WillAlwaysResolveCustomParameterFromServiceComponent()
    {
        Container.Register(Component.For<CompA>(),
            Component.For<CompB>().DependsOn(new { myArgument = "foo" }),
            Component.For<CompC>().DependsOn(new { test = 15 }));
        var b = Kernel.Resolve<CompB>();
        Assert.NotNull(b);
        Assert.Equal(15, b.Compc.Test);
    }

    [Fact]
    public void WithoutParameters()
    {
        Container.Register(Component.For<CompA>().Named("compa"),
            Component.For<CompB>().Named("compb"));
        var expectedMessage =
            string.Format(
                "Can't create component 'compb' as it has dependencies to be satisfied.{0}{0}'compb' is waiting for the following dependencies:{0}- Service 'Castle.Windsor.Tests.RuntimeParameters.CompC' which was not registered.{0}- Parameter 'myArgument' which was not provided. Did you forget to set the dependency?{0}",
                Environment.NewLine);
        var exception = Assert.Throws<HandlerException>(() => Kernel.Resolve<CompB>());
        Assert.Equal(expectedMessage, exception.Message);
    }
}