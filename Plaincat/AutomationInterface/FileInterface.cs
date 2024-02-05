using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;
using System.Xml.Linq;

namespace Plaincat.AutomationInterface
{
    public class FileInterface
    {
        static public void RecreateAllGuids(string directoryName, bool recursive = true, HashSet<Guid> guidPool = null)
        {
            if (guidPool == null)
                guidPool = new HashSet<Guid>();

            foreach (var fileName in Directory.GetFiles(directoryName, "*.st", recursive ? SearchOption.AllDirectories : SearchOption.TopDirectoryOnly))
            {
                var fi = new FileInfo(fileName);
                if (!fi.Extension.Contains("Tc"))
                    continue;

                XDocument document = XDocument.Load(fileName);
                foreach (var node in document.Descendants().Where(x => x.Attribute("Id") != null && x.Attribute("Name") != null))
                {
                    Guid guid = Guid.NewGuid();

                    while (guidPool.Contains(guid))
                        guid = Guid.NewGuid();

                    node.Attribute("Id").Value = $"{{{guid}}}";
                    guidPool.Add(guid);
                }
                document.Save(fileName);
            }
        }

        public static void AddPlcProjInclude(XDocument plc, string plcprojPath, string relFilePath, string code)
        {
            var fi = new FileInfo(plcprojPath);
            XNamespace xmlns = "http://schemas.microsoft.com/developer/msbuild/2003";
            var itemGroup = plc.Elements(xmlns + "Project")?.Elements(xmlns + "ItemGroup")?.FirstOrDefault();

            if (itemGroup == null)
            {
                itemGroup = new XElement(xmlns + "ItemGroup");
                var project = plc.Element(xmlns + "Project");
                project.Add(itemGroup);
            }

            var compile = new XElement(xmlns + "Compile");
            compile.SetAttributeValue("Include", relFilePath);
            compile.Add(new XElement(xmlns + "SubType", "Code"));
            itemGroup.Add(compile);

            File.WriteAllText($@"{fi.Directory.FullName}\{relFilePath}", code);
        }

        public static void UpdatePlcProjInclude(XDocument plc, string plcprojPath, string relFilePath, string code)
        {
            var fi = new FileInfo(plcprojPath);
            XNamespace xmlns = "http://schemas.microsoft.com/developer/msbuild/2003";
            var found = plc.Elements(xmlns + "Project").Elements(xmlns + "ItemGroup").Elements(xmlns + "Compile").Where(x => 0 == string.Compare(x.Attribute("Include")?.Value, relFilePath, ignoreCase: true)).Any();

            if (!found)
                throw new FileNotFoundException($"{relFilePath} not found in plcproj file");

            File.WriteAllText($@"{fi.Directory.FullName}\{relFilePath}", code);
        }

        public static string CreateEmptyPlcProject(string targetPath)
        {
            var assembly = Assembly.GetExecutingAssembly();
            var resources = assembly.GetManifestResourceNames();
            var files = new[] {
                    new { filename = @"templates\ProjectTemplate\ProjectTemplate.sln", overwrite = false },
                    new { filename = @"templates\ProjectTemplate\ProjectTemplate\ProjectTemplate.tsproj", overwrite = false },
                    new { filename = @"templates\ProjectTemplate\ProjectTemplate\PlcTemplate\PlcTemplate.plcproj", overwrite = true },
                };

            foreach (var f in files)
            {
                var resourceName = $"Plaincat.{f.filename.Replace(@"\", ".")}";
                var filename = Path.Combine(targetPath, f.filename.Replace(@"templates\", ""));

                if (!resources.Contains(resourceName))
                    throw new FileNotFoundException($"Resource {resourceName} is missing in assembly");

                if (File.Exists(filename) && !f.overwrite)
                    continue;

                using (Stream stream = assembly.GetManifestResourceStream(resourceName))
                using (StreamReader reader = new StreamReader(stream))
                {
                    Directory.CreateDirectory(new FileInfo(filename).DirectoryName);
                    File.WriteAllText(filename, reader.ReadToEnd());
                }
            }

            return $@"{targetPath}\ProjectTemplate\ProjectTemplate\PlcTemplate\PlcTemplate.plcproj";
        }
    }
}
