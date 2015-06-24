using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using CommandLine;

namespace GranularNugetizer
{
    public class CommandLineOptions
    {
        [Option('r', "root folder", HelpText = "Root folder of folders to be nugetized", Required = true)]
        public string RootFolder { get; set; }

    }
}
