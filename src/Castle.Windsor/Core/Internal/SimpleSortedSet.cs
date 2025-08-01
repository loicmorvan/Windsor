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

using System.Collections;
using JetBrains.Annotations;

namespace Castle.Windsor.Core.Internal;

public class SimpleSortedSet<T>(IComparer<T> comparer) : ICollection<T>
{
    private readonly List<T> _items = [];

    public SimpleSortedSet() : this(Comparer<T>.Default)
    {
    }

    [PublicAPI]
    public SimpleSortedSet(IEnumerable<T> other, IComparer<T> comparer) : this(comparer)
    {
        foreach (var item in other)
        {
            Add(item);
        }
    }

    public T this[int index] => _items[index];

    public int Count => _items.Count;

    bool ICollection<T>.IsReadOnly => false;

    public void Add(T item)
    {
        var count = Count;
        for (var i = 0; i < count; i++)
        {
            var result = comparer.Compare(item, _items[i]);
            if (result < 0)
            {
                _items.Insert(i, item);
                return;
            }

            if (result == 0)
            {
                return;
            }
        }

        _items.Add(item);
    }

    public void Clear()
    {
        _items.Clear();
    }

    public bool Contains(T item)
    {
        return _items.Contains(item);
    }

    public void CopyTo(T[] array, int arrayIndex)
    {
        _items.CopyTo(array, arrayIndex);
    }

    public bool Remove(T item)
    {
        return _items.Remove(item);
    }

    public IEnumerator<T> GetEnumerator()
    {
        return _items.GetEnumerator();
    }

    IEnumerator IEnumerable.GetEnumerator()
    {
        return GetEnumerator();
    }
}