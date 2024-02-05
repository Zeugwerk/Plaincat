using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml.Linq;
using Plaincat.AutomationInterface;

namespace Plaincat.Transcoder
{
    public class Twincat
    {
        public static XNamespace TcNs = "http://schemas.microsoft.com/developer/msbuild/2003";

        public void Decode(string sourcePlcProj, string targetPath)
        {
            var path = new FileInfo(sourcePlcProj).DirectoryName;
            XDocument xdoc = XDocument.Load(sourcePlcProj);
            foreach (var file in xdoc.Elements(TcNs + "Project")
                    .Elements(TcNs + "ItemGroup")
                    .Elements(TcNs + "Compile")
                    .Where(x => x.Elements(TcNs + "ExcludeFromBuild").Count() == 0 || x.Element(TcNs + "ExcludeFromBuild").Value == "false"))

            {
                var sourceFilePath = $@"{path}\{file.Attribute("Include").Value}";
                var targetFilePath = $@"{targetPath}\{file.Attribute("Include").Value}";
                Directory.CreateDirectory(new FileInfo(targetFilePath).Directory.FullName);
                File.WriteAllText(Path.ChangeExtension(targetFilePath, "st"), Parser.ExtractCode(sourceFilePath));
            }
        }

        public string GenerateGvl(Lextm.AnsiC.StParserStripped.Global_varContext gvl)
        {
            var name = gvl.global_var_declarations().derived_function_name();
            var decl = Parser.Utils.GetFullText(gvl);
            var sb = new StringBuilder();
            sb.Append($"""
<?xml version="1.0" encoding="utf-8"?>
<TcPlcObject Version="1.1.0.1">
  <GVL Name="{name}" Id="" ParameterList="True">
    <Declaration><![CDATA[{decl}]]></Declaration>
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
  <DUT Name="{name}" Id="">
    <Declaration><![CDATA[{decl}]]></Declaration>
  </DUT>
</TcPlcObject>
""");

            return sb.ToString();
        }

        public string GenerateFunction(Lextm.AnsiC.StParserStripped.FunctionContext pou)
        {
            var name = Parser.Utils.GetFullText(pou.function_declaration().derived_function_name());
            var decl = Parser.Utils.GetFullText(pou, pou.Start, pou.declaration() != null ? pou.declaration().Stop : pou.function_declaration().Stop);
            var impl = Parser.Utils.GetFullText(pou.implementation()).Replace("END_IMPLEMENTATION", "");

            var sb = new StringBuilder();
            sb.Append($"""
<?xml version="1.0" encoding="utf-8"?>
<TcPlcObject Version="1.1.0.1">
  <POU Name="{name}" Id="" SpecialFunc="None">
    <Declaration><![CDATA[{decl}]]></Declaration>
    <Implementation>
      <ST><![CDATA[{impl}]]></ST>
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
            Parser.Utils.GetFullText(intrf.implementation()).Replace("END_IMPLEMENTATION", "");

            var sb = new StringBuilder();
            sb.Append($"""
<?xml version="1.0" encoding="utf-8"?>
<TcPlcObject Version="1.1.0.1">
  <Itf Name="{name}" Id="" SpecialFunc="None">
    <Declaration><![CDATA[{decl}]]></Declaration>
  </Itf>
</TcPlcObject>
""");

            return sb.ToString();
        }

        public string GenerateFunctionblock(Lextm.AnsiC.StParserStripped.Function_blockContext fb)
        {
            var name = Parser.Utils.GetFullText(fb.function_block_declaration().derived_function_block_name());
            var decl = Parser.Utils.GetFullText(fb, fb.Start, fb.declaration() != null ? fb.declaration().Stop : fb.function_block_declaration().Stop);
            var impl = Parser.Utils.GetFullText(fb.implementation()).Replace("END_IMPLEMENTATION", "");

            var sb = new StringBuilder();
            sb.Append($"""
<?xml version="1.0" encoding="utf-8"?>
<TcPlcObject Version="1.1.0.1">
  <POU Name="{name}" Id="" SpecialFunc="None">
    <Declaration><![CDATA[{decl}]]></Declaration>
    <Implementation>
      <ST><![CDATA[{impl}]]></ST>
    </Implementation>
  </POU>
</TcPlcObject>
""");

            return sb.ToString();
        }

        public string GenerateProgram(Lextm.AnsiC.StParserStripped.ProgramContext prg)
        {
            var name = Parser.Utils.GetFullText(prg.program_declaration().program_type_name());
            var decl = Parser.Utils.GetFullText(prg, prg.Start, prg.declaration() != null ? prg.declaration().Stop : prg.program_declaration().Stop);
            var impl = Parser.Utils.GetFullText(prg.implementation()).Replace("END_IMPLEMENTATION", "");

            var sb = new StringBuilder();
            sb.Append($"""
<?xml version="1.0" encoding="utf-8"?>
<TcPlcObject Version="1.1.0.1">
  <POU Name="{name}" Id="" SpecialFunc="None">
    <Declaration><![CDATA[{decl}]]></Declaration>
    <Implementation>
      <ST><![CDATA[{impl}]]></ST>
    </Implementation>
  </POU>
</TcPlcObject>
""");

            return sb.ToString();
        }

        public void Encode(string sourcePath, string targetPath)
        {
            sourcePath = Path.GetFullPath(sourcePath);
            var plcProjPath = FileInterface.CreateEmptyPlcProject(targetPath);
            var plcProjDir = new FileInfo(plcProjPath).DirectoryName;
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
                        xml = GenerateGvl(content.global_var());
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
                    //case 6:
                    //    extension = "TcPOU";
                    //    xml = GeneratePrg(content.program());
                    //    break;
                    default:
                        throw new NotImplementedException("Content Element is not implemented!");
                }

                Directory.CreateDirectory(new FileInfo($@"{plcProjDir}\\{relFilepath}").DirectoryName);
                FileInterface.AddPlcProjInclude(plc, plcProjPath, Path.ChangeExtension(relFilepath, extension), xml);
            }

            plc.Save(plcProjPath);
        }
    }
}
