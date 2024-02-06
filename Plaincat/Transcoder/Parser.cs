using Antlr4.Runtime;
using System.Text;
using System.Text.RegularExpressions;
using Antlr4.Runtime.Tree;
using System.Xml.Linq;

namespace Plaincat.Transcoder
{
    public class Parser
    {
        public class Utils
        {
            public static string Eol = "\n";
            public static string Indents = "  ";
            private static int level;

            public static string ToPrettyTree(IParseTree t, string[] ruleNames)
            {
                level = 0;
                string lines = Regex.Replace(Process(t, ruleNames), @"(?m)^\s+$", "");
                return Regex.Replace(lines, @"\r?\n\r?\n", Eol);
            }

            public static string GetFullText(ParserRuleContext context)
            {
                if (context == null)
                    return "";

                if (context.Start == null || context.Stop == null || context.Start.StartIndex < 0 || context.Stop.StopIndex < 0)
                    return context.GetText(); // Fallback

                return context.Start.InputStream.GetText(Antlr4.Runtime.Misc.Interval.Of(context.Start.StartIndex, context.Stop.StopIndex));
            }

            public static string GetFullText(ParserRuleContext context, IToken start, IToken stop)
            {
                return context.Start.InputStream.GetText(Antlr4.Runtime.Misc.Interval.Of(start.StartIndex, stop.StopIndex));
            }

            private static string Process(IParseTree t, string[] ruleNames)
            {
                if (t == null)
                    return "";

                if (t.ChildCount == 0)
                    return Antlr4.Runtime.Misc.Utils.EscapeWhitespace(Trees.GetNodeText(t, ruleNames), false);


                StringBuilder sb = new StringBuilder();
                sb.Append(Lead(level));
                level++;
                string s = Antlr4.Runtime.Misc.Utils.EscapeWhitespace(Trees.GetNodeText(t, ruleNames), false);
                sb.Append(s + ' ');
                for (int i = 0; i < t.ChildCount; i++)
                {
                    sb.Append(Process(t.GetChild(i), ruleNames));
                }
                level--;
                sb.Append(Lead(level));
                return sb.ToString();
            }

            private static string Lead(int level)
            {
                StringBuilder sb = new StringBuilder();
                if (level > 0)
                {
                    sb.Append(Eol);
                    for (int cnt = 0; cnt < level; cnt++)
                    {
                        sb.Append(Indents);
                    }
                }
                return sb.ToString();
            }

            public static string CleanupImplementation(string impl)
            {
                return Regex.Replace(impl, @"^  ", "", RegexOptions.Multiline)
                    .Replace("END_METHOD", "")
                    .Replace("END_PROPERTY", "")
                    .Replace("END_IMPLEMENTATION", "")
                    .Replace("END_FUNCTION_BLOCK", "")
                    .Replace("END_FUNCTIONBLOCK", "")
                    .Replace("END_FUNCTION", "")
                    .Replace("END_PROGRAM", "")
                    .Replace("END_GET", "")
                    .Replace("END_SET", "")
                    .TrimEnd();
            }
        }

        /*
               <Get Name="Get" Id="{7c0b4089-1bc3-059e-27e0-90eec858d0c2}">
                <Declaration><![CDATA[VAR
        END_VAR
        ]]></Declaration>
                <Implementation>
                  <ST><![CDATA[Booted := THIS^.State >= ObjectState.Idle;]]></ST>
                </Implementation>
              </Get> 
         */

        static readonly string declRegex = @"(<(Get|Set).*?>.*?)?<Declaration><!\[CDATA\[(?<decl>.*?)\]\]></Declaration>";
        static readonly string implRegex = @"<Implementation>\s*?<ST>\s*?<!\[CDATA\[(?<impl>.*?)\]\]>\s*?</ST>\s*?</Implementation>";

        static private string CloseScope(string decl, bool getter, bool setter)
        {
            StringBuilder sb = new StringBuilder();

            if (decl != "")
            {
                var declContent = ParseContent(decl);
                switch (declContent.element)
                {
                    case 3:
                        sb.Append("END_FUNCTION\r\n");
                        break;
                    case 5:
                        sb.Append("END_FUNCTION_BLOCK\r\n");
                        break;
                    case 6:
                        sb.Append("END_PROGRAM\r\n");
                        break;
                    case 7:
                        sb.Append("END_METHOD\r\n");
                        break;
                    case 8:
                        break;
                    default:

                        if (getter)
                        {
                            sb.Append("END_GET\r\n");
                        }
                        else if (setter)
                        {
                            sb.Append("END_SET\r\n");
                        }

                        break;
                }

                sb.Append("\r\n");
            }

            return sb.ToString();
        }
        static public string ExtractCode(string filepath)
        {
            string source = File.ReadAllText(filepath);
            MatchCollection matches = Regex.Matches(source, $@"{declRegex}|{implRegex}", RegexOptions.Singleline);

            StringBuilder sb = new StringBuilder();
            var decl = "";
            bool setter = false;
            bool getter = false;
            foreach (var m in matches.Cast<Match>())
            {
                if (m.Groups["decl"].Success)
                {
                    // close previous decl
                    sb.Append(CloseScope(decl, getter, setter));
                    
                    decl = m.Groups["decl"].Value.Trim() + "\r\n"; // the line break is important for parsing

                    // quick and dirty solution to seperate getter and setter ...
                    if (m.Groups[2].Value == "Get")
                    {
                        getter = true;
                        setter = false;
                        sb.Append("GET\r\n");
                        sb.Append(decl);
                    }
                    else if (m.Groups[2].Value == "Set")
                    {
                        setter = true;
                        getter = false;
                        sb.Append("SET\r\n");
                        sb.Append(decl);
                    }
                    else
                    {
                        sb.Append(decl);
                    }
                }
                else if (m.Groups["impl"].Success)
                {
                    var impl = m.Groups["impl"].Value.Trim();

                    if (!string.IsNullOrEmpty(impl))
                    {
                        sb.Append(Regex.Replace(impl, @"^", @"  ", RegexOptions.Multiline));
                        sb.Append("\r\n");
                    }
                }
                sb.Append("\r\n");
            }

            // close previous decl
            sb.Append(CloseScope(decl, getter, setter));

            return sb.ToString();
        }
        public static Lextm.AnsiC.StParserStripped.ContentContext ParseContent(string code, string context=null)
        {
            ICharStream stream = CharStreams.fromString(code);
            ITokenSource lexer = new StLexerStripped(stream);
            ITokenStream tokens = new CommonTokenStream(lexer);
            Lextm.AnsiC.StParserStripped parser = new Lextm.AnsiC.StParserStripped(tokens);
            parser.BuildParseTree = true;

            var content = parser.content();

            if (context != null && parser.NumberOfSyntaxErrors > 0)
                throw new Exception($"Parsing failed in {context}");

            return content;
        }
    }
}
