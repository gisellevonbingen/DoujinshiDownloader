using CommandLine;
using CommandLine.Text;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Giselle.DoujinshiDownloader
{
    public class CommandLineOptions
    {
        [Option("language", Required = false, HelpText = "Set ui language")]
        public string Language { get; set; }

    }

}
