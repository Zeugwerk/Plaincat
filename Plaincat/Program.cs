using CommandLine;
using Plaincat;

var transcoder = new Plaincat.Transcoder.Twincat();
CommandLine.Parser.Default.ParseArguments(args, new[] { typeof(DecodeOptions), typeof(EncodeOptions) })
    .WithParsed<DecodeOptions>((opts) => transcoder.Decode(opts.SourcePlcProj, opts.TargetPath))
    .WithParsed<EncodeOptions>((opts) => transcoder.Encode(opts.SourcePath, opts.TargetPath))
    .WithNotParsed((err) => throw new Exception(err.ToString()));

return 0;