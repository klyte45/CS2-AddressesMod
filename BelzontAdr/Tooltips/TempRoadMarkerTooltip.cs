using Belzont.Utils;
using Colossal.Entities;
using Game.Buildings;
using Game.Common;
using Game.Objects;
using Game.SceneFlow;
using Game.Tools;
using Game.UI;
using Game.UI.Tooltip;
using Unity.Collections;
using Unity.Entities;

namespace BelzontAdr
{
    public partial class TempRoadMarkerTooltip : TooltipSystemBase
    {

        protected override void OnCreate()
        {
            base.OnCreate();
            m_nameSystem = World.GetOrCreateSystemManaged<NameSystem>();
            m_TempQuery = GetEntityQuery(new ComponentType[]
            {
                ComponentType.ReadOnly<ADRHighwayMarkerData>(),
                ComponentType.ReadOnly<Attached>(),
                ComponentType.ReadOnly<Temp>(),
                ComponentType.Exclude<Deleted>()
            });
            m_Location = new StringTooltip
            {
                icon = "coui://adr.k45/UI/images/ADR.svg",
            };
            RequireForUpdate(m_TempQuery);
        }

        protected override void OnUpdate()
        {
            base.CompleteDependency();

            using NativeArray<ArchetypeChunk> nativeArray = m_TempQuery.ToArchetypeChunkArray(Allocator.Temp);
            var entitiesHandle = GetEntityTypeHandle();
            bool anyValidResult = false;
            foreach (var archetypeChunk in nativeArray)
            {
                foreach (var tempEntity in archetypeChunk.GetNativeArray(entitiesHandle))
                {
                    bool thisResult = anyValidResult |= DrawTooltip(tempEntity, out var isTemp, out var tempData);
                    if (isTemp && tempData.m_Original == Entity.Null)
                    {
                        anyValidResult = thisResult;
                        goto end;
                    }
                }
            }
        end:
            if (anyValidResult) AddMouseTooltip(m_Location);
        }

        private bool DrawTooltip(Entity tempEntity, out bool isTemp, out Temp tempData)
        {
            if (EntityManager.TryGetComponent<Attached>(tempEntity, out var component2))
            {
                var parent = component2.m_Parent;

                if (isTemp = EntityManager.TryGetComponent(parent, out tempData))
                {
                    parent = tempData.m_Original;
                }

                if (BuildingUtils.GetAddress(EntityManager, tempEntity, parent, component2.m_CurvePosition, out Entity road, out int number))
                {
                    var lengthStr = GameManager.instance.settings.userInterface.unitSystem == Game.Settings.InterfaceSettings.UnitSystem.Freedom ? $"{number * 3:#,##0}ft, {number:#,##0}yd, {number / 1760f:#,##0.00}mi" : $"{number:#,##0}m, {number / 1000f:#,##0.00}km";

                    m_Location.value = $"{m_nameSystem.GetName(road).Translate()} @ {lengthStr}";
                    return true;
                }
            }
            isTemp = false;
            tempData = default;
            return false;
        }

        private EntityQuery m_TempQuery;
        private StringTooltip m_Location;
        private NameSystem m_nameSystem;
    }
}
