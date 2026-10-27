using Game;
using Game.Rendering;
using Game.Tools;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.Scripting;

namespace BelzontAdr
{
    /// <summary>
    /// LBL-04: Name-only DrawMesh path mirroring AggregateRenderSystem name loop.
    /// Does not draw arrows. Re-diff baseline GUID d124bec16c0748da9ebf48366d9c1d68.
    /// </summary>
    public partial class AdrRoadNameLabelRenderSystem : GameSystemBase
    {
        private AdrRoadNameLabelMeshSystem m_MeshSystem;
        private RenderingSystem m_RenderingSystem;
        private ToolSystem m_ToolSystem;

        [Preserve]
        protected override void OnCreate()
        {
            base.OnCreate();
            m_MeshSystem = World.GetOrCreateSystemManaged<AdrRoadNameLabelMeshSystem>();
            m_RenderingSystem = World.GetOrCreateSystemManaged<RenderingSystem>();
            m_ToolSystem = World.GetOrCreateSystemManaged<ToolSystem>();
            RenderPipelineManager.beginContextRendering += Render;
        }

        [Preserve]
        protected override void OnDestroy()
        {
            RenderPipelineManager.beginContextRendering -= Render;
            base.OnDestroy();
        }

        [Preserve]
        protected override void OnUpdate()
        {
        }

        private void Render(ScriptableRenderContext context, List<Camera> cameras)
        {
            if (m_RenderingSystem.hideOverlay || !Enabled)
            {
                return;
            }

            // Parity with AggregateRenderSystem: when arrows are required, names are not drawn.
            if (m_ToolSystem.activeTool != null && m_ToolSystem.activeTool.requireNetArrows)
            {
                return;
            }

            int nameMaterialCount = m_MeshSystem.GetNameMaterialCount();
            for (int i = 0; i < nameMaterialCount; i++)
            {
                if (!m_MeshSystem.TryGetNameMesh(i, out Mesh mesh, out int subMeshCount))
                {
                    continue;
                }

                for (int j = 0; j < subMeshCount; j++)
                {
                    if (!m_MeshSystem.TryGetNameMaterial(i, j, out Material material))
                    {
                        continue;
                    }

                    foreach (Camera camera in cameras)
                    {
                        if (camera.cameraType == CameraType.Game || camera.cameraType == CameraType.SceneView)
                        {
                            Graphics.DrawMesh(mesh, Matrix4x4.identity, material, 0, camera, j, null, castShadows: false, receiveShadows: false);
                        }
                    }
                }
            }
        }
    }
}
