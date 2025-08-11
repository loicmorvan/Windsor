// Copyright 2018–2020 Castle Project – http://www.castleproject.org/
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

using System.Runtime.ExceptionServices;
using System.Text;
using System.Text.RegularExpressions;
using JetBrains.Annotations;

namespace Castle.Windsor.Tests;

public static partial class TestUtils
{
    public static void AssertNoFirstChanceExceptions([InstantHandle] Action action)
    {
        var firstChanceExceptions = new List<Exception>();

        var handler = new EventHandler<FirstChanceExceptionEventArgs>((_, e) =>
            firstChanceExceptions.Add(e.Exception));

        AppDomain.CurrentDomain.FirstChanceException += handler;
        try
        {
            action.Invoke();
        }
        finally
        {
            AppDomain.CurrentDomain.FirstChanceException -= handler;
        }

        if (firstChanceExceptions.Count == 0)
        {
            return;
        }

        var message = new StringBuilder();
        for (var i = 0; i < firstChanceExceptions.Count; i++)
        {
            message.Append("First-chance exception ").Append(i + 1).Append(" of ")
                .Append(firstChanceExceptions.Count).AppendLine(":");
            message.AppendLine(firstChanceExceptions[i].ToString());
            message.AppendLine();
        }

        message.Append("Expected: no first-chance exceptions.");

        Assert.Fail(message.ToString());
    }

    public static string ConvertToEnvironmentLineEndings(this string value)
    {
        return MyRegex().Replace(value, Environment.NewLine);
    }

    [GeneratedRegex(@"\r?\n")]
    private static partial Regex MyRegex();
}