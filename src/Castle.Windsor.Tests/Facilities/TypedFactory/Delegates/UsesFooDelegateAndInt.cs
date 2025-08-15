﻿// Copyright 2004-2010 Castle Project - http://www.castleproject.org/
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

using JetBrains.Annotations;

namespace Castle.Windsor.Tests.Facilities.TypedFactory.Delegates;

#pragma warning disable CS9113 // Parameter is unread.
public class UsesFooDelegateAndInt(Func<int, Foo> myFooFactory, int additionalArgument)
#pragma warning restore CS9113 // Parameter is unread.
{
    private int _counter;

    [PublicAPI]
    public Foo GetFoo()
    {
        return myFooFactory(++_counter);
    }
}