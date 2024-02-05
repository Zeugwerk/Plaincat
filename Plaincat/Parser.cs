using Antlr4.Runtime;
using System.Text;
using System.Text.RegularExpressions;
using Antlr4.Runtime.Tree;
using System.Xml.Linq;

namespace Plaincat
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
        }

        static readonly string declRegex = @"<Declaration><!\[CDATA\[(?<decl>.*?)\]\]></Declaration>";
        static readonly string implRegex = @"<Implementation>\s*?<ST>\s*?<!\[CDATA\[(?<impl>.*?)\]\]>\s*?</ST>\s*?</Implementation>";

        static public List<string> Declarations(string filepath)
        {
            string source = File.ReadAllText(filepath);
            MatchCollection matches = Regex.Matches(source, declRegex, RegexOptions.Singleline);

            return matches.Cast<Match>().Select(x => x.Groups[1].Value).ToList();
        }

        static public string ExtractDeclarations(string filepath)
        {
          return String.Join("\n\n", Declarations(filepath));
        }

        static public string ExtractImplementation(string filepath)
        {
          string source = File.ReadAllText(filepath);
          MatchCollection matches = Regex.Matches(source, implRegex, RegexOptions.Singleline);
          return String.Join("\n\n", from m in matches.Cast<Match>() select m.Groups[1].Value);
        }

        static public string ExtractCode(string filepath)
        {
            string source = File.ReadAllText(filepath);
            MatchCollection matches = Regex.Matches(source, $@"{declRegex}|{implRegex}", RegexOptions.Singleline);

            StringBuilder sb = new StringBuilder();
            foreach (var m in matches.Cast<Match>())
            {
                sb.Append(m.Groups["decl"].Value);
                sb.Append("\n\n");
                if (m.Groups["impl"].Success)
                {
                    sb.Append(m.Groups["impl"].Value);
                    sb.Append("\n\nEND_IMPLEMENTATION\n\n");
                }
            }

            return sb.ToString();
        }
        static public Lextm.AnsiC.StParserStripped Parse(string code)
        {
          ICharStream stream = CharStreams.fromString(code);
          ITokenSource lexer = new StLexerStripped(stream);
          ITokenStream tokens = new CommonTokenStream(lexer);
          Lextm.AnsiC.StParserStripped parser = new Lextm.AnsiC.StParserStripped(tokens);
          parser.BuildParseTree = true;

          return parser;
        }
    }
}
