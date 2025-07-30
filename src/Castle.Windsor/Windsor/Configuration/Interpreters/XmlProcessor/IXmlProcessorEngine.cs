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

using System.Xml;
using Castle.Core.Resource;

namespace Castle.Windsor.Windsor.Configuration.Interpreters.XmlProcessor;

public interface IXmlProcessorEngine
{
    void AddFlag(string flag);

    void AddNodeProcessor(Type type);

    void AddProperty(XmlElement element);

    void DispatchProcessAll(IXmlProcessorNodeList nodeList);

    void DispatchProcessCurrent(IXmlProcessorNodeList nodeList);

    XmlElement GetProperty(string name);

    IResource GetResource(string uri);

    bool HasFlag(string flag);

    bool HasProperty(string name);

    bool HasSpecialProcessor(XmlNode node);

    void PopResource();

    void PushResource(IResource resource);

    void RemoveFlag(string flag);
}