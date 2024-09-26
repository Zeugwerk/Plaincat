using CommandLine;
using Plaincat;
var transcoder = new Plaincat.Transcoder.Twincat();

Parser.Default.ParseArguments(args, new[] { typeof(DecodeOptions), typeof(EncodeOptions), typeof(ReencodeOptions) })
    .WithParsed<DecodeOptions>((opts) => transcoder.Decode(opts.SourcePlcProj, opts.TargetPath))
    .WithParsed<EncodeOptions>((opts) => transcoder.Encode(opts.SourcePath, opts.TargetPath))
    .WithParsed<ReencodeOptions>((opts) =>
    {
        transcoder.Decode(opts.SourcePlcProj, opts.IntermediatePath);
        transcoder.Encode(opts.IntermediatePath, opts.TargetPath);
    })
    .WithNotParsed((err) => throw new Exception(string.Join(',', err.Select(x => x.ToString()))));

return 0;