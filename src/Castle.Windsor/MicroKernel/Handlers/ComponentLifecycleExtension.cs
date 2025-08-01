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

namespace Castle.Windsor.MicroKernel.Handlers;

public class ComponentLifecycleExtension : IResolveExtension
{
    private readonly List<ComponentResolvingDelegate> _resolvers = new(4);
    private IKernel _kernel;

    public void Init(IKernel kernel, IHandler handler)
    {
        _kernel = kernel;
    }

    public void Intercept(ResolveInvocation invocation)
    {
        Releasing releasing = null;
        if (_resolvers.Count > 0)
        {
            foreach (var resolver in _resolvers)
            {
                var releaser = resolver(_kernel, invocation.Context);
                if (releaser == null)
                {
                    continue;
                }

                if (releasing == null)
                {
                    releasing = new Releasing(_resolvers.Count, _kernel);
                    invocation.RequireDecommission();
                }

                releasing.Add(releaser);
            }
        }

        invocation.Proceed();

        if (releasing == null)
        {
            return;
        }

        var burden = invocation.Burden;
        if (burden == null)
        {
            return;
        }

        burden.Releasing += releasing.Invoked;
    }

    public void AddHandler(ComponentResolvingDelegate handler)
    {
        _resolvers.Add(handler);
    }

    private class Releasing(int count, IKernel kernel)
    {
        private readonly List<ComponentReleasingDelegate> _releasers = new(count);

        public void Add(ComponentReleasingDelegate releaser)
        {
            _releasers.Add(releaser);
        }

        public void Invoked(Burden burden)
        {
            burden.Releasing -= Invoked;

            _releasers.ForEach(r => r.Invoke(kernel));
        }
    }
}