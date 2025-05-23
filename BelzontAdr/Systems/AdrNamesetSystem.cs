using Belzont.Interfaces;
using Belzont.Serialization;
using Belzont.Utils;
using Colossal;
using Colossal.OdinSerializer.Utilities;
using Colossal.Serialization.Entities;
using Game.Areas;
using Game.Citizens;
using Game.Common;
using Game.Net;
using Game.Tools;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Net;
using System.Text;
using System.Xml.Serialization;
using Unity.Collections;
using Unity.Entities;
using Unity.Jobs;
using UnityEngine;
using static BelzontAdr.AdrNameFile;

namespace BelzontAdr
{
    public partial class AdrNamesetSystem : SystemBase, IBelzontBindable, IDefaultSerializable
    {
        const int CURRENT_VERSION = 1;

        private AdrMainSystem mainSystem;
        private EntityQuery m_UnsetRandomQuery;
        private static Unity.Mathematics.Random seedGenerator = Unity.Mathematics.Random.CreateFromIndex(0xf4a54);
        internal static ref Unity.Mathematics.Random SeedGenerator => ref seedGenerator;

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
            eventCaller("namesets.goToSimpleNamesFolder", () => RemoteProcess.OpenFolder(AdrNameFilesManager.NamesetsFolder));
            eventCaller("namesets.sortValues", SortValues);
            eventCaller("namesets.goToGitHubRepo", () => Application.OpenURL("https://github.com/klyte45/AddressesFiles"));
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

            m_UnsetRandomQuery = GetEntityQuery(new EntityQueryDesc[]
              {
                    new() {
                        Any = new ComponentType[]
                        {
                            ComponentType.ReadOnly<Aggregate>(),
                            ComponentType.ReadOnly<Household>(),
                            ComponentType.ReadOnly<HouseholdPet>(),
                            ComponentType.ReadOnly<Citizen>(),
                            ComponentType.ReadOnly<HouseholdMember>(),
                            ComponentType.ReadOnly<District>(),
                        },
                        None = new ComponentType[]
                        {
                            ComponentType.ReadWrite<ADRRandomizationData>(),
                            ComponentType.ReadOnly<Temp>(),
                            ComponentType.ReadOnly<Deleted>(),
                        }
                    }
              });
            RequireForUpdate(m_UnsetRandomQuery);
        }

        protected override void OnUpdate()
        {
            if (!m_UnsetRandomQuery.IsEmpty)
            {
                var nameLessList = m_UnsetRandomQuery.ToEntityArray(Allocator.Temp);
                if (AddressesCs2Mod.TraceMode) LogUtils.DoTraceLog($"Running NamesetSystem OnUpdate for {nameLessList.Length} entities");
                for (int i = 0; i < nameLessList.Length; i++)
                {
                    var data = new ADRRandomizationData();
                    data.Redraw();
                    EntityManager.AddComponentData(nameLessList[i], data);
                    if (!EntityManager.HasComponent<BatchesUpdated>(nameLessList[i])) EntityManager.AddComponent<BatchesUpdated>(nameLessList[i]);
                    if (!EntityManager.HasComponent<Updated>(nameLessList[i])) EntityManager.AddComponent<Updated>(nameLessList[i]);
                }
            }
        }
        private Dictionary<Colossal.Hash128, AdrNameFile> CityNamesets = new();

        internal bool GetForGuid(Colossal.Hash128 guid, out AdrNameFile file) => CityNamesets.TryGetValue(guid, out file);
        private void OnCityNamesetsChanged()
        {
            eventCaller.Invoke("namesets.onCityNamesetsChanged", null);
            mainSystem.MarkDistrictsDirty();
            mainSystem.MarkRoadsDirty();
            isDirty = true;
        }

        private List<AdrNameFile> ListCityNamesets() => CityNamesets.Values.ToList();
        private List<AdrNameFile> ListLibraryNamesets() => AdrNameFilesManager.Instance.SimpleNamesFromFolder.Values.ToList();

        private void AddCityNameset(string name, string[] names, string[] namesAlternative)
        {
            var effectiveNewNameset = new AdrNameFile(name, names, namesAlternative);
            CityNamesets[effectiveNewNameset.Id] = effectiveNewNameset;
            OnCityNamesetsChanged();
        }

        private void DeleteCityNameset(string guid)
        {
            var parsedGuid = new Colossal.Hash128(guid);
            if (CityNamesets.ContainsKey(parsedGuid))
            {
                CityNamesets.Remove(parsedGuid);
                OnCityNamesetsChanged();
            }
        }
        private string ExportToLibrary(string guid)
        {
            var parsedGuid = new Colossal.Hash128(guid);
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
        private void UpdateCityNameset(string guid, string name, string[] names, string[] namesAlternative)
        {
            var targetGuid = new Colossal.Hash128(guid);
            if (CityNamesets.TryGetValue(targetGuid, out var nameset))
            {
                nameset.Name = name;
                nameset.Values = new ImmutableList<string>(names);
                nameset.ValuesAlternative = new ImmutableList<string>(namesAlternative);
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

        #region legacy
        private void LegacyDeserialize(IReader reader)
        {
            string namesetData;

            reader.Read(out int size);
            NativeArray<byte> byteNativeArray = new(new byte[size], Allocator.Temp);
            reader.Read(byteNativeArray);
            namesetData = ZipUtils.Unzip(byteNativeArray.ToArray());
            byteNativeArray.Dispose();

            var Namesets = XmlUtils.DefaultXmlDeserialize<AdrNamesetSystemXML>(namesetData);
            CityNamesets.Clear();
            foreach (var item in Namesets.CityNamesets)
            {
                var id = item.Id;
                var xml = FromXML(item);

                CityNamesets[item.Id] = xml;
            }
            seedGenerator.state = Namesets.seedId == 0 ? (uint)new System.Random().Next() : Namesets.seedId;
            OnCityNamesetsChanged();
        }

        #endregion


        void ISerializable.Serialize<TWriter>(TWriter writer)
        {
            writer.Write(CURRENT_VERSION);
            writer.Write(CityNamesets.Count);
            foreach (var entry in CityNamesets.Values)
            {
                writer.WriteNullCheck(entry);
            }
            writer.Write(seedGenerator.state);
        }

        void ISerializable.Deserialize<TReader>(TReader reader)
        {
            reader.Read(out int version);
            if (version > CURRENT_VERSION)
            {
                throw new Exception("Invalid version of AdrNamesetsystem!");
            }
            if (version == 0)
            {
                LegacyDeserialize(reader);
                return;
            }
            CityNamesets.Clear();
            reader.Read(out int count);
            for (int i = 0; i < count; i++)
            {
                reader.ReadNullCheck(out AdrNameFile item);
                CityNamesets[item.Id] = item;
            }
            reader.Read(out uint seedId);
            seedGenerator.state = seedId;
            OnCityNamesetsChanged();

        }
        public void SetDefaults(Context context)
        {
            CityNamesets.Clear();
            seedGenerator = new Unity.Mathematics.Random();
        }

        private bool isDirty;
        internal bool RequireLinesColorsReprocess() => isDirty;

        internal void OnLinesColorsReprocessed() => isDirty = false;

        [XmlRoot("XtmNamesetsystem")]
        public class AdrNamesetSystemXML
        {
            public List<AdrNameFileXML> CityNamesets;
            public uint seedId = 0;
        }
        #endregion      
    }
}

