using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using ICSharpCode.SharpZipLib.Zip;

namespace ReleaseTool
{
    internal static class ReleaseTool
    {
        private static TextWriter _originalOut;

        private static string CleanPath(string path)
        {
            return path.Trim().Replace('\\', '/').Trim('/').Trim();
        }
        public static void CopyAll(DirectoryInfo source, DirectoryInfo target)
        {
            Directory.CreateDirectory(target.FullName);

            // Copy each file into the new directory.
            foreach (FileInfo fi in source.GetFiles())
            {
                fi.CopyTo(Path.Combine(target.FullName, fi.Name), true);
            }

            // Copy each subdirectory using recursion.
            foreach (DirectoryInfo diSourceSubDir in source.GetDirectories())
            {
                DirectoryInfo nextTargetSubDir =
                    target.CreateSubdirectory(diSourceSubDir.Name);
                CopyAll(diSourceSubDir, nextTargetSubDir);
            }
        }

        private static void Main(string[] args)
        {

            try
            {
                if (args.Length != 1 || !Directory.Exists(args[0]))
                {
                    ShowInvalidArgsError();
                }
                else
                {
                    var root = new DirectoryInfo(args[0]);
                    var translationName = root.Name.Replace("-master", ""); //todo let user enter it

                    var translationDir = Path.Combine(root.FullName, "translation");
                    if (!Directory.Exists(translationDir))
                    {
                        ShowInvalidArgsError();
                    }
                    else
                    {
                        Console.ForegroundColor = ConsoleColor.Yellow;
                        Console.WriteLine("Beginning release creation!");
                        Console.ForegroundColor = ConsoleColor.White;

                        var programDir = Path.GetDirectoryName(typeof(ReleaseTool).Assembly.Location) ??
                                         Environment.CurrentDirectory;

                        var detailsPath = Path.Combine(programDir, "results.txt");

                        var outputPath = Path.Combine(programDir, translationName + "_Release_" + DateTime.Now.ToString("yyyy-MM-dd") + ".zip");
                        File.Delete(outputPath);

                        using (var output = ZipFile.Create(outputPath))
                        {
                            void AddToZip(string filePath, string entryName)
                            {
                                Console.WriteLine("Adding file: " + entryName);
                                output.Add(filePath, entryName);
                            }

                            Console.WriteLine("Writing the release to " + outputPath);

                            output.BeginUpdate(new MemoryArchiveStorage(FileUpdateMode.Direct));

                            var readmePath = Path.Combine(root.FullName, "README.md");
                            if (File.Exists(readmePath)) output.Add(readmePath, "README.md");

                            var licensePath = Path.Combine(root.FullName, "LICENSE");
                            if (File.Exists(licensePath)) output.Add(licensePath, "LICENSE");

                            var configDir = Path.Combine(root.FullName, "config");
                            if (Directory.Exists(configDir))
                            {
                                foreach (var file in Directory.GetFiles(configDir, "*", SearchOption.AllDirectories))
                                {
                                    var entryName = Path.Combine("BepInEx", CleanPath(file.Substring(root.FullName.Length)));
                                    AddToZip(file, entryName);
                                }
                            }
                            else
                            {
                                Console.WriteLine("No config directory found, skipping adding AT config file");
                            }

                            var userdataDir = Path.Combine(root.FullName, "UserData");
                            if (Directory.Exists(userdataDir) && Directory.GetFiles(userdataDir, "*", SearchOption.AllDirectories).Any())
                            {
                                foreach (var file in Directory.GetFiles(userdataDir, "*", SearchOption.AllDirectories))
                                {
                                    var entryName = CleanPath(file.Substring(root.FullName.Length));
                                    AddToZip(file, entryName);
                                }
                            }

                            var tlDir = Path.Combine(root.FullName, "Translation\\en");

                            // Make a working copy
                            Console.WriteLine("Creating a work copy of the translation files...");
                            var copyDir = new DirectoryInfo(Path.Combine(GetTempPath(), "releasetool_tl_temp"));
                            if (copyDir.Exists) copyDir.Delete(true);
                            CopyAll(new DirectoryInfo(tlDir), copyDir);

                            Console.WriteLine("Cleaning untranslated lines and files...");
                            _originalOut = Console.Out;
                            Console.SetOut(new StreamWriter(detailsPath));
                            // Clean up the copied files
                            var result = CleanTranslations(copyDir);
                            Console.Out.Flush();
                            Console.SetOut(_originalOut);

                            // Use the cleaned up copy
                            tlDir = copyDir.FullName;

                            Console.WriteLine("Writing cleaned translation files into archives...");

                            var texDir = Path.Combine(tlDir, "Texture");
                            if (Directory.Exists(texDir))
                            {
                                foreach (var subTexDir in Directory.GetDirectories(texDir))
                                {
                                    if (!Directory.GetFiles(subTexDir, "*.png", SearchOption.AllDirectories).Any())
                                        continue;

                                    var texZipPath = GetTempFileName();
                                    using (var texZipFile = ZipFile.Create(texZipPath))
                                    {
                                        texZipFile.BeginUpdate(new MemoryArchiveStorage(FileUpdateMode.Direct));

                                        foreach (var file in Directory.GetFiles(subTexDir, "*.png", SearchOption.AllDirectories))
                                        {
                                            var entryName = CleanPath(file.Substring(subTexDir.Length));
                                            //Console.WriteLine("Adding texture to texture archive: " + entryName);
                                            texZipFile.Add(file, entryName);
                                        }

                                        texZipFile.CommitUpdate();
                                    }
                                    AddToZip(texZipPath, "BepInEx\\Translation\\en\\Texture\\" + Path.GetFileName(subTexDir) + ".zip");
                                }
                            }

                            var assetDir = Path.Combine(tlDir, "RedirectedResources\\assets");
                            if (Directory.Exists(assetDir) && Directory.GetFiles(assetDir, "*.txt", SearchOption.AllDirectories).Any())
                            {
                                var assZipPath = GetTempFileName();
                                using (var assZipFile = ZipFile.Create(assZipPath))
                                {
                                    assZipFile.BeginUpdate(new MemoryArchiveStorage(FileUpdateMode.Direct));

                                    foreach (var file in Directory.GetFiles(assetDir, "*.txt", SearchOption.AllDirectories))
                                    {
                                        var entryName = CleanPath(file.Substring(assetDir.Length));
                                        //Console.WriteLine("Adding to redirected assets archive: " + entryName);
                                        assZipFile.Add(file, entryName);
                                    }

                                    assZipFile.CommitUpdate();
                                }
                                AddToZip(assZipPath, "BepInEx\\Translation\\en\\RedirectedResources\\assets\\" + translationName + "_Assets.zip");
                            }

                            var textDir = Path.Combine(tlDir, "Text");
                            if (Directory.Exists(textDir) && Directory.GetFiles(textDir, "*.txt", SearchOption.AllDirectories).Any())
                            {
                                var textZipPath = GetTempFileName();
                                using (var textZipFile = ZipFile.Create(textZipPath))
                                {
                                    textZipFile.BeginUpdate(new MemoryArchiveStorage(FileUpdateMode.Direct));

                                    foreach (var file in Directory.GetFiles(textDir, "*.txt", SearchOption.AllDirectories))
                                    {
                                        var entryName = CleanPath(file.Substring(textDir.Length));
                                        //Console.WriteLine("Adding to redirected assets archive: " + entryName);
                                        textZipFile.Add(file, entryName);
                                    }

                                    textZipFile.CommitUpdate();
                                }
                                AddToZip(textZipPath, "BepInEx\\Translation\\en\\Text\\" + translationName + "_Text.zip");
                            }

                            foreach (ZipEntry entry in output)
                            {
                                entry.CompressionMethod = CompressionMethod.Deflated;
                            }

                            Console.WriteLine("Compressing...");

                            output.CommitUpdate();

                            Console.ForegroundColor = ConsoleColor.Green;
                            Console.WriteLine($"Creating release done, {result.GetPercent() * 100:F3}% completed.");
                            Console.ForegroundColor = ConsoleColor.Gray;
                            Console.WriteLine("Detailed results were saved to " + detailsPath);

                            copyDir.Delete(true);
                        }
                    }
                }
            }
            catch (Exception e)
            {
                Console.ForegroundColor = ConsoleColor.Red;
                Console.WriteLine("Aborting release creation because of an unexpected error: " + e.Message);
            }

            Console.ForegroundColor = ConsoleColor.White;

            if (_toCleanup.Any())
            {
                Console.WriteLine("Cleaning up temporary files...");
                foreach (var info in _toCleanup)
                {
                    try { info.Delete(); }
                    catch { }
                }
            }

            Console.WriteLine("Finished! Press any key to exit.");
            Console.ReadKey(true);
        }

