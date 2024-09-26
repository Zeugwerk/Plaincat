using SharpCompress.Common;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Security.Cryptography;
using System.Text;
using System.Text.Json;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using System.Xml.Linq;
using Twinpack.Models;

namespace Plaincat.AutomationInterface
{
    public class FileInterface
    {
        public static XNamespace TcNs = "http://schemas.microsoft.com/developer/msbuild/2003";
        public static void RecreateAllGuids(string directoryName, bool recursive = true, HashSet<Guid> guidPool = null)
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

        public static string SeededGuid(string filename, string identifier)
        {
            return $"{{{Guid.NewGuid()}}}";
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

            File.WriteAllText(Path.Combine(fi.Directory.FullName, relFilePath), code);
        }

        public static void UpdatePlcProjInclude(XDocument plc, string plcprojPath, string relFilePath, string code)
        {
            var fi = new FileInfo(plcprojPath);
            XNamespace xmlns = "http://schemas.microsoft.com/developer/msbuild/2003";
            var found = plc.Elements(xmlns + "Project").Elements(xmlns + "ItemGroup").Elements(xmlns + "Compile").Where(x => 0 == string.Compare(x.Attribute("Include")?.Value, relFilePath, ignoreCase: true)).Any();

            if (!found)
                throw new FileNotFoundException($"{relFilePath} not found in plcproj file");

            File.WriteAllText(Path.Combine(fi.Directory.FullName, relFilePath), code);
        }

