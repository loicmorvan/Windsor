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

using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using Castle.Core.Configuration;
using Castle.Core.Internal;
using Microsoft.Extensions.DependencyModel;

namespace Castle.MicroKernel.SubSystems.Conversion;

#if !FEATURE_APPDOMAIN

#endif

/// <summary>
///     Convert a type name to a Type instance.
/// </summary>
[Serializable]
public class TypeNameConverter : AbstractTypeConverter
{
	private static readonly Assembly Mscorlib = typeof(object).GetTypeInfo().Assembly;

	private readonly HashSet<Assembly> _assemblies = [];

	private readonly IDictionary<string, MultiType> _fullName2Type =
		new Dictionary<string, MultiType>(StringComparer.OrdinalIgnoreCase);

	private readonly IDictionary<string, MultiType> _justName2Type =
		new Dictionary<string, MultiType>(StringComparer.OrdinalIgnoreCase);

	private readonly ITypeNameParser _parser;

	public TypeNameConverter(ITypeNameParser parser)
	{
		ArgumentNullException.ThrowIfNull(parser);

		_parser = parser;
	}

	public override bool CanHandleType(Type type)
	{
		return type == typeof(Type);
	}

	public override object PerformConversion(string value, Type targetType)
	{
		try
		{
			return GetType(value);
		}
		catch (ConverterException)
		{
			throw;
		}
		catch (Exception ex)
		{
			throw new ConverterException(string.Format("Could not convert string '{0}' to a type.", value), ex);
		}
	}

	private Type GetType(string name)
	{
		// try to create type using case sensitive search first.
		var type = Type.GetType(name, false, false);
		if (type != null) return type;
		// if case sensitive search did not create the type, try case insensitive.
		type = Type.GetType(name, false, true);
		if (type != null) return type;
		var typeName = ParseName(name);
		if (typeName == null)
			throw new ConverterException(string.Format(
				"Could not convert string '{0}' to a type. It doesn't appear to be a valid type name.", name));

		InitializeAppDomainAssemblies(false);
		type = typeName.GetType(this);
		if (type != null) return type;
		if (InitializeAppDomainAssemblies(true)) type = typeName.GetType(this);
		if (type != null) return type;
		var assemblyName = typeName.ExtractAssemblyName();
		if (assemblyName != null)
		{
			var namePart = assemblyName + ", Version=";
			var assembly =
				_assemblies.FirstOrDefault(a => a.FullName.StartsWith(namePart, StringComparison.OrdinalIgnoreCase));
			if (assembly != null)
				throw new ConverterException(string.Format(
					"Could not convert string '{0}' to a type. Assembly {1} was matched, but it doesn't contain the type. Make sure that the type name was not mistyped.",
					name, assembly.FullName));
			throw new ConverterException(string.Format(
				"Could not convert string '{0}' to a type. Assembly was not found. Make sure it was deployed and the name was not mistyped.",
				name));
		}

		throw new ConverterException(string.Format(
			"Could not convert string '{0}' to a type. Make sure assembly containing the type has been loaded into the process, or consider specifying assembly qualified name of the type.",
			name));
	}

	private bool InitializeAppDomainAssemblies(bool forceLoad)
	{
		var anyAssemblyAdded = false;
#if FEATURE_APPDOMAIN
			if (forceLoad || assemblies.Count == 0)
			{
				var loadedAssemblies = AppDomain.CurrentDomain.GetAssemblies();
				foreach (var assembly in loadedAssemblies)
				{
					if (assemblies.Contains(assembly) || ShouldSkipAssembly(assembly))
					{
						continue;
					}
					anyAssemblyAdded = true;
					assemblies.Add(assembly);
					Scan(assembly);
				}
			}
#else
		if (_assemblies.Count == 0)
		{
			var context = DependencyContext.Default;
			var dependencies = context.RuntimeLibraries
				.SelectMany(library => library.GetDefaultAssemblyNames(context))
				.Distinct();

			foreach (var assemblyName in dependencies)
			{
				if (ShouldSkipAssembly(assemblyName)) continue;

				var assembly = Assembly.Load(assemblyName);

				_assemblies.Add(assembly);
				Scan(assembly);
				anyAssemblyAdded = true;
			}
		}
#endif
		return anyAssemblyAdded;
	}

	protected virtual bool ShouldSkipAssembly(Assembly assembly)
	{
		return assembly == Mscorlib || assembly.FullName.StartsWith("System");
	}

	protected virtual bool ShouldSkipAssembly(AssemblyName assemblyName)
	{
		return assemblyName.FullName.StartsWith("System", StringComparison.Ordinal)
		       || assemblyName.FullName.StartsWith("mscorlib", StringComparison.OrdinalIgnoreCase)
		       || assemblyName.ContentType != AssemblyContentType.Default;
	}

	private void Scan(Assembly assembly)
	{
		if (assembly.IsDynamic) return;
		try
		{
			var exportedTypes = assembly.GetAvailableTypes();
			foreach (var type in exportedTypes)
			{
				Insert(_fullName2Type, type.FullName, type);
				Insert(_justName2Type, type.Name, type);
			}
		}
		catch (NotSupportedException)
		{
			// This might fail in an ASPNET scenario for Desktop CLR
		}
	}

	private void Insert(IDictionary<string, MultiType> collection, string key, Type value)
	{
		MultiType existing;
		if (collection.TryGetValue(key, out existing) == false)
		{
			collection[key] = new MultiType(value);
			return;
		}

		existing.AddInnerType(value);
	}

	private TypeName ParseName(string name)
	{
		return _parser.Parse(name);
	}

	public override object PerformConversion(IConfiguration configuration, Type targetType)
	{
		return PerformConversion(configuration.Value, targetType);
	}

	public Type GetTypeByFullName(string fullName)
	{
		return GetUniqueType(fullName, _fullName2Type, "assembly qualified name");
	}

	public Type GetTypeByName(string justName)
	{
		return GetUniqueType(justName, _justName2Type, "full name, or assembly qualified name");
	}

	private Type GetUniqueType(string name, IDictionary<string, MultiType> map, string description)
	{
		MultiType type;
		if (map.TryGetValue(name, out type))
		{
			EnsureUnique(type, name, description);
			return type.Single();
		}

		return null;
	}

	private void EnsureUnique(MultiType type, string value, string missingInformation)
	{
		if (type.HasOne) return;

		var message = new StringBuilder(string.Format("Could not uniquely identify type for '{0}'. ", value));
		message.AppendLine("The following types were matched:");
		foreach (var matchedType in type) message.AppendLine(matchedType.AssemblyQualifiedName);
		message.AppendFormat("Provide more information ({0}) to uniquely identify the type.", missingInformation);
		throw new ConverterException(message.ToString());
	}

	private class MultiType : IEnumerable<Type>
	{
		private readonly LinkedList<Type> _inner = [];

		public MultiType(Type type)
		{
			_inner.AddFirst(type);
		}

		public bool HasOne => _inner.Count == 1;

		public IEnumerator<Type> GetEnumerator()
		{
			return _inner.GetEnumerator();
		}

		IEnumerator IEnumerable.GetEnumerator()
		{
			return ((IEnumerable)_inner).GetEnumerator();
		}

		public MultiType AddInnerType(Type type)
		{
			_inner.AddLast(type);
			return this;
		}
	}
}