// Copyright 2004-2014 Castle Project - http://www.castleproject.org/
// Licensed under the Apache License, Version 2.0 (the "License");
// you may not use this file except in compliance with the License.
// You may obtain a copy of the License at
//     http://www.apache.org/licenses/LICENSE-2.0
// Unless required by applicable law or agreed to in writing, software
// distributed under the License is distributed on an "AS IS" BASIS,
// WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied.
// See the License for the specific language governing permissions and
// limitations under the License.

using System;
using System.Threading;

namespace Castle.Core.Internal;

[Serializable]
public class GraphNode :
#if FEATURE_REMOTING
		MarshalByRefObject,
#endif
	IVertex
{
	private SimpleThreadSafeCollection<GraphNode> _outgoing;

	public void AddDependent(GraphNode node)
	{
		var collection = _outgoing;
		if (collection == null)
		{
			var @new = new SimpleThreadSafeCollection<GraphNode>();
			collection = Interlocked.CompareExchange(ref _outgoing, @new, null) ?? @new;
		}

		collection.Add(node);
	}

	/// <summary>The nodes that this node depends on</summary>
	public GraphNode[] Dependents
	{
		get
		{
			var collection = _outgoing;
			if (collection == null) return [];
			return collection.ToArray();
		}
	}

	IVertex[] IVertex.Adjacencies => Dependents;
}