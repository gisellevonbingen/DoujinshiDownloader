using CommandLine;
using CommandLine.Text;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace Giselle.DoujinshiDownloader
{
    public class CommandLineOptions
    {
        [Option("console", HelpText = "Show Console")]
        public bool Console { get; set; }
        [Option("multiInst", HelpText = "Run New Instance Without Mutex")]
        public bool MultiInstance { get; set; }

        [Option("language", Required = false, HelpText = "Set ui language, ex.) en-US, ko-KR")]
        public string Language { get; set; }

    }

}
