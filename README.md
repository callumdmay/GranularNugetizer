# GranularNugetizer

Use the test.bat to create the nuspec file(s). The -r argument is the working directory. Set it to one level above one or more folders containing .csproj files. The program will move through each sub folder one level below the specified folder and attempt a nuget spec. Then as a test, it will move through each folder again attempting to generate a package from the .nuspec.
