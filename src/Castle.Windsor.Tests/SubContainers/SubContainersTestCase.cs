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

using Castle.Windsor.Core;
using Castle.Windsor.MicroKernel;
using Castle.Windsor.MicroKernel.Context;
using Castle.Windsor.MicroKernel.Handlers;
using Castle.Windsor.MicroKernel.Registration;
using Castle.Windsor.Tests.ClassComponents;
using Castle.Windsor.Tests.Components;

namespace Castle.Windsor.Tests.SubContainers;

/// <summary>Summary description for SubContainersTestCase.</summary>
public class SubContainersTestCase : AbstractContainerTestCase
{
    [Fact]
    public void AddChildKernelToTwoParentsThrowsException()
    {
        const string expectedMessage =
            "You can not change the kernel parent once set, use the RemoveChildKernel and AddChildKernel methods together to achieve this.";

        var kernel2 = new DefaultKernel();

        var subkernel = new DefaultKernel();

        Kernel.AddChildKernel(subkernel);
        Assert.Equal(Kernel, subkernel.Parent);

        var exception = Assert.Throws<KernelException>(() => kernel2.AddChildKernel(subkernel));
        Assert.Equal(expectedMessage, exception.Message);
    }

    [Fact]
    public void ChildDependenciesIsSatisfiedEvenWhenComponentTakesLongToBeAddedToParentContainer()
    {
        var container = new DefaultKernel();
        var childContainer = new DefaultKernel();

        container.AddChildKernel(childContainer);
        childContainer.Register(Component.For(typeof(UsesIEmptyService)).Named("component"));

        container.Register(
            Component.For(typeof(IEmptyService)).ImplementedBy<EmptyServiceA>().Named("service1"));

        childContainer.Resolve<UsesIEmptyService>();
    }

    [Fact]
    public void ChildDependenciesSatisfiedAmongContainers()
    {
        var subkernel = new DefaultKernel();

        Kernel.Register(Component.For(typeof(DefaultTemplateEngine)).Named("templateengine"));
        Kernel.Register(Component.For(typeof(DefaultMailSenderService)).Named("mailsender"));

        Kernel.AddChildKernel(subkernel);
        subkernel.Register(Component.For(typeof(DefaultSpamService)).Named("spamservice"));

        var spamservice = subkernel.Resolve<DefaultSpamService>("spamservice");

        Assert.NotNull(spamservice);
        Assert.NotNull(spamservice.MailSender);
        Assert.NotNull(spamservice.TemplateEngine);
    }

    [Fact]
    public void ChildKernelFindsAndCreateParentComponent()
    {
        var subkernel = new DefaultKernel();

        Kernel.Register(Component.For(typeof(DefaultTemplateEngine)).Named("templateengine"));

        Kernel.AddChildKernel(subkernel);

        Assert.True(subkernel.HasComponent(typeof(DefaultTemplateEngine)));
        Assert.NotNull(subkernel.Resolve<DefaultTemplateEngine>());
    }

    [Fact]
    public void ChildKernelOverloadsParentKernel1()
    {
        var instance1 = new DefaultTemplateEngine();
        var instance2 = new DefaultTemplateEngine();

        // subkernel added with already registered components that overload parent components.

        var subkernel = new DefaultKernel();
        subkernel.Register(Component.For<DefaultTemplateEngine>().Named("engine").Instance(instance1));
        Assert.Equal(instance1, subkernel.Resolve<DefaultTemplateEngine>("engine"));

        Kernel.Register(Component.For<DefaultTemplateEngine>().Named("engine").Instance(instance2));
        Assert.Equal(instance2, Kernel.Resolve<DefaultTemplateEngine>("engine"));

        Kernel.AddChildKernel(subkernel);
        Assert.Equal(instance1, subkernel.Resolve<DefaultTemplateEngine>("engine"));
        Assert.Equal(instance2, Kernel.Resolve<DefaultTemplateEngine>("engine"));
    }

