using Belzont.Utils;
using Colossal.Entities;
using Game.Common;
using Game.Net;
using Game.Prefabs;
using Game.Tools;
using Game.UI;
using Game.UI.InGame;
using Game.UI.Tooltip;
using HarmonyLib;
using System.Collections.Generic;
using System.Reflection;
using System.Reflection.Emit;
using Unity.Entities;
using Unity.Mathematics;

namespace BelzontAdr
{
    /// <summary>
    /// LBL-05: Forked net label pick (RaycastSystem transpiler), SIP Aggregate parity, tooltip/actions,
    /// TrafficRoutes live path for S2. Areas.RaycastLabelsJob (districts) stays vanilla.
    /// Re-diff GUID d124bec16c0748da9ebf48366d9c1d68.
    /// </summary>
    public class AdrRoadNameLabelPickOverrides : Redirector, IRedirectable
    {
        private static readonly AccessTools.FieldRef<RoadSection, float[]> s_volume =
            AccessTools.FieldRefAccess<RoadSection, float[]>("m_Volume");
        private static readonly AccessTools.FieldRef<RoadSection, float[]> s_flow =
            AccessTools.FieldRefAccess<RoadSection, float[]>("m_Flow");

        private static ToolSystem s_toolSystem;
        private static DefaultToolSystem s_defaultTool;
        private static ToolRaycastSystem s_toolRaycastSystem;
        private static ImageSystem s_imageSystem;
        private static AccessTools.FieldRef<RaycastNameTooltipSystem, NameTooltip> s_tooltip;
        private static MethodInfo s_addMouseTooltip;
        private static AccessTools.FieldRef<InfoSectionBase, bool> s_visibleBacking;

        public void DoPatches(World world)
        {
            s_toolSystem = world.GetOrCreateSystemManaged<ToolSystem>();
            s_defaultTool = world.GetOrCreateSystemManaged<DefaultToolSystem>();
            s_toolRaycastSystem = world.GetOrCreateSystemManaged<ToolRaycastSystem>();
            s_imageSystem = world.GetOrCreateSystemManaged<ImageSystem>();
            s_tooltip = AccessTools.FieldRefAccess<RaycastNameTooltipSystem, NameTooltip>("m_Tooltip");
            s_addMouseTooltip = AccessTools.Method(typeof(TooltipSystemBase), "AddMouseTooltip");
            s_visibleBacking = AccessTools.FieldRefAccess<InfoSectionBase, bool>("<visible>k__BackingField");

            var performRaycast = typeof(RaycastSystem).GetMethod("PerformRaycast", RedirectorUtils.allFlags);
            AddRedirect(performRaycast, null, null,
                GetType().GetMethod(nameof(PerformRaycast_Transpiler), RedirectorUtils.allFlags));

            var roadSection = typeof(RoadSection);
            AddRedirect(
                roadSection.GetMethod("OnUpdate", RedirectorUtils.allFlags),
                null,
                GetType().GetMethod(nameof(RoadSection_OnUpdate_Postfix), RedirectorUtils.allFlags));
            AddRedirect(
                roadSection.GetMethod("OnProcess", RedirectorUtils.allFlags),
                GetType().GetMethod(nameof(RoadSection_OnProcess_Prefix), RedirectorUtils.allFlags));

            AddRedirect(
                typeof(TitleSection).GetMethod(nameof(TitleSection.GetVirtualKeyboardLocaleKey), RedirectorUtils.allFlags),
                GetType().GetMethod(nameof(GetVirtualKeyboardLocaleKey_Prefix), RedirectorUtils.allFlags));

            AddRedirect(
                typeof(ImageSystem).GetMethod(nameof(ImageSystem.GetInstanceIcon), RedirectorUtils.allFlags, null,
                    new[] { typeof(Entity), typeof(Entity) }, null),
                null,
                GetType().GetMethod(nameof(GetInstanceIcon_Postfix), RedirectorUtils.allFlags));

            AddRedirect(
                typeof(RaycastNameTooltipSystem).GetMethod("OnUpdate", RedirectorUtils.allFlags),
                GetType().GetMethod(nameof(RaycastNameTooltip_OnUpdate_Prefix), RedirectorUtils.allFlags));

            AddRedirect(
                typeof(ActionsSection).GetMethod("OnProcess", RedirectorUtils.allFlags),
                null,
                GetType().GetMethod(nameof(ActionsSection_OnProcess_Postfix), RedirectorUtils.allFlags));

            var trafficRoutes = typeof(TrafficRoutesSystem);
            AddRedirect(
                trafficRoutes.GetMethod("OnUpdate", RedirectorUtils.allFlags),
                GetType().GetMethod(nameof(TrafficRoutes_OnUpdate_Prefix), RedirectorUtils.allFlags),
                GetType().GetMethod(nameof(TrafficRoutes_OnUpdate_Postfix), RedirectorUtils.allFlags));
        }

