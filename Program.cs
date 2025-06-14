using System.Diagnostics;
using System.Text.RegularExpressions;

namespace harmony_csharp_impl;

// ReSharper disable once ArrangeTypeModifiers
static partial class Program
{
    private const string HelpMessage = """
                                       BUILD.EXE [/L[INK]] [/C[LEAN]] [/R[UN]] [/H[ELP]] [/D[EBUG]] [/CDB] [/LICENSE]
                                       
                                         /L[INK]       Create symlinks to header files specified by the property.
                                         /C[LEAN]      Clean files and directories produced by the compiler.
                                         /R[UN]        Run an output executable file produced by the compiler.
                                         /H[ELP]       Display this help message and finish execution of the build.
                                         /D[EBUG]      Display debugging messages during execution of the build.
                                         /CDB          Write compilation options to `compile_flags.txt` file and exit.
                                         /LICENSE      Display the license text of this `build.bat` file and exit.
                                         
                                         
                                       .HARMONY~BUILD <COMPILER> [OPTIONS] <OUTPUT> <INCLUDE> [SOURCES] [HEADERS] [LIBRARIES] [BINARIES]
                                       
                                         COMPILER      Specifies path to an executable program used for compilation of source files.
                                         OPTIONS       Specifies paths to files containing compilation options or a set of themself.
                                         OUTPUT        Specifies path with file name to an output file produced by the compiler.
                                         INCLUDE       Specifies path to an include directory used by the link process and the flag.
                                         SOURCES       Specifies paths to source files used by the compiler to produce a final program.
                                         HEADERS       Specifies paths to header files used by the link process to create symlinks.
                                         LIBRARIES     Specifies paths to library files used by the final program as dependencies.
                                         BINARIES      Specifies paths to binary files used by the final program as dependencies.
                                       """;

    private const string LicenseText = """
                                       MIT License
                                       
                                       Copyright (c) 2024 anominy
                                       
                                       Permission is hereby granted, free of charge, to any person obtaining a copy
                                       of this software and associated documentation files (the "Software"), to deal
                                       in the Software without restriction, including without limitation the rights
                                       to use, copy, modify, merge, publish, distribute, sublicense, and/or sell
                                       copies of the Software, and to permit persons to whom the Software is
                                       furnished to do so, subject to the following conditions:
                                       
                                       The above copyright notice and this permission notice shall be included in all
                                       copies or substantial portions of the Software.
                                       
                                       THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND, EXPRESS OR
                                       IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF MERCHANTABILITY,
                                       FITNESS FOR A PARTICULAR PURPOSE AND NONINFRINGEMENT. IN NO EVENT SHALL THE
                                       AUTHORS OR COPYRIGHT HOLDERS BE LIABLE FOR ANY CLAIM, DAMAGES OR OTHER
                                       LIABILITY, WHETHER IN AN ACTION OF CONTRACT, TORT OR OTHERWISE, ARISING FROM,
                                       OUT OF OR IN CONNECTION WITH THE SOFTWARE OR THE USE OR OTHER DEALINGS IN THE
                                       SOFTWARE.
                                       """;

    private const string FlagNameLinkShort = "/l";
    private const string FlagNameCleanShort = "/c";
    private const string FlagNameRunShort = "/r";
    private const string FlagNameHelpShort = "/h";
    private const string FlagNameDebugShort = "/d";

    private const string FlagNameLinkLong = "/link";
    private const string FlagNameCleanLong = "/clean";
    private const string FlagNameRunLong = "/run";
    private const string FlagNameHelpLong = "/help";
    private const string FlagNameDebugLong = "/debug";
    private const string FlagNameCdbLong = "/cdb";
    private const string FlagNameLicenseLong = "/license";

    private const string PropertyNameCompiler = "compiler";
    private const string PropertyNameOptions = "options";
    private const string PropertyNameOutput = "output";
    private const string PropertyNameInclude = "include";
    private const string PropertyNameSources = "sources";
    private const string PropertyNameHeaders = "headers";
    private const string PropertyNameLibraries = "libraries";
    private const string PropertyNameBinaries = "binaries";

