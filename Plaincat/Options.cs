using CommandLine;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text.Json;
using System.Timers;

namespace Plaincat
{

    [Verb("decode", HelpText = "")]
    class DecodeOptions
    {
        [Option('s', "source", Required = true, Default = "", HelpText = "")]
        public string? SourcePlcProj { get; set; }


        [Option('t', "target", Required = true, Default = "", HelpText = "")]
        public string? TargetPath { get; set; }
    }

    [Verb("encode", HelpText = "")]
    class EncodeOptions
    {
        [Option('s', "source", Required = true, Default = "", HelpText = "")]
        public string? SourcePath { get; set; }

        [Option('t', "target", Required = true, Default = "This can either be a folder or a plcproj", HelpText = "")]
        public string? TargetPath { get; set; }
    }
}

