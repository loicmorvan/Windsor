using Castle.Windsor.MicroKernel.Registration;
using Castle.Windsor.MicroKernel.SubSystems.Configuration;
using Castle.Windsor.Windsor;

namespace Castle.Windsor.Tests.Installer;

internal class Installer : IWindsorInstaller
{
    private readonly Action<IWindsorContainer> _install;

    public Installer(Action<IWindsorContainer> install)
    {
        _install = install;
    }

    public void Install(IWindsorContainer container, IConfigurationStore store)
    {
        _install(container);
    }
}