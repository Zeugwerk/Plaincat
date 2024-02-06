using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Security.AccessControl;
using System.Text;
using System.Text.Json;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using System.Xml.Linq;
using Microsoft.Build.Utilities;
using Plaincat.AutomationInterface;
using SharpCompress.Common;
using Twinpack.Models;

namespace Plaincat.Transcoder
{
    public class Twincat
    {
        public static XNamespace TcNs = "http://schemas.microsoft.com/developer/msbuild/2003";

        public void Decode(string sourcePlcProj, string targetPath)
        {
            if (!File.Exists(sourcePlcProj))
                throw new FileNotFoundException($"File {sourcePlcProj} could not be found");

            var filename = Path.GetFileName(sourcePlcProj);

            if(Directory.Exists(targetPath))
            {
                var stFiles = Directory.EnumerateFiles(targetPath, "*.st", SearchOption.AllDirectories);
                var otherFiles = Directory.EnumerateFiles(targetPath, "*", SearchOption.AllDirectories);
                if (stFiles.Count() + 1 < otherFiles.Count())
                    throw new FileNotFoundException($"Folder {targetPath} already exists and contains non-st files! Use a clean folder");
            }

            var path = new FileInfo(sourcePlcProj).DirectoryName;
            XDocument plc = XDocument.Load(sourcePlcProj);
            foreach (var file in plc.Elements(TcNs + "Project")
                    .Elements(TcNs + "ItemGroup")
                    .Elements(TcNs + "Compile")
                    .Where(x => x.Elements(TcNs + "ExcludeFromBuild").Count() == 0 || x.Element(TcNs + "ExcludeFromBuild").Value == "false"))

            {
                var sourceFilePath = $@"{path}\{file.Attribute("Include").Value}";
                var folders = file.Attribute("Include").Value.Split(@"\").ToList();
                var exclude = false;
                while (folders.Count() > 1)
                {
                    exclude |= plc.Elements(TcNs + "Project")?
                        .Elements(TcNs + "ItemGroup")?
                        .Elements(TcNs + "Folder")?
                        .Where(x => x.Attribute("Include")?.Value == string.Join('\\', folders))?
                            .Elements(TcNs + "ExcludeFromBuild")?
                            .Where(x => x.Value == "true")?.Any() == true;

                    folders = folders.Take(folders.Count() - 1).ToList();
                }

                if (exclude)
                    continue;

                var targetFilePath = $@"{targetPath}\{file.Attribute("Include").Value}";
                Directory.CreateDirectory(new FileInfo(targetFilePath).Directory.FullName);
                File.WriteAllText(Path.ChangeExtension(targetFilePath, "st"), Parser.ExtractCode(sourceFilePath));
            }

            File.WriteAllText($@"{targetPath}\references.json", JsonSerializer.Serialize(FileInterface.References(plc), new JsonSerializerOptions { WriteIndented = true }));
        }

        public string GenerateMethods(string objectName, Lextm.AnsiC.StParserStripped.MethodContext[] methods)
        {
            var sb = new StringBuilder();
            foreach (var method in methods)
            {
                var name = Parser.Utils.GetFullText(method.method_declaration().derived_function_name());
                var decl = Parser.Utils.GetFullText(method, method.Start, method.declaration() != null ? method.declaration().Stop : method.method_declaration().Stop);
                var impl = Parser.Utils.CleanupImplementation(Parser.Utils.GetFullText(method.implementation()));

                sb.Append($"""
    <Method Name="{name}" Id="{FileInterface.SeededGuid(objectName, name)}">
      <Declaration><![CDATA[{decl.TrimEnd()}]]></Declaration>
      <Implementation>
        <ST><![CDATA[{impl.TrimEnd()}]]></ST>
      </Implementation>
    </Method>

""");
            }

            return sb.ToString();
        }

        public string GenerateProperties(string objectName, Lextm.AnsiC.StParserStripped.PropertyContext[] properties)
        {
            var sb = new StringBuilder();
            foreach (var property in properties)
            {
                var name = Parser.Utils.GetFullText(property.property_declaration().derived_function_name());
                var decl = Parser.Utils.GetFullText(property, property.Start, property.property_declaration().Stop);

                var sbaccessors = new StringBuilder();
                bool getter = false;
                foreach (var accessor in property.property_accessor())
                {
                    getter = Regex.IsMatch(Parser.Utils.GetFullText(accessor.implementation()), $@"{name}\s+(REF|:)=");

                    sbaccessors.Append($"""
      <{(!getter ? "Set" : "Get")} Name="{(!getter ? "Set" : "Get")}" Id="{FileInterface.SeededGuid(objectName, name + (!getter ? "#Set" : "#Get"))}">
        <Declaration><![CDATA[{Parser.Utils.GetFullText(accessor.declaration()).TrimEnd()}]]></Declaration>
        <Implementation>
          <ST><![CDATA[{Parser.Utils.CleanupImplementation(Parser.Utils.GetFullText(accessor.implementation()))}]]></ST>
        </Implementation>
      </{(!getter ? "Set" : "Get")}>

""");
                }

                // todo: maybe it is just a parse error that this can be 0
                if(property.property_accessor().Length == 0)
                {
                    sbaccessors.Append($"""
      <Get Name="Get" Id="{FileInterface.SeededGuid(objectName, name + (!getter ? "#Set" : "#Get"))}">
        <Declaration><![CDATA[]]></Declaration>
      </Get>
""");
                }


                sb.Append($"""
    <Property Name="{name}" Id="{FileInterface.SeededGuid(objectName, name)}">
      <Declaration><![CDATA[{decl.TrimEnd()}]]></Declaration>
{sbaccessors.ToString().TrimEnd()}
    </Property>

""");
            }

            return sb.ToString();
        }

        public string GenerateGvl(string name, Lextm.AnsiC.StParserStripped.Global_varContext gvl)
        {
            var decl = Parser.Utils.GetFullText(gvl);
            var sb = new StringBuilder();
            sb.Append($"""
<?xml version="1.0" encoding="utf-8"?>
<TcPlcObject Version="1.1.0.1">
  <GVL Name="{name}" Id="{FileInterface.SeededGuid(name, "")}" ParameterList="True">
    <Declaration><![CDATA[{decl.TrimEnd()}]]></Declaration>
  </GVL>
</TcPlcObject>
""");

            return sb.ToString();
        }

        public string GenerateDatatype(Lextm.AnsiC.StParserStripped.Data_typeContext dut)
        {
            var name = Parser.Utils.GetFullText(dut.data_type_declaration().data_type_name());
            var decl = Parser.Utils.GetFullText(dut);
            var sb = new StringBuilder();
            sb.Append($"""
<?xml version="1.0" encoding="utf-8"?>
<TcPlcObject Version="1.1.0.1">
  <DUT Name="{name}" Id="{FileInterface.SeededGuid(name, "")}">
    <Declaration><![CDATA[{decl.TrimEnd()}]]></Declaration>
  </DUT>
</TcPlcObject>
""");

            return sb.ToString();
        }

        public string GenerateFunction(Lextm.AnsiC.StParserStripped.FunctionContext pou)
        {
            var name = Parser.Utils.GetFullText(pou.function_declaration().derived_function_name());
            var decl = Parser.Utils.GetFullText(pou, pou.Start, pou.declaration() != null ? pou.declaration().Stop : pou.function_declaration().Stop);
            var impl = Parser.Utils.CleanupImplementation(Parser.Utils.GetFullText(pou.implementation()));

            var sb = new StringBuilder();
            sb.Append($"""
<?xml version="1.0" encoding="utf-8"?>
<TcPlcObject Version="1.1.0.1">
  <POU Name="{name}" Id="{FileInterface.SeededGuid(name, "")}" SpecialFunc="None">
    <Declaration><![CDATA[{decl.TrimEnd()}]]></Declaration>
    <Implementation>
      <ST><![CDATA[{impl.TrimEnd()}]]></ST>
    </Implementation>
  </POU>
</TcPlcObject>
""");

            return sb.ToString();
        }

        public string GenerateInterface(Lextm.AnsiC.StParserStripped.InterfaceContext intrf)
        {
            var name = Parser.Utils.GetFullText(intrf.interface_declaration().derived_function_block_name());
            var decl = Parser.Utils.GetFullText(intrf, intrf.Start, intrf.declaration() != null ? intrf.declaration().Stop : intrf.interface_declaration().Stop);
            var properties = GenerateProperties(name, intrf.property());
            var methods = GenerateMethods(name, intrf.method());

            var sb = new StringBuilder();
            sb.Append($"""
<?xml version="1.0" encoding="utf-8"?>
<TcPlcObject Version="1.1.0.1">
  <Itf Name="{name}" Id="{FileInterface.SeededGuid(name, "")}" SpecialFunc="None">
    <Declaration><![CDATA[{decl.TrimEnd()}]]></Declaration>
{properties}{methods}
  </Itf>
</TcPlcObject>
""");

            return sb.ToString();
        }

        public string GenerateFunctionblock(Lextm.AnsiC.StParserStripped.Function_blockContext fb)
        {
            var name = Parser.Utils.GetFullText(fb.function_block_declaration().derived_function_block_name());
            var decl = Parser.Utils.GetFullText(fb, fb.Start, fb.declaration() != null ? fb.declaration().Stop : fb.function_block_declaration().Stop);
            var impl = Parser.Utils.CleanupImplementation(Parser.Utils.GetFullText(fb.implementation()));
            var properties = GenerateProperties(name, fb.property());
            var methods = GenerateMethods(name, fb.method());

            var sb = new StringBuilder();
            sb.Append($"""
<?xml version="1.0" encoding="utf-8"?>
<TcPlcObject Version="1.1.0.1">
  <POU Name="{name}" Id="{FileInterface.SeededGuid(name, "")}" SpecialFunc="None">
    <Declaration><![CDATA[{decl.TrimEnd()}]]></Declaration>
    <Implementation>
      <ST><![CDATA[{impl.TrimEnd()}]]></ST>
    </Implementation>
{properties}{methods}
  </POU>
</TcPlcObject>
""");

            return sb.ToString();
        }

        public string GenerateProgram(Lextm.AnsiC.StParserStripped.ProgramContext prg)
        {
            var name = Parser.Utils.GetFullText(prg.program_declaration().program_type_name());
            var decl = Parser.Utils.GetFullText(prg, prg.Start, prg.declaration() != null ? prg.declaration().Stop : prg.program_declaration().Stop);
            var impl = Parser.Utils.CleanupImplementation(Parser.Utils.GetFullText(prg.implementation()));
            var properties = GenerateProperties(name, prg.property());
            var methods = GenerateMethods(name, prg.method());

            var sb = new StringBuilder();
            sb.Append($"""
<?xml version="1.0" encoding="utf-8"?>
<TcPlcObject Version="1.1.0.1">
  <POU Name="{name}" Id="{FileInterface.SeededGuid(name, "")}" SpecialFunc="None">
    <Declaration><![CDATA[{decl.TrimEnd()}]]></Declaration>
    <Implementation>
      <ST><![CDATA[{impl.TrimEnd()}]]></ST>
    </Implementation>
{properties}{methods}
  </POU>
</TcPlcObject>
""");
            return sb.ToString();
        }
        public void Encode(string sourcePath, string targetPath)
        {
            if (!Directory.Exists(sourcePath))
                throw new DirectoryNotFoundException($"Folder {sourcePath} could not be found");

            if (File.Exists(targetPath))
                throw new FileLoadException($"Plcproj {targetPath} already exists");

            if(Directory.Exists(targetPath))
                Directory.Delete(targetPath, true);

            sourcePath = Path.GetFullPath(sourcePath);
            var plcProjPath = FileInterface.CreateEmptyPlcProject(targetPath);
            var plcFileInfo = new FileInfo(plcProjPath);
            var plcProjDir = plcFileInfo.DirectoryName;
            var plc = XDocument.Load(plcProjPath);

            foreach (var file in Directory.GetFileSystemEntries(sourcePath, "*.st", SearchOption.AllDirectories))
            {
                var relFilepath = file.Substring(sourcePath.Length + 1);
                var parsed = Parser.Parse(File.ReadAllText(file));
                var content = parsed.content();
                string extension;
                string xml;
                switch (content.element)
                {
                    case 1:
                        extension = "TcGVL";
                        xml = GenerateGvl(Path.GetFileName(file).Replace(".st", ""), content.global_var());
                        break;
                    case 2:
                        extension = "TcDUT";
                        xml = GenerateDatatype(content.data_type());
                        break;
                    case 3:
                        extension = "TcPOU";
                        xml = GenerateFunction(content.function());
                        break;
                    case 4:
                        extension = "TcIO";
                        xml = GenerateInterface(content.@interface());
                        break;
                    case 5:
                        extension = "TcPOU";
                        xml = GenerateFunctionblock(content.function_block());
                        break;
                    case 6:
                        extension = "TcPOU";
                        xml = GenerateProgram(content.program());
                        break;
                    default:
                        throw new NotImplementedException("Content Element is not implemented!");
                }

                Directory.CreateDirectory(new FileInfo($@"{plcProjDir}\\{relFilepath}").DirectoryName);
                FileInterface.AddPlcProjInclude(plc, plcProjPath, Path.ChangeExtension(relFilepath, extension), xml);
            }

            FileInterface.AddReferences(plc, JsonSerializer.Deserialize<List<PlcLibrary>>(File.ReadAllText($@"{sourcePath}\references.json")));
            plc.Save(plcProjPath);
        }
    }
}
