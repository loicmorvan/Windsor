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

namespace Castle.MicroKernel.Tests.ClassComponents;

using CastleTests.Components;

public class ServiceUser2 : ServiceUser
{
	public ServiceUser2(A a, string name, int port) : base(a)
	{
		Name = name;
		Port = port;
	}

	public ServiceUser2(A a, string name, int port, int scheduleinterval) : this(a, name, port)
	{
		ScheduleInterval = scheduleinterval;
	}

	public string Name { get; }

	public int Port { get; }

	public int ScheduleInterval { get; }
}