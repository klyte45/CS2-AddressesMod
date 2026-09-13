#if !RELEASE 
//#define LOCAL
#endif
using Belzont.Interfaces;
using Belzont.Utils;
using Colossal.Core;
using Colossal.IO.AssetDatabase;
using Game;
using Game.Modding;
using Game.Rendering;
using Game.SceneFlow;
using Game.Serialization;
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
            // Name transfers use NameSystem.SetCustomName → EndFrameBarrier (allowed in MainLoop only).
            updateSystem.UpdateAt<AdrNameAggregateMembershipSystem>(SystemUpdatePhase.MainLoop);
            updateSystem.UpdateAt<AdrNameAggregateMaintenanceSystem>(SystemUpdatePhase.ModificationEnd);
            // LBL-02/03/04: TMP bake + placement + FillNameMeshes (after vanilla AggregateMeshSystem prefab init).
            updateSystem.UpdateAfter<AdrRoadNameLabelMeshSystem, AggregateMeshSystem>(SystemUpdatePhase.PreCulling);
            updateSystem.UpdateBefore<PreDeserialize<AdrRoadNameLabelMeshSystem>>(SystemUpdatePhase.Deserialize);
            // LBL-04: name DrawMesh path (parity with AggregateRenderSystem name loop; no arrows).
            updateSystem.UpdateAt<AdrRoadNameLabelRenderSystem>(SystemUpdatePhase.Rendering);
            updateSystem.UpdateAt<AdrVehicleSystem>(SystemUpdatePhase.UIUpdate);
            updateSystem.UpdateAt<RoadVehiclePlateEditorController>(SystemUpdatePhase.UIUpdate);
            updateSystem.UpdateAt<RailVehiclePlateEditorController>(SystemUpdatePhase.UIUpdate);
            updateSystem.UpdateAt<WaterVehiclePlateEditorController>(SystemUpdatePhase.UIUpdate);
            updateSystem.UpdateAt<AirVehiclePlateEditorController>(SystemUpdatePhase.UIUpdate);
            updateSystem.UpdateAt<TempRoadMarkerTooltip>(SystemUpdatePhase.UITooltip);
            updateSystem.UpdateAt<AdrHighwayRoutesSystem>(SystemUpdatePhase.ModificationEnd);
            updateSystem.UpdateAt<AdrRegionsSystem>(SystemUpdatePhase.ModificationEnd);
            // UIUpdate: segment number-card projection tracks camera like SelectedInfoUISystem.
            updateSystem.UpdateAt<AdrRoadGroupingController>(SystemUpdatePhase.UIUpdate);
            updateSystem.UpdateAt<AdrRoadGroupingUISystem>(SystemUpdatePhase.UIUpdate);
            // Route-style GPU ribbons + Overlay absolute node markers (squares/circles).
            updateSystem.UpdateBefore<AdrRoadGroupingOverlayRenderSystem, OverlayRenderSystem>(SystemUpdatePhase.Rendering);
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
