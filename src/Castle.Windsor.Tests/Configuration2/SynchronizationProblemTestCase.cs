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
// limitations under the License.

using Castle.Windsor.Tests.Components;
using Castle.Windsor.Windsor;
using Castle.Windsor.Windsor.Configuration.Interpreters;

namespace Castle.Windsor.Tests.Configuration2;

public class SynchronizationProblemTestCase : IDisposable
{
    private readonly WindsorContainer _container;
    private readonly ManualResetEvent _startEvent = new(false);
    private readonly ManualResetEvent _stopEvent = new(false);

    public SynchronizationProblemTestCase()
    {
        _container =
            new WindsorContainer(
                new XmlInterpreter(ConfigHelper.ResolveConfigPath("Configuration2/synchtest_config.xml")));

        _container.Resolve<ComponentWithConfigs>();
    }

    public void Dispose()
    {
        _container.Dispose();
    }

    [Fact]
    public void ResolveWithConfigTest()
    {
        const int threadCount = 50;

        var threads = new Thread[threadCount];

        for (var i = 0; i < threadCount; i++)
        {
            threads[i] = new Thread(ExecuteMethodUntilSignal);
            threads[i].Start();
        }

        _startEvent.Set();

        Thread.CurrentThread.Join(10 * 2000);

        _stopEvent.Set();
    }

    private void ExecuteMethodUntilSignal()
    {
        _startEvent.WaitOne(int.MaxValue);

        while (!_stopEvent.WaitOne(1))
        {
            try
            {
                var comp = _container.Resolve<ComponentWithConfigs>();

                Assert.Equal(AppContext.BaseDirectory, comp.Name);
                Assert.Equal(90, comp.Port);
                Assert.Single(comp.Dict);
            }
            catch (Exception ex)
            {
                Console.WriteLine(DateTime.Now.Ticks + @" ---------------------------" + Environment.NewLine + ex);
            }
        }
    }
}