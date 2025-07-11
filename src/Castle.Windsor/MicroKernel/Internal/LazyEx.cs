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

namespace Castle.MicroKernel.Internal;

public class LazyEx<T> : Lazy<T>, IDisposable
{
	private readonly IKernel _kernel;

	public LazyEx(IKernel kernel, Arguments arguments)
		: base(() => kernel.Resolve<T>(arguments))
	{
		_kernel = kernel;
	}

	public LazyEx(IKernel kernel, string overrideComponentName)
		: base(() => kernel.Resolve<T>(overrideComponentName))
	{
		_kernel = kernel;
	}

	public LazyEx(IKernel kernel, string overrideComponentName, Arguments arguments)
		: base(() => kernel.Resolve<T>(overrideComponentName, arguments))
	{
		_kernel = kernel;
	}

	public LazyEx(IKernel kernel)
		: base(kernel.Resolve<T>)
	{
		_kernel = kernel;
	}

	public void Dispose()
	{
		if (IsValueCreated) _kernel.ReleaseComponent(Value);
	}
}