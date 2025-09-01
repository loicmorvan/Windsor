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

using Castle.Core.Resource;
using Castle.Windsor.MicroKernel;
using Castle.Windsor.MicroKernel.SubSystems.Configuration;
using JetBrains.Annotations;

namespace Castle.Windsor.Windsor.Configuration.Interpreters;

/// <summary>Provides common methods for those who wants to implement <see cref="IConfigurationInterpreter" /></summary>
public abstract class AbstractInterpreter : IConfigurationInterpreter
{
    private readonly Stack<IResource> _resourceStack = new();

    protected AbstractInterpreter(IResource source)
    {
        Source = source ?? throw new ArgumentNullException(nameof(source), "IResource is null");

        PushResource(source);
    }

    protected AbstractInterpreter(string filename) : this(new FileResource(filename))
    {
    }

    [PublicAPI] protected IResource? CurrentResource => _resourceStack.Count == 0 ? null : _resourceStack.Peek();


    /// <summary>
    ///     Should obtain the contents from the resource, interpret it and populate the <see cref="IConfigurationStore" />
    ///     accordingly.
    /// </summary>
    /// <param name="resource"></param>
    /// <param name="store"></param>
    /// <param name="kernel"></param>
    public abstract void ProcessResource(IResource resource, IConfigurationStore store, IKernel kernel);

    /// <summary>Exposes the reference to <see cref="IResource" /> which the interpreter is likely to hold</summary>
    /// <value></value>
    public IResource Source { get; }

    /// <summary>Gets or sets the name of the environment.</summary>
    /// <value>The name of the environment.</value>
    public string? EnvironmentName { get; set; }

    [PublicAPI]
    protected void PushResource(IResource resource)
    {
        _resourceStack.Push(resource);
    }

    [PublicAPI]
    protected void PopResource()
    {
        _resourceStack.Pop();
    }
}