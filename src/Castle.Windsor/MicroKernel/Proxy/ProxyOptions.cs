// Copyright 2004-2012 Castle Project - http://www.castleproject.org/
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

using Castle.DynamicProxy;
using Castle.Windsor.Core;
using JetBrains.Annotations;

namespace Castle.Windsor.MicroKernel.Proxy;

/// <summary>Represents options to configure proxies.</summary>
public class ProxyOptions
{
    private readonly ComponentModel _component;
    private IReference<IProxyGenerationHook> _hook;
    private List<Type> _interfaceList;
    private List<IReference<object>> _mixInList;

    private IReference<IInterceptorSelector> _selector;

    /// <summary>Initializes a new instance of the <see cref="ProxyOptions" /> class.</summary>
    public ProxyOptions(ComponentModel component)
    {
        _component = component;
    }

    /// <summary>Gets the additional interfaces to proxy.</summary>
    /// <value> The interfaces. </value>
    public Type[] AdditionalInterfaces => _interfaceList != null ? _interfaceList.ToArray() : Type.EmptyTypes;

    /// <summary>Determines if the proxied component can change targets.</summary>
    // ReSharper disable once UnusedAutoPropertyAccessor.Global
    public bool AllowChangeTarget { get; set; }

    /// <summary>Gets or sets the proxy hook.</summary>
    public IReference<IProxyGenerationHook> Hook
    {
        get => _hook;
        set => SetReferenceValue(ref _hook, value);
    }

    /// <summary>Gets the mix ins to integrate.</summary>
    /// <value> The interfaces. </value>
    public IEnumerable<IReference<object>> MixIns
    {
        get
        {
            if (_mixInList != null)
            {
                return _mixInList;
            }

            return [];
        }
    }

    /// <summary>Determines if the proxied component uses a target.</summary>
    public bool OmitTarget { get; set; }

    /// <summary>Gets or sets the interceptor selector.</summary>
    public IReference<IInterceptorSelector> Selector
    {
        get => _selector;
        set => SetReferenceValue(ref _selector, value);
    }

    public bool RequiresProxy => _interfaceList != null || _mixInList != null || _hook != null;

    /// <summary>Adds the additional interfaces to proxy.</summary>
    /// <param name="interfaces"> The interfaces. </param>
    public void AddAdditionalInterfaces(params Type[] interfaces)
    {
        if (interfaces == null || interfaces.Length == 0)
        {
            return;
        }

        _interfaceList ??= [];

        _interfaceList.AddRange(interfaces);
    }

    /// <summary>Adds the additional mix ins to integrate.</summary>
    /// <param name="mixIns"> The mix ins. </param>
    [PublicAPI]
    public void AddMixIns(params object[] mixIns)
    {
        if (mixIns == null || mixIns.Length == 0)
        {
            return;
        }

        _mixInList ??= [];

        foreach (var mixIn in mixIns)
        {
            var reference = new InstanceReference<object>(mixIn);
            _mixInList.Add(reference);
            reference.Attach(_component);
        }
    }

    /// <summary>Adds the additional mix in to integrate.</summary>
    /// <param name="mixIn"> The mix in. </param>
    public void AddMixinReference(IReference<object> mixIn)
    {
        ArgumentNullException.ThrowIfNull(mixIn);

        _mixInList ??= [];

        _mixInList.Add(mixIn);
        mixIn.Attach(_component);
    }

    /// <summary>Equals the specified obj.</summary>
    /// <param name="obj"> The obj. </param>
    /// <returns> true if equal. </returns>
    public override bool Equals(object obj)
    {
        if (this == obj)
        {
            return true;
        }

        if (obj is not ProxyOptions proxyOptions)
        {
            return false;
        }

        if (!Equals(Hook, proxyOptions.Hook))
        {
            return false;
        }

        if (!Equals(Selector, proxyOptions.Selector))
        {
            return false;
        }

        if (!Equals(OmitTarget, proxyOptions.OmitTarget))
        {
            return false;
        }

        return AdditionalInterfacesAreEquals(proxyOptions) &&
               MixInsAreEquals(proxyOptions);
    }

    /// <summary>Gets the hash code.</summary>
    /// <returns> </returns>
    public override int GetHashCode()
    {
        return 0;
    }

    private bool AdditionalInterfacesAreEquals(ProxyOptions proxyOptions)
    {
        if (!Equals(_interfaceList == null, proxyOptions._interfaceList == null))
        {
            return false;
        }

        if (_interfaceList == null)
        {
            return true; //both are null, nothing more to check
        }

        if (proxyOptions._interfaceList != null && _interfaceList.Count != proxyOptions._interfaceList.Count)
        {
            return false;
        }

        return _interfaceList.All(t => proxyOptions._interfaceList == null || proxyOptions._interfaceList.Contains(t));
    }

    private bool MixInsAreEquals(ProxyOptions proxyOptions)
    {
        if (!Equals(_mixInList == null, proxyOptions._mixInList == null))
        {
            return false;
        }

        if (_mixInList == null)
        {
            return true; //both are null, nothing more to check
        }

        if (proxyOptions._mixInList != null && _mixInList.Count != proxyOptions._mixInList.Count)
        {
            return false;
        }

        return _mixInList.All(t => proxyOptions._mixInList == null || proxyOptions._mixInList.Contains(t));
    }

    private void SetReferenceValue<T>(ref IReference<T> reference, IReference<T> value)
    {
        reference?.Detach(_component);
        value?.Attach(_component);
        reference = value;
    }
}