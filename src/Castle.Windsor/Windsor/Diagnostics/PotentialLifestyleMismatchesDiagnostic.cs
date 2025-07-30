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

using Castle.Windsor.Core;
using Castle.Windsor.MicroKernel;

namespace Castle.Windsor.Windsor.Diagnostics;

public class PotentialLifestyleMismatchesDiagnostic(IKernel kernel) : IPotentialLifestyleMismatchesDiagnostic
{
    public IHandler[][] Inspect()
    {
        var allHandlers = kernel.GetAssignableHandlers(typeof(object));
        var handlersByComponentModel = allHandlers.ToDictionary(h => h.ComponentModel);

        var items = new List<MismatchedLifestyleDependency>();
        foreach (var handler in allHandlers)
        {
            items.AddRange(GetMismatches(handler, handlersByComponentModel));
        }

        return items.Select(m => m.GetHandlers()).ToArray();
    }

    private static IEnumerable<MismatchedLifestyleDependency> GetMismatch(MismatchedLifestyleDependency parent,
        ComponentModel component,
        IDictionary<ComponentModel, IHandler> model2Handler)
    {
        if (parent.Checked(component))
        {
            yield break;
        }

        var handler = model2Handler[component];
        var item = new MismatchedLifestyleDependency(handler, parent);
        if (item.Mismatched())
        {
            yield return item;
        }
        else
        {
            foreach (var dependent in handler.ComponentModel.Dependents.Cast<ComponentModel>())
            foreach (var mismatch in GetMismatch(item, dependent, model2Handler))
            {
                yield return mismatch;
            }
        }
    }

    private static IEnumerable<MismatchedLifestyleDependency> GetMismatches(IHandler handler,
        IDictionary<ComponentModel, IHandler> model2Handler)
    {
        if (IsSingleton(handler) == false)
        {
            yield break;
        }

        var root = new MismatchedLifestyleDependency(handler);
        foreach (var dependent in handler.ComponentModel.Dependents.Cast<ComponentModel>())
        {
            foreach (var mismatch in GetMismatch(root, dependent, model2Handler))
            {
                yield return mismatch;
            }
        }
    }

    private static bool IsSingleton(IHandler component)
    {
        var lifestyle = component.ComponentModel.LifestyleType;
        return lifestyle is LifestyleType.Undefined or LifestyleType.Singleton;
    }

    private class MismatchedLifestyleDependency
    {
        private readonly HashSet<ComponentModel> _checkedComponents;

        public MismatchedLifestyleDependency(IHandler handler, MismatchedLifestyleDependency parent = null)
        {
            Handler = handler;
            Parent = parent;

            if (parent == null)
            {
                _checkedComponents = [handler.ComponentModel];
            }
            else
            {
                _checkedComponents = [parent.Handler.ComponentModel];
            }
        }

        // ReSharper disable once MemberCanBePrivate.Local
        public IHandler Handler { get; }

        // ReSharper disable once MemberCanBePrivate.Local
        public MismatchedLifestyleDependency Parent { get; }

        public bool Checked(ComponentModel component)
        {
            return _checkedComponents.Add(component) == false;
        }

        public IHandler[] GetHandlers()
        {
            var handlers = new List<IHandler>();
            BuildHandlersList(handlers);
            return handlers.ToArray();
        }

        public bool Mismatched()
        {
            return Handler.ComponentModel.LifestyleType == LifestyleType.Transient;
        }

        private void BuildHandlersList(List<IHandler> handlers)
        {
            Parent?.BuildHandlersList(handlers);

            handlers.Add(Handler);
        }
    }
}