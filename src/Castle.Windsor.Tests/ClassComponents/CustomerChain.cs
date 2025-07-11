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
// WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or Chainied.
// See the License for the specific language governing permissions and
// limitations under the License.

using System;

namespace Castle.Windsor.Tests.ClassComponents;

public class CustomerChain1(ICustomer customer) : CustomerImpl
{
	public readonly ICustomer CustomerBase = customer;
}

public class CustomerChain2(ICustomer customer) : CustomerChain1(customer);

public class CustomerChain3(ICustomer customer) : CustomerChain1(customer);

public class CustomerChain4(ICustomer customer) : CustomerChain1(customer);

public class CustomerChain5(ICustomer customer) : CustomerChain1(customer);

public class CustomerChain6(ICustomer customer) : CustomerChain1(customer);

public class CustomerChain7(ICustomer customer) : CustomerChain1(customer);

[Serializable]
public class CustomerChain8(ICustomer customer) : CustomerChain1(customer);

[Serializable]
public class CustomerChain9(ICustomer customer) : CustomerChain1(customer);