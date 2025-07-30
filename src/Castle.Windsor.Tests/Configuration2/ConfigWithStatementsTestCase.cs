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

// we do not support xml config on SL

using Castle.Windsor.Windsor;

namespace Castle.Windsor.Tests.Configuration2;

public class ConfigWithStatementsTestCase
{
    private IWindsorContainer _container;

    [Theory]
    [InlineData("debug")]
    [InlineData("prod")]
    [InlineData("qa")]
    [InlineData("default")]
    public void SimpleChoose(string flag)
    {
        var file = ConfigHelper.ResolveConfigPath("Configuration2/config_with_define_{0}.xml", flag);

        _container = new WindsorContainer(file);

        var store = _container.Kernel.ConfigurationStore;

        Assert.Single(store.GetComponents());

        var config = store.GetComponentConfiguration(flag);

        Assert.NotNull(config);
    }

    [Fact]
    public void SimpleIf()
    {
        _container = new WindsorContainer(ConfigHelper.ResolveConfigPath("Configuration2/config_with_if_stmt.xml"));
        var store = _container.Kernel.ConfigurationStore;

        Assert.Equal(4, store.GetComponents().Length);

        var config = store.GetComponentConfiguration("debug");
        Assert.NotNull(config);

        var childItem = config.Children["item"];
        Assert.NotNull(childItem);
        Assert.Equal("some value", childItem.Value);

        childItem = config.Children["item2"];
        Assert.NotNull(childItem);
        Assert.Equal("some <&> value2", childItem.Value);

        config = store.GetComponentConfiguration("qa");
        Assert.NotNull(config);

        config = store.GetComponentConfiguration("default");
        Assert.NotNull(config);

        config = store.GetComponentConfiguration("notprod");
        Assert.NotNull(config);
    }
}