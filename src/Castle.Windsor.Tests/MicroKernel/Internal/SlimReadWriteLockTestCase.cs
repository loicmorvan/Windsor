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
using Castle.Windsor.MicroKernel.Internal;

namespace Castle.Windsor.Tests.MicroKernel.Internal;

public class SlimReadWriteLockTestCase
{
	private readonly SlimReadWriteLock _lock;

	public SlimReadWriteLockTestCase()
	{
		_lock = new SlimReadWriteLock();
	}

	[Fact]
	public void Can_be_used_ForReading_multiple_nested_time()
	{
		using (_lock.ForReading())
		{
			using (_lock.ForReading())
			{
				Assert.True(_lock.IsReadLockHeld);
			}

			Assert.True(_lock.IsReadLockHeld);
		}
	}

	[Fact]
	public void Can_be_used_ForWriting_multiple_nested_time()
	{
		using (_lock.ForWriting())
		{
			using (_lock.ForWriting())
			{
				Assert.True(_lock.IsWriteLockHeld);
			}

			Assert.True(_lock.IsWriteLockHeld);
		}
	}

	[Fact]
	public void Can_be_used_ForReadingUpgradeable_multiple_nested_time()
	{
		using (_lock.ForReadingUpgradeable())
		{
			using (_lock.ForReadingUpgradeable())
			{
				Assert.True(_lock.IsUpgradeableReadLockHeld);
			}

			Assert.True(_lock.IsUpgradeableReadLockHeld);
		}
	}

	[Fact]
	public void Can_be_upgraded_from_nested_ForReadingUpgradeable()
	{
		using (_lock.ForReadingUpgradeable())
		{
			using (var holder = _lock.ForReadingUpgradeable())
			{
				holder.Upgrade();
				Assert.True(_lock.IsWriteLockHeld);
			}
		}
	}

	[Fact]
	public void Can_be_used_ForReading_when_used_ForWriting()
	{
		using (_lock.ForWriting())
		{
			using (var holder = _lock.ForReading())
			{
				Assert.True(_lock.IsWriteLockHeld);
				Assert.True(holder.LockAcquired);
			}
		}
	}

	[Fact]
	public void Can_be_used_ForReading_when_used_ForReadingUpgradeable()
	{
		using (_lock.ForReadingUpgradeable())
		{
			using (var holder = _lock.ForReading())
			{
				Assert.True(_lock.IsUpgradeableReadLockHeld);
				Assert.True(holder.LockAcquired);
			}
		}
	}

	[Fact]
	public void Can_NOT_be_used_ForReadingUpgradeable_when_used_ForReading()
	{
		using (_lock.ForReading())
		{
			Assert.Throws<LockRecursionException>(() => _lock.ForReadingUpgradeable());
		}
	}

	[Fact]
	public void Can_be_used_ForReadingUpgradeable_when_used_ForWriting()
	{
		using (_lock.ForWriting())
		{
			using (var holder = _lock.ForReadingUpgradeable())
			{
				Assert.True(_lock.IsWriteLockHeld);
				Assert.True(holder.LockAcquired);
			}
		}
	}

	[Fact]
	public void Can_NOT_be_used_ForWriting_when_used_ForReading()
	{
		using (_lock.ForReading())
		{
			Assert.Throws<LockRecursionException>(() => _lock.ForWriting());
		}
	}

	[Fact]
	public void Can_be_used_ForWriting_when_used_ForReadingUpgradeable()
	{
		using (_lock.ForReadingUpgradeable())
		{
			using (var holder = _lock.ForWriting())
			{
				Assert.True(_lock.IsUpgradeableReadLockHeld);
				Assert.True(holder.LockAcquired);
			}

			Assert.True(_lock.IsUpgradeableReadLockHeld);
		}
	}

	[Fact]
	public void Can_be_used_ForWriting_when_used_ForReadingUpgradeable_and_upgraded_after()
	{
		using var upg = _lock.ForReadingUpgradeable();
		using (var holder = _lock.ForWriting())
		{
			Assert.True(_lock.IsUpgradeableReadLockHeld);
			Assert.True(holder.LockAcquired);
			upg.Upgrade();
		}

		Assert.True(_lock.IsUpgradeableReadLockHeld);
	}

	[Fact]
	public void Can_be_used_ForWriting_when_used_ForReadingUpgradeable_and_upgraded_before()
	{
		using var upg = _lock.ForReadingUpgradeable();
		upg.Upgrade();
		using (var holder = _lock.ForWriting())
		{
			Assert.True(_lock.IsUpgradeableReadLockHeld);
			Assert.True(holder.LockAcquired);
		}

		Assert.True(_lock.IsUpgradeableReadLockHeld);
	}
}