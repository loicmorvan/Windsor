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

namespace Castle.Windsor.Core.Internal;

/// <summary>Represents a collection of objects which are guaranteed to be unique and holds a color for them</summary>
internal class ColorsSet
{
    private readonly IDictionary<IVertex, VertexColor> _items = new Dictionary<IVertex, VertexColor>();

    public ColorsSet(IEnumerable<IVertex> items)
    {
        foreach (var item in items)
        {
            Set(item, VertexColor.White);
        }
    }

    public VertexColor ColorOf(IVertex item)
    {
        return _items.TryGetValue(item, out var vertexColor)
            ? vertexColor
            : VertexColor.NotInThisSet;
    }

    public void Set(IVertex item, VertexColor color)
    {
        _items[item] = color;
    }
}