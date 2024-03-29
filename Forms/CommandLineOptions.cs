﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using CommandLine;

namespace Giselle.DoujinshiDownloader
{
    public class CommandLineOptions
    {
        [Option("console", HelpText = "Show Console")]
        public bool Console { get; set; }
        [Option("multiInst", HelpText = "Run New Instance Without Mutex")]
        public bool MultiInstance { get; set; }

        [Option("language", Required = false, HelpText = "Set ui language, e.g.) en-US, ko-KR")]
        public string Language { get; set; }

    }

}