        private static Entity s_tempAggregateTag;

        /// <summary>
        /// S2: mirror ADR membership into AggregateElement and briefly add Aggregate so
        /// TrafficRoutesSystem FillTargetMapJob walks edges like a vanilla Aggregate.
        /// </summary>
        private static void TrafficRoutes_OnUpdate_Prefix(TrafficRoutesSystem __instance)
        {
            s_tempAggregateTag = Entity.Null;
            if (!__instance.routesVisible || s_toolSystem == null)
            {
                return;
            }

            Entity selected = s_toolSystem.selected;
            EntityManager em = __instance.EntityManager;
            if (!AdrNameAggregateMembershipHelper.IsS2Owner(em, selected))
            {
                return;
            }

            DynamicBuffer<AggregateElement> dest = em.HasBuffer<AggregateElement>(selected)
                ? em.GetBuffer<AggregateElement>(selected)
                : em.AddBuffer<AggregateElement>(selected);
            AdrNameAggregateMembershipHelper.FillAggregateElementFromAdr(em, selected, dest);

            if (!em.HasComponent<Aggregate>(selected))
            {
                em.AddComponentData(selected, default(Aggregate));
                s_tempAggregateTag = selected;
            }
        }

        private static void TrafficRoutes_OnUpdate_Postfix(TrafficRoutesSystem __instance)
        {
            if (s_tempAggregateTag == Entity.Null)
            {
                return;
            }

            EntityManager em = __instance.EntityManager;
            if (em.Exists(s_tempAggregateTag) && em.HasComponent<Aggregate>(s_tempAggregateTag))
            {
                em.RemoveComponent<Aggregate>(s_tempAggregateTag);
            }
            s_tempAggregateTag = Entity.Null;
        }

        /// <summary>
        /// Replace Net RaycastLabelsJob.Schedule with ADR schedule (S1/S2). Areas job untouched.
        /// Stack before Schedule: job (by value), NativeList, batch, deps — then ldarg.0 (this).
        /// </summary>
        private static IEnumerable<CodeInstruction> PerformRaycast_Transpiler(IEnumerable<CodeInstruction> instructions)
        {
            var codes = new List<CodeInstruction>(instructions);
            LogUtils.PrintMethodIL(codes);

            var vanillaJobType = typeof(Game.Net.RaycastJobs).GetNestedType("RaycastLabelsJob");
            var replacement = typeof(AdrRaycastLabelsJobs).GetMethod(
                nameof(AdrRaycastLabelsJobs.ScheduleFromVanillaStub), RedirectorUtils.allFlags);

            if (vanillaJobType == null || replacement == null)
            {
                LogUtils.DoWarnLog("[AdrRoadNameLabelPickOverrides] PerformRaycast transpiler: missing types — skipped.");
                LogUtils.PrintMethodIL(codes);
                return codes;
            }

            int replaced = 0;
            for (int i = 0; i < codes.Count; i++)
            {
                if (codes[i].opcode != OpCodes.Call || codes[i].operand is not MethodInfo mi)
                {
                    continue;
                }
                // Only IJobParallelForDefer Schedule(list, batch, deps) — not ScheduleParallel / IJob.
                if (mi.Name != "Schedule" || !mi.IsGenericMethod)
                {
                    continue;
                }
                var args = mi.GetGenericArguments();
                if (args.Length < 1 || args[0] != vanillaJobType)
                {
                    continue;
                }
                var parameters = mi.GetParameters();
                if (parameters.Length < 4)
                {
                    continue;
                }

                codes.Insert(i, new CodeInstruction(OpCodes.Ldarg_0));
                codes[i + 1] = new CodeInstruction(OpCodes.Call, replacement);
                replaced++;
                i++;
            }

            if (replaced == 0)
            {
                LogUtils.DoWarnLog(
                    "[AdrRoadNameLabelPickOverrides] PerformRaycast: Net RaycastLabelsJob.Schedule not found — pick fork inactive.");
            }
            LogUtils.PrintMethodIL(codes);
            return codes;
        }

