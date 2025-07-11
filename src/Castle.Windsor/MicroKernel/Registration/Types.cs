// Copyright 2004-2022 Castle Project - http://www.castleproject.org/
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
using System.Reflection;
using System.Runtime.CompilerServices;
using Castle.Core.Internal;

namespace Castle.MicroKernel.Registration;

/// <summary>
///     Entry point to fluent way to register, by convention, multiple types. No upfront filtering is done so literally
///     every type will be considered. That means that usually some filtering done by user
///     will be required. For a most common case where non-abstract classes only are to be considered use
///     <see cref="Classes" /> class instead. Use static methods on the class to fluently build
///     registration.
/// </summary>
public static class Types
{
	/// <summary>Prepares to register types from a list of types.</summary>
	/// <param name="types"> The list of types. </param>
	/// <returns>
	///     The corresponding <see cref="FromDescriptor" />
	/// </returns>
	public static FromTypesDescriptor From(IEnumerable<Type> types)
	{
		return new FromTypesDescriptor(types, null);
	}

	/// <summary>Prepares to register types from a list of types.</summary>
	/// <param name="types"> The list of types. </param>
	/// <returns>
	///     The corresponding <see cref="FromDescriptor" />
	/// </returns>
	public static FromTypesDescriptor From(params Type[] types)
	{
		return new FromTypesDescriptor(types, null);
	}

	/// <summary>Prepares to register types from an assembly.</summary>
	/// <param name="assembly"> The assembly. </param>
	/// <returns>
	///     The corresponding <see cref="FromDescriptor" />
	/// </returns>
	public static FromAssemblyDescriptor FromAssembly(Assembly assembly)
	{
		ArgumentNullException.ThrowIfNull(assembly);
		return new FromAssemblyDescriptor(assembly, null);
	}

	/// <summary>Prepares to register types from an assembly containing the type.</summary>
	/// <param name="type"> The type belonging to the assembly. </param>
	/// <returns>
	///     The corresponding <see cref="FromDescriptor" />
	/// </returns>
	public static FromAssemblyDescriptor FromAssemblyContaining(Type type)
	{
		ArgumentNullException.ThrowIfNull(type);
		return new FromAssemblyDescriptor(type.GetTypeInfo().Assembly, null);
	}

	/// <summary>Prepares to register types from an assembly containing the type.</summary>
	/// <typeparam name="T"> The type belonging to the assembly. </typeparam>
	/// <returns>
	///     The corresponding <see cref="FromDescriptor" />
	/// </returns>
	public static FromAssemblyDescriptor FromAssemblyContaining<T>()
	{
		return FromAssemblyContaining(typeof(T));
	}

	/// <summary>
	///     Prepares to register types from assemblies found in a given directory that meet additional optional
	///     restrictions.
	/// </summary>
	/// <param name="filter"> </param>
	/// <returns> </returns>
	public static FromAssemblyDescriptor FromAssemblyInDirectory(AssemblyFilter filter)
	{
		ArgumentNullException.ThrowIfNull(filter);
		var assemblies = ReflectionUtil.GetAssemblies(filter);
		return new FromAssemblyDescriptor(assemblies, null);
	}

	/// <summary>Scans current assembly and all referenced assemblies with the same first part of the name.</summary>
	/// <returns> </returns>
	/// <remarks>
	///     Assemblies are considered to belong to the same application based on the first part of the name. For example if the
	///     method is called from within <c>MyApp.exe</c> and <c>MyApp.exe</c> references
	///     <c>MyApp.SuperFeatures.dll</c>, <c>mscorlib.dll</c> and <c>ThirdPartyCompany.UberControls.dll</c> the
	///     <c>MyApp.exe</c> and <c>MyApp.SuperFeatures.dll</c> will be scanned for components, and other
	///     assemblies will be ignored.
	/// </remarks>
	[MethodImpl(MethodImplOptions.NoInlining)]
	public static FromAssemblyDescriptor FromAssemblyInThisApplication()
	{
		return FromAssemblyInThisApplication(Assembly.GetCallingAssembly());
	}

	public static FromAssemblyDescriptor FromAssemblyInThisApplication(Assembly rootAssembly)
	{
		var assemblies = new HashSet<Assembly>(ReflectionUtil.GetApplicationAssemblies(rootAssembly));
		return new FromAssemblyDescriptor(assemblies, null);
	}

	/// <summary>Prepares to register types from an assembly.</summary>
	/// <param name="assemblyName"> The assembly name. </param>
	/// <returns>
	///     The corresponding <see cref="FromDescriptor" />
	/// </returns>
	public static FromAssemblyDescriptor FromAssemblyNamed(string assemblyName)
	{
		var assembly = ReflectionUtil.GetAssemblyNamed(assemblyName);
		return FromAssembly(assembly);
	}

	/// <summary>Prepares to register types from the assembly containing the code invoking this method.</summary>
	/// <returns>
	///     The corresponding <see cref="FromDescriptor" />
	/// </returns>
	[MethodImpl(MethodImplOptions.NoInlining)]
	public static FromAssemblyDescriptor FromThisAssembly()
	{
		return FromAssembly(Assembly.GetCallingAssembly());
	}
}