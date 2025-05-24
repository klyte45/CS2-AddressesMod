using Belzont.Interfaces;
using Belzont.Utils;
using Colossal.Entities;
using Colossal.OdinSerializer.Utilities;
using Game;
using Game.Areas;
using Game.Common;
using Game.Tools;
using Game.UI;
using System;
using System.Collections.Generic;
using Unity.Collections;
using Unity.Entities;
using static Belzont.Utils.NameSystemExtensions;

namespace BelzontAdr
{
    public partial class AdrDistrictsSystem : GameSystemBase, IBelzontBindable
    {
        private Action<string, object[]> m_eventCaller;
        private EntityQuery m_districtsUpdatedQuery;
        private EntityQuery m_districtsAreaQuery;
        private NameSystem nameSystem;
        private AdrMainSystem mainSystem;
        private AdrNamesetSystem namesetSystem;
        private bool dirtyDistricts;
        private EndFrameBarrier m_barrier;
        public void SetupCallBinder(Action<string, Delegate> eventCaller)
        {
            eventCaller("district.listAllDistricts", ListAllDistricts);
            eventCaller("district.setRoadNamesFile", SetRoadNamesFile);
        }

        public void SetupCaller(Action<string, object[]> eventCaller)
        {
            m_eventCaller = eventCaller;
        }

        public void SetupEventBinder(Action<string, Delegate> eventCaller)
        {

        }

        protected override void OnUpdate()
        {
            if (dirtyDistricts)
            {
                dirtyDistricts = false;
                mainSystem.MarkRoadsDirty();
                OnDistrictChanged();
            }
            else if (!m_districtsUpdatedQuery.IsEmptyIgnoreFilter)
            {
                mainSystem.MarkRoadsDirty();
                OnDistrictChanged();
            }
        }
        protected override void OnCreate()
        {
            base.OnCreate();
            m_districtsUpdatedQuery = base.GetEntityQuery(new EntityQueryDesc[]
            {
                new EntityQueryDesc
                {
                    All = new ComponentType[]
                    {
                        ComponentType.ReadOnly<Area>(),
                        ComponentType.ReadOnly<District>(),
                    },
                    Any = new ComponentType[]
                    {
                        ComponentType.ReadOnly<Updated>(),
                        ComponentType.ReadOnly<BatchesUpdated>(),
                        ComponentType.ReadOnly<Deleted>()
                    },
                    None =new ComponentType[]
                    {
                        ComponentType.ReadOnly<Temp>()
                    }
                }
            });
            m_districtsAreaQuery = base.GetEntityQuery(new ComponentType[]
            {
                ComponentType.ReadOnly<Area>(),
                ComponentType.ReadOnly<District>(),
                ComponentType.ReadOnly<Node>(),
                ComponentType.ReadOnly<Triangle>(),
                ComponentType.Exclude<Deleted>()
            });
            nameSystem = World.GetOrCreateSystemManaged<NameSystem>();
            mainSystem = World.GetOrCreateSystemManaged<AdrMainSystem>();
            namesetSystem = World.GetOrCreateSystemManaged<AdrNamesetSystem>();
            m_barrier = World.GetOrCreateSystemManaged<EndFrameBarrier>();
        }
        private struct DistrictListItem
        {
            public Entity Entity;
            public ValuableName Name;
            public string CurrentValue;
        }

        private List<DistrictListItem> ListAllDistricts()
        {
            var entities = m_districtsAreaQuery.ToEntityArray(Allocator.Temp);
            var result = new List<DistrictListItem>();

            for (var i = 0; i < entities.Length; i++)
            {
                var entity = entities[i];
                if (BasicIMod.TraceMode) LogUtils.DoTraceLog($"result = {result} | nameSystem = {nameSystem} | EntityManager = {EntityManager}");
                result.Add(new()
                {
                    Entity = entity,
                    Name = nameSystem.GetName(entity).ToValueableName(),
                    CurrentValue = EntityManager.TryGetComponent(entity, out ADRDistrictData data) ? data.m_roadsNamesId.ToString() : null
                }); ;

            }
            entities.Dispose();
            return result;
        }

        internal void OnDistrictChanged()
        {
            m_eventCaller?.Invoke("district.onDistrictsChanged", new object[0]);
        }

        private void SetRoadNamesFile(Entity district, string fileGuid)
        {
            if (EntityManager.HasComponent<District>(district))
            {
                if (fileGuid.IsNullOrWhitespace())
                {
                    if (EntityManager.TryGetComponent<ADRDistrictData>(district, out var adrDistrict))
                    {
                        adrDistrict.m_roadsNamesId = default;
                        EntityManager.SetComponentData(district, adrDistrict);

                        dirtyDistricts = true;
                    }
                    return;
                }
                var guid = new Colossal.Hash128(fileGuid);
                if (namesetSystem.GetForGuid(guid, out _))
                {
                    if (EntityManager.TryGetComponent<ADRDistrictData>(district, out var adrDistrict))
                    {
                        adrDistrict.m_roadsNamesId = guid;
                        EntityManager.SetComponentData(district, adrDistrict);
                    }
                    else
                    {
                        adrDistrict = new ADRDistrictData
                        {
                            m_roadsNamesId = guid
                        };
                        m_barrier.CreateCommandBuffer().AddComponent(district, adrDistrict);
                    }
                    dirtyDistricts = true;
                }
                else
                {
                    LogUtils.DoWarnLog("Invalid name file GUID for settings");
                }
            }
        }
    }
}
