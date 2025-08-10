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

using System.ComponentModel;

namespace Castle.Windsor.MicroKernel.Internal;

[EditorBrowsable(EditorBrowsableState.Never)]
internal class SlimReadLockHolder : ILockHolder
{
    private readonly ReaderWriterLockSlim _locker;

    public SlimReadLockHolder(ReaderWriterLockSlim locker, bool waitForLock)
    {
        _locker = locker;
        if (waitForLock)
        {
            locker.EnterReadLock();
            LockAcquired = true;
            return;
        }

        LockAcquired = locker.TryEnterReadLock(0);
    }

    public void Dispose()
    {
        if (!LockAcquired)
        {
            return;
        }

        _locker.ExitReadLock();
        LockAcquired = false;
    }

    public bool LockAcquired { get; private set; }
}