using CommandLine;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text.Json;
using System.Timers;

namespace Plaincat
{

    [Verb("decode", HelpText = "")]
    class DecodeOptions
    {
        [Option('s', "source", Required = true, Default = "", HelpText = "")]
        public string? SourcePlcProj { get; set; }
    }

    [Verb("encode", HelpText = "")]
    class EncodeOptions
    {
        [Option('s', "target", Required = true, Default = "", HelpText = "")]
        public string? TargetPlcProj { get; set; }
    }
}

