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
    public class AdrDistrictsSystem : GameSystemBase, IBelzontBindable
    {
        private Action<string, object[]> m_eventCaller;
        private EntityQuery m_districtsUpdatedQuery;
        private EntityQuery m_districtsAreaQuery;
        private NameSystem nameSystem;
        private EndFrameBarrier m_EndFrameBarrier;
        private AdrMainSystem mainSystem;
        private bool dirtyDistricts;
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
                mainSystem.OnChangedRoadNameGenerationRules();
                OnDistrictChanged();
            }
            else if (!m_districtsUpdatedQuery.IsEmptyIgnoreFilter)
            {
                mainSystem.OnChangedRoadNameGenerationRules();
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
            nameSystem = World.GetExistingSystemManaged<NameSystem>();
            m_EndFrameBarrier = World.GetOrCreateSystemManaged<EndFrameBarrier>();
            mainSystem = World.GetOrCreateSystemManaged<AdrMainSystem>();
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
                var commandBuffer = m_EndFrameBarrier.CreateCommandBuffer();
                if (fileGuid.IsNullOrWhitespace())
                {
                    if (EntityManager.TryGetComponent<ADRDistrictData>(district, out var adrDistrict))
                    {
                        adrDistrict.m_roadsNamesId = default;
                        commandBuffer.SetComponent(district, adrDistrict);

                        dirtyDistricts = true;
                    }
                }
                else if (Guid.TryParse(fileGuid, out var guid) && AdrNameFilesManager.Instance.SimpleNamesDict.ContainsKey(guid))
                {

                    var hasComponent = EntityManager.TryGetComponent<ADRDistrictData>(district, out var adrDistrict);
                    adrDistrict.m_roadsNamesId = guid;
                    if (hasComponent)
                    {
                        commandBuffer.SetComponent(district, adrDistrict);
                    }
                    else
                    {
                        commandBuffer.AddComponent(district, adrDistrict);
                    }
                    dirtyDistricts = true;
                }
            }
        }
    }
}