    [Fact]
    public void ChildKernelOverloadsParentKernel2()
    {
        var instance1 = new DefaultTemplateEngine();
        var instance2 = new DefaultTemplateEngine();

        var subkernel = new DefaultKernel();
        Kernel.AddChildKernel(subkernel);

        // subkernel added first, then populated with overloaded components after

        Kernel.Register(Component.For<DefaultTemplateEngine>().Named("engine").Instance(instance2));
        Assert.Equal(instance2, Kernel.Resolve<DefaultTemplateEngine>("engine"));
        Assert.Equal(instance2, subkernel.Resolve<DefaultTemplateEngine>("engine"));

        subkernel.Register(Component.For<DefaultTemplateEngine>().Named("engine").Instance(instance1));
        Assert.Equal(instance1, subkernel.Resolve<DefaultTemplateEngine>("engine"));
        Assert.Equal(instance2, Kernel.Resolve<DefaultTemplateEngine>("engine"));
    }

    [Fact]
    public void DependenciesSatisfiedAmongContainers()
    {
        var subkernel = new DefaultKernel();

        Kernel.Register(Component.For(typeof(DefaultMailSenderService)).Named("mailsender"));
        Kernel.Register(Component.For(typeof(DefaultTemplateEngine)).Named("templateengine"));

        Kernel.AddChildKernel(subkernel);

        subkernel.Register(Component.For(typeof(DefaultSpamService)).Named("spamservice"));

        var spamservice = subkernel.Resolve<DefaultSpamService>("spamservice");

        Assert.NotNull(spamservice);
        Assert.NotNull(spamservice.MailSender);
        Assert.NotNull(spamservice.TemplateEngine);
    }

    [Fact]
    public void DependenciesSatisfiedAmongContainersUsingEvents()
    {
        var subkernel = new DefaultKernel();

        subkernel.Register(Component.For(typeof(DefaultSpamServiceWithConstructor)).Named("spamservice"));

        Kernel.Register(Component.For(typeof(DefaultMailSenderService)).Named("mailsender"));
        Kernel.Register(Component.For(typeof(DefaultTemplateEngine)).Named("templateengine"));

        Kernel.AddChildKernel(subkernel);

        var spamservice =
            subkernel.Resolve<DefaultSpamServiceWithConstructor>("spamservice");

        Assert.NotNull(spamservice);
        Assert.NotNull(spamservice.MailSender);
        Assert.NotNull(spamservice.TemplateEngine);
    }

    [Fact]
    public void ParentKernelFindsAndCreateChildComponent()
    {
        var subkernel = new DefaultKernel();

        subkernel.Register(Component.For(typeof(DefaultTemplateEngine)).Named("templateengine"));

        Kernel.AddChildKernel(subkernel);

        Assert.False(Kernel.HasComponent(typeof(DefaultTemplateEngine)));

        Assert.Throws<ComponentNotFoundException>(() => Kernel.Resolve<DefaultTemplateEngine>());
    }

    [Fact]
    public void RemoveChildKernelCleansUp()
    {
        var subkernel = new DefaultKernel();
        var eventCollector = new EventsCollector(subkernel);
        subkernel.RemovedAsChildKernel += eventCollector.RemovedAsChildKernel;
        subkernel.AddedAsChildKernel += eventCollector.AddedAsChildKernel;

        Kernel.AddChildKernel(subkernel);
        Assert.Equal(Kernel, subkernel.Parent);
        Assert.Single(eventCollector.Events);
        Assert.Equal(EventsCollector.Added, eventCollector.Events[0]);

        Kernel.RemoveChildKernel(subkernel);
        Assert.Null(subkernel.Parent);
        Assert.Equal(2, eventCollector.Events.Count);
        Assert.Equal(EventsCollector.Removed, eventCollector.Events[1]);
    }

    [Fact]
    public void RemovingChildKernelUnsubscribesFromParentEvents()
    {
        IKernel subkernel = new DefaultKernel();
        var eventCollector = new EventsCollector(subkernel);
        subkernel.RemovedAsChildKernel += eventCollector.RemovedAsChildKernel;
        subkernel.AddedAsChildKernel += eventCollector.AddedAsChildKernel;

        Kernel.AddChildKernel(subkernel);
        Kernel.RemoveChildKernel(subkernel);
        Kernel.AddChildKernel(subkernel);
        Kernel.RemoveChildKernel(subkernel);

        Assert.Equal(4, eventCollector.Events.Count);
        Assert.Equal(EventsCollector.Added, eventCollector.Events[0]);
        Assert.Equal(EventsCollector.Removed, eventCollector.Events[1]);
        Assert.Equal(EventsCollector.Added, eventCollector.Events[2]);
        Assert.Equal(EventsCollector.Removed, eventCollector.Events[3]);
    }

