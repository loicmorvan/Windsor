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

using System;

namespace Castle.MicroKernel.Releasers;

/// <summary>
///     No tracking of component instances are made.
/// </summary>
[Serializable]
[Obsolete(
	"This class is a hack, will be removed in the future release and should be avoided. Please implement proper lifecycle management instead.")]
public class NoTrackingReleasePolicy : IReleasePolicy
{
	public void Dispose()
	{
	}

	public IReleasePolicy CreateSubPolicy()
	{
		return this;
	}

	public bool HasTrack(object instance)
	{
		return false;
	}

	public void Release(object instance)
	{
	}

	public void Track(object instance, Burden burden)
	{
	}
}