    // ReSharper disable once ArrangeTypeMemberModifiers
    static void Main(string[] args)
    {
        var isFlagLink = false;
        var isFlagClean = false;
        var isFlagRun = false;
        var isFlagHelp = false;
#pragma warning disable CS0219 // Variable is assigned but its value is never used
        var isFlagDebug = false;
#pragma warning restore CS0219 // Variable is assigned but its value is never used
        var isFlagCdb = false;
        var isFlagLicense = false;

        foreach (var arg in args)
        {
            switch (arg.ToLower())
            {
                case FlagNameLinkShort or FlagNameLinkLong:
                    isFlagLink = true;
                    break;
                case FlagNameCleanShort or FlagNameCleanLong:
                    isFlagClean = true;
                    break;
                case FlagNameRunShort or FlagNameRunLong:
                    isFlagRun = true;
                    break;
                case FlagNameHelpShort or FlagNameHelpLong:
                    isFlagHelp = true;
                    break;
                case FlagNameDebugShort or FlagNameDebugLong:
                    // ReSharper disable once RedundantAssignment
                    isFlagDebug = true;
                    break;
                case FlagNameCdbLong:
                    isFlagCdb = true;
                    break;
                case FlagNameLicenseLong:
                    isFlagLicense = true;
                    break;
            }
        }

        if (isFlagHelp)
        {
            Console.WriteLine(HelpMessage);
            return;
        }

        if (isFlagLicense)
        {
            Console.WriteLine(LicenseText);
            return;
        }

        var harmonyProperties = new Dictionary<string, string>();
        var harmonyText = File.ReadAllLines(".harmony~build");
        foreach (var line in harmonyText)
        {
            if (string.IsNullOrWhiteSpace(line)
                || line.StartsWith('#'))
            {
                continue;
            }

            var split = line.Split('=', 2);
            if (!harmonyProperties.TryAdd(split[0], ""))
            {
                harmonyProperties[split[0]] += " ";
            }

            harmonyProperties[split[0]] += split[1];
        }

        if (isFlagCdb)
        {
            var options0 = InitCompilerOptions(harmonyProperties[PropertyNameOptions]);
            File.WriteAllText("compile_flags.txt", options0.Replace(' ', '\n') + "\n");
            return;
        }

        string propertyError0 = "";
        string propertyError1 = "";

        if (string.IsNullOrWhiteSpace(harmonyProperties[PropertyNameCompiler]))
        {
            propertyError0 += $" {PropertyNameCompiler}=";
            propertyError1 += $" {new string('^', PropertyNameCompiler.Length + 1)}";
        }

        if (string.IsNullOrWhiteSpace(harmonyProperties[PropertyNameOutput]))
        {
            propertyError0 += $" {PropertyNameOutput}=";
            propertyError1 += $" {new string('^', PropertyNameOutput.Length + 1)}";
        }

        if (string.IsNullOrWhiteSpace(harmonyProperties[PropertyNameInclude]))
        {
            propertyError0 += $" {PropertyNameInclude}=";
            propertyError1 += $" {new string('^', PropertyNameInclude.Length + 1)}";
        }

        if (!string.IsNullOrWhiteSpace(propertyError0))
        {
            propertyError0 = propertyError0[1..];
            propertyError1 = propertyError1[1..];

            Console.WriteLine($"""
                               .HARMONY~BUILD @ REQUIRED PROPERTIES ARE EMPTY
                               
                                 {propertyError0}
                                 {propertyError1}
                               """);
            return;
        }

        if (isFlagClean)
        {
            var path = Path.GetDirectoryName(harmonyProperties[PropertyNameOutput]);
            if (path != null)
            {
                try
                {
                    Directory.Delete(path, true);
                }
                catch (DirectoryNotFoundException)
                {
                }
            }
        }

        if (isFlagLink)
        {
            var includeFullPath = Path.GetFullPath(harmonyProperties[PropertyNameInclude]);
            try
            {
                Directory.Delete(includeFullPath, true);
            }
            catch (DirectoryNotFoundException)
            {
            }

            var headerPaths = Regex.Matches(harmonyProperties[PropertyNameHeaders], @"[\""].+?[\""]|[^ ]+")
                .Select(m => m.Value)
                .ToList();

            foreach (var headerPath in headerPaths)
            {
                var headerFullPath = Path.GetFullPath(headerPath);

                FileAttributes headerAttr;
                try
                {
                    headerAttr = File.GetAttributes(headerFullPath);
                }
                catch (FileNotFoundException)
                {
                    continue;
                }

                if (headerAttr.HasFlag(FileAttributes.Directory))
                {
                    foreach (var headerFileFullPath in Directory.GetFiles(headerFullPath, "*.h", SearchOption.AllDirectories))
                    {
                        MakeLink(harmonyProperties[PropertyNameInclude], headerFileFullPath, headerPath);
                    }
                }
                else
                {
                    MakeLink(harmonyProperties[PropertyNameInclude], headerPath, "");
                }
            }
        }

        var options = InitCompilerOptions(harmonyProperties[PropertyNameOptions]);
        var sources = InitSourceFiles(harmonyProperties[PropertyNameSources]);
        var libraries = InitLibraryFiles(harmonyProperties[PropertyNameLibraries]);
        var output = InitCompilerOutput(harmonyProperties[PropertyNameOutput], harmonyProperties[PropertyNameBinaries]);

        var process = new Process();
        var processStartInfo = new ProcessStartInfo
        {
            WindowStyle = ProcessWindowStyle.Hidden,
            FileName = "cmd.exe",
            Arguments = $"/c {harmonyProperties[PropertyNameCompiler]} {options} {sources} {libraries} -o {output}"
        };
        process.StartInfo = processStartInfo;
        process.Start();
        process.WaitForExit();

        // ReSharper disable once InvertIf
        if (isFlagRun)
        {
            var runArgs = args.Where(arg => !arg.StartsWith('/'))
                .ToArray();

            var process0 = new Process();
            var processStartInfo0 = new ProcessStartInfo
            {
                WindowStyle = ProcessWindowStyle.Hidden,
                FileName = "cmd.exe",
                Arguments = $"/c {output} {string.Join(' ', runArgs)}"
            };
            process0.StartInfo = processStartInfo0;
            process0.Start();
            process0.WaitForExit();
        }
    }

