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
using Castle.Core.Resource;
using Castle.Windsor.Core;
using Castle.Windsor.MicroKernel;
using Castle.Windsor.MicroKernel.Registration;
using Castle.Windsor.Tests.ClassComponents;
using Castle.Windsor.Tests.Components;
using Castle.Windsor.Tests.Config.Components;
using Castle.Windsor.Windsor;
using Castle.Windsor.Windsor.Installer;

namespace Castle.Windsor.Tests.Config;

public class ConfigurationTestCase : AbstractContainerTestCase
{
    [Fact]
    [Bug("https://github.com/castleproject/Windsor/issues/574")]
    public void DictionaryWithReferencedProperty()
    {
        const string config =
            """
            <configuration>
            	<properties>
            		<value1>Property Value 1</value1>
                    <value2>Property Value 2</value2>
                </properties>
            	<components>
            				<component id='stringToStringDictionary' type='System.Collections.Generic.Dictionary`2[System.String, System.String]'>
            					<parameters>
            						<dictionary>
            							<dictionary>
            								<entry key='Key 1'>#{value1}</entry>
            								<entry key='Key 2'>#{value2}</entry>
            							</dictionary>
            						</dictionary>
            					</parameters>
            				</component>
            	</components>
            </configuration>
            """;

        Container.Install(Configuration.FromXml(new StaticContentResource(config)));
        var stringToStringDictionary = Container.Resolve<Dictionary<string, string>>("stringToStringDictionary");
        Assert.NotNull(stringToStringDictionary);
        Assert.Equal(2, stringToStringDictionary.Count);
        Assert.Equal("Property Value 1", stringToStringDictionary["Key 1"]);
        Assert.Equal("Property Value 2", stringToStringDictionary["Key 2"]);
    }

    [Fact]
    [Bug("https://github.com/castleproject/Windsor/issues/574")]
    public void DictionaryWithReferencedList()
    {
        const string config =
            """
            <configuration>
                <facilities>
                </facilities>
            	<components>
            				<component id='list' type='System.Collections.Generic.List`1[[System.String]]'>
            					<parameters>
            						<collection>
            							<array>
            								<item>11</item>
            								<item>12</item>
            							</array>
            						</collection>
            					</parameters>
            				</component>

            				<component id='list2' type='System.Collections.Generic.List`1[[System.String]]'>
            					<parameters>
            						<collection>
            							<array>
            								<item>21</item>
            								<item>22</item>
            							</array>
            						</collection>
            					</parameters>
            				</component>

            				<component id='stringToListDictionary' type='System.Collections.Generic.Dictionary`2[System.String, System.Collections.Generic.List`1[[System.String]]]'>
            					<parameters>
            						<dictionary>
            							<dictionary>
            								<entry key='Key 1'>${list}</entry>
            								<entry key='Key 2'>${list2}</entry>
            							</dictionary>
            						</dictionary>
            					</parameters>
            				</component>
            	</components>
            </configuration>
            """;

        Container.Install(Configuration.FromXml(new StaticContentResource(config)));
        Container.Resolve<List<string>>("list");
        var stringToListDictionary = Container.Resolve<Dictionary<string, List<string>>>("stringToListDictionary");
        Assert.NotNull(stringToListDictionary);
        Assert.Equal(2, stringToListDictionary.Count);
    }

    [Fact]
    [Bug("IOC-155")]
    public void Type_not_implementing_service_should_throw()
    {
        var exception = Assert.Throws<ComponentRegistrationException>(() =>
            Container.Install(Configuration.FromXml(
                new StaticContentResource(
                    """
                    <castle>
                    <components>
                        <component
                            service="EmptyServiceA"
                            type="IEmptyService"/>
                    </components>
                    </castle>
                    """))));

        var expected =
            $"Could not set up component '{typeof(IEmptyService).FullName}'. Type '{typeof(IEmptyService).AssemblyQualifiedName}' does not implement service '{typeof(EmptyServiceA).AssemblyQualifiedName}'";

        Assert.Equal(expected, exception.Message);
    }

