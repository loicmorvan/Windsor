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

using System.Diagnostics;
using System.Reflection;
using Castle.Core.Internal;
using Castle.Windsor.Core;
using Castle.Windsor.MicroKernel.Registration;
using Castle.Windsor.Tests.ClassComponents;

namespace Castle.Windsor.Tests.Registration;

public class AllTypesTestCase : AbstractContainerTestCase
{
    [Fact]
    public void RegisterAssemblyTypes_BasedOn_RegisteredInContainer1()
    {
        Kernel.Register(Classes
            .FromAssembly(AssemblyHelper.GetCurrentAssembly())
            .BasedOn<ICommon>()
        );

        var handlers = Kernel.GetHandlers(typeof(ICommon));
        Assert.Empty(handlers);

        handlers = Kernel.GetAssignableHandlers(typeof(ICommon));
        Assert.NotEmpty(handlers);
    }

    [Fact]
    public void RegisterAssemblyTypes_BasedOn_RegisteredInContainer2()
    {
        Kernel.Register(Classes
            .FromThisAssembly()
            .BasedOn<ICommon>()
        );

        var handlers = Kernel.GetHandlers(typeof(ICommon));
        Assert.Empty(handlers);

        handlers = Kernel.GetAssignableHandlers(typeof(ICommon));
        Assert.NotEmpty(handlers);
    }

    [Fact]
    public void RegisterAssemblyTypesFromThisAssembly_BasedOn_RegisteredInContainer()
    {
        Kernel.Register(Classes.FromAssembly(AssemblyHelper.GetCurrentAssembly()).BasedOn<ICommon>());

        var handlers = Kernel.GetHandlers(typeof(ICommon));
        Assert.Empty(handlers);

        handlers = Kernel.GetAssignableHandlers(typeof(ICommon));
        Assert.NotEmpty(handlers);
    }

    [Fact]
    public void RegisterDirectoryAssemblyTypes_BasedOn_RegisteredInContainer()
    {
        var directory = AppContext.BaseDirectory;
        Kernel.Register(Classes.FromAssemblyInDirectory(new AssemblyFilter(directory))
            .BasedOn<ICommon>());

        var handlers = Kernel.GetHandlers(typeof(ICommon));
        Assert.Empty(handlers);

        handlers = Kernel.GetAssignableHandlers(typeof(ICommon));
        Assert.NotEmpty(handlers);
    }

    [Fact]
    public void RegisterAssemblyTypes_NoService_RegisteredInContainer()
    {
        Kernel.Register(Classes.FromAssembly(AssemblyHelper.GetCurrentAssembly())
            .BasedOn<ICommon>());

        var handlers = Kernel.GetHandlers(typeof(ICommon));
        Assert.Empty(handlers);

        handlers = Kernel.GetAssignableHandlers(typeof(ICommon));
        Assert.NotEmpty(handlers);
    }

    [Fact]
    public void RegisterAssemblyTypes_FirstInterfaceService_RegisteredInContainer()
    {
        Kernel.Register(Classes.FromAssembly(AssemblyHelper.GetCurrentAssembly())
            .BasedOn<ICommon>()
            .WithService.FirstInterface()
        );

        var handlers = Kernel.GetHandlers(typeof(ICommon));
        Assert.NotEmpty(handlers);

        handlers = Kernel.GetAssignableHandlers(typeof(ICommon));
        Assert.NotEmpty(handlers);
    }

    [Fact]
    public void RegisterAssemblyTypes_LookupInterfaceService_RegisteredInContainer()
    {
        Kernel.Register(Classes.FromAssembly(AssemblyHelper.GetCurrentAssembly())
            .BasedOn<ICommon>()
            .WithService.FromInterface()
        );

        var handlers = Kernel.GetHandlers(typeof(ICommon));
        Assert.NotEmpty(handlers);

        handlers = Kernel.GetHandlers(typeof(ICommonSub1));
        Assert.NotEmpty(handlers);

        handlers = Kernel.GetAssignableHandlers(typeof(ICommonSub1));
        Assert.NotEmpty(handlers);

        handlers = Kernel.GetHandlers(typeof(ICommonSub2));
        Assert.NotEmpty(handlers);

        handlers = Kernel.GetAssignableHandlers(typeof(ICommonSub2));
        Assert.NotEmpty(handlers);
    }