        private static void RoadSection_OnUpdate_Postfix(RoadSection __instance)
        {
            EntityManager em = __instance.EntityManager;
            Entity selected = __instance.World.GetOrCreateSystemManaged<SelectedInfoUISystem>().selectedEntity;
            if (AdrNameAggregateMembershipHelper.IsS2Owner(em, selected))
            {
                s_visibleBacking(__instance) = true;
                return;
            }
            if (AdrNameAggregateMembershipHelper.IsS1Owner(em, selected))
            {
                s_visibleBacking(__instance) = AdrNameAggregateMembershipHelper.HasS1Remainder(em, selected);
            }
        }

        private static bool RoadSection_OnProcess_Prefix(RoadSection __instance)
        {
            EntityManager em = __instance.EntityManager;
            Entity selected = __instance.World.GetOrCreateSystemManaged<SelectedInfoUISystem>().selectedEntity;

            if (AdrNameAggregateMembershipHelper.IsS2Owner(em, selected))
            {
                ProcessEdges(__instance, em, EnumerateAdrEdges(em, selected));
                return false;
            }

            if (AdrNameAggregateMembershipHelper.IsS1Owner(em, selected))
            {
                ProcessEdges(__instance, em, EnumerateS1RemainderEdges(em, selected));
                return false;
            }

            return true;
        }

        private static IEnumerable<Entity> EnumerateAdrEdges(EntityManager em, Entity adrAggregate)
        {
            DynamicBuffer<ADRNameAggregateElement> buffer =
                em.GetBuffer<ADRNameAggregateElement>(adrAggregate, true);
            for (int i = 0; i < buffer.Length; i++)
            {
                yield return buffer[i].m_Edge;
            }
        }

        private static IEnumerable<Entity> EnumerateS1RemainderEdges(EntityManager em, Entity aggregate)
        {
            DynamicBuffer<AggregateElement> buffer = em.GetBuffer<AggregateElement>(aggregate, true);
            for (int i = 0; i < buffer.Length; i++)
            {
                Entity edge = buffer[i].m_Edge;
                if (!AdrNameAggregateMembershipHelper.IsAdrAssociated(em, edge))
                {
                    yield return edge;
                }
            }
        }