    [Fact]
    public void Parent_component_will_NOT_have_dependencies_from_child()
    {
        Kernel.Register(Component.For<DefaultTemplateEngine>(),
            Component.For<DefaultSpamService>());

        var child = new DefaultKernel();
        Kernel.AddChildKernel(child);

        child.Register(Component.For<DefaultMailSenderService>());

        var spamservice = child.Resolve<DefaultSpamService>();

        Assert.NotNull(spamservice);
        Assert.NotNull(spamservice.TemplateEngine);
        Assert.Null(spamservice.MailSender);
    }

    [Fact]
    public void Singleton_withNonSingletonDependencies_doesNotReResolveDependencies()
    {
        Kernel.Register(Component.For(typeof(DefaultSpamService)).Named("spamservice"));
        Kernel.Register(Component.For(typeof(DefaultMailSenderService)).Named("mailsender"));

        var subkernel1 = new DefaultKernel();
        subkernel1.Register(Component.For(typeof(DefaultTemplateEngine)).Named("templateengine"));
        Kernel.AddChildKernel(subkernel1);

        var subkernel2 = new DefaultKernel();
        subkernel2.Register(Component.For(typeof(DefaultTemplateEngine)).Named("templateengine")
            .LifeStyle.Is(LifestyleType.Transient));
        Kernel.AddChildKernel(subkernel2);

        subkernel1.Resolve<DefaultTemplateEngine>("templateengine");
        var spamservice1 = subkernel1.Resolve<DefaultSpamService>("spamservice");

        Assert.Null(spamservice1.TemplateEngine);

        subkernel2.Resolve<DefaultTemplateEngine>("templateengine");
        var spamservice2 = subkernel2.Resolve<DefaultSpamService>("spamservice");

        Assert.Same(spamservice1, spamservice2);
    }

    [Fact]
    [Bug("IOC-345")]
    public void Do_NOT_UseChildComponentsForParentDependenciesWhenRequestedFromChild()
    {
        var subkernel = new DefaultKernel();

        Kernel.Register(Component.For(typeof(DefaultSpamService)).Named("spamservice").LifeStyle
            .Is(LifestyleType.Transient));
        Kernel.Register(Component.For(typeof(DefaultMailSenderService)).Named("mailsender"));
        Kernel.Register(Component.For(typeof(DefaultTemplateEngine)).Named("templateengine"));

        Kernel.AddChildKernel(subkernel);
        subkernel.Register(Component.For(typeof(DefaultTemplateEngine)).Named("templateengine"));

        var templateengine = Kernel.Resolve<DefaultTemplateEngine>("templateengine");
        var subTemplateengine = subkernel.Resolve<DefaultTemplateEngine>("templateengine");

        var spamservice = subkernel.Resolve<DefaultSpamService>("spamservice");
        Assert.NotEqual(spamservice.TemplateEngine, subTemplateengine);
        Assert.Equal(spamservice.TemplateEngine, templateengine);

        spamservice = Kernel.Resolve<DefaultSpamService>("spamservice");
        Assert.NotEqual(spamservice.TemplateEngine, subTemplateengine);
        Assert.Equal(spamservice.TemplateEngine, templateengine);
    }

    [Fact]
    [Bug("IOC-325")]
    public void TryResolvingViaChildKernelShouldNotThrowException()
    {
        using var childKernel = new DefaultKernel();
        Kernel.Register(Component.For<BookStore>());
        Kernel.AddChildKernel(childKernel);
        var handler = childKernel.GetHandler(typeof(BookStore));

        // Assert setup invariant
        Assert.IsType<ParentHandlerWrapper>(handler);

        handler.TryResolve(CreationContext.CreateEmpty());
    }

    /// <summary>
    ///     collects events in an array list, used for ensuring we are cleaning up the parent kernel event subscriptions
    ///     correctly.
    /// </summary>
    // ReSharper disable once ParameterOnlyUsedForPreconditionCheck.Local
    private class EventsCollector(object expectedSender)
    {
        public const string Added = "added";
        public const string Removed = "removed";

        public List<string> Events { get; } = [];

        public void AddedAsChildKernel(object sender, EventArgs e)
        {
            Assert.Equal(expectedSender, sender);
            Events.Add(Added);
        }

        public void RemovedAsChildKernel(object sender, EventArgs e)
        {
            Assert.Equal(expectedSender, sender);
            Events.Add(Removed);
        }
    }
}