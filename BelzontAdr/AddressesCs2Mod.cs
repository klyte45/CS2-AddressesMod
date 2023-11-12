using Belzont.Interfaces;
using Game;
using Game.Modding;
using Game.UI.Menu;
using System.Collections.Generic;
using System.Reflection;

namespace BelzontAdr
{
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
