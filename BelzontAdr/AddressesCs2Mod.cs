#if !RELEASE 
//#define LOCAL
#endif
using Belzont.Interfaces;
using Belzont.Utils;
using Colossal.Core;
using Colossal.IO.AssetDatabase;
using Game;
using Game.Modding;
using Game.SceneFlow;
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
            updateSystem.UpdateAfter<AdrSettingsUISystem>(SystemUpdatePhase.UIUpdate);
            updateSystem.UpdateAt<AdrNamesetSystem>(SystemUpdatePhase.Modification2B);
            updateSystem.UpdateAt<AdrVehicleSystem>(SystemUpdatePhase.UIUpdate);
            updateSystem.UpdateAt<RoadVehiclePlateEditorController>(SystemUpdatePhase.UIUpdate);
            updateSystem.UpdateAt<RailVehiclePlateEditorController>(SystemUpdatePhase.UIUpdate);
            updateSystem.UpdateAt<WaterVehiclePlateEditorController>(SystemUpdatePhase.UIUpdate);
            updateSystem.UpdateAt<AirVehiclePlateEditorController>(SystemUpdatePhase.UIUpdate);
            updateSystem.UpdateAt<TempRoadMarkerTooltip>(SystemUpdatePhase.UITooltip);
            updateSystem.UpdateAt<AdrHighwayRoutesSystem>(SystemUpdatePhase.ModificationEnd);
            updateSystem.UpdateAt<AdrHighwayRoutes2BSystem>(SystemUpdatePhase.Modification2B);
            updateSystem.UpdateAt<AdrRegionsSystem>(SystemUpdatePhase.ModificationEnd);
            updateSystem.UpdateAt<AdrRoadGroupingController>(SystemUpdatePhase.ModificationEnd);
            updateSystem.UpdateAt<AdrRoadGroupingUISystem>(SystemUpdatePhase.UIUpdate);
            updateSystem.UpdateBefore<AdrClearSystem>(SystemUpdatePhase.Deserialize);
        }

        protected override void AfterRegisterAssets()
        {
            base.AfterRegisterAssets();
            World.DefaultGameObjectInjectionWorld.GetOrCreateSystemManaged<Adr_WEIntegrationSystem>().IntializeWE();
        }

        public override void OnDispose()
        {
            AdrNameFilesManager.Instance.Dispose();
        }

        public override void DoOnLoad()
        {
            MainThreadDispatcher.RegisterUpdater(() =>
            {
                var asset = AssetDatabase.global.GetAsset(SearchFilter<UIModuleAsset>.ByCondition(asset => asset.name == "k45_adr_vuio"));
                LogUtils.DoInfoLog($"Forcing loading UI asset: {asset?.name} ({asset?.path})");
                GameManager.instance.modManager.AddUIModule(asset);
            });
        }


        public override BasicModData CreateSettingsFile()
        {
            return new AdrModData(this);
        }

    }
}
