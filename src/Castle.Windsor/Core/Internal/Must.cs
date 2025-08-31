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

using System.Collections;
using System.Diagnostics;
using System.Diagnostics.CodeAnalysis;

namespace Castle.Windsor.Core.Internal;

[DebuggerStepThrough]
[DebuggerNonUserCode]
public static class Must
{
    public static T NotBeEmpty<T>(T arg, string name) where T : class, IEnumerable
    {
        NotBeNull(arg, name);

        var enumerator = arg.GetEnumerator();
        var any = enumerator.MoveNext();
        if (enumerator is IDisposable disposable)
        {
            disposable.Dispose();
        }

        return !any ? throw new ArgumentException(name) : arg;
    }

    [return: NotNull]
    public static T NotBeNull<T>(T? arg, string name) where T : class
    {
        return arg ?? throw new ArgumentNullException(name);
    }
}