    [Fact]
    public void RegisterAssemblyTypes_DefaultService_RegisteredInContainer()
    {
        Kernel.Register(Classes.FromAssembly(AssemblyHelper.GetCurrentAssembly())
            .BasedOn<ICommon>()
            .WithService.Base()
        );

        var handlers = Kernel.GetHandlers(typeof(ICommon));
        Assert.NotEmpty(handlers);

        handlers = Kernel.GetAssignableHandlers(typeof(ICommon));
        Assert.NotEmpty(handlers);
    }

    [Fact]
    public void RegisterAssemblyTypes_WithConfiguration_RegisteredInContainer()
    {
        Kernel.Register(Classes.FromAssembly(AssemblyHelper.GetCurrentAssembly())
            .BasedOn<ICommon>()
            .Configure(delegate(ComponentRegistration component)
            {
                component.LifeStyle.Transient
                    .Named(component.Implementation.FullName + "XYZ");
            })
        );

        foreach (var handler in Kernel.GetAssignableHandlers(typeof(ICommon)))
        {
            Assert.Equal(LifestyleType.Transient, handler.ComponentModel.LifestyleType);
            Assert.Equal(handler.ComponentModel.Implementation.FullName + "XYZ", handler.ComponentModel.Name);
        }
    }

    [Fact]
    public void RegisterAssemblyTypes_WithConfigurationBasedOnImplementation_RegisteredInContainer()
    {
        Kernel.Register(Classes.FromAssembly(AssemblyHelper.GetCurrentAssembly())
            .BasedOn<ICommon>()
            .Configure(delegate(ComponentRegistration component)
            {
                component.LifeStyle.Transient
                    .Named(component.Implementation.FullName + "XYZ");
            })
            .ConfigureFor<CommonImpl1>(component => component.DependsOn(Property.ForKey("key1").Eq(1)))
            .ConfigureFor<CommonImpl2>(component => component.DependsOn(Property.ForKey("key2").Eq(2)))
        );

        foreach (var handler in Kernel.GetAssignableHandlers(typeof(ICommon)))
        {
            Assert.Equal(LifestyleType.Transient, handler.ComponentModel.LifestyleType);
            Assert.Equal(handler.ComponentModel.Implementation.FullName + "XYZ", handler.ComponentModel.Name);

            if (handler.ComponentModel.Implementation == typeof(CommonImpl1))
            {
                Assert.Equal(1, handler.ComponentModel.CustomDependencies.Count);
                Assert.True(handler.ComponentModel.CustomDependencies.Contains("key1"));
            }
            else if (handler.ComponentModel.Implementation == typeof(CommonImpl2))
            {
                Assert.Equal(1, handler.ComponentModel.CustomDependencies.Count);
                Assert.True(handler.ComponentModel.CustomDependencies.Contains("key2"));
            }
        }
    }

    [Fact]
    public void RegisterGenericTypes_BasedOnGenericDefinition_RegisteredInContainer()
    {
        Kernel.Register(Classes.From(typeof(DefaultRepository<>))
            .Pick()
            .WithService.FirstInterface()
        );

        var repository = Kernel.Resolve<IRepository<CustomerImpl>>();
        Assert.NotNull(repository);
    }

