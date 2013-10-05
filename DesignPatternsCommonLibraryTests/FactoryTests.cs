﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using DesignPatternsCommonLibrary;
using Microsoft.VisualStudio.TestPlatform.UnitTestFramework;

namespace DesignPatternsCommonLibraryTests
{
    public class FactoryTests
    {
        private IDesignPattensFileManager _dpFileManager;
        public FactoryTests(IDesignPattensFileManager dpFileManager)
        {
            _dpFileManager = dpFileManager;
        }
        public void FactoryCreation(String factoryType)
        {
            var parameters = new Dictionary<string, string>
                {
                    {"{NAMESPACE}", "BuiltDesignPatternsTest.FactoryTest.Shape"+factoryType},
                    {"{PARENT_OBJECT}", "Shape"}
                };
            var shapes = new Dictionary<String, List<String>>
                {
                    {
                        "{OBJECT}", new List<string>
                            {
                                "Circle",
                                "Square"
                            }
                    }
                };

            var designPatternBuilder = new DesignPatternBuilder(_dpFileManager);

            var files = designPatternBuilder.BuildFromXml(factoryType, parameters, shapes).Result;

            foreach (var classInformation in files)
            {
                _dpFileManager.CreateFile(classInformation.FileName, "TestDrops",
                                                               classInformation.Content);
                var fileExits = _dpFileManager.FileExistsInFolder(classInformation.FileName, "TestDrops").Result;
                Assert.IsTrue(fileExits);
            }
        }
    }
}
