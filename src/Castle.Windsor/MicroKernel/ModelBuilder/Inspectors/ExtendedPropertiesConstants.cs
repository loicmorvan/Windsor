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

namespace Castle.MicroKernel.ModelBuilder.Inspectors;

/// <summary>
///     Only to hold internal constants and get rid of
///     magic numbers and hardcode names.
/// </summary>
internal abstract class ExtendedPropertiesConstants
{
	public static readonly int PoolDefaultInitialPoolSize = 5;
	public static readonly int PoolDefaultMaxPoolSize = 15;

	public static readonly string PoolInitialPoolSize = "pool.initial.pool.size";
	public static readonly string PoolMaxPoolSize = "pool.max.pool.size";
}