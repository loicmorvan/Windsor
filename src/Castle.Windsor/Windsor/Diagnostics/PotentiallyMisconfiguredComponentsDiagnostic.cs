// Copyright 2004-2022 Castle Project - http://www.castleproject.org/
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

using System.Linq;
using Castle.MicroKernel;

namespace Castle.Windsor.Diagnostics;

public class PotentiallyMisconfiguredComponentsDiagnostic(IKernel kernel)
	: IPotentiallyMisconfiguredComponentsDiagnostic
{
	public IHandler[] Inspect()
	{
		var allHandlers = kernel.GetAssignableHandlers(typeof(object));
		var waitingHandlers = allHandlers.Where(IsWaitingForDependencies).ToArray();
		return waitingHandlers;
	}

	private bool IsWaitingForDependencies(IHandler handler)
	{
		return handler.CurrentState == HandlerState.WaitingDependency;
	}
}