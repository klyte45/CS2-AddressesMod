using Belzont.Utils;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BelzontAdr
{

    public class AdrNameFilesManager
    {
        public static string NamesetsFolder { get; } = Path.Combine(AddressesCs2Mod.ModSettingsRootFolder, "NamesetsFolder");
        public static AdrNameFilesManager Instance => instance ??= new();
        private static AdrNameFilesManager instance;

        internal readonly Dictionary<Colossal.Hash128, AdrNameFile> SimpleNamesFromFolder = new();

        private readonly Dictionary<string, DateTime> fileTimestampCache = new();
        public bool IsLoading { get; private set; }
        public event Action OnLoadingComplete;

        private AdrNameFilesManager()
        {
            KFileUtils.EnsureFolderCreation(NamesetsFolder);
        }

        public void ReloadNameFiles()
        {
            LoadSimpleNamesFiles(SimpleNamesFromFolder, NamesetsFolder);
        }

        public void ReloadNameFilesAsync()
        {
            if (IsLoading) return;
            IsLoading = true;
            Task.Run(() =>
            {
                try
                {
                    LoadSimpleNamesFiles(SimpleNamesFromFolder, NamesetsFolder);
                }
                finally
                {
                    IsLoading = false;
                    OnLoadingComplete?.Invoke();
                }
            });
        }

        private Dictionary<Colossal.Hash128, AdrNameFile> LoadSimpleNamesFiles(Dictionary<Colossal.Hash128, AdrNameFile> result, string path)
        {
            var filesToRemove = new HashSet<Colossal.Hash128>(result.Keys);
            foreach (string filename in Directory.GetFiles(path, "*.txt", SearchOption.AllDirectories))
            {
                var lastWrite = File.GetLastWriteTimeUtc(filename);
                if (fileTimestampCache.TryGetValue(filename, out var cachedTime) && cachedTime == lastWrite)
                {
                    var existingKey = result.Values.FirstOrDefault(f => f.FilePath == filename)?.Id;
                    if (existingKey.HasValue) filesToRemove.Remove(existingKey.Value);
                    continue;
                }
                fileTimestampCache[filename] = lastWrite;
                var name = filename.Replace(NamesetsFolder, "")[1..^4].Replace("\\", "/");
                var fileContents = File.ReadAllLines(filename, Encoding.UTF8);
                AdrNameFile file = new(name, fileContents.Select(x => x?.Trim()).Where(x => !string.IsNullOrEmpty(x)).ToArray(), filename);
                result[file.Id] = file;
                filesToRemove.Remove(file.Id);
                LogUtils.DoLog($"LOADED Files at {path} ({filename} - GUID: {file.Id}) QTT: {result[file.Id].Values.Count}");
            }
            foreach (var id in filesToRemove)
            {
                result.Remove(id);
                var pathToRemove = fileTimestampCache.Keys.FirstOrDefault(k => result.Values.Any(v => v.Id == id));
                if (pathToRemove != null) fileTimestampCache.Remove(pathToRemove);
            }
            return result;
        }
    }
}
