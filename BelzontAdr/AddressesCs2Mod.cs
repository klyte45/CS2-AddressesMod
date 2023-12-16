#if BEPINEX_CS2
using AddressesCS2;
using BepInEx;
#endif
using Belzont.Interfaces;
using Belzont.Utils;
using Game;
using Game.Modding;

namespace BelzontAdr
{
#if BEPINEX_CS2
    [BepInPlugin(MyPluginInfo.PLUGIN_GUID, MyPluginInfo.PLUGIN_NAME, MyPluginInfo.PLUGIN_VERSION)]
    public class EUIBepinexPlugin : BaseUnityPlugin
    {
        public void Awake()
        {
            LogUtils.LogsEnabled = false;
            LogUtils.Logger = Logger;
            LogUtils.DoInfoLog($"STARTING MOD!");
            Redirector.PatchAll();
        }
    }
#endif
    public class AddressesCs2Mod : BasicIMod, IMod
    {
        public static new AddressesCs2Mod Instance => (AddressesCs2Mod)BasicIMod.Instance;

        public override string SimpleName => "Addresses CS2";

        public override string SafeName => "Addresses";

        public override string Acronym => "Adr";

        public override string Description => "!!!";

        public override void DoOnCreateWorld(UpdateSystem updateSystem)
        {
            AdrNameFilesManager.Instance.ReloadNameFiles();
            updateSystem.UpdateAfter<AdrDistrictsSystem>(SystemUpdatePhase.ModificationEnd);
        }

        public override void OnDispose()
        {
        }

        public override void DoOnLoad()
        {
        }


        public override BasicModData CreateSettingsFile()
        {
            return new AdrModData(this);
        }

    }
}