    [Fact]
    [Bug("IOC-197")]
    public void DictionaryAsParameterInXml()
    {
        Container.Install(Configuration.FromXml(
            new StaticContentResource(
                $$"""
                  <castle>
                  <components>
                  	<component lifestyle="singleton"
                  		id="Id.MyClass"
                  		type="{{typeof(HasDictionaryDependency).AssemblyQualifiedName}}">
                  		<parameters>
                  			<DictionaryProperty>${Id.dictionary}</DictionaryProperty>
                  		</parameters>
                  	</component>

                  	<component id="Id.dictionary" lifestyle="singleton"
                  						 service="System.Collections.IDictionary, mscorlib"
                  						 type="System.Collections.Generic.Dictionary`2[[System.String, mscorlib],[System.String, mscorlib]]"
                  						 >
                  		<parameters>
                  			<dictionary>
                  				<dictionary>
                  					<entry key="string.key.1">string value 1</entry>
                  					<entry key="string.key.2">string value 2</entry>
                  				</dictionary>
                  			</dictionary>
                  		</parameters>
                  	</component>
                  </components>
                  </castle>
                  """)));

        var myInstance = Container.Resolve<HasDictionaryDependency>();
        Assert.Equal(2, myInstance.DictionaryProperty.Count);
    }

    [Fact]
    [Bug("IOC-73")]
    public void ShouldNotThrowCircularDependencyException()
    {
        const string config =
            """
            <configuration>
                <facilities>
                </facilities>
                <components>
                    <component id='MyClass'
                        service='IEmptyService'
                        type='EmptyServiceA'/>
                    <component id='Proxy'
                        service='IEmptyService'
                        type='EmptyServiceDecorator'>
                        <parameters>
                            <other>${MyClass}</other>
                        </parameters>
                    </component>
                    <component id='ClassUser'
                        type='UsesIEmptyService'>
                        <parameters>
                            <emptyService>${Proxy}</emptyService>
                        </parameters>
                    </component>
                </components>
            </configuration>
            """;

        Container.Install(Configuration.FromXml(new StaticContentResource(config)));
        var user = Container.Resolve<UsesIEmptyService>();
        Assert.NotNull(user.EmptyService);
    }

    [Fact]
    public void Can_properly_populate_array_dependency_from_xml_config_when_registering_by_convention()
    {
        Container
            .Install(
                Configuration
                    .FromXmlFile("config/ComponentWithArrayDependency.config"))
            .Register(
                Component
                    .For<IConfig>()
                    .ImplementedBy<Components.Config>()
                    .Named("componentWithArrayDependency"));

        Container.Register(
            Classes.FromAssembly(GetCurrentAssembly()).Pick().WithServiceFirstInterface());

        var configDependency = Container.Resolve<IClassWithConfigDependency>();

        Assert.Equal("value", configDependency.GetName());
        Assert.Equal("3.24.23.33", configDependency.GetServerIp("Database"));
    }

    [Fact]
    [Bug("IOC-142")]
    public void Can_satisfy_nullable_ctor_dependency()
    {
        var container = new WindsorContainer();
        var configuration = new MutableConfiguration("parameters");
        configuration.CreateChild("foo", "5");
        container.Register(Component.For<HasNullableDoubleConstructor>().Configuration(configuration));

        container.Resolve<HasNullableDoubleConstructor>();
    }

    [Fact]
    [Bug("IOC-142")]
    public void Can_satisfy_nullable_property_dependency()
    {
        var container = new WindsorContainer();
        var configuration = new MutableConfiguration("parameters");
        configuration.CreateChild("SomeVal", "5");
        container.Register(Component.For<HasNullableIntProperty>().Configuration(configuration));

        var s = container.Resolve<HasNullableIntProperty>();
        Assert.NotNull(s.SomeVal);
    }

