// Copyright 2004-2014 Castle Project - http://www.castleproject.org/
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
using Castle.Windsor.MicroKernel.Context;
using Lock = Castle.Windsor.MicroKernel.Internal.Lock;

namespace Castle.Windsor.MicroKernel.Lifestyle.Pool;

[Serializable]
public class DefaultPool(int initialSize, int maxsize, IComponentActivator componentActivator)
    : IPool
{
    private readonly Stack<Burden> _available = new(initialSize);
    private readonly IComponentActivator _componentActivator = componentActivator;
    private readonly int _initialSize = initialSize;
    private readonly Dictionary<object, Burden> _inUse = new();
    private readonly int _maxsize = maxsize;
    private readonly Lock _rwlock = Lock.Create();
    private bool _initialized;

    public virtual void Dispose()
    {
        _initialized = false;

        foreach (var burden in _available)
        {
            burden.Release();
        }

        _inUse.Clear();
        _available.Clear();
    }

    public virtual bool Release(object instance)
    {
        using (_rwlock.ForWriting())
        {
            Burden burden;

            if (_initialized == false)
            {
                _inUse.Remove(instance, out burden);
            }
            else
            {
                if (_inUse.Remove(instance, out burden) == false)
                {
                    return false;
                }

                if (_available.Count < _maxsize)
                {
                    if (instance is IRecyclable recyclable)
                    {
                        recyclable.Recycle();
                    }

                    _available.Push(burden);
                    return false;
                }
            }
        }

        // Pool is full or has been disposed.

        _componentActivator.Destroy(instance);
        return true;
    }

    public virtual object Request(CreationContext context, Func<CreationContext, Burden> creationCallback)
    {
        Burden burden;
        using (_rwlock.ForWriting())
        {
            if (!_initialized)
            {
                Intitialize(creationCallback, context);
            }

            if (_available.Count != 0)
            {
                burden = _available.Pop();
                context.AttachExistingBurden(burden);
            }
            else
            {
                burden = creationCallback.Invoke(context);
            }

            try
            {
                _inUse.Add(burden.Instance, burden);
            }
            catch (NullReferenceException)
            {
                throw new PoolException("creationCallback didn't return a valid burden");
            }
            catch (ArgumentNullException)
            {
                throw new PoolException(
                    "burden returned by creationCallback does not have root instance associated with it (its Instance property is null).");
            }
        }

        return burden.Instance;
    }

    protected virtual void Intitialize(Func<CreationContext, Burden> createCallback, CreationContext c)
    {
        _initialized = true;
        for (var i = 0; i < _initialSize; i++)
        {
            var burden = createCallback(c);
            _available.Push(burden);
        }
    }
}