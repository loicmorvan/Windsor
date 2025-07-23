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

using Castle.Windsor.Core;

// ReSharper disable UnusedParameter.Local

namespace Castle.Windsor.Tests.Facilities.Startable.Components;

public class StartableChainParent : IStartable
{
	public static int Createcount;
	public static int Startcount;

	public StartableChainParent(StartableChainDependency item1, StartableChainGeneric<string> item2)
	{
		++Createcount;
	}

	public void Start()
	{
		++Startcount;
	}

	public void Stop()
	{
	}
}

public class StartableChainDependency : IStartable
{
	public static int Createcount;
	public static int Startcount;

	public StartableChainDependency(StartableChainGeneric<string> item)
	{
		++Createcount;
	}

	public void Start()
	{
		++Startcount;
	}

	public void Stop()
	{
	}
}

// ReSharper disable once UnusedTypeParameter
public class StartableChainGeneric<T>
{
	public static int Createcount;

	public StartableChainGeneric()
	{
		++Createcount;
	}
}