        /// <summary>Vanilla RoadSection.OnProcess per-edge math (GUID d124bec16c0748da9ebf48366d9c1d68).</summary>
        private static void ProcessEdges(RoadSection section, EntityManager em, IEnumerable<Entity> edges)
        {
            float[] volume = s_volume(section);
            float[] flow = s_flow(section);
            float length = 0f;
            float bestCondition = 100f;
            float worstCondition = 0f;
            float condition = 0f;
            float upkeep = 0f;
            int roadElementCount = 0;

            foreach (Entity edge in edges)
            {
                if (em.TryGetComponent(edge, out Curve curve))
                {
                    length += curve.m_Length;
                }
                if (em.TryGetComponent(edge, out Road road))
                {
                    roadElementCount++;
                    float4 float5 = (road.m_TrafficFlowDistance0 + road.m_TrafficFlowDistance1) * 16f;
                    float4 float6 = NetUtils.GetTrafficFlowSpeed(road) * 100f;
                    volume[0] += float5.x * 4f / 24f;
                    volume[1] += float5.y * 4f / 24f;
                    volume[2] += float5.z * 4f / 24f;
                    volume[3] += float5.w * 4f / 24f;
                    flow[0] += float6.x;
                    flow[1] += float6.y;
                    flow[2] += float6.z;
                    flow[3] += float6.w;
                }
                if (em.TryGetComponent(edge, out NetCondition netCondition))
                {
                    float2 wear = netCondition.m_Wear;
                    if (wear.x > worstCondition) worstCondition = wear.x;
                    if (wear.y > worstCondition) worstCondition = wear.y;
                    if (wear.x < bestCondition) bestCondition = wear.x;
                    if (wear.y < bestCondition) bestCondition = wear.y;
                    condition += math.csum(wear) * 0.5f;
                }
                if (em.TryGetComponent(edge, out PrefabRef prefabRef)
                    && em.TryGetComponent(prefabRef.m_Prefab, out PlaceableNetData placeable))
                {
                    upkeep += placeable.m_DefaultUpkeepCost;
                }
            }

            if (roadElementCount > 0)
            {
                volume[0] /= roadElementCount;
                volume[1] /= roadElementCount;
                volume[2] /= roadElementCount;
                volume[3] /= roadElementCount;
                volume[4] = volume[0];
                flow[0] /= roadElementCount;
                flow[1] /= roadElementCount;
                flow[2] /= roadElementCount;
                flow[3] /= roadElementCount;
                flow[4] = flow[0];
                bestCondition = 100f - bestCondition / 10f * 100f;
                worstCondition = 100f - worstCondition / 10f * 100f;
                condition = condition / 10f * 100f;
                condition = 100f - condition / roadElementCount;
            }

            SetRoadSectionStat(section, "roadElementCount", roadElementCount);
            SetRoadSectionStat(section, "length", length);
            SetRoadSectionStat(section, "bestCondition", bestCondition);
            SetRoadSectionStat(section, "worstCondition", worstCondition);
            SetRoadSectionStat(section, "condition", condition);
            SetRoadSectionStat(section, "upkeep", upkeep);
        }

        private static void SetRoadSectionStat(RoadSection section, string propertyName, object value)
        {
            PropertyInfo prop = AccessTools.Property(typeof(RoadSection), propertyName);
            prop?.SetValue(section, value);
        }

        private static bool GetVirtualKeyboardLocaleKey_Prefix(
            EntityManager entityManager,
            Entity entity,
            ref string __result)
        {
            if (entityManager.HasComponent<ADRNameAggregate>(entity))
            {
                __result = "RoadName";
                return false;
            }
            return true;
        }

        private static void GetInstanceIcon_Postfix(
            ImageSystem __instance,
            Entity instanceEntity,
            Entity prefabEntity,
            ref string __result)
        {
            if (!string.IsNullOrEmpty(__result))
            {
                return;
            }
            EntityManager em = __instance.EntityManager;
            if (!em.HasComponent<ADRNameAggregate>(instanceEntity))
            {
                return;
            }
            if (!AdrNameAggregateMembershipHelper.TryGetFirstAdrEdgePrefab(em, instanceEntity, out Entity edgePrefab))
            {
                return;
            }
            string icon = __instance.GetIconOrGroupIcon(edgePrefab);
            if (icon != null)
            {
                __result = icon;
            }
        }

        private static bool RaycastNameTooltip_OnUpdate_Prefix(RaycastNameTooltipSystem __instance)
        {
            if (s_toolSystem.activeTool != s_defaultTool
                || !s_toolRaycastSystem.GetRaycastResult(out RaycastResult result)
                || !__instance.EntityManager.HasComponent<ADRNameAggregate>(result.m_Owner)
                || !__instance.EntityManager.TryGetComponent(result.m_Owner, out PrefabRef prefabRef))
            {
                return true;
            }

            NameTooltip tooltip = s_tooltip(__instance);
            tooltip.icon = s_imageSystem.GetInstanceIcon(result.m_Owner, prefabRef.m_Prefab);
            tooltip.entity = result.m_Owner;
            s_addMouseTooltip?.Invoke(__instance, new object[] { tooltip });
            return false;
        }

        private static void ActionsSection_OnProcess_Postfix(ActionsSection __instance)
        {
            Entity selected = __instance.World.GetOrCreateSystemManaged<SelectedInfoUISystem>().selectedEntity;
            if (!__instance.EntityManager.HasComponent<ADRNameAggregate>(selected))
            {
                return;
            }
            PropertyInfo prop = AccessTools.Property(typeof(ActionsSection), "hasTrafficRoutes");
            if (prop != null)
            {
                prop.SetValue(__instance, true);
            }
        }
    }
}
