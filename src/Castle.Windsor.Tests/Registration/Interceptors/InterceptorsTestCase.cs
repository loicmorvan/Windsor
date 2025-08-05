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

using Castle.Windsor.MicroKernel;
using Castle.Windsor.MicroKernel.Registration;
using Castle.Windsor.Tests.ClassComponents;
using Castle.Windsor.Tests.Registration.Interceptors.Multiple;
using Castle.Windsor.Tests.Registration.Interceptors.Single;

namespace Castle.Windsor.Tests.Registration.Interceptors;

public sealed class InterceptorsTestCase : AbstractContainerTestCase
{
    private void ExecuteScenario<TScenario>() where TScenario : InterceptorsTestCaseHelper, new()
    {
        var scenario = new TScenario();
        var registration = Component.For<ICustomer>();

        scenario.RegisterInterceptors(registration);

        Kernel.Register(registration);

        var handler = Kernel.GetHandler(typeof(ICustomer));

        AssertInterceptorReferencesAreEqual(handler, scenario);
    }

    private static void AssertInterceptorReferencesAreEqual(IHandler handler, InterceptorsTestCaseHelper helper)
    {
        Assert.Equal(helper.GetExpectedInterceptorsInCorrectOrder(),
            handler.ComponentModel.Interceptors);
    }

    [Fact]
    public void GenericInterceptor()
    {
        ExecuteScenario<SingleGenericInterceptor>();
    }

    [Fact]
    public void GenericInterceptorsInSingleCall()
    {
        ExecuteScenario<GenericInterceptorsInSingleCall>();
    }

    [Fact]
    public void GenericInterceptorsMultipleCall()
    {
        ExecuteScenario<GenericInterceptorsMultipleCall>();
    }

    [Fact]
    public void InterceptorKeyInSingleCall()
    {
        ExecuteScenario<InterceptorKeyInSingleCall>();
    }

    [Fact]
    public void InterceptorKeyMultipleCall()
    {
        ExecuteScenario<InterceptorKeyMultipleCall>();
    }

    [Fact]
    public void InterceptorReferenceAnywhereMultipleCall()
    {
        ExecuteScenario<InterceptorReferenceAnywhereMultipleCall>();
    }

    [Fact]
    public void InterceptorReferenceWithPositionMultipleCall1()
    {
        ExecuteScenario<InterceptorReferenceWithPositionMultipleCall1>();
    }

    [Fact]
    public void InterceptorReferenceWithPositionMultipleCall2()
    {
        ExecuteScenario<InterceptorReferenceWithPositionMultipleCall2>();
    }

    [Fact]
    public void InterceptorReferenceWithPositionMultipleCall3()
    {
        ExecuteScenario<InterceptorReferenceWithPositionMultipleCall3>();
    }

    [Fact]
    public void InterceptorReferencesAnywhereInSingleCall()
    {
        ExecuteScenario<InterceptorReferencesWithPositionInSingleCall>();
    }

    [Fact]
    public void InterceptorReferencesWithPositionInSingleCall1()
    {
        ExecuteScenario<InterceptorReferencesWithPositionInSingleCall1>();
    }

    [Fact]
    public void InterceptorReferencesWithPositionInSingleCall2()
    {
        ExecuteScenario<InterceptorReferencesWithPositionInSingleCall2>();
    }

    [Fact]
    public void InterceptorReferencesWithPositionInSingleCall3()
    {
        ExecuteScenario<InterceptorReferencesWithPositionInSingleCall3>();
    }

    [Fact]
    public void InterceptorTypeMultipleCall()
    {
        ExecuteScenario<InterceptorTypeMultipleCall>();
    }

    [Fact]
    public void InterceptorTypesInSingleCall()
    {
        ExecuteScenario<InterceptorTypesInSingleCall>();
    }

    [Fact]
    public void SingleInterceptorKey()
    {
        ExecuteScenario<SingleInterceptorKey>();
    }

    [Fact]
    public void SingleInterceptorReference()
    {
        ExecuteScenario<SingleInterceptorReference>();
    }

    [Fact]
    public void SingleInterceptorType()
    {
        ExecuteScenario<SingleInterceptorType>();
    }
}