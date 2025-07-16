using Belzont.Interfaces;
using Belzont.Utils;
using BridgeWE;
using Game;
using Game.SceneFlow;
using HarmonyLib;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;

namespace BelzontAdr
{
    public partial class Adr_WEIntegrationSystem : GameSystemBase, IBelzontBindable
    {
        private bool weInitialized;

        public bool WeAvailable { get; private set; }

        public void SetupCallBinder(Action<string, Delegate> eventCaller)
        {
        }

        public void SetupCaller(Action<string, object[]> eventCaller)
        {
        }

        public void SetupEventBinder(Action<string, Delegate> eventCaller)
        {
        }


        protected override void OnCreate()
        {
            base.OnCreate();
        }


        protected override void OnStartRunning()
        {
            base.OnStartRunning();
        }

        internal void IntializeWE()
        {
            if (!weInitialized)
            {
                weInitialized = true;
                if (AppDomain.CurrentDomain.GetAssemblies().SingleOrDefault(assembly => assembly.GetName().Name == "BelzontWE") is Assembly weAssembly
                    && weAssembly.GetExportedTypes().FirstOrDefault(x => x.Name == "WEVehicleFn") is Type t)
                {
                    var exportedTypes = weAssembly.ExportedTypes;
                    foreach (var (type, sourceClassName) in new List<(Type, string)>() {
                    (typeof(WEFontManagementBridge), "FontManagementBridge"),
                    (typeof(WEImageManagementBridge), "ImageManagementBridge"),
                    (typeof(WETemplatesManagementBridge), "TemplatesManagementBridge"),
                    (typeof(WEMeshManagementBridge), "MeshManagementBridge"),
                })
                    {
                        var targetType = exportedTypes.First(x => x.Name == sourceClassName);
                        foreach (var method in type.GetMethods(BindingFlags.Public | BindingFlags.Static))
                        {
                            var srcMethod = targetType.GetMethod(method.Name, RedirectorUtils.allFlags, null, method.GetParameters().Select(x => x.ParameterType).ToArray(), null);
                            if (srcMethod != null) Harmony.ReversePatch(srcMethod, new HarmonyMethod(method));
                            else LogUtils.DoWarnLog($"Method not found while patching WE: {targetType.FullName} {srcMethod.Name}({string.Join(", ", method.GetParameters().Select(x => $"{x.ParameterType}"))})");
                        }
                    }
                    RegisterModFiles();
                    WeAvailable = true;
                }
                if (!WeAvailable)
                {
                    Enabled = false;
                }
            }
        }

        private void RegisterModFiles()
        {
            GameManager.instance.modManager.TryGetExecutableAsset(AddressesCs2Mod.Instance, out var asset);
            var modDir = Path.Combine(Path.GetDirectoryName(asset.path), "WriteEverywhere", ".Resources");

            var imagesDirectory = Path.Combine(modDir, "atlases");
            if (Directory.Exists(imagesDirectory))
            {
                var atlases = Directory.GetDirectories(imagesDirectory, "*", SearchOption.TopDirectoryOnly);
                foreach (var atlasFolder in atlases)
                {
                    WEImageManagementBridge.RegisterImageAtlas(typeof(AddressesCs2Mod).Assembly, Path.GetFileName(atlasFolder), Directory.GetFiles(atlasFolder, "*.png"));
                }
            }

            var layoutsDirectory = Path.Combine(modDir, "layouts");
            LogUtils.DoInfoLog($"RegisterCustomTemplates = {WETemplatesManagementBridge.RegisterCustomTemplates(typeof(AddressesCs2Mod).Assembly, layoutsDirectory)}");
            WETemplatesManagementBridge.RegisterLoadableTemplatesFolder(typeof(AddressesCs2Mod).Assembly, layoutsDirectory);


            var fontsDirectory = Path.Combine(modDir, "fonts");
            WEFontManagementBridge.RegisterModFonts(typeof(AddressesCs2Mod).Assembly, fontsDirectory);


            var objDirctory = Path.Combine(modDir, "objMeshes");

            var meshes = Directory.GetFiles(objDirctory, "*.obj", SearchOption.AllDirectories);
            foreach (var meshFile in meshes)
            {
                var meshName = Path.GetFileNameWithoutExtension(meshFile);
                if (!WEMeshManagementBridge.RegisterMesh(typeof(AddressesCs2Mod).Assembly, meshName, meshFile))
                {
                    LogUtils.DoWarnLog($"Failed to register mesh: {meshName} from {meshFile}");
                }
            }
        }

        protected override void OnUpdate()
        {

        }
    }
}
