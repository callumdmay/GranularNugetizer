using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using CommandLine.Text;
using CommandLine;
using System.IO;
using System.Xml.Linq;
using System.Diagnostics;


namespace GranularNugetizer
{
    public class Program
    {
        static void Main(string[] args)
        {
            var options = new CommandLineOptions();

            if (args == null || !CommandLine.Parser.Default.ParseArguments(args, options))
            {
                Console.WriteLine(HelpText.AutoBuild(options));
                return;
            }

            if (Parser.Default.ParseArguments(args, options))
            {
                Nugetizer nugetizer;
                if (options.Overwrite != null)
                {
                    nugetizer = new Nugetizer(options.RootFolder, bool.Parse(options.Overwrite));
                }
                else
                {
                    nugetizer = new Nugetizer(options.RootFolder);
                }
                
                nugetizer.StartNugetizing();
                nugetizer.TestNugetizing();
            }

        }
    }

}