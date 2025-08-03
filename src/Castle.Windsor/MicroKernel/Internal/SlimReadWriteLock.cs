// Copyright 2004-2009 Castle Project - http://www.castleproject.org/
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

namespace Castle.Windsor.MicroKernel.Internal;

public sealed class SlimReadWriteLock : Lock
{
    private readonly ReaderWriterLockSlim _locker = new(LockRecursionPolicy.NoRecursion);

    public bool IsReadLockHeld => _locker.IsReadLockHeld;

    public bool IsUpgradeableReadLockHeld => _locker.IsUpgradeableReadLockHeld;

    public bool IsWriteLockHeld => _locker.IsWriteLockHeld;

    public override IUpgradeableLockHolder ForReadingUpgradeable()
    {
        return ForReadingUpgradeable(true);
    }

    public override ILockHolder ForReading()
    {
        return ForReading(true);
    }

    public override ILockHolder ForWriting()
    {
        return ForWriting(true);
    }

    private IUpgradeableLockHolder ForReadingUpgradeable(bool waitForLock)
    {
        return new SlimUpgradeableReadLockHolder(_locker, waitForLock,
            _locker.IsUpgradeableReadLockHeld || _locker.IsWriteLockHeld);
    }

    public override ILockHolder ForReading(bool waitForLock)
    {
        if (_locker.IsReadLockHeld || _locker.IsUpgradeableReadLockHeld || _locker.IsWriteLockHeld)
        {
            return NoOpLock.Lock;
        }

        return new SlimReadLockHolder(_locker, waitForLock);
    }

    private ILockHolder ForWriting(bool waitForLock)
    {
        return _locker.IsWriteLockHeld ? NoOpLock.Lock : new SlimWriteLockHolder(_locker, waitForLock);
    }
}