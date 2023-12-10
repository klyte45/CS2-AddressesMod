using Belzont.Interfaces;
using Belzont.Serialization;
using Belzont.Utils;
using Colossal;
using Colossal.OdinSerializer.Utilities;
using Colossal.Serialization.Entities;
using Game;
using MonoMod.Utils;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Net;
using System.Text;
using System.Xml.Serialization;
using Unity.Collections;
using Unity.Jobs;
using UnityEngine;
using static BelzontAdr.AdrNameFile;

namespace BelzontAdr
{
    public class AdrNamesetSystem : GameSystemBase, IBelzontBindable, IBelzontSerializableSingleton<AdrNamesetSystem>
    {
        const int CURRENT_VERSION = 0;

        private AdrMainSystem mainSystem;

        #region UI Bindings
        public void SetupCallBinder(Action<string, Delegate> eventCaller)
        {
            eventCaller("namesets.listCityNamesets", ListCityNamesets);
            eventCaller("namesets.listLibraryNamesets", ListLibraryNamesets);
            eventCaller("namesets.addNamesetToCity", AddCityNameset);
            eventCaller("namesets.deleteFromCity", DeleteCityNameset);
            eventCaller("namesets.updateForCity", UpdateCityNameset);
            eventCaller("namesets.exportToLibrary", ExportToLibrary);
            eventCaller("namesets.reloadLibraryNamesets", () =>
            {
                AdrNameFilesManager.Instance.ReloadNameFiles();
                return AdrNameFilesManager.Instance.SimpleNamesFromFolder.Values.ToArray();
            });
            eventCaller("namesets.goToSimpleNamesFolder", () => { RemoteProcess.OpenFolder(AdrNameFilesManager.NamesetsFolder); });
            eventCaller("namesets.sortValues", SortValues);
            eventCaller("namesets.goToGitHubRepo", () => { Application.OpenURL("https://github.com/klyte45/AddressesFiles"); });
        }

        private Action<string, object[]> eventCaller; 
        public void SetupCaller(Action<string, object[]> eventCaller)
        {
            this.eventCaller = eventCaller;
        }

        public void SetupEventBinder(Action<string, Delegate> eventCaller)
        {
        }
        #endregion

        protected override void OnCreate()
        {
            base.OnCreate();
            mainSystem = World.GetOrCreateSystemManaged<AdrMainSystem>();
        }

        protected override void OnUpdate()
        {

        }
        private readonly Dictionary<Guid, AdrNameFile> CityNamesets = new();

        internal bool GetForGuid(Guid guid, out AdrNameFile file) => CityNamesets.TryGetValue(guid, out file);
        private void OnCityNamesetsChanged()
        {
            eventCaller.Invoke("namesets.onCityNamesetsChanged", null);
            mainSystem.OnChangedDistrictNameGenerationRules();
            mainSystem.OnChangedRoadNameGenerationRules();
            isDirty = true;
        }

        private List<AdrNameFile> ListCityNamesets() => CityNamesets.Values.ToList();
        private List<AdrNameFile> ListLibraryNamesets() => AdrNameFilesManager.Instance.SimpleNamesFromFolder.Values.ToList();

        private void AddCityNameset(string name, string[] names)
        {
            var effectiveNewNameset = new AdrNameFile(name, names);
            CityNamesets[effectiveNewNameset.Id] = effectiveNewNameset;
            OnCityNamesetsChanged();
        }

