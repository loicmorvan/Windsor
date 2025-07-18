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

namespace CastleTests.Facilities;

using System;

using Castle.Core.Configuration;
using Castle.Facilities.Startable;
using Castle.MicroKernel;
using Castle.MicroKernel.Tests.ClassComponents;

public class FacilityTestCase
{
	private static readonly string facilityKey = typeof(HiperFacility).FullName;
	private readonly HiperFacility facility;
	private readonly IKernel kernel;

	public FacilityTestCase()
	{
		kernel = new DefaultKernel();

		IConfiguration confignode = new MutableConfiguration("facility");
		IConfiguration facilityConf = new MutableConfiguration(facilityKey);
		confignode.Children.Add(facilityConf);
		kernel.ConfigurationStore.AddFacilityConfiguration(facilityKey, confignode);

		facility = new HiperFacility();

		Assert.False(facility.Initialized);
		kernel.AddFacility(facility);
	}

	[Fact]
	public void Cant_have_two_instances_of_any_facility_type()
	{
		kernel.AddFacility<StartableFacility>();

		var exception = Assert.Throws<ArgumentException>(() => kernel.AddFacility<StartableFacility>());

		Assert.Equal(
			"Facility of type 'Castle.Facilities.Startable.StartableFacility' has already been registered with the container. Only one facility of a given type can exist in the container.",
			exception.Message);
	}

	[Fact]
	public void Creation()
	{
		var facility = kernel.GetFacilities()[0];

		Assert.NotNull(facility);
		Assert.Same(this.facility, facility);
	}

	[Fact]
	public void LifeCycle()
	{
		Assert.False(this.facility.Terminated);

		var facility = kernel.GetFacilities()[0];

		Assert.True(this.facility.Initialized);
		Assert.False(this.facility.Terminated);

		kernel.Dispose();

		Assert.True(this.facility.Initialized);
		Assert.True(this.facility.Terminated);
	}

	[Fact]
	public void OnCreationCallback()
	{
		StartableFacility facility = null;

		kernel.AddFacility<StartableFacility>(f => facility = f);

		Assert.NotNull(facility);
	}
}