    [Fact]
    public void RegisterGenericTypes_WithGenericDefinition_RegisteredInContainer()
    {
        Kernel.Register(Classes.FromAssembly(AssemblyHelper.GetCurrentAssembly())
            .BasedOn(typeof(IValidator<>))
            .WithService.Base()
        );

        var handlers = Kernel.GetHandlers(typeof(IValidator<ICustomer>));
        Assert.NotEmpty(handlers);
        var validator = Kernel.Resolve<IValidator<ICustomer>>();
        Assert.NotNull(validator);

        handlers = Kernel.GetHandlers(typeof(IValidator<CustomerChain1>));
        Assert.NotEmpty(handlers);
        var validator2 = Kernel.Resolve<IValidator<CustomerChain1>>();
        Assert.NotNull(validator2);
    }

    [Fact]
    public void RegisterAssemblyTypes_ClosedGenericTypes_RegisteredInContainer()
    {
        Kernel.Register(Classes.FromAssembly(AssemblyHelper.GetCurrentAssembly())
            .BasedOn(typeof(IMapper<>))
            .WithService.FirstInterface()
        );

        var handlers = Kernel.GetHandlers(typeof(IMapper<CommonImpl1>));
        Assert.Single(handlers);

        handlers = Kernel.GetAssignableHandlers(typeof(IMapper<CommonImpl2>));
        Assert.Single(handlers);
    }

    [Fact]
    public void RegisterAssemblyTypes_IfCondition_RegisteredInContainer()
    {
        Kernel.Register(Classes.FromAssembly(AssemblyHelper.GetCurrentAssembly())
            .BasedOn<ICustomer>()
            .If(t =>
            {
                Debug.Assert(t.FullName != null);
                return t.FullName.Contains("Chain");
            })
        );

        var handlers = Kernel.GetAssignableHandlers(typeof(ICustomer));
        Assert.NotEmpty(handlers);

        foreach (var handler in handlers)
        {
            Assert.Contains("Chain", handler.ComponentModel.Implementation.FullName);
        }
    }

    [Fact]
    public void RegisterAssemblyTypes_MultipleIfCondition_RegisteredInContainer()
    {
        Kernel.Register(Classes.FromAssembly(AssemblyHelper.GetCurrentAssembly())
            .BasedOn<ICustomer>()
            .If(t => t.Name.EndsWith('2'))
            .If(t =>
            {
                Debug.Assert(t.FullName != null);
                return t.FullName.Contains("Chain");
            })
        );

        var handlers = Kernel.GetAssignableHandlers(typeof(ICustomer));
        Assert.Single(handlers);
        Assert.Equal(typeof(CustomerChain2), handlers.Single().ComponentModel.Implementation);
    }

    [Fact]
    public void RegisterAssemblyTypes_UnlessCondition_RegisteredInContainer()
    {
        Kernel.Register(Classes.FromAssembly(AssemblyHelper.GetCurrentAssembly())
            .BasedOn<ICustomer>()
            .Unless(t => typeof(CustomerChain1).IsAssignableFrom(t))
        );

        foreach (var handler in Kernel.GetAssignableHandlers(typeof(ICustomer)))
        {
            Assert.False(typeof(CustomerChain1).IsAssignableFrom(handler.ComponentModel.Implementation));
        }
    }

    [Fact]
    public void RegisterAssemblyTypes_MultipleUnlessCondition_RegisteredInContainer()
    {
        Kernel.Register(Classes.FromAssembly(AssemblyHelper.GetCurrentAssembly())
            .BasedOn<ICustomer>()
            .Unless(t => t.Name.EndsWith('2'))
            .Unless(t => t.Name.EndsWith('3'))
        );

        var handlers = Kernel.GetAssignableHandlers(typeof(ICustomer));
        Assert.NotEmpty(handlers);
        foreach (var handler in handlers)
        {
            var name = handler.ComponentModel.Implementation.Name;
            Assert.False(name.EndsWith('2'));
            Assert.False(name.EndsWith('3'));
        }
    }

