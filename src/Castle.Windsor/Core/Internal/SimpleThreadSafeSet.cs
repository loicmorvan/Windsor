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

using JetBrains.Annotations;
using Lock = Castle.Windsor.MicroKernel.Internal.Lock;

namespace Castle.Windsor.Core.Internal;

public class SimpleThreadSafeSet<T>
{
    private readonly HashSet<T> _implementation = [];
    private readonly Lock _lock = Lock.Create();

    [PublicAPI]
    public int Count
    {
        get
        {
            using (_lock.ForReading())
            {
                return _implementation.Count;
            }
        }
    }

    [PublicAPI]
    public bool Add(T item)
    {
        using (_lock.ForWriting())
        {
            return _implementation.Add(item);
        }
    }

    [PublicAPI]
    public bool Remove(T item)
    {
        using (_lock.ForWriting())
        {
            return _implementation.Remove(item);
        }
    }

    public T[] ToArray()
    {
        List<T> hashSetCopy;
        using (_lock.ForReading())
        {
            hashSetCopy = new List<T>(_implementation);
        }

        return hashSetCopy.ToArray();
    }
}