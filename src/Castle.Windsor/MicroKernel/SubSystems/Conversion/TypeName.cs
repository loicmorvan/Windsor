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

public class TypeName
{
    private readonly string _assemblyQualifiedName;
    private readonly TypeName[] _genericTypes;
    private readonly string _namespace;

    public TypeName(string @namespace, string name, TypeName[] genericTypes)
    {
        Name = name;
        _genericTypes = genericTypes;
        _namespace = @namespace;
    }

    public TypeName(string assemblyQualifiedName)
    {
        _assemblyQualifiedName = assemblyQualifiedName;
    }

    private string FullName
    {
        get
        {
            if (HasNamespace)
            {
                return _namespace + "." + Name;
            }

            throw new InvalidOperationException("Namespace was not defined.");
        }
    }

    private bool HasGenericParameters => _genericTypes.Length > 0;

    private bool HasNamespace => string.IsNullOrEmpty(_namespace) == false;

    private bool IsAssemblyQualified => _assemblyQualifiedName != null;

    private string Name { get; }

    public string ExtractAssemblyName()
    {
        if (IsAssemblyQualified == false)
        {
            return null;
        }

        var tokens = _assemblyQualifiedName.Split([','], StringSplitOptions.None);
        var indexOfVersion = Array.FindLastIndex(tokens, s => s.TrimStart(' ').StartsWith("Version="));
        return indexOfVersion <= 0 ? tokens.Last().Trim() : tokens[indexOfVersion - 1].Trim();
    }

    public Type GetType(TypeNameConverter converter)
    {
        ArgumentNullException.ThrowIfNull(converter);
        if (IsAssemblyQualified)
        {
            return Type.GetType(_assemblyQualifiedName, false, true);
        }

        var type = HasNamespace ? converter.GetTypeByFullName(FullName) : converter.GetTypeByName(Name);

        if (!HasGenericParameters)
        {
            return type;
        }

        var genericArgs = new Type[_genericTypes.Length];
        for (var i = 0; i < genericArgs.Length; i++)
        {
            genericArgs[i] = _genericTypes[i].GetType(converter);
        }

        return type.MakeGenericType(genericArgs);
    }
}