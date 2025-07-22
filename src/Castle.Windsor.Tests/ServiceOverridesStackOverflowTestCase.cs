// Copyright 2004-2010 Castle Project - http://www.castleproject.org/
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

using System.Collections.Generic;
using System.Linq;
using Castle.Windsor.Tests.XmlFiles;
using Castle.Windsor.Windsor;
using Castle.Windsor.Windsor.Installer;
using JetBrains.Annotations;

namespace Castle.Windsor.Tests;

public class ServiceOverridesStackOverflowTestCase
{
	[Fact]
	public void Should_not_StackOverflow()
	{
		var container = new WindsorContainer()
			.Install(Configuration.FromXml(Xml.Embedded("channel1.xml")));

		var channel = container.Resolve<MessageChannel>("MessageChannel1");
		var array = channel.RootDevice.Children.ToArray();

		Assert.Same(channel.RootDevice, container.Resolve<IDevice>("device1"));
		Assert.Equal(2, array.Length);
		Assert.Same(array[0], container.Resolve<IDevice>("device2"));
		Assert.Same(array[1], container.Resolve<IDevice>("device3"));
	}
}

[UsedImplicitly]
public class MessageChannel(IDevice root)
{
	public IDevice RootDevice { get; } = root;
}

public interface IDevice
{
	MessageChannel Channel { get; }
	IEnumerable<IDevice> Children { get; }
}

public abstract class BaseDevice : IDevice
{
	public abstract IEnumerable<IDevice> Children { get; }

	// ReSharper disable once UnusedAutoPropertyAccessor.Global
	public MessageChannel Channel { get; set; }
}

[UsedImplicitly]
public class TestDevice : BaseDevice
{
	private readonly List<IDevice> _children;

	public TestDevice()
	{
	}

	public TestDevice(IEnumerable<IDevice> theChildren)
	{
		_children = new List<IDevice>(theChildren);
	}

	public override IEnumerable<IDevice> Children => _children;
}