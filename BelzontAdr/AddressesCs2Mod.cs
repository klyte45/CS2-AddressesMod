using Belzont.Interfaces;
using Game;
using Game.Modding;

namespace BelzontAdr
{
    public class AddressesCs2Mod : BasicIMod, IMod
    {
        public static new AddressesCs2Mod Instance => (AddressesCs2Mod)BasicIMod.Instance;

        public override string Acronym => "Adr";

        public override void DoOnCreateWorld(UpdateSystem updateSystem)
        {
            AdrNameFilesManager.Instance.ReloadNameFiles();
            updateSystem.UpdateAfter<AdrDistrictsSystem>(SystemUpdatePhase.ModificationEnd);
            updateSystem.UpdateAfter<AdrMainSystem>(SystemUpdatePhase.ModificationEnd);
            updateSystem.UpdateAfter<AdrEditorUISystem>(SystemUpdatePhase.UIUpdate);
            updateSystem.UpdateAt<AdrNamesetSystem>(SystemUpdatePhase.Modification2B);
            updateSystem.UpdateAt<AdrVehicleSystem>(SystemUpdatePhase.ModificationEnd);
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

    }
}
