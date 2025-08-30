using System.Diagnostics;
using Castle.Windsor.MicroKernel;
using Castle.Windsor.Windsor;
using Castle.Windsor.Windsor.Diagnostics;

namespace Castle.Windsor.Tests.Diagnostics;

public abstract class DiagnosticsContainerTestCase : AbstractContainerTestCase
{
    protected DiagnosticsContainerTestCase(WindsorContainer? container = null) : base(container)
    {
        var host = (IDiagnosticsHost?)Kernel.GetSubSystem(SubSystemConstants.DiagnosticsKey);
        Debug.Assert(host != null);

        PotentiallyMisconfiguredComponentsDiagnostic =
            host.GetDiagnostic<IPotentiallyMisconfiguredComponentsDiagnostic>()
            ?? throw new InvalidOperationException();

        PotentialLifestyleMismatchesDiagnostic = host.GetDiagnostic<IPotentialLifestyleMismatchesDiagnostic>()
                                                 ?? throw new InvalidOperationException();

        UsingContainerAsServiceLocatorDiagnostic = host.GetDiagnostic<IUsingContainerAsServiceLocatorDiagnostic>()
                                                   ?? throw new InvalidOperationException();
    }

    protected IUsingContainerAsServiceLocatorDiagnostic UsingContainerAsServiceLocatorDiagnostic { get; }

    protected IPotentialLifestyleMismatchesDiagnostic PotentialLifestyleMismatchesDiagnostic { get; }

    protected IPotentiallyMisconfiguredComponentsDiagnostic PotentiallyMisconfiguredComponentsDiagnostic { get; }
}