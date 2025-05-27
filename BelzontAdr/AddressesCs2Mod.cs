#if !RELEASE 
#define LOCAL
#endif
using Belzont.Interfaces;
using Colossal.IO.AssetDatabase;
using Game;
using Game.Modding;
using Game.Net;
using System.Collections.Generic;
using Unity.Entities;

namespace BelzontAdr
{

    [FileLocation("K45_ADR_settings")]
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
            updateSystem.UpdateAt<TempRoadMarkerTooltip>(SystemUpdatePhase.UITooltip);
            updateSystem.UpdateAt<AdrHighwayRoutesSystem>(SystemUpdatePhase.ModificationEnd);
            updateSystem.UpdateAt<AdrHighwayRoutes2BSystem>(SystemUpdatePhase.Modification2B);
        }

        protected override void AfterRegisterAssets()
        {
            base.AfterRegisterAssets();
            World.DefaultGameObjectInjectionWorld.GetOrCreateSystemManaged<Adr_WEIntegrationSystem>().IntializeWE();
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
