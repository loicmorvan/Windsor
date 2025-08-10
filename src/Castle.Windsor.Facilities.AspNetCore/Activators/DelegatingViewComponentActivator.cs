// Copyright 2004-2018 Castle Project - http://www.castleproject.org/
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

using Microsoft.AspNetCore.Mvc.ViewComponents;

namespace Castle.Windsor.Facilities.AspNetCore.Activators;

internal sealed class DelegatingViewComponentActivator : IViewComponentActivator
{
    private readonly Func<Type, object> _viewComponentCreator;
    private readonly Action<object> _viewComponentReleaser;

    public DelegatingViewComponentActivator(Func<Type, object> viewComponentCreator,
        Action<object> viewComponentReleaser)
    {
        _viewComponentCreator = viewComponentCreator ?? throw new ArgumentNullException(nameof(viewComponentCreator));
        _viewComponentReleaser =
            viewComponentReleaser ?? throw new ArgumentNullException(nameof(viewComponentReleaser));
    }

    public object Create(ViewComponentContext context)
    {
        return _viewComponentCreator(context.ViewComponentDescriptor.TypeInfo.AsType());
    }

    public void Release(ViewComponentContext context, object viewComponent)
    {
        _viewComponentReleaser(viewComponent);
    }
}