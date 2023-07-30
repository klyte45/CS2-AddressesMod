using Belzont.Utils;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;

namespace BelzontAdr
{

    public class AdrNameFilesManager
    {
        public static string SimpleNameFolder { get; } = Path.Combine(AddressesCs2Mod.ModSettingsRootFolder, "SimpleNameFiles");
        public static AdrNameFilesManager Instance => instance ??= new();
        private static AdrNameFilesManager instance;

        internal readonly Dictionary<Guid, AdrNameFile> SimpleNamesDict = new();


        private AdrNameFilesManager()
        {
            KFileUtils.EnsureFolderCreation(SimpleNameFolder);
        }

        public void ReloadNameFiles()
        {
            LoadSimpleNamesFiles(SimpleNamesDict, SimpleNameFolder);
        }

        private static Dictionary<Guid, AdrNameFile> LoadSimpleNamesFiles(Dictionary<Guid, AdrNameFile> result, string path)
        {
            result.Clear();
            foreach (string filename in Directory.GetFiles(path, "*.txt", SearchOption.AllDirectories))
            {
                var name = filename.Replace(SimpleNameFolder, "")[1..^4].Replace("\\", "/");
                var fileContents = File.ReadAllLines(filename, Encoding.UTF8);
                AdrNameFile file = new(name, fileContents.Select(x => x?.Trim()).Where(x => !string.IsNullOrEmpty(x)).ToArray());
                result[file.Id] = file;
                LogUtils.DoLog($"LOADED Files at {path} ({filename} - GUID: {file.Id}) QTT: {result[file.Id].Values.Length}");
            }
            return result;
        }
    }
}
