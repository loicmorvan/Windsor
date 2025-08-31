// Copyright 2004-2011 Castle Project - http://www.castleproject.org/
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

using Castle.Windsor.Core.Internal;
using Castle.Windsor.MicroKernel;

namespace Castle.Windsor.Windsor.Diagnostics;

public partial class DefaultDiagnosticsSubSystem :
    AbstractSubSystem
{
    private readonly IDictionary<Type, IDiagnostic<object>> _diagnostics = new Dictionary<Type, IDiagnostic<object>>();

    public void AddDiagnostic<TDiagnostic>(TDiagnostic diagnostic) where TDiagnostic : IDiagnostic<object>
    {
        _diagnostics.Add(typeof(TDiagnostic), diagnostic);
    }

    public TDiagnostic? GetDiagnostic<TDiagnostic>() where TDiagnostic : IDiagnostic<object>
    {
        _diagnostics.TryGetValue(typeof(TDiagnostic), out var value);
        return (TDiagnostic?)value;
    }

    public override void Terminate()
    {
        // ReSharper disable once SuspiciousTypeConversion.Global
        _diagnostics.Values.OfType<IDisposable>().ForEach(e => e.Dispose());
    }
}