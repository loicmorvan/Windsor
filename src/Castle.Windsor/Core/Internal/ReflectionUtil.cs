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
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;
using System.Runtime.Loader;
using System.Text;

namespace Castle.Core.Internal;

#if !FEATURE_ASSEMBLIES

#endif

public static class ReflectionUtil
{
	public static readonly Type[] OpenGenericArrayInterfaces = typeof(object[]).GetInterfaces()
		.Where(i => i.GetTypeInfo().IsGenericType)
		.Select(i => i.GetGenericTypeDefinition())
		.ToArray();

	private static readonly ConcurrentDictionary<ConstructorInfo, Func<object[], object>> Factories = new();

	public static TBase CreateInstance<TBase>(this Type subtypeofTBase, params object[] ctorArgs)
	{
		EnsureIsAssignable<TBase>(subtypeofTBase);

		return Instantiate<TBase>(subtypeofTBase, ctorArgs ?? []);
	}

	public static IEnumerable<Assembly> GetApplicationAssemblies(Assembly rootAssembly)
	{
		var index = rootAssembly.FullName.IndexOfAny(['.', ',']);
		if (index < 0)
			throw new ArgumentException(
				string.Format(
					"Could not determine application name for assembly \"{0}\". Please use a different method for obtaining assemblies.",
					rootAssembly.FullName));

		var applicationName = rootAssembly.FullName.Substring(0, index);
		var assemblies = new HashSet<Assembly>();
		AddApplicationAssemblies(rootAssembly, assemblies, applicationName);
		return assemblies;
	}

	public static IEnumerable<Assembly> GetAssemblies(IAssemblyProvider assemblyProvider)
	{
		return assemblyProvider.GetAssemblies();
	}

	public static Assembly GetAssemblyNamed(string assemblyName)
	{
		Debug.Assert(string.IsNullOrEmpty(assemblyName) == false);

		try
		{
			Assembly assembly;
#if FEATURE_ASSEMBLIES
				if (IsAssemblyFile(assemblyName))
				{
					if (Path.GetDirectoryName(assemblyName) == AppContext.BaseDirectory)
					{
						assembly = Assembly.Load(Path.GetFileNameWithoutExtension(assemblyName));
					}
					else
					{
						assembly = Assembly.LoadFile(assemblyName);
					}
				}
				else
				{
					assembly = Assembly.Load(assemblyName);
				}
#else
			if (IsAssemblyFile(assemblyName))
				assembly = Assembly.Load(AssemblyLoadContext.GetAssemblyName(assemblyName));
			else
				assembly = Assembly.Load(new AssemblyName(assemblyName));
#endif
			return assembly;
		}
		catch (FileNotFoundException)
		{
			throw;
		}
		catch (FileLoadException)
		{
			throw;
		}
		catch (BadImageFormatException)
		{
			throw;
		}
		catch (Exception e)
		{
			// in theory there should be no other exception kind
			throw new Exception(string.Format("Could not load assembly {0}", assemblyName), e);
		}
	}

	public static Assembly GetAssemblyNamed(string filePath, Predicate<AssemblyName> nameFilter,
		Predicate<Assembly> assemblyFilter)
	{
		var assemblyName = GetAssemblyName(filePath);
		if (nameFilter != null)
			foreach (Predicate<AssemblyName> predicate in nameFilter.GetInvocationList())
				if (predicate(assemblyName) == false)
					return null;

		var assembly = LoadAssembly(assemblyName);
		if (assemblyFilter != null)
			foreach (Predicate<Assembly> predicate in assemblyFilter.GetInvocationList())
				if (predicate(assembly) == false)
					return null;

		return assembly;
	}

	public static Type[] GetAvailableTypes(this Assembly assembly, bool includeNonExported = false)
	{
		try
		{
			if (includeNonExported) return assembly.GetTypes();
			return assembly.GetExportedTypes();
		}
		catch (ReflectionTypeLoadException e)
		{
			return e.Types.Where(t => t != null).ToArray();
			// NOTE: perhaps we should not ignore the exceptions here, and log them?
		}
	}

	public static Type[] GetAvailableTypesOrdered(this Assembly assembly, bool includeNonExported = false)
	{
		return assembly.GetAvailableTypes(includeNonExported).OrderBy(t => t.FullName).ToArray();
	}

	private static Assembly LoadAssembly(AssemblyName assemblyName)
	{
		return Assembly.Load(assemblyName);
	}

	public static TAttribute[] GetAttributes<TAttribute>(this MemberInfo item, bool inherit)
		where TAttribute : Attribute
	{
		return (TAttribute[])item.GetCustomAttributes(typeof(TAttribute), inherit);
	}

	internal static TAttribute[] GetAttributes<TAttribute>(this Type item, bool inherit) where TAttribute : Attribute
	{
		return (TAttribute[])item.GetTypeInfo().GetCustomAttributes(typeof(TAttribute), inherit);
	}

	/// <summary>
	///     If the extended type is a Foo[] or IEnumerable{Foo} which is assignable from Foo[] this method will return
	///     typeof(Foo)
	///     otherwise <c>null</c>.
	/// </summary>
	/// <param name="type"></param>
	/// <returns></returns>
	public static Type GetCompatibleArrayItemType(this Type type)
	{
		if (type == null) return null;
		if (type.IsArray) return type.GetElementType();
		if (type.GetTypeInfo().IsGenericType == false || type.GetTypeInfo().IsGenericTypeDefinition) return null;
		var openGeneric = type.GetGenericTypeDefinition();
		if (OpenGenericArrayInterfaces.Contains(openGeneric)) return type.GetGenericArguments()[0];
		return null;
	}

