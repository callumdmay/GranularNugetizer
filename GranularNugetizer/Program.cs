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
                Nugetizer nugetizer = new Nugetizer(options.RootFolder);
                nugetizer.Start();
            }

        }
    }


    public class Nugetizer
    {
        string rootFolder;

        string[] NuSpecFilters = { "licenseUrl", "projectUrl", "iconUrl", "releaseNotes", "copyright", "tags" };


        public Nugetizer(string pFolder)
        {
            rootFolder = pFolder;
        }

        public void Start()
        {
             System.IO.DirectoryInfo dir = new System.IO.DirectoryInfo(rootFolder);
            foreach (System.IO.DirectoryInfo subDir in dir.GetDirectories().Where(subDir => !subDir.Name.Equals("bin")))
            {
                CreateNugetSpec(subDir);
                ModifyAssemblyInfo(subDir);
            }
        }

        public void ModifyAssemblyInfo(DirectoryInfo subDir)
        {
            if (File.Exists(String.Format(@"{0}\properties\AssemblyInfo.cs", subDir.FullName, subDir.Name)))
            {
                string assemblyText = File.ReadAllText(String.Format(@"{0}\properties\AssemblyInfo.cs", subDir.FullName, subDir.Name));
                
                assemblyText = assemblyText.Replace("AssemblyDescription(\"\")", String.Format("AssemblyDescription(\"{0}\")", subDir.Name + " description required"));
                assemblyText = assemblyText.Replace("AssemblyCompany(\"\")", "AssemblyCompany(\"Replicon Inc.\")");
                
                if(!assemblyText.Contains("[assembly: AssemblyInformationalVersion(\"1.0.0.0\")]"))
                    assemblyText += "\n[assembly: AssemblyInformationalVersion(\"1.0.0.0\")]";
                
                File.WriteAllText(String.Format(@"{0}\properties\AssemblyInfo.cs", subDir.FullName, subDir.Name), assemblyText);

            }
        }

        public void CreateNugetSpec(DirectoryInfo subDir)
        {
                Console.Write("Creating nuget spec for "+subDir.Name+"...");                
                ExecCommandLine(subDir.FullName, "/C nuget spec");
                TrimLines(subDir);
                Console.WriteLine("Done");
        }

        private void TrimLines(DirectoryInfo subDir)
        {


            if (File.Exists(String.Format(@"{0}\{1}.nuspec", subDir.FullName, subDir.Name)))
            {
                var xdoc = XDocument.Parse(File.ReadAllText(String.Format(@"{0}\{1}.nuspec", subDir.FullName, subDir.Name)));

                foreach(string filter in NuSpecFilters)
                {
                    xdoc.Descendants(filter).Remove();
                }

                File.WriteAllText(String.Format(@"{0}\{1}.nuspec", subDir.FullName, subDir.Name), xdoc.ToString());
            }
        
        }



        public void ExecCommandLine(string workingDir, string arguments)
        {
            var process = new Process
            {
                StartInfo = new ProcessStartInfo("cmd.exe", arguments)
            };
            if (!string.IsNullOrEmpty(workingDir))
                process.StartInfo.WorkingDirectory = workingDir;
            process.StartInfo.UseShellExecute = false;
            process.StartInfo.CreateNoWindow = true;
            process.StartInfo.RedirectStandardError = true;
            process.StartInfo.RedirectStandardOutput = true;

            process.Start();
        }



    }


}
