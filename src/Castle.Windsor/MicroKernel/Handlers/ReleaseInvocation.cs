﻿// Copyright 2004-2011 Castle Project - http://www.castleproject.org/
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

using System;

namespace Castle.MicroKernel.Handlers;

public class ReleaseInvocation(Burden burden)
{
	private Action _proceed;

	public Burden Burden { get; } = burden;

	public object Instance => Burden.Instance;

	public bool ReturnValue { get; set; }

	public void Proceed()
	{
		_proceed.Invoke();
	}

	internal void SetProceedDelegate(Action value)
	{
		_proceed = value;
	}
}