	public static bool HasDefaultValue(this ParameterInfo item)
	{
		return (item.Attributes & ParameterAttributes.HasDefault) != 0;
	}

	public static bool Is<TType>(this Type type)
	{
		return typeof(TType).IsAssignableFrom(type);
	}

	public static bool IsAssemblyFile(string filePath)
	{
		ArgumentNullException.ThrowIfNull(filePath);

		string extension;
		try
		{
			extension = Path.GetExtension(filePath);
		}
		catch (ArgumentException)
		{
			// path contains invalid characters...
			return false;
		}

		return IsDll(extension) || IsExe(extension);
	}

	private static Func<object[], object> BuildFactory(ConstructorInfo ctor)
	{
		var parameterInfos = ctor.GetParameters();
		var parameterExpressions = new Expression[parameterInfos.Length];
		var argument = Expression.Parameter(typeof(object[]), "parameters");
		for (var i = 0; i < parameterExpressions.Length; i++)
			parameterExpressions[i] = Expression.Convert(
				Expression.ArrayIndex(argument, Expression.Constant(i, typeof(int))),
				parameterInfos[i].ParameterType.IsByRef
					? parameterInfos[i].ParameterType.GetElementType()
					: parameterInfos[i].ParameterType);
		return Expression.Lambda<Func<object[], object>>(
			Expression.New(ctor, parameterExpressions), argument).Compile();
	}

	private static void EnsureIsAssignable<TBase>(Type subtypeofTBase)
	{
		if (subtypeofTBase.Is<TBase>()) return;

		string message;
		if (typeof(TBase).GetTypeInfo().IsInterface)
			message = string.Format("Type {0} does not implement the interface {1}.", subtypeofTBase.FullName,
				typeof(TBase).FullName);
		else
			message = string.Format("Type {0} does not inherit from {1}.", subtypeofTBase.FullName,
				typeof(TBase).FullName);
		throw new InvalidCastException(message);
	}

	private static AssemblyName GetAssemblyName(string filePath)
	{
		AssemblyName assemblyName;
#if FEATURE_ASSEMBLIES
			try
			{
				assemblyName = AssemblyName.GetAssemblyName(filePath);
			}
			catch (ArgumentException)
			{
				assemblyName = new AssemblyName { CodeBase = filePath };
			}
#else
		assemblyName = AssemblyLoadContext.GetAssemblyName(filePath);
#endif
		return assemblyName;
	}

	private static TBase Instantiate<TBase>(Type subtypeofTBase, object[] ctorArgs)
	{
		ctorArgs ??= [];
		var types = ctorArgs.ConvertAll(a => a == null ? typeof(object) : a.GetType());
		var constructor = subtypeofTBase.GetConstructor(BindingFlags.Instance | BindingFlags.Public, null, types, null);
		if (constructor != null) return (TBase)Instantiate(constructor, ctorArgs);
		try
		{
			return (TBase)Activator.CreateInstance(subtypeofTBase, ctorArgs);
		}
		catch (MissingMethodException ex)
		{
			string message;
			if (ctorArgs.Length == 0)
			{
				message = string.Format(
					"Type {0} does not have a public default constructor and could not be instantiated.",
					subtypeofTBase.FullName);
			}
			else
			{
				var messageBuilder = new StringBuilder();
				messageBuilder.AppendLine(
					string.Format(
						"Type {0} does not have a public constructor matching arguments of the following types:",
						subtypeofTBase.FullName));
				foreach (var type in ctorArgs.Select(o => o.GetType())) messageBuilder.AppendLine(type.FullName);
				message = messageBuilder.ToString();
			}

			throw new ArgumentException(message, ex);
		}
		catch (Exception ex)
		{
			var message = string.Format("Could not instantiate {0}.", subtypeofTBase.FullName);
			throw new Exception(message, ex);
		}
	}

	public static object Instantiate(this ConstructorInfo ctor, object[] ctorArgs)
	{
		Func<object[], object> factory;
		factory = Factories.GetOrAdd(ctor, BuildFactory);

		return factory.Invoke(ctorArgs);
	}

	private static bool IsDll(string extension)
	{
		return ".dll".Equals(extension, StringComparison.OrdinalIgnoreCase);
	}

	private static bool IsExe(string extension)
	{
		return ".exe".Equals(extension, StringComparison.OrdinalIgnoreCase);
	}

	private static void AddApplicationAssemblies(Assembly assembly, HashSet<Assembly> assemblies,
		string applicationName)
	{
		if (assemblies.Add(assembly) == false) return;
		foreach (var referencedAssembly in assembly.GetReferencedAssemblies())
			if (IsApplicationAssembly(applicationName, referencedAssembly.FullName))
				AddApplicationAssemblies(LoadAssembly(referencedAssembly), assemblies, applicationName);
	}

	private static bool IsApplicationAssembly(string applicationName, string assemblyName)
	{
		return assemblyName.StartsWith(applicationName);
	}
}