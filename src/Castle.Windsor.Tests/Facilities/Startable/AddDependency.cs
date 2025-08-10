using Castle.Windsor.Core;
using Castle.Windsor.MicroKernel;
using Castle.Windsor.MicroKernel.ModelBuilder;

namespace Castle.Windsor.Tests.Facilities.Startable;

public class AddDependency : IComponentModelDescriptor
{
    private readonly DependencyModel _dependency;

    public AddDependency(DependencyModel dependency)
    {
        _dependency = dependency;
    }

    public void BuildComponentModel(IKernel kernel, ComponentModel model)
    {
        model.Dependencies.Add(_dependency);
    }

    public void ConfigureComponentModel(IKernel kernel, ComponentModel model)
    {
    }
}