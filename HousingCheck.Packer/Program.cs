using System;
using System.Diagnostics;
using System.IO;
using System.IO.Compression;
using System.Linq;
using Lotlab.PluginCommon.Updater;

namespace GardeningTracker.Packer 
{
    internal class Program
    {
        static void Main(string[] args)
        {
            var currentRoot = AppDomain.CurrentDomain.BaseDirectory;
            var root = args.Length >= 1 ? args[0] : currentRoot;

            var entry = Path.Combine(root, "HousingCheck.dll");
            var ver = FileVersionInfo.GetVersionInfo(entry).FileVersion;

            // Generate update info
            var generator = new UpdateGenerater(root, Path.Combine(root, "..", "updatev2"));
            generator.GenerateV2(ver, "");

            // Pack release file
            var packDir = Path.Combine(root, "..", "pack");
            if (!Directory.Exists(packDir))
                Directory.CreateDirectory(packDir);

            PackZip(root, Path.Combine(packDir, $"HousingCheckXP-{ver}.zip"));
        }

        static void PackZip(string rootDir, string outName)
        {
            using (var ms = new MemoryStream())
            {
                using (var archive = new ZipArchive(ms, ZipArchiveMode.Create, true))
                {
                    var files = Directory.EnumerateFiles(rootDir, "*", SearchOption.AllDirectories)
                        .Select(p => p.Replace(rootDir, ""));

                    foreach (var file in files)
                    {
                        archive.CreateEntryFromFile(Path.Combine(rootDir, file), file);
                    }
                }

                using (var fileStream = new FileStream(Path.Combine(AppDomain.CurrentDomain.BaseDirectory, outName), FileMode.Create))
                {
                    ms.Seek(0, SeekOrigin.Begin);
                    ms.CopyTo(fileStream);
                }
            }
        }
    }
}
