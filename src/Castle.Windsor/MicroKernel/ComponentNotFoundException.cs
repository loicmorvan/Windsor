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

namespace Castle.Windsor.MicroKernel;

/// <summary>
///     Exception threw when a request for a component cannot be satisfied because the component does not exist in the
///     container
/// </summary>
[Serializable]
public class ComponentNotFoundException : ComponentResolutionException
{
    public ComponentNotFoundException(string name, Type? service, int countOfHandlersForTheService)
        : base(BuildMessage(name, service, countOfHandlersForTheService))
    {
        Name = name;
        Service = service;
    }

    /// <summary>Initializes a new instance of the <see cref="ComponentNotFoundException" /> class.</summary>
    /// <param name="name">The name.</param>
    /// <param name="message">Exception message.</param>
    public ComponentNotFoundException(string name, string message)
        : base(message)
    {
        Name = name;
    }

    /// <summary>Initializes a new instance of the <see cref="ComponentNotFoundException" /> class.</summary>
    /// <param name="service">The service.</param>
    /// <param name="message">Exception message.</param>
    public ComponentNotFoundException(Type service, string message)
        : base(message)
    {
        Service = service;
    }

    /// <summary>Initializes a new instance of the <see cref="ComponentNotFoundException" /> class.</summary>
    /// <param name="service">The service.</param>
    public ComponentNotFoundException(Type service) :
        this(service, $"No component for supporting the service {service.FullName} was found")
    {
    }


    public string Name { get; private set; }
    public Type Service { get; private set; }

    private static string BuildMessage(string name, Type service, int countOfHandlersForTheService)
    {
        var message =
            $"Requested component named '{name}' was not found in the container. Did you forget to register it?{Environment.NewLine}";
        return countOfHandlersForTheService switch
        {
            0 => message +
                 $"There are no components supporting requested service '{service.FullName}'. You need to register components in order to be able to use them.",
            1 => message +
                 $"There is one other component supporting requested service '{service.FullName}'. Is it what you were looking for?",
            > 1 => message +
                   $"There are {countOfHandlersForTheService} other components supporting requested service '{service.FullName}'. Were you looking for any of them?",
            _ => message
        };

        // this should never happen but if someone passes us wrong information we just ignore it
    }
}