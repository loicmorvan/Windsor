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

using System.Text.RegularExpressions;
using System.Xml;
using Castle.Windsor.Windsor.Configuration.Interpreters;

namespace Castle.Windsor.Tests.XmlProcessor;

/// <summary>Summary description for Class1.</summary>
public class XmlProcessorTestCase
{
    [Fact]
    public void InvalidFiles()
    {
        var files = Directory.GetFiles(GetFullPath(), "Invalid*.xml");
        Assert.NotEmpty(files);

        foreach (var fileName in files)
        {
            var doc = GetXmlDocument(fileName);
            var processor = new Castle.Windsor.Windsor.Configuration.Interpreters.XmlProcessor.XmlProcessor();

            Assert.Throws<ConfigurationProcessingException>(() =>
                processor.Process(doc.DocumentElement));
        }
    }

    /// <summary>Runs the tests.</summary>
    [Fact]
    public void RunTests()
    {
        var files = Directory.GetFiles(GetFullPath(), "*Test.xml");
        Assert.NotEmpty(files);

        foreach (var fileName in files)
        {
            if (fileName.EndsWith("PropertiesWithAttributesTest.xml"))
            {
                continue;
            }

            var doc = GetXmlDocument(fileName);

            var resultFileName = fileName[..^4] + "Result.xml";

            var resultDoc = GetXmlDocument(resultFileName);

            var processor = new Castle.Windsor.Windsor.Configuration.Interpreters.XmlProcessor.XmlProcessor();

            try
            {
                var result = processor.Process(doc.DocumentElement);

                var resultDocStr = StripSpaces(resultDoc.OuterXml);
                var resultStr = StripSpaces(result.OuterXml);

                // Debug.WriteLine(resultDocStr);
                // Debug.WriteLine(resultStr);

                Assert.Equal(resultDocStr, resultStr);
            }
            catch (Exception e)
            {
                throw new Exception("Error processing " + fileName, e);
            }
        }
    }

    public XmlDocument GetXmlDocument(string fileName)
    {
        var doc = new XmlDocument();

        var content = File.ReadAllText(fileName);
        doc.LoadXml(content);

        return doc;
    }

    private string StripSpaces(string xml)
    {
        return Regex.Replace(xml, "\\s+", "", RegexOptions.Compiled);
    }

    private string GetFullPath()
    {
        return Path.Combine(AppContext.BaseDirectory, ConfigHelper.ResolveConfigPath("XmlProcessor/TestFiles/"));
    }
}