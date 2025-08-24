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

using System.Reflection;
using Castle.Windsor.Core.Internal;
using Castle.Windsor.MicroKernel.Util;
using JetBrains.Annotations;
using Lock = Castle.Windsor.MicroKernel.Internal.Lock;

namespace Castle.Windsor.MicroKernel.SubSystems.Naming;

[Serializable]
public class DefaultNamingSubSystem : AbstractSubSystem, INamingSubSystem
{
    private readonly Dictionary<Type, IHandler[]> _assignableHandlerListsByTypeCache =
        new(SimpleTypeEqualityComparer.Instance);

    protected readonly Dictionary<Type, IHandler[]> HandlerListsByTypeCache = new(SimpleTypeEqualityComparer.Instance);

    protected readonly Lock Lock = Lock.Create();

    /// <summary>
    ///     Map(String, IHandler) to map component names to <see cref="IHandler" /> Items in this dictionary are sorted in
    ///     insertion order.
    /// </summary>
    protected readonly Dictionary<string, IHandler> Name2Handler = new(StringComparer.OrdinalIgnoreCase);

    /// <summary>
    ///     Map(Type, IHandler) to map a service to <see cref="IHandler" /> . If there is more than a single service of the
    ///     type, only the first registered services are stored in this dictionary. It serves
    ///     as a fast lookup for the common case of having a single handler for a type.
    /// </summary>
    protected readonly Dictionary<Type, HandlerWithPriority> Service2Handler = new(SimpleTypeEqualityComparer.Instance);

    private Dictionary<string, IHandler>? _handlerByNameCache;
    private Dictionary<Type, IHandler>? _handlerByServiceCache;

    protected IList<IHandlersFilter>? Filters;
    protected IList<IHandlerSelector>? Selectors;

    protected IDictionary<string, IHandler> HandlerByNameCache
    {
        get
        {
            var cache = _handlerByNameCache;
            if (cache != null)
            {
                return cache;
            }

            using (Lock.ForWriting())
            {
                cache = new Dictionary<string, IHandler>(Name2Handler, Name2Handler.Comparer);
                _handlerByNameCache = cache;
                return cache;
            }
        }
    }

    protected IDictionary<Type, IHandler> HandlerByServiceCache
    {
        get
        {
            var cache = _handlerByServiceCache;
            if (cache != null)
            {
                return cache;
            }

            using (Lock.ForWriting())
            {
                cache = new Dictionary<Type, IHandler>(Service2Handler.Count, Service2Handler.Comparer);
                foreach (var item in Service2Handler)
                {
                    cache.Add(item.Key, item.Value.Handler);
                }

                _handlerByServiceCache = cache;
                return cache;
            }
        }
    }

    public virtual int ComponentCount => HandlerByNameCache.Count;

    public void AddHandlerSelector(IHandlerSelector selector)
    {
        Selectors ??= new List<IHandlerSelector>();
        Selectors.Add(selector);
    }

    public void AddHandlersFilter(IHandlersFilter filter)
    {
        Filters ??= new List<IHandlersFilter>();
        Filters.Add(filter);
    }

    public virtual bool Contains(string name)
    {
        return HandlerByNameCache.ContainsKey(name);
    }

    public virtual bool Contains(Type service)
    {
        return GetHandler(service) != null;
    }

    public virtual IHandler[] GetAllHandlers()
    {
        var cache = HandlerByNameCache;
        var list = new IHandler[cache.Values.Count];
        cache.Values.CopyTo(list, 0);
        return list;
    }

    public virtual IHandler[] GetAssignableHandlers(Type service)
    {
        ArgumentNullException.ThrowIfNull(service);
        return service == typeof(object)
            ? GetAllHandlers()
            : GetAssignableHandlersNoFiltering(service);
    }

    public virtual IHandler GetHandler(string name)
    {
        ArgumentNullException.ThrowIfNull(name);

        if (Selectors != null)
        {
            var selectorsOpinion = GetSelectorsOpinion(name, null);
            if (selectorsOpinion != null)
            {
                return selectorsOpinion;
            }
        }

        HandlerByNameCache.TryGetValue(name, out var value);
        return value;
    }

    public virtual IHandler? GetHandler(Type service)
    {
        ArgumentNullException.ThrowIfNull(service);
        if (Selectors != null)
        {
            var selectorsOpinion = GetSelectorsOpinion(null, service);
            if (selectorsOpinion != null)
            {
                return selectorsOpinion;
            }
        }

        if (HandlerByServiceCache.TryGetValue(service, out var handler))
        {
            return handler;
        }

        if (!service.GetTypeInfo().IsGenericType || service.GetTypeInfo().IsGenericTypeDefinition)
        {
            return null;
        }

        var openService = service.GetGenericTypeDefinition();
        if (HandlerByServiceCache.TryGetValue(openService, out handler) && handler.Supports(service))
        {
            return handler;
        }

        var handlerCandidates = GetHandlers(openService);
        return handlerCandidates.FirstOrDefault(handlerCandidate => handlerCandidate.Supports(service));
    }

