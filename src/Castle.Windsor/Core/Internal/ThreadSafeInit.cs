﻿// Copyright 2004-2011 Castle Project - http://www.castleproject.org/
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

namespace Castle.Core.Internal;

public sealed class ThreadSafeInit
{
	// We are free to use negative values here, as ManagedTreadId is guaranteed to be always > 0
	// http://www.netframeworkdev.com/net-base-class-library/threadmanagedthreadid-18626.shtml
	// the ids can be recycled as mentioned, but that does not affect us since at any given point in time
	// there can be no two threads with the same managed id, and that's all we care about
	private const int Initialized = int.MinValue + 1;
	private const int NotInitialized = int.MinValue;
	private int _state = NotInitialized;

	public void EndThreadSafeOnceSection()
	{
		if (_state == Initialized) return;
		if (_state == Thread.CurrentThread.ManagedThreadId) _state = Initialized;
	}

	public bool ExecuteThreadSafeOnce()
	{
		if (_state == Initialized) return false;
		var inProgressByThisThread = Thread.CurrentThread.ManagedThreadId;
		var preexistingState = Interlocked.CompareExchange(ref _state, inProgressByThisThread, NotInitialized);
		if (preexistingState == NotInitialized) return true;
		if (preexistingState == Initialized || preexistingState == inProgressByThisThread) return false;
		var spinWait = new SpinWait();
		while (_state != Initialized) spinWait.SpinOnce();

		return false;
	}
}