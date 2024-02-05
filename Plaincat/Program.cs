// See https://aka.ms/new-console-template for more information
using Plaincat;

var transcoder = new Plaincat.Transcoder.Twincat();
transcoder.Decode(@"C:\appl\Zeugwerk-Framework\ZExperimental\ZExperimental\ZExperimental.plcproj", @"generated");

transcoder.Encode(@"generated", @"generated_plc");



CommandLine.Parser.Default.ParseArguments(args, new[] { typeof(DecodeOptions), typeof(EncodeOptions) });
return 0;
/*
        .MapResult(
            (ImportOptions opts) =>
            {
                return 0;
            },
            (ExportOptions opts) =>
            {
                return 0;
            },
            errs => 1);
*/