    [Fact]
    public void RegisterTypes_WithLinq_RegisteredInContainer()
    {
        Kernel.Register(Classes.From(from type in AssemblyHelper.GetCurrentAssembly().GetExportedTypes()
            where type.GetTypeInfo().GetAttribute<SerializableAttribute>() is not null
            select type
        ).BasedOn<CustomerChain1>());

        var handlers = Kernel.GetAssignableHandlers(typeof(CustomerChain1));
        Assert.Equal(2, handlers.Length);
    }

    [Fact]
    public void RegisterAssemblyTypes_WithLinqConfiguration_RegisteredInContainer()
    {
        Kernel.Register(Classes
            .FromAssembly(AssemblyHelper.GetCurrentAssembly())
            .BasedOn<ICommon>()
            .Configure(component => component.LifeStyle.Transient
                .Named(component.Implementation.FullName + "XYZ")
            )
        );

        foreach (var handler in Kernel.GetAssignableHandlers(typeof(ICommon)))
        {
            Assert.Equal(LifestyleType.Transient, handler.ComponentModel.LifestyleType);
            Assert.Equal(handler.ComponentModel.Implementation.FullName + "XYZ", handler.ComponentModel.Name);
        }
    }

    [Fact]
    public void RegisterAssemblyTypes_WithLinqConfigurationReturningValue_RegisteredInContainer()
    {
        Kernel.Register(Classes
            .FromAssembly(AssemblyHelper.GetCurrentAssembly())
            .BasedOn<ICommon>()
            .Configure(component => component.LifestyleTransient())
        );

        foreach (var handler in Kernel.GetAssignableHandlers(typeof(ICommon)))
        {
            Assert.Equal(LifestyleType.Transient, handler.ComponentModel.LifestyleType);
        }
    }

    [Fact]
    public void RegisterMultipleAssemblyTypes_BasedOn_RegisteredInContainer()
    {
        Kernel.Register(Classes.FromAssembly(AssemblyHelper.GetCurrentAssembly()).BasedOn<ICommon>());
        Kernel.Register(Classes.FromAssembly(AssemblyHelper.GetCurrentAssembly()).BasedOn<ICustomer>()
            .If(t =>
            {
                Debug.Assert(t.FullName != null);
                return t.FullName.Contains("Chain");
            }));
        Kernel.Register(Classes.FromAssembly(AssemblyHelper.GetCurrentAssembly()).BasedOn<DefaultTemplateEngine>());
        Kernel.Register(Classes.FromAssembly(AssemblyHelper.GetCurrentAssembly()).BasedOn<DefaultMailSenderService>());
        Kernel.Register(Classes.FromAssembly(AssemblyHelper.GetCurrentAssembly())
            .BasedOn<DefaultSpamServiceWithConstructor>());

        var handlers = Kernel.GetHandlers(typeof(ICommon));
        Assert.Empty(handlers);

        handlers = Kernel.GetAssignableHandlers(typeof(ICommon));
        Assert.NotEmpty(handlers);

        handlers = Kernel.GetAssignableHandlers(typeof(ICustomer));
        Assert.NotEmpty(handlers);

        foreach (var handler in handlers)
        {
            Assert.Contains("Chain", handler.ComponentModel.Implementation.FullName);
        }

        handlers = Kernel.GetAssignableHandlers(typeof(DefaultSpamServiceWithConstructor));
        Assert.Single(handlers);
    }

    [Fact]
    public void RegisterAssemblyTypes_WhereConditionSatisifed_RegisteredInContainer()
    {
        Kernel.Register(
            Classes.FromAssembly(AssemblyHelper.GetCurrentAssembly())
                .Where(t => t.Name == "CustomerImpl")
                .WithService.FirstInterface()
        );

        var handlers = Kernel.GetHandlers(typeof(ICustomer));
        Assert.Single(handlers);
    }