    [Fact]
    public void ComplexConfigurationParameter()
    {
        const string key = "key";
        const string value1 = "value1";
        const string value2 = "value2";

        var confignode = new MutableConfiguration(key);

        IConfiguration parameters = new MutableConfiguration("parameters");
        confignode.Children.Add(parameters);

        IConfiguration complexParam = new MutableConfiguration("complexparam");
        parameters.Children.Add(complexParam);

        IConfiguration complexNode = new MutableConfiguration("complexparametertype");
        complexParam.Children.Add(complexNode);

        complexNode.Children.Add(new MutableConfiguration("mandatoryvalue", value1));
        complexNode.Children.Add(new MutableConfiguration("optionalvalue", value2));

        Kernel.ConfigurationStore.AddComponentConfiguration(key, confignode);
        Kernel.Register(Component.For(typeof(ClassWithComplexParameter)).Named(key));

        var instance = Kernel.Resolve<ClassWithComplexParameter>(key);

        Assert.NotNull(instance);
        Assert.NotNull(instance.ComplexParam);
        Assert.Equal(value1, instance.ComplexParam.MandatoryValue);
        Assert.Equal(value2, instance.ComplexParam.OptionalValue);
    }

    [Fact]
    public void ConstructorWithArrayParameter()
    {
        var confignode = new MutableConfiguration("key");

        IConfiguration parameters = new MutableConfiguration("parameters");
        confignode.Children.Add(parameters);

        IConfiguration hosts = new MutableConfiguration("hosts");
        parameters.Children.Add(hosts);
        IConfiguration array = new MutableConfiguration("array");
        hosts.Children.Add(array);
        array.Children.Add(new MutableConfiguration("item", "castle"));
        array.Children.Add(new MutableConfiguration("item", "uol"));
        array.Children.Add(new MutableConfiguration("item", "folha"));

        Kernel.ConfigurationStore.AddComponentConfiguration("key", confignode);

        Kernel.Register(Component.For(typeof(ClassWithConstructors)).Named("key"));

        var instance = Kernel.Resolve<ClassWithConstructors>("key");
        Assert.NotNull(instance);
        Assert.Null(instance.Host);
        Assert.Equal("castle", instance.Hosts[0]);
        Assert.Equal("uol", instance.Hosts[1]);
        Assert.Equal("folha", instance.Hosts[2]);
    }

    [Fact]
    public void ConstructorWithArrayParameterAndCustomType()
    {
        var confignode = new MutableConfiguration("key");

        IConfiguration parameters = new MutableConfiguration("parameters");
        confignode.Children.Add(parameters);

        IConfiguration services = new MutableConfiguration("services");
        parameters.Children.Add(services);
        var array = new MutableConfiguration("array");
        services.Children.Add(array);

        array.Children.Add(new MutableConfiguration("item", "${commonservice1}"));
        array.Children.Add(new MutableConfiguration("item", "${commonservice2}"));

        Kernel.ConfigurationStore.AddComponentConfiguration("key", confignode);

        Kernel.Register(Component.For<ClassWithArrayConstructor>().Named("key"),
            Component.For<ICommon>().ImplementedBy<CommonImpl1>().Named("commonservice1"),
            Component.For<ICommon>().ImplementedBy<CommonImpl2>().Named("commonservice2"));

        var instance = Kernel.Resolve<ClassWithArrayConstructor>("key");
        Assert.NotNull(instance.Services);
        Assert.Equal(2, instance.Services.Length);
        Assert.Equal("CommonImpl1", instance.Services[0].GetType().Name);
        Assert.Equal("CommonImpl2", instance.Services[1].GetType().Name);
    }

    [Fact]
    public void ConstructorWithListParameterAndCustomType()
    {
        var confignode = new MutableConfiguration("key");

        IConfiguration parameters = new MutableConfiguration("parameters");
        confignode.Children.Add(parameters);

        IConfiguration services = new MutableConfiguration("services");
        parameters.Children.Add(services);
        var list = new MutableConfiguration("list");
        services.Children.Add(list);
        list.Attributes.Add("type", "ICommon");

        list.Children.Add(new MutableConfiguration("item", "${commonservice1}"));
        list.Children.Add(new MutableConfiguration("item", "${commonservice2}"));

        Kernel.ConfigurationStore.AddComponentConfiguration("key", confignode);
        Kernel.Register(Component.For(typeof(ClassWithListConstructor)).Named("key"));

        Kernel.Register(Component.For(typeof(ICommon)).ImplementedBy<CommonImpl1>().Named("commonservice1"));
        Kernel.Register(Component.For(typeof(ICommon)).ImplementedBy<CommonImpl2>().Named("commonservice2"));

        var instance = Kernel.Resolve<ClassWithListConstructor>("key");
        Assert.NotNull(instance.Services);
        Assert.Equal(2, instance.Services.Count);
        Assert.NotNull(instance.Services[0]);
        Assert.Equal("CommonImpl1", instance.Services[0].GetType().Name);
        Assert.NotNull(instance.Services[1]);
        Assert.Equal("CommonImpl2", instance.Services[1].GetType().Name);
    }

