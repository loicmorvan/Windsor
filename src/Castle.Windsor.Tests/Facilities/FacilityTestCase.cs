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
using Castle.Windsor.Facilities.Startable;
using Castle.Windsor.MicroKernel;
using Castle.Windsor.Tests.ClassComponents;

namespace Castle.Windsor.Tests.Facilities;

public class FacilityTestCase
{
    private static readonly string FacilityKey = typeof(HiperFacility).FullName;
    private readonly HiperFacility _facility;
    private readonly IKernel _kernel;

    public FacilityTestCase()
    {
        _kernel = new DefaultKernel();

        IConfiguration confignode = new MutableConfiguration("facility");
        IConfiguration facilityConf = new MutableConfiguration(FacilityKey);
        confignode.Children.Add(facilityConf);
        _kernel.ConfigurationStore.AddFacilityConfiguration(FacilityKey, confignode);

        _facility = new HiperFacility();

        Assert.False(_facility.Initialized);
        _kernel.AddFacility(_facility);
    }

    [Fact]
    public void Cant_have_two_instances_of_any_facility_type()
    {
        _kernel.AddFacility<StartableFacility>();

        var exception = Assert.Throws<ArgumentException>(() => _kernel.AddFacility<StartableFacility>());

        Assert.Equal(
            "Facility of type 'Castle.Windsor.Facilities.Startable.StartableFacility' has already been registered with the container. Only one facility of a given type can exist in the container.",
            exception.Message);
    }

    [Fact]
    public void Creation()
    {
        var facility = _kernel.GetFacilities()[0];

        Assert.NotNull(facility);
        Assert.Same(_facility, facility);
    }

    [Fact]
    public void LifeCycle()
    {
        Assert.False(_facility.Terminated);

        _ = _kernel.GetFacilities()[0];

        Assert.True(_facility.Initialized);
        Assert.False(_facility.Terminated);

        _kernel.Dispose();

        Assert.True(_facility.Initialized);
        Assert.True(_facility.Terminated);
    }

    [Fact]
    public void OnCreationCallback()
    {
        StartableFacility facility = null;

        _kernel.AddFacility<StartableFacility>(f => facility = f);

        Assert.NotNull(facility);
    }
}