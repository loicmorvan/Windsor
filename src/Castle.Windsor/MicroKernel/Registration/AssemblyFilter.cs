// Copyright 2004-2012 Castle Project - http://www.castleproject.org/
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

using System.Globalization;
using System.Reflection;
using Castle.Windsor.Core.Internal;
using JetBrains.Annotations;

namespace Castle.Windsor.MicroKernel.Registration;

public class AssemblyFilter : IAssemblyProvider
{
    private static readonly Assembly CastleWindsorDll = typeof(AssemblyFilter).GetTypeInfo().Assembly;

    private readonly string _directoryName;
    private readonly string? _mask;
    private Predicate<Assembly> _assemblyFilter;
    private Predicate<AssemblyName>? _nameFilter;

    public AssemblyFilter(string directoryName, string? mask = null)
    {
        ArgumentNullException.ThrowIfNull(directoryName);

        _directoryName = GetFullPath(directoryName);
        _mask = mask;
        _assemblyFilter += a => a != CastleWindsorDll;
    }

    IEnumerable<Assembly> IAssemblyProvider.GetAssemblies()
    {
        return from file in GetFiles()
            where ReflectionUtil.IsAssemblyFile(file)
            select LoadAssemblyIgnoringErrors(file)
            into assembly
            where assembly != null
            select assembly;
    }

    [PublicAPI]
    public AssemblyFilter FilterByAssembly(Predicate<Assembly> filter)
    {
        ArgumentNullException.ThrowIfNull(filter);

        _assemblyFilter += filter;
        return this;
    }

    private AssemblyFilter FilterByName(Predicate<AssemblyName> filter)
    {
        ArgumentNullException.ThrowIfNull(filter);

        _nameFilter += filter;
        return this;
    }

    public void WithKeyToken(string publicKeyToken)
    {
        WithKeyToken(ExtractKeyToken(publicKeyToken));
    }

    private AssemblyFilter WithKeyToken(byte[] publicKeyToken)
    {
        ArgumentNullException.ThrowIfNull(publicKeyToken);
        return FilterByName(n => IsTokenEqual(n.GetPublicKeyToken(), publicKeyToken));
    }

    [PublicAPI]
    public AssemblyFilter WithKeyToken(Type typeFromAssemblySignedWithKey)
    {
        return WithKeyToken(typeFromAssemblySignedWithKey.GetTypeInfo().Assembly);
    }

    [PublicAPI]
    public AssemblyFilter WithKeyToken<TTypeFromAssemblySignedWithKey>()
    {
        return WithKeyToken(typeof(TTypeFromAssemblySignedWithKey).GetTypeInfo().Assembly);
    }

    private AssemblyFilter WithKeyToken(Assembly assembly)
    {
        return WithKeyToken(assembly.GetName().GetPublicKeyToken());
    }

    private static byte[] ExtractKeyToken(string keyToken)
    {
        ArgumentNullException.ThrowIfNull(keyToken);
        if (keyToken.Length != 16)
        {
            throw new ArgumentException(
                string.Format(
                    "The string '{1}' does not appear to be a valid public key token. It should have 16 characters, has {0}.",
                    keyToken.Length, keyToken));
        }

        try
        {
            var tokenBytes = new byte[8];
            for (var i = 0; i < 8; i++)
            {
                tokenBytes[i] = byte.Parse(keyToken.Substring(2 * i, 2), NumberStyles.HexNumber);
            }

            return tokenBytes;
        }
        catch (Exception e)
        {
            throw new ArgumentException(
                $"The string '{keyToken}' does not appear to be a valid public key token. It could not be processed.",
                e);
        }
    }

    private IEnumerable<string> GetFiles()
    {
        try
        {
            if (!Directory.Exists(_directoryName))
            {
                return [];
            }

            return string.IsNullOrEmpty(_mask)
                ? Directory.EnumerateFiles(_directoryName)
                : Directory.EnumerateFiles(_directoryName, _mask);
        }
        catch (IOException e)
        {
            throw new ArgumentException("Could not resolve assemblies.", e);
        }
    }

    private Assembly LoadAssemblyIgnoringErrors(string file)
    {
        // based on MEF DirectoryCatalog
        try
        {
            return ReflectionUtil.GetAssemblyNamed(file, _nameFilter, _assemblyFilter);
        }
        catch (FileNotFoundException)
        {
        }
        catch (FileLoadException)
        {
            // File was found but could not be loaded
        }
        catch (BadImageFormatException)
        {
            // Dlls that contain native code or assemblies for wrong runtime (like .NET 4 asembly when we're in CLR2 process)
        }
        catch (ReflectionTypeLoadException)
        {
            // Dlls that have missing Managed dependencies are not loaded, but do not invalidate the Directory 
        }

        // TODO: log
        return null;
    }

    private static string GetFullPath(string path)
    {
        ArgumentNullException.ThrowIfNull(path);

        if (Path.IsPathRooted(path))
        {
            return Path.GetFullPath(path);
        }

        path = Path.Combine(AppContext.BaseDirectory, path);

        return Path.GetFullPath(path);
    }

    private static bool IsTokenEqual(byte[]? actualToken, byte[] expectedToken)
    {
        if (actualToken == null)
        {
            return false;
        }

        if (actualToken.Length != expectedToken.Length)
        {
            return false;
        }

        return !actualToken.Where((t, i) => t != expectedToken[i]).Any();
    }
}