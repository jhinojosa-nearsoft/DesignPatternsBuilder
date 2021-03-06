﻿using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using System.Xml.Linq;
using System.Linq;

namespace DesignPatternsCommonLibrary
{
    public class DesignPatternsUpdater
    {
        private IDesignPattensFileManager _fileManager;
        public DesignPatternsUpdater(IDesignPattensFileManager fileManager)
        {
            _fileManager = fileManager;
        }
        public async Task<IEnumerable<DesignPatternFile>> UpdateDesignPatterns(bool forceUpdate = false)
        {
            var fileExists = await _fileManager.FileExistsInFolder("DesignPatternsList.dsxml", _fileManager.GetDesignPatternsTemplatesPath());

            if (!fileExists || forceUpdate)
            {
                return await UpdateDesignPatternsFile();
            }

            var readFileXml = await _fileManager.ReadFile("DesignPatternsList.dsxml", _fileManager.GetDesignPatternsTemplatesPath());

            var designPatternsXml = XDocument.Parse(readFileXml);
            var designPatternsXmlCount = designPatternsXml.Descendants("DesignPattern").Count();
            var designPatternsFilesCount = await GetDesignPatternsFiles();

            if (designPatternsFilesCount.Count() != designPatternsXmlCount)
            {
                return await UpdateDesignPatternsFile();
            }

            var dpList = new List<DesignPatternFile>();
            var i = 0;
            foreach (var dp in designPatternsXml.Descendants("DesignPattern"))
            {
                dpList.Add(new DesignPatternFile()
                {
                    Id = i,
                    DesignPatternName = dp.Descendants("Name").FirstOrDefault().Value,
                    DesignPatternType = dp.Attribute("type").Value,
                    Description = dp.Descendants("Description").FirstOrDefault().Value.Trim(),
                    Path = dp.Descendants("Name").FirstOrDefault().Value.Replace(" ","") + ".xml"
                });
                i++;
            }


            return dpList;
        }

        private async Task<IEnumerable<String>> GetDesignPatternsFiles()
        {
            var designPatternsTemplatesFiles = await _fileManager.GetFilesFromFolder(_fileManager.GetDesignPatternsTemplatesPath(), new[] { ".xml" });
            return designPatternsTemplatesFiles.ToList();
        }
        private async Task<IEnumerable<DesignPatternFile>> UpdateDesignPatternsFile()
        {
            var designPatternFiles = new List<DesignPatternFile>();
            try
            {
                var designPatternsXml = new XDocument(
                    new XDeclaration("1.0", "utf-8", "yes"),
                    new XElement("DesignPatterns")
                    );
                var files = await GetDesignPatternsFiles();
                var i = 0;
                foreach (var f in files)
                {
                    var readFile = await _fileManager.ReadFile(f, _fileManager.GetDesignPatternsTemplatesPath());
                    var doc = XDocument.Parse(readFile);
                    var designPattern = doc.Descendants("DesignPattern").FirstOrDefault();
                    var fileName = designPattern.Attribute("name").Value;
                    var type = designPattern.Attribute("type").Value;
                    var description = designPattern.Descendants("Description").FirstOrDefault();

                    var xmlFile = new XElement("DesignPattern", 
                        new XElement("Name",fileName), 
                        new XElement("Description",description.Value.Trim()), 
                        new XAttribute("type", type));
                    designPatternsXml.Element("DesignPatterns").Add(xmlFile);

                    var designPatternFile = new DesignPatternFile
                    {
                        Id = i,
                        Description = description.Value.Trim(),
                        DesignPatternName = fileName,
                        DesignPatternType = type,
                        Path = f
                    };
                    designPatternFiles.Add(designPatternFile);
                    i++;
                }
                await _fileManager.CreateFile("DesignPatternsList.dsxml", _fileManager.GetDesignPatternsTemplatesPath(),
                                        designPatternsXml.ToString());
            }
            catch (Exception e)
            {
                throw new Exception(e.Message); 
            }
            

            return designPatternFiles;
        }
    }
}