    public virtual IHandler[] GetHandlers(Type service)
    {
        ArgumentNullException.ThrowIfNull(service);
        if (Filters != null)
        {
            var filtersOpinion = GetFiltersOpinion(service);
            if (filtersOpinion != null)
            {
                return filtersOpinion;
            }
        }

        using var locker = Lock.ForReadingUpgradeable();
        if (HandlerListsByTypeCache.TryGetValue(service, out var result))
        {
            return result;
        }

        result = GetHandlersNoLock(service);

        locker.Upgrade();
        HandlerListsByTypeCache[service] = result;

        return result;
    }


    public virtual void Register(IHandler handler)
    {
        var name = handler.ComponentModel.Name;
        using (Lock.ForWriting())
        {
            try
            {
                Name2Handler.Add(name, handler);
            }
            catch (ArgumentException)
            {
                throw new ComponentRegistrationException(
                    $"Component {name} could not be registered. There is already a component with that name. Did you want to modify the existing component instead? If not, make sure you specify a unique name.");
            }

            var serviceSelector = GetServiceSelector(handler);
            foreach (var service in handler.ComponentModel.Services)
            {
                var handlerForService = serviceSelector(service);
                if (!Service2Handler.TryGetValue(service, out var previous) ||
                    handlerForService.Triumphs(previous))
                {
                    Service2Handler[service] = handlerForService;
                }
            }

            InvalidateCache();
        }
    }

    protected IHandler[] GetAssignableHandlersNoFiltering(Type service)
    {
        using var locker = Lock.ForReadingUpgradeable();
        if (_assignableHandlerListsByTypeCache.TryGetValue(service, out var result))
        {
            return result;
        }

        locker.Upgrade();
        if (_assignableHandlerListsByTypeCache.TryGetValue(service, out result))
        {
            return result;
        }

        result = Name2Handler.Values.Where(h => h.SupportsAssignable(service)).ToArray();
        _assignableHandlerListsByTypeCache[service] = result;

        return result;
    }

    [PublicAPI]
    protected virtual IHandler[] GetFiltersOpinion(Type service)
    {
        return Filters == null
            ? null
            : (from filter in Filters
                where filter.HasOpinionAbout(service)
                let handlers = GetAssignableHandlersNoFiltering(service)
                select filter.SelectHandlers(service, handlers)).FirstOrDefault(handlers => handlers != null);
    }

    [PublicAPI]
    protected virtual IHandler? GetSelectorsOpinion(string? name, Type? type)
    {
        if (Selectors == null)
        {
            return null;
        }

        type ??= typeof(object); // if type is null, we want everything, so object does well for that
        IHandler[]? handlers = null; //only init if we have a selector with an opinion about this type
        foreach (var selector in Selectors)
        {
            if (!selector.HasOpinionAbout(name, type))
            {
                continue;
            }

            handlers ??= GetAssignableHandlersNoFiltering(type);
            var handler = selector.SelectHandler(name, type, handlers);
            if (handler != null)
            {
                return handler;
            }
        }

        return null;
    }

    protected void InvalidateCache()
    {
        HandlerListsByTypeCache.Clear();
        _assignableHandlerListsByTypeCache.Clear();
        _handlerByNameCache = null;
        _handlerByServiceCache = null;
    }

    private IHandler[] GetHandlersNoLock(Type service)
    {
        //we have 3 segments
        const int defaults = 0;
        const int regulars = 1;
        const int fallbacks = 2;
        var handlers = new SegmentedList<IHandler>(3);
        foreach (var handler in Name2Handler.Values.Where(handler => handler.Supports(service)))
        {
            if (IsDefault(handler, service))
            {
                handlers.AddFirst(defaults, handler);
                continue;
            }

            if (IsFallback(handler, service))
            {
                handlers.AddLast(fallbacks, handler);
                continue;
            }

            handlers.AddLast(regulars, handler);
        }

        return handlers.ToArray();
    }

    private static Func<Type, HandlerWithPriority> GetServiceSelector(IHandler handler)
    {
        var defaultsFilter = handler.ComponentModel.GetDefaultComponentForServiceFilter();
        var fallbackFilter = handler.ComponentModel.GetFallbackComponentForServiceFilter();
        if (defaultsFilter == null)
        {
            if (fallbackFilter == null)
            {
                return _ => new HandlerWithPriority(0, handler);
            }

            return service => new HandlerWithPriority(fallbackFilter(service) ? -1 : 0, handler);
        }

        if (fallbackFilter == null)
        {
            return service => new HandlerWithPriority(defaultsFilter(service) ? 1 : 0, handler);
        }

        return service =>
            new HandlerWithPriority(defaultsFilter(service) ? 1 : fallbackFilter(service) ? -1 : 0, handler);
    }

    private static bool IsDefault(IHandler handler, Type service)
    {
        var filter = handler.ComponentModel.GetDefaultComponentForServiceFilter();
        return filter != null && filter(service);
    }

    private static bool IsFallback(IHandler handler, Type service)
    {
        var filter = handler.ComponentModel.GetFallbackComponentForServiceFilter();
        return filter != null && filter(service);
    }

    protected struct HandlerWithPriority(int priority, IHandler handler)
    {
        private readonly int _priority = priority;

        public IHandler Handler { get; } = handler;

        public bool Triumphs(HandlerWithPriority other)
        {
            if (_priority > other._priority)
            {
                return true;
            }

            return _priority == other._priority && _priority > 0;
        }
    }
}