        public static string CreateEmptyPlcProject(string targetPath)
        {
            var plcName = Path.GetFileName(targetPath);
            var assembly = Assembly.GetExecutingAssembly();
            var resources = assembly.GetManifestResourceNames();
            var files = new[] {
                    new { filename = Path.Combine("templates", "ProjectTemplate", "ProjectTemplate.sln"), overwrite = false },
                    new { filename = Path.Combine("templates", "ProjectTemplate", "ProjectTemplate", "ProjectTemplate.tsproj"), overwrite = false },
                    new { filename = Path.Combine("templates", "ProjectTemplate", "ProjectTemplate", "PlcTemplate", "PlcTemplate.plcproj"), overwrite = true },
                };

            foreach (var f in files)
            {
                var resourceName = $"Plaincat.{f.filename.Replace($"{Path.DirectorySeparatorChar}", ".")}";
                var filename = Path.Combine(targetPath, f.filename.Replace(@"templates" + Path.DirectorySeparatorChar, "")
                                                                    .Replace("ProjectTemplate", plcName)
                                                                    .Replace("PlcTemplate", plcName));

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

            var plcprojPath = Path.Combine(targetPath,"ProjectTemplate","ProjectTemplate","PlcTemplate","PlcTemplate.plcproj")
                            .Replace("ProjectTemplate", plcName)
                            .Replace("PlcTemplate", plcName);

            var tspProjPath = Path.Combine(targetPath,"ProjectTemplate","ProjectTemplate","ProjectTemplate.tsproj")
                            .Replace("ProjectTemplate", plcName)
                            .Replace("PlcTemplate", plcName);

            var solutionPath = Path.Combine(targetPath,"ProjectTemplate","ProjectTemplate.sln")
                            .Replace("ProjectTemplate", plcName)
                            .Replace("PlcTemplate", plcName);

            var plc = XDocument.Load(plcprojPath);
            { // todo: properly deserialize a plcproj
                plc.Element(TcNs + "Project").Element(TcNs + "PropertyGroup").Element(TcNs + "Name").Value = plcName;
                plc.Element(TcNs + "Project").Element(TcNs + "PropertyGroup").Element(TcNs + "Title").Value = plcName;
                plc.Element(TcNs + "Project").Element(TcNs + "PropertyGroup").Element(TcNs + "DefaultNamespace").Value = plcName;
            }
            plc.Save(plcprojPath);

            File.WriteAllText(tspProjPath, File.ReadAllText(tspProjPath).Replace("ProjectTemplate", plcName).Replace("PlcTemplate", plcName));
            File.WriteAllText(solutionPath, File.ReadAllText(solutionPath).Replace("ProjectTemplate", plcName).Replace("PlcTemplate", plcName));

            return plcprojPath;
        }

        public static List<PlcLibrary> References(XDocument plc)
        {
            // collect references
            var references = new List<PlcLibrary>();
            var re = new Regex(@"(.*?),(.*?) \((.*?)\)");
            foreach (XElement g in plc.Elements(TcNs + "Project").Elements(TcNs + "ItemGroup").Elements(TcNs + "PlaceholderResolution").Elements(TcNs + "Resolution"))
            {
                var match = re.Match(g.Value);
                if (match.Success)
                    references.Add(new PlcLibrary { Name = match.Groups[1].Value.Trim(), Version = match.Groups[2].Value.Trim(), DistributorName = match.Groups[3].Value.Trim() });
            }

            foreach (XElement g in plc.Elements(TcNs + "Project").Elements(TcNs + "ItemGroup").Elements(TcNs + "PlaceholderReference").Elements(TcNs + "DefaultResolution"))
            {
                var match = re.Match(g.Value);
                if (match.Success && references.Any(x => x.Name == match.Groups[1].Value.Trim()) == false)
                    references.Add(new PlcLibrary { Name = match.Groups[1].Value.Trim(), Version = match.Groups[2].Value.Trim(), DistributorName = match.Groups[3].Value.Trim() });
            }

            re = new Regex(@"(.*?),(.*?),(.*)");
            foreach (XElement g in plc.Elements(TcNs + "Project").Elements(TcNs + "ItemGroup").Elements(TcNs + "LibraryReference"))
            {
                var libraryReference = g.Attribute("Include")?.Value?.ToString();
                if (libraryReference == null)
                    continue;

                var match = re.Match(libraryReference);
                if (match.Success)
                {
                    var ns = g.Element(TcNs + "Namespace")?.Value ?? match.Groups[1].Value.Trim();
                    references.Add(new PlcLibrary { Name = match.Groups[1].Value.Trim(), Version = match.Groups[2].Value.Trim(), DistributorName = match.Groups[3].Value.Trim(), Namespace = ns });
                }
            }

            return references;
        }

        public static void AddReferences(XDocument plc, List<PlcLibrary> references)
        {
/*
  <ItemGroup>
    <PlaceholderReference Include="ZCore">
      <DefaultResolution>ZCore, 1.5.4.1 (Zeugwerk GmbH)</DefaultResolution>
      <Namespace>ZCore</Namespace>
      <QualifiedOnly>true</QualifiedOnly>
    </PlaceholderReference>
    <PlaceholderReference Include="ZPlatform">
      <DefaultResolution>ZPlatform, 1.5.4.1 (Zeugwerk GmbH)</DefaultResolution>
      <Namespace>ZPlatform</Namespace>
      <QualifiedOnly>true</QualifiedOnly>
    </PlaceholderReference>
  </ItemGroup>
  <ItemGroup>
    <PlaceholderResolution Include="ZCore">
      <Resolution>ZCore, 1.5.4.1 (Zeugwerk GmbH)</Resolution>
    </PlaceholderResolution>
    <PlaceholderResolution Include="ZPlatform">
      <Resolution>ZPlatform, 1.5.4.1 (Zeugwerk GmbH)</Resolution>
    </PlaceholderResolution>
  </ItemGroup>
*/
            var itemGroupResolution = new XElement(TcNs + "ItemGroup");
            var itemGroupReference = new XElement(TcNs + "ItemGroup");
            var project = plc.Element(TcNs + "Project");
            
            foreach(var reference in references)
            {
                var placeholderResolution = new XElement(TcNs + "PlaceholderResolution");
                placeholderResolution.SetAttributeValue("Include", reference.Name);
                var resolution = new XElement(TcNs + "Resolution");
                resolution.Value = $@"{reference.Name}, {reference.Version} ({reference.DistributorName})";
                placeholderResolution.Add(resolution);
                itemGroupResolution.Add(placeholderResolution);

                var placeholderReference = new XElement(TcNs + "PlaceholderReference");
                placeholderReference.SetAttributeValue("Include", reference.Name);
                var defaultResolution = new XElement(TcNs + "DefaultResolution");
                var ns = new XElement(TcNs + "Namespace");
                var qualifiedOnly = new XElement(TcNs + "QualifiedOnly");

                defaultResolution.Value = resolution.Value;
                ns.Value = reference.Namespace ?? reference.Name;
                qualifiedOnly.Value = "false";

                placeholderReference.Add(defaultResolution);
                placeholderReference.Add(ns);
                placeholderReference.Add(qualifiedOnly);
                itemGroupReference.Add(placeholderReference);
            }

            project.Add(itemGroupResolution);
            project.Add(itemGroupReference);
        }
    }
}