    [Fact]
    public void ConstructorWithStringParameters()
    {
        var confignode = new MutableConfiguration("key");

        IConfiguration parameters = new MutableConfiguration("parameters");
        confignode.Children.Add(parameters);

        parameters.Children.Add(new MutableConfiguration("host", "castleproject.org"));

        Kernel.ConfigurationStore.AddComponentConfiguration("key", confignode);

        Kernel.Register(Component.For<ClassWithConstructors>().Named("key"));

        var instance = Kernel.Resolve<ClassWithConstructors>("key");
        Assert.NotNull(instance);
        Assert.NotNull(instance.Host);
        Assert.Equal("castleproject.org", instance.Host);
    }

    [Fact]
    public void CustomLifestyleManager()
    {
        const string key = "key";

        var confignode = new MutableConfiguration(key);
        confignode.Attributes.Add("lifestyle", "custom");

        confignode.Attributes.Add("customLifestyleType", "CustomLifestyleManager");

        Kernel.ConfigurationStore.AddComponentConfiguration(key, confignode);
        Kernel.Register(Component.For(typeof(ICommon)).ImplementedBy<CommonImpl1>().Named(key));

        var instance = Kernel.Resolve<ICommon>(key);
        var handler = Kernel.GetHandler(key);

        Assert.NotNull(instance);
        Assert.Equal(LifestyleType.Custom, handler.ComponentModel.LifestyleType);
        Assert.Equal(typeof(CustomLifestyleManager), handler.ComponentModel.CustomLifestyle);
    }

    [Fact]
    public void ServiceOverride()
    {
        var confignode = new MutableConfiguration("key");

        IConfiguration parameters = new MutableConfiguration("parameters");
        confignode.Children.Add(parameters);

        parameters.Children.Add(new MutableConfiguration("common", "${commonservice2}"));

        Kernel.ConfigurationStore.AddComponentConfiguration("commonserviceuser", confignode);

        Kernel.Register(Component.For(typeof(ICommon)).ImplementedBy<CommonImpl1>().Named("commonservice1"));
        Kernel.Register(Component.For(typeof(ICommon)).ImplementedBy<CommonImpl2>().Named("commonservice2"));
        Kernel.Register(Component.For(typeof(CommonServiceUser)).Named("commonserviceuser"));

        var instance = Kernel.Resolve<CommonServiceUser>("commonserviceuser");

        Assert.NotNull(instance);
        Assert.Equal(typeof(CommonImpl2), instance.CommonService.GetType());
    }

    [Fact]
    public void ServiceOverrideUsingProperties()
    {
        var confignode = new MutableConfiguration("key");

        IConfiguration parameters = new MutableConfiguration("parameters");
        confignode.Children.Add(parameters);

        parameters.Children.Add(new MutableConfiguration("CommonService", "${commonservice2}"));

        Kernel.ConfigurationStore.AddComponentConfiguration("commonserviceuser", confignode);

        Kernel.Register(Component.For(typeof(ICommon)).ImplementedBy<CommonImpl1>().Named("commonservice1"));
        Kernel.Register(Component.For(typeof(ICommon)).ImplementedBy<CommonImpl2>().Named("commonservice2"));

        Kernel.Register(Component.For(typeof(CommonServiceUser2)).Named("commonserviceuser"));

        var instance = Kernel.Resolve<CommonServiceUser2>("commonserviceuser");

        Assert.NotNull(instance);
        Assert.Equal(typeof(CommonImpl2), instance.CommonService.GetType());
    }
}