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

using System.Text;
using Castle.Windsor.Core;
using Castle.Windsor.Core.Internal;
using Castle.Windsor.MicroKernel;

namespace Castle.Windsor.Windsor.Diagnostics;

public class DuplicatedDependenciesDiagnostic(IKernel kernel) : IDuplicatedDependenciesDiagnostic
{
    public Tuple<IHandler, DependencyDuplicate[]>[] Inspect()
    {
        var allHandlers = kernel.GetAssignableHandlers(typeof(object));

        return (
            from handler in allHandlers
            let duplicateDependencies = FindDuplicateDependenciesFor(handler)
            where duplicateDependencies.Length > 0
            select new Tuple<IHandler, DependencyDuplicate[]>(handler, duplicateDependencies)).ToArray();
    }

    public static string GetDetails(DependencyDuplicate duplicates)
    {
        var details = new StringBuilder();
        Describe(details, duplicates.Dependency1);
        details.Append(" duplicates ");
        Describe(details, duplicates.Dependency2);
        // ReSharper disable once SwitchStatementMissingSomeEnumCasesNoDefault
        switch (duplicates.Reason)
        {
            case DependencyDuplicationReason.Name:
                details.Append(", they both have the same name.");
                break;
            case DependencyDuplicationReason.Type:
                details.Append(", they both have the same type.");
                break;
            case DependencyDuplicationReason.NameAndType:
                details.Append(", they both have the same name and type.");
                break;
            case DependencyDuplicationReason.Reference:
                details.Append(", they both reference the same component " +
                               duplicates.Dependency1.ReferencedComponentName);
                break;
        }

        return details.ToString();
    }

    private static void CollectDuplicatesBetween(IReadOnlyList<DependencyModel> array,
        ICollection<DependencyDuplicate> duplicates)
    {
        for (var i = 0; i < array.Count; i++)
        for (var j = i + 1; j < array.Count; j++)
        {
            var reason = IsDuplicate(array[i], array[j]);
            if (reason != DependencyDuplicationReason.Unspecified)
            {
                duplicates.Add(new DependencyDuplicate(array[i], array[j], reason));
            }
        }
    }

    private static void CollectDuplicatesBetweenConstructorParameters(ConstructorCandidateCollection constructors,
        ICollection<DependencyDuplicate> duplicates)
    {
        foreach (var constructor in constructors)
        {
            CollectDuplicatesBetween(constructor.Dependencies, duplicates);
        }
    }

    private static void CollectDuplicatesBetweenProperties(DependencyModel[] properties,
        ICollection<DependencyDuplicate> duplicates)
    {
        CollectDuplicatesBetween(properties, duplicates);
    }

    private static void CollectDuplicatesBetweenPropertiesAndConstructors(ConstructorCandidateCollection constructors,
        DependencyModel[] properties, ICollection<DependencyDuplicate> duplicates)
    {
        foreach (var constructor in constructors)
        foreach (var dependency in constructor.Dependencies)
        foreach (var property in properties)
        {
            var reason = IsDuplicate(property, dependency);
            if (reason != DependencyDuplicationReason.Unspecified)
            {
                duplicates.Add(new DependencyDuplicate(property, dependency, reason));
            }
        }
    }

    private static DependencyDuplicate[] FindDuplicateDependenciesFor(IHandler handler)
    {
        // TODO: handler non-default activators
        // NOTE: how exactly? We don't have enough context to know, other than via the well known activators that we ship with
        //		 but we can only inspect the type here...
        var duplicates = new HashSet<DependencyDuplicate>();
        var properties = handler.ComponentModel.Properties
            .Select(p => p.Dependency)
            .OrderBy(d => d.ToString())
            .ToArray();
        var constructors = handler.ComponentModel.Constructors;
        CollectDuplicatesBetweenProperties(properties, duplicates);
        CollectDuplicatesBetweenConstructorParameters(constructors, duplicates);
        CollectDuplicatesBetweenPropertiesAndConstructors(constructors, properties, duplicates);
        return duplicates.ToArray();
    }

    private static DependencyDuplicationReason IsDuplicate(DependencyModel foo, DependencyModel bar)
    {
        if (foo.ReferencedComponentName != null || bar.ReferencedComponentName != null)
        {
            if (string.Equals(foo.ReferencedComponentName, bar.ReferencedComponentName,
                    StringComparison.OrdinalIgnoreCase))
            {
                return DependencyDuplicationReason.Reference;
            }
        }

        if (string.Equals(foo.DependencyKey, bar.DependencyKey, StringComparison.OrdinalIgnoreCase))
        {
            return foo.TargetItemType == bar.TargetItemType
                ? DependencyDuplicationReason.NameAndType
                : DependencyDuplicationReason.Name;
        }

        return foo.TargetItemType == bar.TargetItemType
            ? DependencyDuplicationReason.Type
            : 0;
    }

    private static void Describe(StringBuilder details, DependencyModel dependency)
    {
        switch (dependency)
        {
            case PropertyDependencyModel:
                details.Append("Property ");
                break;
            case ConstructorDependencyModel:
                details.Append("Constructor parameter ");
                break;
            default:
                details.Append("Dependency ");
                break;
        }

        details.Append(dependency.TargetItemType.ToCSharpString() + " " + dependency.DependencyKey);
    }
}