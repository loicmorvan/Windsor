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

using System;
using System.Runtime.ExceptionServices;
using System.Threading;
using System.Threading.Tasks;
using Castle.Windsor.MicroKernel.Lifestyle;
using Castle.Windsor.MicroKernel.Registration;
using Castle.Windsor.Tests.Components;

namespace Castle.Windsor.Tests.Lifestyle;

public class ScopedLifestyleExplicitAndMultipleThreadsTestCase : AbstractContainerTestCase
{
	protected override void AfterContainerCreated()
	{
		Container.Register(Component.For<A>().LifestyleScoped());
	}

#if FEATURE_REMOTING //async delegates depend on Remoting https://github.com/dotnet/corefx/issues/5940 
		[Fact]
		public void Context_is_passed_onto_the_next_thread_Begin_End_Invoke()
		{
			using (Container.BeginScope())
			{
				var instance = default(A);
				var instanceFromOtherThread = default(A);
				instance = Container.Resolve<A>();
				var initialThreadId = Thread.CurrentThread.ManagedThreadId;
				Action action = () =>
				{
					Assert.NotEqual(Thread.CurrentThread.ManagedThreadId, initialThreadId);
					instanceFromOtherThread = Container.Resolve<A>();
				};

				var result = action.BeginInvoke(null, null);
				result.AsyncWaitHandle.WaitOne();
				Assert.Same(instance, instanceFromOtherThread);
			}
		}

		[Fact]
		public void Context_is_NOT_visible_in_unrelated_thread_Begin_End_Invoke()
		{
			var startLock = new ManualResetEvent(false);
			var resolvedLock = new ManualResetEvent(false);
			var instanceFromOtherThread = default(A);
			var initialThreadId = Thread.CurrentThread.ManagedThreadId;
			Action action = () =>
			{
				using (Container.BeginScope())
				{
					startLock.WaitOne();
					Assert.NotEqual(Thread.CurrentThread.ManagedThreadId, initialThreadId);
					instanceFromOtherThread = Container.Resolve<A>();
					resolvedLock.Set();
				}
			};
			var result = action.BeginInvoke(null, null);
			using (Container.BeginScope())
			{
				startLock.Set();
				var instance = Container.Resolve<A>();
				resolvedLock.WaitOne();

				result.AsyncWaitHandle.WaitOne();
				Assert.NotSame(instance, instanceFromOtherThread);
			}
		}
#endif

	[Fact]
	public async Task Context_is_passed_onto_the_next_thread_TPL()
	{
		using (Container.BeginScope())
		{
			var instance = default(A);
			var instanceFromOtherThread = default(A);
			instance = Container.Resolve<A>();
			var initialThreadId = Thread.CurrentThread.ManagedThreadId;
			var task = Task.Factory.StartNew(() => { instanceFromOtherThread = Container.Resolve<A>(); });
			await task;
			Assert.Same(instance, instanceFromOtherThread);
		}
	}

	[Fact]
	public void Context_is_passed_onto_the_next_thread_ThreadPool()
	{
		using (Container.BeginScope())
		{
			var instance = default(A);
			var @event = new ManualResetEvent(false);
			var instanceFromOtherThread = default(A);
			instance = Container.Resolve<A>();
			var initialThreadId = Thread.CurrentThread.ManagedThreadId;
			var exceptionFromTheOtherThread = default(Exception);
			ThreadPool.QueueUserWorkItem(_ =>
			{
				Assert.NotEqual(Thread.CurrentThread.ManagedThreadId, initialThreadId);
				try
				{
					instanceFromOtherThread = Container.Resolve<A>();
				}
				catch (Exception e)
				{
					exceptionFromTheOtherThread = e;
				}
				finally
				{
					@event.Set();
				}
			});
			var signalled = @event.WaitOne(TimeSpan.FromSeconds(2));
			if (exceptionFromTheOtherThread != null)
			{
				var capture = ExceptionDispatchInfo.Capture(exceptionFromTheOtherThread);
				capture.Throw();
			}

			Assert.True(signalled, "The other thread didn't finish on time.");
			Assert.Same(instance, instanceFromOtherThread);
		}
	}

	[Fact]
	public void Context_is_passed_onto_the_next_thread_explicit()
	{
		using (Container.BeginScope())
		{
			var instance = default(A);
			var @event = new ManualResetEvent(false);
			var instanceFromOtherThread = default(A);
			instance = Container.Resolve<A>();
			var initialThreadId = Thread.CurrentThread.ManagedThreadId;
			var exceptionFromTheOtherThread = default(Exception);
			var otherThread = new Thread(() =>
			{
				Assert.NotEqual(Thread.CurrentThread.ManagedThreadId, initialThreadId);
				try
				{
					instanceFromOtherThread = Container.Resolve<A>();
				}
				catch (Exception e)
				{
					exceptionFromTheOtherThread = e;
				}
				finally
				{
					@event.Set();
				}
			});
			otherThread.Start();
			var signalled = @event.WaitOne(TimeSpan.FromSeconds(2));
			if (exceptionFromTheOtherThread != null)
			{
				var capture = ExceptionDispatchInfo.Capture(exceptionFromTheOtherThread);
				capture.Throw();
			}

			Assert.True(signalled, "The other thread didn't finish on time.");
			Assert.Same(instance, instanceFromOtherThread);
		}
	}
}