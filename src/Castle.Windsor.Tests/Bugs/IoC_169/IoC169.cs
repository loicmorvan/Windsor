using System.Reflection;
using Castle.Windsor.Facilities.Startable;
using Castle.Windsor.MicroKernel.Registration;
using Castle.Windsor.Tests.Facilities.TypedFactory;
using Castle.Windsor.Windsor;

namespace Castle.Windsor.Tests.Bugs.IoC_169;

public class IoC169
{
    [Fact]
    public void BulkRegistrations_WhenRegistrationMatchesNoInstancesOfService_StopsStartableFacilityFromWorking()
    {
        var dataRepository = new DataRepository();
            
        var container = new WindsorContainer();

        container.AddFacility(new StartableFacility());

        container.Register(
            Component.For<DataRepository>().Instance(dataRepository),
            Component.For(typeof(IBlackboard)).ImplementedBy<Blackboard>().Named("blackboard"));

        var registrations = Classes.FromAssembly(GetType().GetTypeInfo().Assembly)
            .BasedOn<IServiceWithoutImplementation>()
            .Unless(t => container.Kernel.HasComponent(t));

        container.Register(registrations);

        container.Kernel.Register(Component.For<IChalk>().Named("chalk").Instance(new Chalk()));

        Assert.Equal(1, dataRepository["Start"]); // fails here, service is never started
    }
}