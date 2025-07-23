// Copyright 2004-2010 Castle Project - http://www.castleproject.org/
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

using System.Threading;
using Castle.Windsor.MicroKernel;
using Castle.Windsor.MicroKernel.Registration;

namespace Castle.Windsor.Tests.Pools;

public class MultithreadedPooledTestCase
{
	private readonly ManualResetEvent _startEvent = new(false);
	private readonly ManualResetEvent _stopEvent = new(false);
	private IKernel _kernel;

	private void ExecuteMethodUntilSignal()
	{
		_startEvent.WaitOne(int.MaxValue);

		while (!_stopEvent.WaitOne(1))
		{
			var instance = _kernel.Resolve<PoolableComponent1>("a");

			Assert.NotNull(instance);

			Thread.Sleep(1 * 500);

			_kernel.ReleaseComponent(instance);
		}
	}

	[Fact]
	public void Multithreaded()
	{
		_kernel = new DefaultKernel();
		_kernel.Register(Component.For(typeof(PoolableComponent1)).Named("a"));

		const int threadCount = 15;

		var threads = new Thread[threadCount];

		for (var i = 0; i < threadCount; i++)
		{
			threads[i] = new Thread(ExecuteMethodUntilSignal);
			threads[i].Start();
		}

		_startEvent.Set();

		Thread.CurrentThread.Join(3 * 1000);

		_stopEvent.Set();
	}
}