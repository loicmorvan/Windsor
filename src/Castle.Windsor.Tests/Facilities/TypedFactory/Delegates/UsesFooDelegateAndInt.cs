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

using System;

namespace Castle.Windsor.Tests.Facilities.TypedFactory.Delegates;

public class UsesFooDelegateAndInt
{
	private readonly Func<int, Foo> _myFooFactory;
	private int _counter;

	public UsesFooDelegateAndInt(Func<int, Foo> myFooFactory, int additionalArgument)
	{
		AdditionalArgument = additionalArgument;
		_myFooFactory = myFooFactory;
		_counter = 0;
	}

	public int AdditionalArgument { get; set; }

	public Foo GetFoo()
	{
		return _myFooFactory(++_counter);
	}
}