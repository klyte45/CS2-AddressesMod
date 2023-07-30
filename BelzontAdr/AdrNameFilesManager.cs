using Belzont.Utils;
using Colossal;
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

        private static readonly Dictionary<Guid, (string, string[])> SimpleNamesDict = new();

        private static readonly Guid baseGuid = new(214, 657, 645, 54, 54, 45, 45, 45, 45, 45, 45);

        private AdrNameFilesManager()
        {
            KFileUtils.EnsureFolderCreation(SimpleNameFolder);
        }

        public void ReloadNameFiles()
        {
            LoadSimpleNamesFiles(SimpleNamesDict, SimpleNameFolder);
        }

        private static Dictionary<Guid, (string, string[])> LoadSimpleNamesFiles(Dictionary<Guid, (string, string[])> result, string path)
        {
            result.Clear();
            foreach (string filename in Directory.GetFiles(path, "*.txt", SearchOption.AllDirectories))
            {
                var name = filename.Replace(SimpleNameFolder, "")[1..];
                Guid guid = GuidUtils.Create(baseGuid, name);
                string fileContents = File.ReadAllText(filename, Encoding.UTF8);
                result[guid] = (name, fileContents.Split(Environment.NewLine.ToCharArray()).Select(x => x?.Trim()).Where(x => !string.IsNullOrEmpty(x)).ToArray());
                LogUtils.DoLog($"LOADED Files at {path} ({filename} - GUID: {guid}) QTT: {result[guid].Item2.Length}");
            }
            return result;
        }
    }
}
