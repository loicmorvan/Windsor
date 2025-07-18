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

namespace Castle.MicroKernel.Tests.Configuration.Components;

using Castle.MicroKernel.SubSystems.Conversion;

public class ClassWithComplexParameter
{
	public ComplexParameterType ComplexParam { get; set; }

	[Convertible]
	public class ComplexParameterType
	{
		public ComplexParameterType()
		{
			// sets default values
			MandatoryValue = "default1";
			OptionalValue = "default2";
		}

		public ComplexParameterType(string mandatoryValue)
		{
			MandatoryValue = mandatoryValue;
		}

		public string MandatoryValue { get; }

		public string OptionalValue { get; set; }
	}
}