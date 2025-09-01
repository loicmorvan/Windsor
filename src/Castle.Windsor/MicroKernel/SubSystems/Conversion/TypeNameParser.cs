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

namespace Castle.Windsor.MicroKernel.SubSystems.Conversion;

public class TypeNameParser : ITypeNameParser
{
    public TypeName? Parse(string name)
    {
        var isPotentiallyFullyQualifiedName = name.Contains(',');
        var genericIndex = name.IndexOf('`');
        var genericTypes = Array.Empty<TypeName>();
        if (genericIndex <= -1)
        {
            return isPotentiallyFullyQualifiedName
                ?
                //well at this point it either is a fully qualified name, or invalid string
                new TypeName(name)
                :
                // at this point we assume we have just the type name, probably prefixed with namespace so let's see which one is it
                BuildName(name, genericTypes);
        }

        var start = name.IndexOf("[[", genericIndex, StringComparison.Ordinal);
        if (start == -1)
        {
            return isPotentiallyFullyQualifiedName
                ?
                //well at this point it either is a fully qualified name, or invalid string
                new TypeName(name)
                :
                // at this point we assume we have just the type name, probably prefixed with namespace so let's see which one is it
                BuildName(name, genericTypes);
        }

        var countString = name.Substring(genericIndex + 1, start - genericIndex - 1);
        if (!int.TryParse(countString, out var count))
        {
            return null;
        }

        genericTypes =
            ParseNames(name.Substring(start + 2, name.LastIndexOf("]]", StringComparison.Ordinal) - 2 - start),
                count);

        name = name[..start];

        return BuildName(name, genericTypes);
    }

    private static TypeName BuildName(string name, TypeName[] genericTypes)
    {
        var typeStartsHere = name.LastIndexOf('.');
        string typeName;
        string? @namespace = null;

        if (typeStartsHere > -1 && typeStartsHere < name.Length - 1)
        {
            typeName = name[(typeStartsHere + 1)..];
            @namespace = name[..typeStartsHere];
        }
        else
        {
            typeName = name;
        }

        return new TypeName(@namespace, typeName, genericTypes);
    }

    private static int MoveToBeginning(int location, string text)
    {
        var currentLocation = location;
        while (currentLocation < text.Length)
        {
            if (text[currentLocation] == '[')
            {
                return currentLocation;
            }

            currentLocation++;
        }

        return currentLocation;
    }

    private static int MoveToEnd(int location, string text)
    {
        var open = 1;
        var currentLocation = location;
        while (currentLocation < text.Length)
        {
            var current = text[currentLocation];
            switch (current)
            {
                case '[':
                    open++;
                    break;
                case ']':
                {
                    open--;
                    if (open == 0)
                    {
                        return currentLocation;
                    }

                    break;
                }
            }

            currentLocation++;
        }

        return currentLocation;
    }

    private TypeName[] ParseNames(string substring, int count)
    {
        if (count == 1)
        {
            var name = Parse(substring);
            if (name == null)
            {
                return [];
            }

            return [name];
        }

        var names = new TypeName[count];

        var location = 0;
        for (var i = 0; i < count; i++)
        {
            var newLocation = MoveToEnd(location, substring);
            names[i] = Parse(substring.Substring(location, newLocation - location)) ??
                       throw new InvalidOperationException();
            location = MoveToBeginning(newLocation, substring) + 1;
        }

        return names;
    }
}