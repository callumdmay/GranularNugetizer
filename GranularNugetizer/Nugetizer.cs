using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml.Linq;

namespace GranularNugetizer
{
    public class Nugetizer
    {
        string rootFolder;

        string[] NuSpecFilters = { "licenseUrl", "projectUrl", "iconUrl", "releaseNotes", "copyright", "tags" };

        List<string> FailedDirectories = new List<string>();

        bool? OverWriteNuSpec= null;

        public Nugetizer(string pFolder)
        {
            rootFolder = pFolder;
        }

        public Nugetizer (string pFolder, bool pOverWriteNuSpec)
        {
            rootFolder = pFolder;
            OverWriteNuSpec = pOverWriteNuSpec;
        }

        public void StartNugetizing()
        {
            System.IO.DirectoryInfo dir = new System.IO.DirectoryInfo(rootFolder);
            foreach (System.IO.DirectoryInfo subDir in dir.GetDirectories().Where(subDir => !subDir.Name.Equals("bin")))
            {
                Console.WriteLine("Nugetizing directory: " + subDir.Name + "...");
                CreateNugetSpec(subDir);
                ModifyAssemblyInfo(subDir);
                Console.WriteLine("Finished\n");
            }
            Console.WriteLine();
            Console.WriteLine("**************************************");
            Console.WriteLine();
        }


        public void TestNugetizing()
        {
            System.IO.DirectoryInfo dir = new System.IO.DirectoryInfo(rootFolder);
            foreach (System.IO.DirectoryInfo subDir in dir.GetDirectories().Where(subDir => !subDir.Name.Equals("bin")))
            {
                Console.Write("Testing package creation of directory: " + subDir.Name + "...");
                CreateNugetPackage(subDir);
                CheckNugetPackageCreation(subDir);
                Console.WriteLine("Done\n");
            }

            CheckFailedDirectories();
        }


        private void CheckFailedDirectories()
        {
            if (FailedDirectories.Count() > 0)
            {
                Console.WriteLine("The following directories failed to nugetize:");
                foreach (string path in FailedDirectories)
                {
                    Console.WriteLine(path);
                }
                Console.WriteLine();
                Console.WriteLine("did you build the project/workspace before execution?");
            }
        }


        public void CreateNugetSpec(DirectoryInfo subDir)
        {
            
            if (OverWriteNuSpec != null)
            {
                if (OverWriteNuSpec == true)
                {
                    Console.Write("\tCreating nuget spec with overwrite enabled" + "...");
                    ExecCommandLine(subDir.FullName, "/C nuget spec -f");
                }
                else
                {
                    Console.Write("\tCreating nuget spec with overwrite disabled" + "...");
                    ExecCommandLine(subDir.FullName, "/C nuget spec ");
                }
            }
            else
            {
                Console.Write("\tCreating nuget spec with overwrite disabled" + "...");
                ExecCommandLine(subDir.FullName, "/C nuget spec");
            }

            Console.WriteLine();
            TrimLines(subDir);
            Console.WriteLine("Done");
        }

        private void TrimLines(DirectoryInfo subDir)
        {


            if (File.Exists(String.Format(@"{0}\{1}.nuspec", subDir.FullName, subDir.Name)))
            {
                Console.Write("\t\tTrimming lines from nuspec"+"...");
                var xdoc = XDocument.Parse(File.ReadAllText(String.Format(@"{0}\{1}.nuspec", subDir.FullName, subDir.Name)));

                foreach (string filter in NuSpecFilters)
                {
                    xdoc.Descendants(filter).Remove();
                }

                File.WriteAllText(String.Format(@"{0}\{1}.nuspec", subDir.FullName, subDir.Name), ToStringWithDeclaration(xdoc));
            }
            else
            {
                throw new FileNotFoundException("The nuget spec could not be found during the trim files operation");
            }

        }

        private string ToStringWithDeclaration(XDocument doc)
        {
            if (doc == null)
            {
                throw new ArgumentNullException("doc");
            }
            StringBuilder builder = new StringBuilder();
            using (TextWriter writer = new StringWriter(builder))
            {
                doc.Save(writer);
            }
            return builder.ToString();
        }


        public void ModifyAssemblyInfo(DirectoryInfo subDir)
        {
            Console.Write(String.Format("\tModifying AssemblyInfo.cs... "));

            if (File.Exists(String.Format(@"{0}\properties\AssemblyInfo.cs", subDir.FullName, subDir.Name)))
            {
                string assemblyText = File.ReadAllText(String.Format(@"{0}\properties\AssemblyInfo.cs", subDir.FullName, subDir.Name));

                assemblyText = assemblyText.Replace("AssemblyDescription(\"\")", String.Format("AssemblyDescription(\"{0}\")", subDir.Name + " description required"));
                assemblyText = assemblyText.Replace("AssemblyCompany(\"\")", "AssemblyCompany(\"Replicon Inc.\")");

                if (!assemblyText.Contains("[assembly: AssemblyInformationalVersion(\"1.0.0.0\")]"))
                    assemblyText += "\n[assembly: AssemblyInformationalVersion(\"1.0.0.0\")]";

                File.WriteAllText(String.Format(@"{0}\properties\AssemblyInfo.cs", subDir.FullName, subDir.Name), assemblyText);

            }

            Console.WriteLine("Done");
        }


        public void CreateNugetPackage(DirectoryInfo subDir)
        {
            ExecCommandLine(subDir.FullName, "/C nuget pack -IncludeReferencedProjects -Prop Platform=AnyCPU");
        }


        public void CheckNugetPackageCreation(DirectoryInfo subDir)
        {
            if (Directory.GetFiles(subDir.FullName, subDir.Name + "*.nupkg").Length !=0 )
            {
                foreach (string path in Directory.GetFiles(subDir.FullName, subDir.Name + "*.nupkg"))
                {
                    File.Delete(path);
                }
            }
            else
            {
                FailedDirectories.Add(subDir.FullName);
                foreach (string path in Directory.GetFiles(subDir.FullName, subDir.Name + ".nuspec"))
                {
                    File.Delete(path);
                }
            }
        }



        public static string ExecCommandLine(string workingDir, string arguments)
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

            process.BeginOutputReadLine();
            string output;
            string consoleOutput = "";
            while ((output = process.StandardError.ReadLine()) != null)
            {
                consoleOutput += output;
            }
            if(!consoleOutput.Equals(""))
                Console.WriteLine("\t"+consoleOutput);
            process.WaitForExit();
            return consoleOutput;
        }



    }
}
