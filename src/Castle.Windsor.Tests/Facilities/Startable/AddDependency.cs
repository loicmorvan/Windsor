using Castle.Windsor.Core;
using Castle.Windsor.MicroKernel;
using Castle.Windsor.MicroKernel.ModelBuilder;

namespace Castle.Windsor.Tests.Facilities.Startable;

public class AddDependency(DependencyModel dependency) : IComponentModelDescriptor
{
    public void BuildComponentModel(IKernel kernel, ComponentModel model)
    {
        model.Dependencies.Add(dependency);
    }

    public void ConfigureComponentModel(IKernel kernel, ComponentModel model)
    {
    }
}