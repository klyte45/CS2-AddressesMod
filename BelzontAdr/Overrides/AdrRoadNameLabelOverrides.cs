using Belzont.Utils;
using Game.Rendering;
using Game.Tools;
using System.Collections.Generic;
using Unity.Entities;
using Unity.Jobs;
using UnityEngine;
using UnityEngine.Rendering;

namespace BelzontAdr
{
    /// <summary>
    /// LBL-01: Always-on shutdown of vanilla road <b>name</b> mesh bake/draw.
    /// Leaves AggregateSystem membership and arrow mesh/render paths untouched.
    /// Re-diff baseline Game.dll GUID d124bec16c0748da9ebf48366d9c1d68 —
    /// AggregateMeshSystem.OnUpdate name half + AggregateRenderSystem.Render.
    /// </summary>
    public class AdrRoadNameLabelOverrides : Redirector, IRedirectable
    {
        private static ToolSystem s_toolSystem;

        public void DoPatches(World world)
        {
            s_toolSystem = world.GetOrCreateSystemManaged<ToolSystem>();

            var meshType = typeof(AggregateMeshSystem);
            AddRedirect(
                meshType.GetMethod("UpdateLabelVertices", RedirectorUtils.allFlags, null, new[] { typeof(bool) }, null),
                GetType().GetMethod(nameof(UpdateLabelVertices_Prefix), RedirectorUtils.allFlags));
            AddRedirect(
                meshType.GetMethod("UpdateLabelPositions", RedirectorUtils.allFlags, null, new[] { typeof(JobHandle), typeof(bool) }, null),
                GetType().GetMethod(nameof(UpdateLabelPositions_Prefix), RedirectorUtils.allFlags));
            AddRedirect(
                meshType.GetMethod("FillNameMeshData", RedirectorUtils.allFlags, null, new[] { typeof(JobHandle) }, null),
                GetType().GetMethod(nameof(FillNameMeshData_Prefix), RedirectorUtils.allFlags));

            AddRedirect(
                typeof(AggregateRenderSystem).GetMethod("Render", RedirectorUtils.allFlags, null,
                    new[] { typeof(ScriptableRenderContext), typeof(List<Camera>) }, null),
                GetType().GetMethod(nameof(AggregateRenderSystem_Render_Prefix), RedirectorUtils.allFlags));
        }

        /// <summary>Skip TMP bake into LabelVertex / LabelExtents.</summary>
        private static bool UpdateLabelVertices_Prefix() => false;

        /// <summary>Skip placement job; pass through dependency for CombineDependencies.</summary>
        private static bool UpdateLabelPositions_Prefix(JobHandle inputDeps, ref JobHandle __result)
        {
            __result = inputDeps;
            return false;
        }

        /// <summary>Skip name mesh batch fill; pass through dependency.</summary>
        private static bool FillNameMeshData_Prefix(JobHandle inputDeps, ref JobHandle __result)
        {
            __result = inputDeps;
            return false;
        }

        /// <summary>
        /// Skip name GetNameMesh/DrawMesh loop. When a tool needs net arrows, run the original
        /// Render (arrow branch then early return before names).
        /// </summary>
        private static bool AggregateRenderSystem_Render_Prefix()
        {
            return s_toolSystem != null
                && s_toolSystem.activeTool != null
                && s_toolSystem.activeTool.requireNetArrows;
        }
    }
}
