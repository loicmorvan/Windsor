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

using JetBrains.Annotations;

namespace Castle.Windsor.MicroKernel.Registration;

/// <summary>
///     Entry point to fluent way to register, by convention, multiple types. No upfront filtering is done so literally
///     every type will be considered. That means that usually some filtering done by user
///     will be required. For a most common case where non-abstract classes only are to be considered use
///     <see cref="Classes" /> class instead. Use static methods on the class to fluently build
///     registration.
/// </summary>
public static class Types
{
    /// <summary>Prepares to register types from a list of types.</summary>
    /// <param name="types"> The list of types. </param>
    /// <returns>The corresponding <see cref="FromDescriptor" /></returns>
    [PublicAPI]
    public static FromTypesDescriptor From(IEnumerable<Type> types)
    {
        return new FromTypesDescriptor(types, null);
    }

    /// <summary>Prepares to register types from a list of types.</summary>
    /// <param name="types"> The list of types. </param>
    /// <returns>The corresponding <see cref="FromDescriptor" /></returns>
    [PublicAPI]
    public static FromTypesDescriptor From(params Type[] types)
    {
        return new FromTypesDescriptor(types, null);
    }
}