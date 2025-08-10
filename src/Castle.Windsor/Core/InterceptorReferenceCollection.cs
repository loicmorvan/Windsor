// Copyright 2004-2013 Castle Project - http://www.castleproject.org/
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

using System.Collections;
using System.Diagnostics;
using Castle.DynamicProxy;
using Castle.Windsor.Core.Internal;
using Castle.Windsor.MicroKernel;

namespace Castle.Windsor.Core;

/// <summary>Collection of <see cref="InterceptorReference" /></summary>
[Serializable]
public class InterceptorReferenceCollection(ComponentModel component) : IMutableCollection<InterceptorReference>
{
    [DebuggerBrowsable(DebuggerBrowsableState.Never)]
    private readonly ComponentModel _component = component;

    [DebuggerBrowsable(DebuggerBrowsableState.RootHidden)] [DebuggerDisplay("Count = {list.Count}", Name = "")]
    private readonly List<InterceptorReference> _list = [];

    /// <summary>Gets a value indicating whether this instance has interceptors.</summary>
    /// <value><c>true</c> if this instance has interceptors; otherwise, <c>false</c>.</value>
    [DebuggerBrowsable(DebuggerBrowsableState.Never)]
    public bool HasInterceptors => _list.Count != 0;

    [DebuggerBrowsable(DebuggerBrowsableState.Never)]
    public int Count => _list.Count;

    /// <summary>Returns an enumerator that can iterate through a collection.</summary>
    /// <returns>An <see cref="T:System.Collections.IEnumerator" /> that can be used to iterate through the collection.</returns>
    public IEnumerator GetEnumerator()
    {
        return _list.GetEnumerator();
    }

    /// <summary>Adds the specified item.</summary>
    /// <param name="item">The interceptor.</param>
    public void Add(InterceptorReference item)
    {
        AddLast(item);
    }

    IEnumerator<InterceptorReference> IEnumerable<InterceptorReference>.GetEnumerator()
    {
        return _list.GetEnumerator();
    }

    bool IMutableCollection<InterceptorReference>.Remove(InterceptorReference item)
    {
        return _list.Remove(item);
    }

    /// <summary>Adds the specified interceptor as the first.</summary>
    /// <param name="item">The interceptor.</param>
    public void AddFirst(InterceptorReference item)
    {
        Insert(0, item);
    }

    /// <summary>Adds the interceptor to the end of the interceptors list if it does not exist already.</summary>
    /// <param name="interceptorReference">The interceptor reference.</param>
    public void AddIfNotInCollection(InterceptorReference interceptorReference)
    {
        if (_list.Contains(interceptorReference) == false)
        {
            AddLast(interceptorReference);
        }
    }

    /// <summary>Adds the specified interceptor as the last.</summary>
    /// <param name="item">The interceptor.</param>
    public void AddLast(InterceptorReference item)
    {
        _list.Add(item);
        Attach(item);
    }

    /// <summary>Inserts the specified interceptor at the specified index.</summary>
    /// <param name="index">The index.</param>
    /// <param name="item">The interceptor.</param>
    public void Insert(int index, InterceptorReference item)
    {
        _list.Insert(index, item);
        Attach(item);
    }

    public InterceptorReference[] ToArray()
    {
        return _list.ToArray();
    }

    private void Attach(IReference<IInterceptor> interceptor)
    {
        interceptor.Attach(_component);
    }
}