    [Fact]
    public void RegisterAssemblyTypes_OnlyPublicTypes_WillNotRegisterNonPublicTypes()
    {
        Kernel.Register(
            Classes.FromAssembly(AssemblyHelper.GetCurrentAssembly())
                .BasedOn<NonPublicComponent>()
        );

        var handlers = Kernel.GetHandlers(typeof(NonPublicComponent));
        Assert.Empty(handlers);
    }

    [Fact]
    public void RegisterAssemblyTypes_IncludeNonPublicTypes_WillNRegisterNonPublicTypes()
    {
        Kernel.Register(
            Classes.FromAssembly(AssemblyHelper.GetCurrentAssembly())
                .IncludeNonPublicTypes()
                .BasedOn<NonPublicComponent>()
        );

        var handlers = Kernel.GetHandlers(typeof(NonPublicComponent));
        Assert.Single(handlers);
    }

    [Fact]
    public void RegisterAssemblyTypes_WhenTypeInNamespace_RegisteredInContainer()
    {
        Kernel.Register(
            Classes.FromAssembly(AssemblyHelper.GetCurrentAssembly())
                .Where(Component.IsInNamespace("Castle.Windsor.Tests.ClassComponents"))
                .WithService.FirstInterface()
        );

        var handlers = Kernel.GetHandlers(typeof(ICustomer));
        Assert.True(handlers.Length > 0);
    }

    [Fact]
    public void RegisterAssemblyTypes_WhenTypeInMissingNamespace_NotRegisteredInContainer()
    {
        Kernel.Register(
            Classes.FromAssembly(AssemblyHelper.GetCurrentAssembly())
                .Where(Component.IsInNamespace("Castle.Windsor.MicroKernel.Tests.AnyClass"))
                .WithService.FirstInterface()
        );

        Assert.Empty(Kernel.GetAssignableHandlers(typeof(object)));
    }

    [Fact]
    public void RegisterAssemblyTypes_WhenTypeInSameNamespaceAsComponent_RegisteredInContainer()
    {
        Kernel.Register(
            Classes.FromAssembly(AssemblyHelper.GetCurrentAssembly())
                .Where(Component.IsInSameNamespaceAs<CustomerImpl2>())
                .WithService.FirstInterface()
        );

        var handlers = Kernel.GetHandlers(typeof(ICustomer));
        Assert.True(handlers.Length > 0);
    }

    [Fact]
    public void RegisterSpecificTypes_WithGenericDefinition_RegisteredInContainer()
    {
        Kernel.Register(Classes.From(typeof(CustomerRepository))
            .Pick()
            .WithService.FirstInterface()
        );

        var repository = Kernel.Resolve<IRepository<ICustomer>>();
        Assert.NotNull(repository);
    }

    [Fact]
    public void RegisterGenericTypes_BasedOnGenericDefinitionUsingSelect_RegisteredInContainer()
    {
        Kernel.Register(Classes.FromAssembly(AssemblyHelper.GetCurrentAssembly())
            .BasedOn<ITask>()
            .WithService.Select((t, _) =>
                from type in t.GetInterfaces()
                where type != typeof(ITask)
                select type));
        Assert.NotNull(Kernel.Resolve<ITask<object>>());
    }

    [Fact]
    public void RegisterAssemblyTypes_MultipleBasedOn_RegisteredInContainer()
    {
        Kernel.Register(Classes
            .FromAssembly(AssemblyHelper.GetCurrentAssembly())
            .BasedOn<ICommon>()
            .OrBasedOn(typeof(ICommon2))
        );

        var handlers = Kernel.GetHandlers(typeof(CommonImpl1));
        Assert.Single(handlers);

        handlers = Kernel.GetHandlers(typeof(Common2Impl));
        Assert.Single(handlers);

        handlers = Kernel.GetHandlers(typeof(TwoInterfacesImpl));
        Assert.Single(handlers);
    }

