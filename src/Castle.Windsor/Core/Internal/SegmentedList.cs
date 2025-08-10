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

namespace Castle.Windsor.Core.Internal;

public class SegmentedList<T>(int segmentCount)
{
    private readonly List<T>[] _segments = new List<T>[segmentCount];

    public void AddFirst(int segmentIndex, T item)
    {
        GetSegment(segmentIndex).Insert(0, item);
    }

    public void AddLast(int segmentIndex, T item)
    {
        GetSegment(segmentIndex).Add(item);
    }

    public T[] ToArray()
    {
        return _segments.Where(l => l != null)
            .SelectMany(l => l)
            .ToArray();
    }

    private List<T> GetSegment(int segmentIndex)
    {
        return _segments[segmentIndex] ?? (_segments[segmentIndex] = new List<T>(4));
    }
}