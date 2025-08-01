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

using System.ComponentModel;
using Castle.Windsor.Core;

namespace Castle.Windsor.MicroKernel.ModelBuilder;

[EditorBrowsable(EditorBrowsableState.Never)]
public static class ComponentModelDescriptorUtil
{
    public static readonly string MetaDescriptorsKey = "Castle.meta-descriptors";

    public static ICollection<IMetaComponentModelDescriptor> GetMetaDescriptors(this ComponentModel model,
        bool ensureExists)
    {
        ArgumentNullException.ThrowIfNull(model);

        var metaDescriptors =
            model.ExtendedProperties[MetaDescriptorsKey] as ICollection<IMetaComponentModelDescriptor>;
        if (metaDescriptors != null || !ensureExists)
        {
            return metaDescriptors;
        }

        metaDescriptors = new List<IMetaComponentModelDescriptor>();
        model.ExtendedProperties[MetaDescriptorsKey] = metaDescriptors;

        return metaDescriptors;
    }

    public static void RemoveMetaDescriptors(ComponentModel model)
    {
        model.ExtendedProperties.Remove(MetaDescriptorsKey);
    }
}