        private void DeleteCityNameset(string guid)
        {
            var parsedGuid = new Guid(guid);
            if (CityNamesets.ContainsKey(parsedGuid))
            {
                CityNamesets.Remove(parsedGuid);
                OnCityNamesetsChanged();
            }
        }
        private string ExportToLibrary(string guid)
        {
            var parsedGuid = new Guid(guid);
            if (CityNamesets.ContainsKey(parsedGuid))
            {
                var file = CityNamesets[parsedGuid];
                var destinationFilename = Path.Combine(new string[] { AdrNameFilesManager.NamesetsFolder, "Exported" }.Concat(file.Name.Split("/").Select(x => string.Concat(x.Split(Path.GetInvalidFileNameChars())))).ToArray()) + $"_{parsedGuid}.txt";
                KFileUtils.EnsureFolderCreation(Path.GetDirectoryName(destinationFilename));
                File.WriteAllLines(destinationFilename, file.Values);
                return destinationFilename.Replace(AdrNameFilesManager.NamesetsFolder, "<NamesetsFolder>");
            }
            return null;
        }
        private void UpdateCityNameset(string guid, string name, string[] names)
        {
            var targetGuid = new Guid(guid);
            if (CityNamesets.TryGetValue(targetGuid, out var nameset))
            {
                nameset.Name = name;
                nameset.Values.Clear();
                nameset.Values.AddRange(names.Where(x => !x.IsNullOrWhitespace()));
                OnCityNamesetsChanged();
            }
            else
            {
                LogUtils.DoWarnLog($"Nameset not found in the city! {guid}");
            }
        }

        private string[] SortValues(string[] values)
        {
            static string RemoveDiacritics(string text)
            {
                if (text != null)
                    text = WebUtility.HtmlDecode(text);

                string formD = text.Normalize(NormalizationForm.FormD);
                StringBuilder sb = new StringBuilder();

                foreach (char ch in formD)
                {
                    UnicodeCategory uc = CharUnicodeInfo.GetUnicodeCategory(ch);
                    if (uc != UnicodeCategory.NonSpacingMark)
                    {
                        sb.Append(ch);
                    }
                }

                return sb.ToString().Normalize(NormalizationForm.FormC);
            }
            return values.OrderBy(RemoveDiacritics, StringComparer.Create(CultureInfo.CurrentUICulture, true)).ToArray();
        }

        #region Serialization

        private AdrNamesetSystemXML ToXml()
        {
            var xml = new AdrNamesetSystemXML
            {
                CityNamesets = CityNamesets.Values.Select(x => x.ToXML()).ToList()
            };
            return xml;
        }


        void IBelzontSerializableSingleton<AdrNamesetSystem>.Serialize<TWriter>(TWriter writer)
        {
            var xml = XmlUtils.DefaultXmlSerialize(ToXml());
            writer.Write(CURRENT_VERSION);
            var arraySave = new NativeArray<byte>(ZipUtils.Zip(xml), Allocator.Temp);
            writer.Write(arraySave.Length);
            writer.Write(arraySave);
            arraySave.Dispose();
        }

        void IBelzontSerializableSingleton<AdrNamesetSystem>.Deserialize<TReader>(TReader reader)
        {
            reader.Read(out int version);
            if (version > CURRENT_VERSION)
            {
                throw new Exception("Invalid version of AdrNamesetsystem!");
            }
            string namesetData;

            reader.Read(out int size);
            NativeArray<byte> byteNativeArray = new(new byte[size], Allocator.Temp);
            reader.Read(byteNativeArray);
            namesetData = ZipUtils.Unzip(byteNativeArray.ToArray());
            byteNativeArray.Dispose();

            var Namesets = XmlUtils.DefaultXmlDeserialize<AdrNamesetSystemXML>(namesetData);
            CityNamesets.Clear();
            CityNamesets.AddRange(Namesets.CityNamesets.ToDictionary(x => x.Id, x => AdrNameFile.FromXML(x)));
            OnCityNamesetsChanged();
        }

        JobHandle IJobSerializable.SetDefaults(Context context)
        {
            CityNamesets.Clear();
            OnCityNamesetsChanged();
            return default;
        }

        private bool isDirty;
        internal bool RequireLinesColorsReprocess() => isDirty;

        internal void OnLinesColorsReprocessed() => isDirty = false;

        [XmlRoot("XtmNamesetsystem")]
        public class AdrNamesetSystemXML
        {
            public List<AdrNameFileXML> CityNamesets;
        }
        #endregion
    }
}
