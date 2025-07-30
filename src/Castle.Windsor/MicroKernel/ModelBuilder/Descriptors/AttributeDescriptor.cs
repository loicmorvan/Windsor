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

using Castle.Core.Configuration;

namespace Castle.Windsor.MicroKernel.ModelBuilder.Descriptors;

public class AttributeDescriptor<TS> : AbstractOverwriteableDescriptor<TS>
    where TS : class
{
    private readonly string _name;
    private readonly string _value;

    /// <summary>Constructs the <see cref="AttributeDescriptor{S}" /> descriptor with name and value.</summary>
    /// <param name="name">The attribute name.</param>
    /// <param name="value">The attribute value.</param>
    public AttributeDescriptor(string name, string value)
    {
        _name = name;
        _value = value;
    }

    protected override void ApplyToConfiguration(IKernel kernel, IConfiguration configuration)
    {
        if (configuration.Attributes[_name] == null || IsOverWrite)
        {
            configuration.Attributes[_name] = _value;
        }
    }
}