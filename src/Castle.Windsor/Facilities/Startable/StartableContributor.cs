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

using System.Reflection;
using Castle.Windsor.Core;
using Castle.Windsor.Core.Internal;
using Castle.Windsor.MicroKernel;
using Castle.Windsor.MicroKernel.ModelBuilder;
using Castle.Windsor.MicroKernel.SubSystems.Conversion;

namespace Castle.Windsor.Facilities.Startable;

public class StartableContributor(ITypeConverter converter) : IContributeComponentModelConstruction
{
    public void ProcessModel(IKernel kernel, ComponentModel model)
    {
        var startable = CheckIfComponentImplementsIStartable(model)
                        || HasStartableAttributeSet(model);

        model.ExtendedProperties["startable"] = startable;

        if (!startable)
        {
            return;
        }

        AddStart(model);
        AddStop(model);
    }

    private static void AddStart(ComponentModel model)
    {
        var startMethod = model.Configuration.Attributes["startMethod"];
        if (startMethod != null)
        {
            var method = model.Implementation.GetTypeInfo().GetMethod(startMethod, Type.EmptyTypes);
            if (method == null)
            {
                throw new ArgumentException(
                    string.Format(
                        "Could not find public parameterless method '{0}' on type {1} designated as start method. Make sure you didn't mistype the method name and that its signature matches.",
                        startMethod, model.Implementation));
            }

            model.ExtendedProperties.Add("Castle.StartableFacility.StartMethod", method);
        }

        model.Lifecycle.Add(StartConcern.Instance);
    }

    private static void AddStop(ComponentModel model)
    {
        var stopMethod = model.Configuration.Attributes["stopMethod"];
        if (stopMethod != null)
        {
            var method = model.Implementation.GetTypeInfo().GetMethod(stopMethod, Type.EmptyTypes);
            if (method == null)
            {
                throw new ArgumentException(
                    string.Format(
                        "Could not find public parameterless method '{0}' on type {1} designated as stop method. Make sure you didn't mistype the method name and that its signature matches.",
                        stopMethod, model.Implementation));
            }

            model.ExtendedProperties.Add("Castle.StartableFacility.StopMethod", method);
        }

        model.Lifecycle.AddFirst(StopConcern.Instance);
    }

    private bool HasStartableAttributeSet(ComponentModel model)
    {
        var startable = model.Configuration?.Attributes["startable"];
        return startable != null && converter.PerformConversion<bool>(startable);
    }

    private static bool CheckIfComponentImplementsIStartable(ComponentModel model)
    {
        return model.Implementation.Is<IStartable>();
    }
}