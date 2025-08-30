// Copyright 2004-2025 Castle Project - http://www.castleproject.org/
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

using System.Runtime.CompilerServices;

namespace Castle.Windsor.Tests.Facilities.TypedFactory;

public class DataRepository
{
    private readonly Dictionary<string, dynamic> _countersByKeys = new();

    public dynamic this[string key] => _countersByKeys.TryGetValue(key, out var value) ? value : 0;

    public void RegisterCallerMemberName([CallerMemberName] string? key = null)
    {
        if (key == null)
        {
            return;
        }

        _countersByKeys[key] = _countersByKeys.TryGetValue(key, out var value) ? value + 1 : 1;
    }

    public void RegisterValue(string key, dynamic value)
    {
        _countersByKeys[key] = value;
    }
}