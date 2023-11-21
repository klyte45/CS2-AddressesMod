using Belzont.Interfaces;
using Belzont.Utils;
using BelzontADR;
using BepInEx;
using Game;
using Game.Modding;
using Game.UI.Menu;
using System.Collections.Generic;

namespace BelzontAdr
{
#if THUNDERSTORE
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
    public class AddressesCs2Mod : BasicIMod
#if THUNDERSTORE 
        <AdrModData>
#endif
        , IMod
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
#if THUNDERSTORE
        public override AdrModData CreateNewModData() => new AdrModData();

        protected override IEnumerable<OptionsUISystem.Section> GenerateModOptionsSections() { yield break; }
#else

        public override BasicModData CreateSettingsFile()
        {
            return new AdrModData(this);
        }
#endif
    }
}
