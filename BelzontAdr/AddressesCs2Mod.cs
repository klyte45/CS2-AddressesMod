#if !RELEASE
#define LOCAL
#endif
using Belzont.Interfaces;
using Game;
using Game.Modding;
using System.Collections.Generic;

namespace BelzontAdr
{
    public class AddressesCs2Mod : BasicIMod, IMod
    {
        public static new AddressesCs2Mod Instance => (AddressesCs2Mod)BasicIMod.Instance;

        public override string Acronym => "Adr";

        public override void DoOnCreateWorld(UpdateSystem updateSystem)
        {
            AdrNameFilesManager.Instance.ReloadNameFiles();
            updateSystem.UpdateAfter<AdrDistrictsSystem>(SystemUpdatePhase.UIUpdate);
            updateSystem.UpdateAfter<AdrMainSystem>(SystemUpdatePhase.UIUpdate);
            updateSystem.UpdateAfter<AdrEditorUISystem>(SystemUpdatePhase.UIUpdate);
            updateSystem.UpdateAt<AdrNamesetSystem>(SystemUpdatePhase.Modification2B);
            updateSystem.UpdateAt<AdrVehicleSystem>(SystemUpdatePhase.UIUpdate);
            updateSystem.UpdateAt<RoadVehiclePlateEditorController>(SystemUpdatePhase.UIUpdate);
            updateSystem.UpdateAt<RailVehiclePlateEditorController>(SystemUpdatePhase.UIUpdate);
            updateSystem.UpdateAt<WaterVehiclePlateEditorController>(SystemUpdatePhase.UIUpdate);
            updateSystem.UpdateAt<AirVehiclePlateEditorController>(SystemUpdatePhase.UIUpdate);
#if DEBUG && ADR_AGGSYS
            updateSystem.UpdateAt<AdrAggregationSystem>(SystemUpdatePhase.Modification2B);
#endif
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

#if LOCAL
        private string BaseUrlApps => "http://localhost:8715";
#else
        private string BaseUrlApps => $"coui://{CouiHost}/UI";
#endif
        protected override bool EuisIsMandatory => true;
        protected override bool UseEuisRegister => true;
        protected override Dictionary<string, EuisAppRegister> EuisApps => new()
        {
            ["main"] = new("Addresses Mod for CS2", $"{BaseUrlApps}/k45-adr-main.js", $"{BaseUrlApps}/k45-adr-main.css", $"coui://{CouiHost}/UI/images/ADR.svg")
        };
    }
}
