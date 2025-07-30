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

using Castle.DynamicProxy;
using Castle.Windsor.Tests.TypedFactoryInterfaces;
using Castle.Windsor.Tests.XmlFiles;
using Castle.Windsor.Windsor;
using Castle.Windsor.Windsor.Configuration.Interpreters;

namespace Castle.Windsor.Tests.Proxies;

public class TypedFactoryFacilityTestCase
{
    [Fact]
    public void TypedFactory_CreateMethodHasNoId_WorksFine()
    {
        var container = new WindsorContainer(new XmlInterpreter(Xml.Embedded("typedFactoryCreateWithoutId.xml")));

        var calcFactory = container.Resolve<ICalculatorFactoryCreateWithoutId>();
        Assert.NotNull(calcFactory);

        var calculator = calcFactory.Create();
        Assert.NotNull(calculator);
        Assert.Equal(3, calculator.Sum(1, 2));
    }

    [Fact]
    public void TypedFactory_WithProxies_WorksFine()
    {
        var container = new WindsorContainer(new XmlInterpreter(Xml.Embedded("typedFactory.xml")));

        var calcFactory = container.Resolve<ICalculatorFactory>();
        Assert.NotNull(calcFactory);

        var calculator = calcFactory.Create("default");
        Assert.IsType<IProxyTargetAccessor>(calculator, false);
        Assert.Equal(3, calculator.Sum(1, 2));

        calcFactory.Release(calculator);
    }
}