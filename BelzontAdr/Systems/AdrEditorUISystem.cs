using Belzont.Interfaces;
using Belzont.Utils;
using Colossal.Entities;
using Colossal.Mathematics;
using Colossal.UI.Binding;
using Game;
using Game.Citizens;
using Game.Common;
using Game.Notifications;
using Game.Prefabs;
using Game.Routes;
using Game.Tools;
using Game.UI;
using Game.UI.InGame;
using System;
using Unity.Entities;
using Unity.Mathematics;
using static Belzont.Utils.NameSystemExtensions;

namespace BelzontAdr
{
    public partial class AdrEditorUISystem : UISystemBase, IBelzontBindable
    {
        private ValueBinding<Entity> m_SelectedEntityBinding;
        private ToolSystem m_ToolSystem;
        private NameSystem m_NameSystem;
        private Entity m_SelectedEntity;
        private Entity m_SelectedPrefab;
        private float3 m_SelectedPosition;

        public override GameMode gameMode
        {
            get
            {
                return GameMode.Editor;
            }
        }

        protected override void OnCreate()
        {
            base.OnCreate();
            m_ToolSystem = World.GetOrCreateSystemManaged<ToolSystem>();
            m_NameSystem = World.GetOrCreateSystemManaged<NameSystem>();

            AddBinding(m_SelectedEntityBinding = new ValueBinding<Entity>("k45::ADR", "AdrEditorUISystem::selectedEntity", Entity.Null, null, null));
        }

        protected override void OnUpdate()
        {
            RefreshSelection();
            m_SelectedEntityBinding.Update(m_SelectedEntity);
        }
        private bool TryGetSelection(out Entity entity)
        {
            entity = m_ToolSystem.selected;
            return entity != Entity.Null;
        }
        private void RefreshSelection()
        {
            if (TryGetSelection(out Entity entity) && EntityManager.TryGetComponent(entity, out PrefabRef prefabRef))
            {
                Entity prefab = prefabRef.m_Prefab;
                FilterSelection(ref entity, ref prefab);
                int elIdx = 0;
                if (SelectedInfoUISystem.TryGetPosition(entity, EntityManager, ref elIdx, out Entity entity2, out float3 selectedPosition, out Bounds3 bounds, out quaternion quaternion, false) || EntityManager.HasComponent<Household>(entity))
                {
                    selectedPosition.y = MathUtils.Center(bounds.y);
                    m_SelectedEntity = entity;
                    m_SelectedPrefab = prefab;
                    m_SelectedPosition = selectedPosition;
                    return;
                }
            }
            m_SelectedEntity = Entity.Null;
            m_SelectedPrefab = Entity.Null;
            m_SelectedPosition = float3.zero;
        }

        private void FilterSelection(ref Entity entity, ref Entity prefab)
        {
            if (EntityManager.HasComponent<Icon>(entity) && EntityManager.TryGetComponent(entity, out Owner owner))
            {
                if (EntityManager.HasComponent<RouteLane>(owner.m_Owner) && EntityManager.HasComponent<Waypoint>(owner.m_Owner) && EntityManager.TryGetComponent(owner.m_Owner, out Owner owner2))
                {
                    entity = owner2.m_Owner;
                }
                else if (EntityManager.TryGetComponent(owner.m_Owner, out CurrentBuilding currentBuilding))
                {
                    if (EntityManager.Exists(currentBuilding.m_CurrentBuilding))
                    {
                        entity = currentBuilding.m_CurrentBuilding;
                    }
                }
                else
                {
                    entity = owner.m_Owner;
                }
                if (EntityManager.TryGetComponent(entity, out PrefabRef prefabRef))
                {
                    prefab = prefabRef.m_Prefab;
                }
                SetSelection(entity);
            }
            if (EntityManager.TryGetComponent(entity, out Game.Creatures.Resident resident) && EntityManager.TryGetComponent(resident.m_Citizen, out PrefabRef prefabRef2))
            {
                entity = resident.m_Citizen;
                prefab = prefabRef2.m_Prefab;
            }
            if (EntityManager.TryGetComponent(entity, out Game.Creatures.Pet pet) && EntityManager.TryGetComponent(pet.m_HouseholdPet, out PrefabRef prefabRef3))
            {
                entity = pet.m_HouseholdPet;
                prefab = prefabRef3.m_Prefab;
            }
        }
        public void SetSelection(Entity entity)
        {
            if (entity == m_SelectedEntity)
            {
                return;
            }
            m_ToolSystem.selected = entity;
        }

        private struct AdrEntityEditorData
        {
            public ValuableName name;
        }

        private AdrEntityEditorData GetEntityData(Entity e)
        {
            var result = new AdrEntityEditorData
            {
                name = m_NameSystem.GetName(e).ToValueableName()
            };
            return result;
        }

        #region Binding EUIS
        private Action<string, object[]> m_eventCaller;

        public void SetupCallBinder(Action<string, Delegate> eventCaller)
        {
            eventCaller("editorUI.getEntityData", GetEntityData);
            eventCaller("editorUI.setEntityCustomName", m_NameSystem.SetCustomName);
        }

        public void SetupCaller(Action<string, object[]> eventCaller)
        {
            m_eventCaller = eventCaller;
        }

        public void SetupEventBinder(Action<string, Delegate> eventCaller)
        {
        }
        #endregion
    }
}