    private static string InitCompilerOptions(string text)
    {
        var options = ArgumentRegex()
            .Matches(text)
            .Select(m => m.Value)
            .ToArray();

        var result = new List<string>();
        foreach (var option in options)
        {
            try
            {
                var attr = File.GetAttributes(option);
                if (!attr.HasFlag(FileAttributes.Directory))
                {
                    result.AddRange(File.ReadAllLines(option));
                }
            }
            catch (FileNotFoundException)
            {
                result.Add(option);
            }
        }

        return string.Join(' ', result);
    }

    private static string InitSourceFiles(string text)
    {
        var sourcePaths = ArgumentRegex()
            .Matches(text)
            .Select(m => m.Value)
            .ToArray();

        var result = new List<string>();
        foreach (var sourcePath in sourcePaths)
        {
            try
            {
                var sourceAttr = File.GetAttributes(sourcePath);
                if (sourceAttr.HasFlag(FileAttributes.Directory))
                {
                    result.AddRange(Directory.GetFiles(sourcePath, "*.c", SearchOption.AllDirectories));
                }
                else
                {
                    result.Add(sourcePath);
                }
            }
            catch (FileNotFoundException)
            {
            }
        }

        return result.Count == 0 ? "" : $"\"{string.Join("\" \"", result)}\"";
    }

    private static string InitLibraryFiles(string text)
    {
        var libraryPaths = ArgumentRegex()
            .Matches(text)
            .Select(m => m.Value)
            .ToArray();

        var result = new List<string>();
        foreach (var libraryPath in libraryPaths)
        {
            try
            {
                var libraryAttr = File.GetAttributes(libraryPath);
                if (libraryAttr.HasFlag(FileAttributes.Directory))
                {
                    result.AddRange(Directory.GetFiles(libraryPath, "*.lib", SearchOption.AllDirectories));
                }
                else
                {
                    result.Add(libraryPath);
                }
            }
            catch (FileNotFoundException)
            {
                result.Add(libraryPath);
            }
        }

        return result.Count == 0 ? "" : $"\"{string.Join("\" \"", result)}\"";
    }

    private static string InitCompilerOutput(string output, string binaries)
    {
        var outputPath = Path.GetDirectoryName(output);
        var executableName = Path.GetFileName(output);

        if (string.IsNullOrEmpty(outputPath))
        {
            return executableName;
        }

        Directory.CreateDirectory(outputPath);

        var binaryPaths = ArgumentRegex()
            .Matches(binaries)
            .Select(m => m.Value)
            .ToArray();

        foreach (var binaryPath in binaryPaths)
        {
            try
            {
                var binaryAttr = File.GetAttributes(binaryPath);
                if (binaryAttr.HasFlag(FileAttributes.Directory))
                {
                    foreach (var binaryFileFullPath in Directory.GetFiles(binaryPath, "*.dll", SearchOption.AllDirectories))
                    {
                        File.Copy(binaryFileFullPath, $"{outputPath}{Path.DirectorySeparatorChar}{Path.GetFileName(binaryFileFullPath)}", true);
                    }
                }
                else
                {
                    File.Copy(binaryPath, $"{outputPath}{Path.DirectorySeparatorChar}{Path.GetFileName(binaryPath)}", true);
                }
            }
            catch (FileNotFoundException)
            {
            }
        }

        var result = $"{outputPath}{Path.DirectorySeparatorChar}{executableName}";
        using (File.Create($"{result}.local"));
        return $"\"{result}\"";
    }

    private static void MakeLink(string includePath, string fileFullPath, string baseFilePath)
    {
        var relativePath = fileFullPath.Replace($"{Environment.CurrentDirectory}{Path.DirectorySeparatorChar}{baseFilePath}{Path.DirectorySeparatorChar}", "");

        var separatorCount = relativePath.Count(c => c == Path.DirectorySeparatorChar);
        separatorCount += includePath.Count(c => c == Path.DirectorySeparatorChar) + 1;

        var multPath = string.Concat(Enumerable.Repeat($"..{Path.DirectorySeparatorChar}", separatorCount));
        var linkPath = $"{Path.GetFullPath(includePath)}{Path.DirectorySeparatorChar}{relativePath}";
        var targetPath = baseFilePath.Length == 0 ? $"{multPath}{relativePath}" : $"{multPath}{baseFilePath}{Path.DirectorySeparatorChar}{relativePath}";

        var directoryPath = Path.GetDirectoryName(linkPath);
        if (!string.IsNullOrEmpty(directoryPath))
        {
            Directory.CreateDirectory(directoryPath);
        }
        File.CreateSymbolicLink(linkPath, targetPath);
    }

    [GeneratedRegex(@"[\""].+?[\""]|[^ ]+")]
    private static partial Regex ArgumentRegex();
}
