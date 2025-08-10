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
using Castle.Windsor.MicroKernel.Proxy;
using Castle.Windsor.MicroKernel.Util;

namespace Castle.Windsor.MicroKernel.ModelBuilder.Inspectors;

[Serializable]
public class MixinInspector : IContributeComponentModelConstruction
{
    public void ProcessModel(IKernel kernel, ComponentModel model)
    {
        var mixins = model.Configuration?.Children["mixins"];
        if (mixins == null)
        {
            return;
        }

        var mixinReferences = new List<ComponentReference<object>>();
        foreach (var mixin in mixins.Children)
        {
            var value = mixin.Value;

            var mixinComponent = ReferenceExpressionUtil.ExtractComponentName(value);
            if (mixinComponent == null)
            {
                throw new Exception(
                    $"The value for the mixin must be a reference to a component (Currently {value})");
            }

            mixinReferences.Add(new ComponentReference<object>(mixinComponent));
        }

        if (mixinReferences.Count == 0)
        {
            return;
        }

        var options = model.ObtainProxyOptions();
        mixinReferences.ForEach(options.AddMixinReference);
    }
}