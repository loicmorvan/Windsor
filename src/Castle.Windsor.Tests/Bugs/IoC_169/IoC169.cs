using System.Reflection;
using Castle.Windsor.Facilities.Startable;
using Castle.Windsor.MicroKernel.Registration;
using Castle.Windsor.Windsor;

namespace Castle.Windsor.Tests.Bugs.IoC_169;

public class IoC169
{
    [Fact]
    public void BulkRegistrations_WhenRegistrationMatchesNoInstancesOfService_StopsStartableFacilityFromWorking()
    {
        AbstractBlackboard.PrepareForTest();

        var container = new WindsorContainer();

        container.AddFacility(new StartableFacility());

        container.Register(Component.For(typeof(IBlackboard)).ImplementedBy<Blackboard>().Named("blackboard"));

        var registrations = Classes.FromAssembly(GetType().GetTypeInfo().Assembly)
            .BasedOn<IServiceWithoutImplementation>()
            .Unless(t => container.Kernel.HasComponent(t));

        container.Register(registrations);

        container.Kernel.Register(Component.For<IChalk>().Named("chalk").Instance(new Chalk()));

        Assert.True(AbstractBlackboard.Started); // fails here, service is never started
    }
}