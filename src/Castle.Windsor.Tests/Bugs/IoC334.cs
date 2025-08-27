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

using System.Diagnostics;
using Castle.Core.Configuration;
using Castle.Windsor.MicroKernel;
using Castle.Windsor.Tests.ClassComponents;

#pragma warning disable 618
namespace Castle.Windsor.Tests.Bugs;

public class IoC334
{
    [Fact]
    public void FacilityConfig_is_not_null()
    {
        using var c = new DefaultKernel();
        var facilityKey = typeof(HiperFacility).FullName;
        var config = new MutableConfiguration("facility");
        Debug.Assert(facilityKey != null, nameof(facilityKey) + " != null");
        c.ConfigurationStore.AddFacilityConfiguration(facilityKey, config);
        var facility = new HiperFacility();
        c.AddFacility(facility);
        Assert.True(facility.Initialized);
    }
}