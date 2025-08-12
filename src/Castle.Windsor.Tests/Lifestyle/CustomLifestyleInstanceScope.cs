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

using Castle.Windsor.Core;
using Castle.Windsor.MicroKernel;

namespace Castle.Windsor.Tests.Lifestyle;

public sealed class CustomLifestyleInstanceScope : IDisposable
{
    [ThreadStatic] private static Stack<CustomLifestyleInstanceScope> _localScopes;

    public CustomLifestyleInstanceScope()
    {
        _localScopes ??= new Stack<CustomLifestyleInstanceScope>();
        _localScopes.Push(this);
    }

    public IDictionary<ComponentModel, Burden> Cache { get; } = new Dictionary<ComponentModel, Burden>();

    public static CustomLifestyleInstanceScope Current
    {
        get
        {
            if (_localScopes == null || _localScopes.Count == 0)
            {
                return null;
            }

            return _localScopes.Peek();
        }
    }

    public void Dispose()
    {
        _localScopes.Pop();
    }
}