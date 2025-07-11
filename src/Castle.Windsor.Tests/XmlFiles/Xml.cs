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

using System;
using System.IO;
using System.Reflection;
using Castle.Core.Resource;

namespace Castle.Windsor.Tests.XmlFiles;

public class Xml
{
	private static readonly string LocalPath =
		"assembly://" + typeof(Xml).GetTypeInfo().Assembly.FullName + "/CastleTests/XmlFiles/";

	public static IResource Embedded(string name)
	{
		var uri = new CustomUri(EmbeddedPath(name));
		var resource = new AssemblyResource(uri);
		return resource;
	}

	public static string EmbeddedPath(string name)
	{
		return LocalPath + name;
	}

	public static IResource File(string name)
	{
		var uri = new CustomUri(FilePath(name));
		var resource = new FileResource(uri);
		return resource;
	}

	public static string FilePath(string name)
	{
		var fullPath = Path.Combine(AppContext.BaseDirectory, "XmlFiles/" + name);
		return fullPath;
	}
}