// Copyright 2004-2009 Castle Project - http://www.castleproject.org/
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
// limitations under the License.using System;

using Castle.Windsor.MicroKernel.SubSystems.Conversion;
using JetBrains.Annotations;

namespace Castle.Windsor.Tests.Config.Components;

public class ClassWithComplexParameter
{
    public ComplexParameterType ComplexParam { get; set; }

    [Convertible]
    [UsedImplicitly]
    public class ComplexParameterType(string mandatoryValue)
    {
        public ComplexParameterType() : this("default1")
        {
            // sets default values
            OptionalValue = "default2";
        }

        public string MandatoryValue { get; } = mandatoryValue;

        // ReSharper disable once AutoPropertyCanBeMadeGetOnly.Global
        public string OptionalValue { get; set; }
    }
}