        private static readonly List<FileSystemInfo> _toCleanup = new List<FileSystemInfo>();
        private static string GetTempFileName()
        {
            var tempFileName = Path.GetTempFileName();
            _toCleanup.Add(new FileInfo(tempFileName));
            return tempFileName;
        }
        private static string GetTempPath()
        {
            var tempPath = Path.GetTempPath();
            _toCleanup.Add(new DirectoryInfo(tempPath));
            return tempPath;
        }

        private static void ShowInvalidArgsError()
        {
            Console.ForegroundColor = ConsoleColor.Red;
            Console.WriteLine("Error: Invalid arguments");
            Console.ForegroundColor = ConsoleColor.White;
            Console.WriteLine("Drag the repository folder (it must contain a translation folder) on to this tool's exe to generate a release. Detailed results will be saved to results.txt in the program directory.");
        }

        private static Results CleanTranslations(DirectoryInfo dir)
        {
            var files = dir.GetFiles("*.txt", SearchOption.TopDirectoryOnly);
            var folders = dir.GetDirectories();

            var folderPercentage = new Results();

            foreach (var file in files)
            {
                var lines = File.ReadAllLines(file.FullName).Where(x => !string.IsNullOrWhiteSpace(x)).ToArray();
                var translatedLines = lines.Where(x => !x.StartsWith("//")).ToArray();

                try
                {
                    file.Delete();

                    if (translatedLines.Length != 0)
                        File.WriteAllLines(file.FullName, translatedLines);
                }
                catch (Exception e)
                {
                    Console.ForegroundColor = ConsoleColor.Red;
                    _originalOut.WriteLine(e.Message);
                }

                //var percentage = (float)translatedLines.Length / (float)lines.Length;
                //Console.ForegroundColor = ConsoleColor.DarkGray;
                //Console.WriteLine($"{file.FullName} - {translatedLines.Length}/{lines.Length} lines - {100 * percentage:F0}%");

                folderPercentage.Total += lines.Length;
                folderPercentage.Translated += translatedLines.Length;
            }

            foreach (var folder in folders)
            {
                var r = CleanTranslations(folder);
                folderPercentage.Add(r);
            }

            Console.ForegroundColor = ConsoleColor.White;
            Console.WriteLine($"{dir.FullName} - {folderPercentage.Translated}/{folderPercentage.Total} lines - {100 * folderPercentage.GetPercent():F1}%");

            if (folderPercentage.GetPercent() == 0f)
            {
                try
                {
                    dir.Delete();
                }
                catch (Exception e)
                {
                    Console.ForegroundColor = ConsoleColor.Red;
                    _originalOut.WriteLine(e.Message);
                }
            }

            return folderPercentage;
        }

        private struct Results
        {
            public int Total;
            public int Translated;

            public float GetPercent()
            {
                if (Total == Translated) return 1;

                return Translated / (float)Total;
            }

            public void Add(Results toAdd)
            {
                Total += toAdd.Total;
                Translated += toAdd.Translated;
            }
        }
    }
}
