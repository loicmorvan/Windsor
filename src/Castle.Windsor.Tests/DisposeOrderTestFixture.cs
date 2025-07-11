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

using System;
using System.Collections.Generic;
using Castle.Core;

namespace Castle.Windsor.Tests;

public class DisposeOrderTestFixture
{
	[Fact]
	public void Dictionary_enumerates_from_oldest_to_latest()
	{
		var expected1 = new[] { 1, 2, 4, 3 };

		var dictionary1 = new Dictionary<int, object>();
		foreach (var key in expected1) dictionary1[key] = new object();
		var index = 0;
		foreach (var keyValuePair in dictionary1)
		{
			Assert.Equal(expected1[index], keyValuePair.Key);
			index++;
		}

		var expected2 = new[] { 4, 1, 2, 3 };

		var dictionary2 = new Dictionary<int, object>();
		foreach (var key in expected2) dictionary2[key] = new object();

		index = 0;
		foreach (var keyValuePair in dictionary2)
		{
			Assert.Equal(expected2[index], keyValuePair.Key);
			index++;
		}
	}

	private interface IMyComponent : IInitializable, IDisposable
	{
		bool IsInitialized { get; }
	}

	private interface IMyService : IInitializable, IDisposable
	{
		bool IsInUse { get; set; }
		bool IsInitialized { get; }
	}

	private class MyComponent(IMyService service) : IMyComponent
	{
		public bool IsInitialized { get; private set; }

		public void Dispose()
		{
			IsInitialized = false;
			service.IsInUse = false;
		}

		public void Initialize()
		{
			service.IsInUse = true;
			IsInitialized = true;
		}
	}

	private class MyService : IMyService
	{
		private bool _inUse;

		public bool IsInUse
		{
			get
			{
				if (IsInitialized == false) throw new Exception("Service must be initialized !!!");
				return _inUse;
			}
			set
			{
				if (IsInitialized == false) throw new Exception("Service must be initialized !!!");
				_inUse = value;
			}
		}

		public bool IsInitialized { get; private set; }

		public void Dispose()
		{
			if (IsInUse) throw new Exception("Cannot dispose : service is still in use !!!");
			IsInitialized = false;
		}

		public void Initialize()
		{
			IsInitialized = true;
		}
	}
}