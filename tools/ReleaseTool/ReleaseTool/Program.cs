using System;
using System.IO;
using System.Linq;

namespace ReleaseTool
{
    internal static class ReleaseTool
    {
        private static TextWriter _originalOut;

        private static void Main(string[] args)
        {
            var fgc = Console.ForegroundColor;
            if (args.Length != 1 || !Directory.Exists(args[0]) || !args[0].Replace('/', '\\').TrimEnd('\\').EndsWith("\\translation", StringComparison.OrdinalIgnoreCase))
            {
                Console.ForegroundColor = ConsoleColor.Red;
                Console.WriteLine("Error: Invalid arguments");
                Console.ForegroundColor = fgc;
                Console.WriteLine("Drag the translation folder from the repository on to this tool's exe to generate a release. Detailed results will be saved to results.txt in the program directory.");
            }
            else
            {
                var root = new DirectoryInfo(args[0]);
                if (root.Exists)
                {
                    Console.ForegroundColor = ConsoleColor.Yellow;
                    Console.WriteLine("Beginning release creation!");

                    _originalOut = Console.Out;
                    var detailsPath = Path.Combine(Environment.CurrentDirectory, "results.txt");
                    Console.SetOut(new StreamWriter(detailsPath));

                    var result = ScanResursively(root);

                    Console.Out.Flush();
                    Console.SetOut(_originalOut);

                    Console.ForegroundColor = ConsoleColor.Green;
                    Console.WriteLine($"Creating release done, {result.GetPercent() * 100:F3}% completed.");
                    Console.ForegroundColor = ConsoleColor.Gray;
                    Console.WriteLine("Detailed results were saved to " + detailsPath);
                }
            }

            Console.ForegroundColor = fgc;
            Console.WriteLine("Press any key to exit...");
            Console.ReadKey(true);
        }

        private static Results ScanResursively(DirectoryInfo dir)
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
                var r = ScanResursively(folder);
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
