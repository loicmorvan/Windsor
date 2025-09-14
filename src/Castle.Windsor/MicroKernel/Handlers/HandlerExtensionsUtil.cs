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

namespace Castle.Windsor.MicroKernel.Handlers;

public static class HandlerExtensionsUtil
{
    private const string ReleaseExtensionsKey = "Castle.ReleaseExtensions";
    public const string ResolveExtensionsKey = "Castle.ResolveExtensions";

    public static ICollection<IReleaseExtension>? GetReleaseExtensions(this ComponentModel model)
    {
        ArgumentNullException.ThrowIfNull(model);

        return model.ExtendedProperties[ReleaseExtensionsKey] as ICollection<IReleaseExtension>;
    }

    public static ICollection<IResolveExtension> GetOrCreateResolveExtensions(this ComponentModel model)
    {
        ArgumentNullException.ThrowIfNull(model);

        var resolveExtensions = model.GetResolveExtensions();
        if (resolveExtensions != null)
        {
            return resolveExtensions;
        }

        resolveExtensions = new HashSet<IResolveExtension>();
        model.ExtendedProperties[ResolveExtensionsKey] = resolveExtensions;

        return resolveExtensions;
    }

    public static ICollection<IResolveExtension>? GetResolveExtensions(this ComponentModel model)
    {
        ArgumentNullException.ThrowIfNull(model);

        return model.ExtendedProperties[ResolveExtensionsKey] as ICollection<IResolveExtension>;
    }
}