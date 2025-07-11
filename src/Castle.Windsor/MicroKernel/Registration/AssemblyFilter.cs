﻿// Copyright 2004-2012 Castle Project - http://www.castleproject.org/
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
using System.Globalization;
using System.IO;
using System.Reflection;
using Castle.Core.Internal;

namespace Castle.MicroKernel.Registration;

public class AssemblyFilter : IAssemblyProvider
{
	private static readonly Assembly CastleWindsorDll = typeof(AssemblyFilter).GetTypeInfo().Assembly;

	private readonly string _directoryName;
	private readonly string _mask;
	private Predicate<Assembly> _assemblyFilter;
	private Predicate<AssemblyName> _nameFilter;

	public AssemblyFilter(string directoryName, string mask = null)
	{
		ArgumentNullException.ThrowIfNull(directoryName);

		_directoryName = GetFullPath(directoryName);
		_mask = mask;
		_assemblyFilter += a => a != CastleWindsorDll;
	}

	IEnumerable<Assembly> IAssemblyProvider.GetAssemblies()
	{
		foreach (var file in GetFiles())
		{
			if (!ReflectionUtil.IsAssemblyFile(file)) continue;

			var assembly = LoadAssemblyIgnoringErrors(file);
			if (assembly != null) yield return assembly;
		}
	}

	public AssemblyFilter FilterByAssembly(Predicate<Assembly> filter)
	{
		ArgumentNullException.ThrowIfNull(filter);

		_assemblyFilter += filter;
		return this;
	}

	public AssemblyFilter FilterByName(Predicate<AssemblyName> filter)
	{
		ArgumentNullException.ThrowIfNull(filter);

		_nameFilter += filter;
		return this;
	}

	public AssemblyFilter WithKeyToken(string publicKeyToken)
	{
		return WithKeyToken(ExtractKeyToken(publicKeyToken));
	}

	public AssemblyFilter WithKeyToken(byte[] publicKeyToken)
	{
		ArgumentNullException.ThrowIfNull(publicKeyToken);
		return FilterByName(n => IsTokenEqual(n.GetPublicKeyToken(), publicKeyToken));
	}

	public AssemblyFilter WithKeyToken(Type typeFromAssemblySignedWithKey)
	{
		return WithKeyToken(typeFromAssemblySignedWithKey.GetTypeInfo().Assembly);
	}

	public AssemblyFilter WithKeyToken<TTypeFromAssemblySignedWithKey>()
	{
		return WithKeyToken(typeof(TTypeFromAssemblySignedWithKey).GetTypeInfo().Assembly);
	}

	public AssemblyFilter WithKeyToken(Assembly assembly)
	{
		return WithKeyToken(assembly.GetName().GetPublicKeyToken());
	}

	private byte[] ExtractKeyToken(string keyToken)
	{
		ArgumentNullException.ThrowIfNull(keyToken);
		if (keyToken.Length != 16)
			throw new ArgumentException(
				string.Format(
					"The string '{1}' does not appear to be a valid public key token. It should have 16 characters, has {0}.",
					keyToken.Length, keyToken));
		try
		{
			var tokenBytes = new byte[8];
			for (var i = 0; i < 8; i++)
				tokenBytes[i] = byte.Parse(keyToken.Substring(2 * i, 2), NumberStyles.HexNumber);
			return tokenBytes;
		}
		catch (Exception e)
		{
			throw new ArgumentException(
				string.Format(
					"The string '{0}' does not appear to be a valid public key token. It could not be processed.",
					keyToken), e);
		}
	}

	private IEnumerable<string> GetFiles()
	{
		try
		{
			if (Directory.Exists(_directoryName) == false) return [];
			if (string.IsNullOrEmpty(_mask)) return Directory.EnumerateFiles(_directoryName);
			return Directory.EnumerateFiles(_directoryName, _mask);
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
		if (Path.IsPathRooted(path) == false && AppContext.BaseDirectory != null)
			path = Path.Combine(AppContext.BaseDirectory, path);
		return Path.GetFullPath(path);
	}

	private static bool IsTokenEqual(byte[] actualToken, byte[] expectedToken)
	{
		if (actualToken == null) return false;
		if (actualToken.Length != expectedToken.Length) return false;
		for (var i = 0; i < actualToken.Length; i++)
			if (actualToken[i] != expectedToken[i])
				return false;

		return true;
	}
}