    [Fact]
    public void RegisterAssemblyTypes_MultipleBasedOnWithServiceBase_RegisteredInContainer()
    {
        Kernel.Register(Classes
            .FromAssembly(AssemblyHelper.GetCurrentAssembly())
            .BasedOn<ICommon>()
            .OrBasedOn(typeof(ICommon2))
            .WithServiceBase()
        );

        var handlers = Kernel.GetHandlers(typeof(ICommon));
        Assert.NotEmpty(handlers);

        handlers = Kernel.GetHandlers(typeof(ICommon2));
        Assert.NotEmpty(handlers);
    }

    [Fact]
    public void RegisterAssemblyTypes_MultipleBasedOnWithThreeBases_RegisteredInContainer()
    {
        Kernel.Register(Classes
            .FromAssembly(AssemblyHelper.GetCurrentAssembly())
            .BasedOn<ICommon>()
            .OrBasedOn(typeof(ICommon2))
            .OrBasedOn(typeof(IValidator<>))
            .WithServiceBase()
        );

        var handlers = Kernel.GetHandlers(typeof(ICommon));
        Assert.NotEmpty(handlers);

        handlers = Kernel.GetHandlers(typeof(ICommon2));
        Assert.NotEmpty(handlers);

        var validatorHandlers = Kernel.GetHandlers(typeof(IValidator<ICustomer>));
        Assert.NotEmpty(validatorHandlers);
    }

    [Fact]
    public void RegisterGenericTypes_MultipleBasedOnWithGenericDefinition_RegisteredInContainer()
    {
        Kernel.Register(Classes
            .FromAssembly(AssemblyHelper.GetCurrentAssembly())
            .BasedOn(typeof(IValidator<>))
            .OrBasedOn(typeof(IRepository<>))
            .WithService.Base()
        );

        var validatorHandlers = Kernel.GetHandlers(typeof(IValidator<ICustomer>));
        Assert.NotEmpty(validatorHandlers);

        var repositoryHandlers = Kernel.GetHandlers(typeof(IRepository<ICustomer>));
        Assert.NotEmpty(repositoryHandlers);
    }

    [Fact]
    public void RegisterGenericTypes_MultipleBasedOnImplementingBothInterfaces_RegisteredWithBothAsServices()
    {
        Kernel.Register(Classes
            .FromAssembly(AssemblyHelper.GetCurrentAssembly())
            .BasedOn(typeof(IValidator<>))
            .OrBasedOn(typeof(IRepository<>))
            .WithService.Base()
        );

        var handler = Kernel
            .GetHandlers(typeof(IValidator<ICustomer>))
            .Single(x => x.ComponentModel.Implementation == typeof(CustomerValidatorAndRepository));

        var services = handler.ComponentModel.Services.ToList();
        Assert.Contains(typeof(IRepository<ICustomer>), services);
    }

    [Fact]
    public void RegisterGenericTypes_MultipleBasedOnImplementingOneInterface_RegisteredWithOneService()
    {
        Kernel.Register(Classes
            .FromAssembly(AssemblyHelper.GetCurrentAssembly())
            .BasedOn(typeof(IValidator<>))
            .OrBasedOn(typeof(IRepository<>))
            .WithService.Base()
        );

        var handler = Kernel
            .GetHandlers(typeof(IValidator<ICustomer>))
            .Single(x => x.ComponentModel.Implementation == typeof(CustomerValidator));

        var services = handler.ComponentModel.Services.ToArray();
        Assert.Single(services);
    }

    [Fact]
    public void RegisterGenericTypes_MultipleBasedOnWithTheSameTypesTwice_SelectedAsOneBase()
    {
        Type[] services = null;
        Kernel.Register(Classes
            .FromAssembly(AssemblyHelper.GetCurrentAssembly())
            .BasedOn(typeof(IValidator<>))
            .OrBasedOn(typeof(IValidator<>))
            .WithService.Select((_, b) =>
            {
                services = b;
                return b;
            })
        );

        Assert.Single(services);
    }
}