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
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using Castle.Core;
using Castle.Core.Configuration;
using Castle.MicroKernel.SubSystems.Conversion;

namespace Castle.MicroKernel.ModelBuilder.Inspectors;

/// <summary>
///     This implementation of <see cref="IContributeComponentModelConstruction" />
///     collects all potential writable public properties exposed by the component
///     implementation and populates the model with them.
///     The Kernel might be able to set some of these properties when the component
///     is requested.
/// </summary>
[Serializable]
public class PropertiesDependenciesModelInspector(IConversionManager converter) : IContributeComponentModelConstruction
{
	[NonSerialized] private readonly IConversionManager _converter = converter;

	/// <summary>
	///     Adds the properties as optional dependencies of this component.
	/// </summary>
	/// <param name="kernel"></param>
	/// <param name="model"></param>
	public virtual void ProcessModel(IKernel kernel, ComponentModel model)
	{
		InspectProperties(model);
	}

	protected virtual void InspectProperties(ComponentModel model)
	{
		var targetType = model.Implementation;

		if (model.InspectionBehavior == PropertiesInspectionBehavior.Undefined)
			model.InspectionBehavior = GetInspectionBehaviorFromTheConfiguration(model.Configuration);

		if (model.InspectionBehavior == PropertiesInspectionBehavior.None)
			// Nothing to be inspected
			return;

		var properties = GetProperties(model, targetType);
		if (properties.Count == 0) return;
		var filters = StandardPropertyFilters.GetPropertyFilters(model, false);
		if (filters == null)
			properties.ForEach(p => model.AddProperty(BuildDependency(p, true)));
		else
			foreach (var filter in filters.Concat([StandardPropertyFilters.Create(PropertyFilter.Default)]))
			{
				var dependencies = filter.Invoke(model, properties, BuildDependency);
				if (dependencies != null)
					foreach (var dependency in dependencies)
						model.AddProperty(dependency);

				if (properties.Count == 0) return;
			}
	}

	private PropertySet BuildDependency(PropertyInfo property, bool isOptional)
	{
		var dependency = new PropertyDependencyModel(property, isOptional);
		return new PropertySet(property, dependency);
	}

	private PropertiesInspectionBehavior GetInspectionBehaviorFromTheConfiguration(IConfiguration config)
	{
		if (config == null || config.Attributes["inspectionBehavior"] == null)
			// return default behavior
			return PropertiesInspectionBehavior.All;

		var enumStringVal = config.Attributes["inspectionBehavior"];

		try
		{
			return _converter.PerformConversion<PropertiesInspectionBehavior>(enumStringVal);
		}
		catch (Exception)
		{
			var message =
				string.Format(
					"Error on properties inspection. Could not convert the inspectionBehavior attribute value into an expected enum value. " +
					"Value found is '{0}' while possible values are '{1}'",
					enumStringVal,
					string.Join(", ", Enum.GetNames(typeof(PropertiesInspectionBehavior))));

			throw new ConverterException(message);
		}
	}

	private List<PropertyInfo> GetProperties(ComponentModel model, Type targetType)
	{
		BindingFlags bindingFlags;
		if (model.InspectionBehavior == PropertiesInspectionBehavior.DeclaredOnly)
			bindingFlags = BindingFlags.Public | BindingFlags.Instance | BindingFlags.DeclaredOnly;
		else // if (model.InspectionBehavior == PropertiesInspectionBehavior.All) or Undefined
			bindingFlags = BindingFlags.Public | BindingFlags.Instance;

		var properties = targetType.GetProperties(bindingFlags);
		return properties.Where(IsValidPropertyDependency).ToList();
	}

	private static bool HasDoNotWireAttribute(PropertyInfo property)
	{
		return property.IsDefined(typeof(DoNotWireAttribute));
	}

	private static bool HasParameters(PropertyInfo property)
	{
		var indexerParams = property.GetIndexParameters();
		return indexerParams != null && indexerParams.Length != 0;
	}

	private static bool IsSettable(PropertyInfo property)
	{
		return property.CanWrite && property.GetSetMethod() != null;
	}

	private static bool IsValidPropertyDependency(PropertyInfo property)
	{
		return IsSettable(property) && HasParameters(property) == false && HasDoNotWireAttribute(property) == false;
	}
}