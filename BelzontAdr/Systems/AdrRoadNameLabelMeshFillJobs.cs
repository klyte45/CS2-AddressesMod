using Colossal.Collections;
using Colossal.Mathematics;
using Game.Net;
using Game.Prefabs;
using Game.Rendering;
using Game.Tools;
using Unity.Burst;
using Unity.Collections;
using Unity.Entities;
using Unity.Jobs;
using Unity.Mathematics;
using UnityEngine;
using UnityEngine.Rendering;

namespace BelzontAdr
{
    /// <summary>
    /// LBL-04: Port of AggregateMeshSystem.FillNameDataJob.
    /// Re-diff baseline GUID d124bec16c0748da9ebf48366d9c1d68.
    /// </summary>
    public static class AdrRoadNameLabelMeshFillJobs
    {
        private struct LabelVertexData
        {
            public Vector3 m_Position;
            public Vector3 m_Normal;
            public Color32 m_Color;
            public Vector4 m_UV0;
            public Vector4 m_UV1;
            public Vector4 m_UV2;
            public Vector4 m_UV3;
        }

        private struct SubMeshData
        {
            public int m_VertexCount;
            public Bounds3 m_Bounds;
        }

#if BURST
        [BurstCompile]
#endif
        public struct FillNameDataJob : IJob
        {
            [ReadOnly] public ComponentTypeHandle<LabelExtents> m_LabelExtentsType;
            [ReadOnly] public ComponentTypeHandle<PrefabRef> m_PrefabRefType;
            [ReadOnly] public ComponentTypeHandle<Temp> m_TempType;
            [ReadOnly] public ComponentTypeHandle<Hidden> m_HiddenType;
            [ReadOnly] public BufferTypeHandle<LabelPosition> m_LabelPositionType;
            [ReadOnly] public BufferTypeHandle<LabelVertex> m_LabelVertexType;
            [ReadOnly] public ComponentLookup<NetNameData> m_NetNameData;
            [ReadOnly] public NativeList<ArchetypeChunk> m_Chunks;
            [ReadOnly] public int m_SubMeshCount;
            /// <summary>Used when PrefabRef has no <see cref="NetNameData"/> (e.g. train AggregateNet).</summary>
            [ReadOnly] public Color32 m_FallbackColor;
            [ReadOnly] public Color32 m_FallbackSelectedColor;
            public Mesh.MeshDataArray m_NameMeshData;

            public void Execute()
            {
                NativeArray<SubMeshData> array = new NativeArray<SubMeshData>(m_SubMeshCount, Allocator.Temp);
                for (int i = 0; i < m_Chunks.Length; i++)
                {
                    ArchetypeChunk archetypeChunk = m_Chunks[i];
                    if (archetypeChunk.Has(ref m_HiddenType))
                    {
                        continue;
                    }

                    BufferAccessor<LabelPosition> bufferAccessor = archetypeChunk.GetBufferAccessor(ref m_LabelPositionType);
                    BufferAccessor<LabelVertex> bufferAccessor2 = archetypeChunk.GetBufferAccessor(ref m_LabelVertexType);
                    for (int j = 0; j < bufferAccessor2.Length; j++)
                    {
                        DynamicBuffer<LabelPosition> dynamicBuffer = bufferAccessor[j];
                        DynamicBuffer<LabelVertex> dynamicBuffer2 = bufferAccessor2[j];
                        for (int k = 0; k < dynamicBuffer.Length; k++)
                        {
                            LabelPosition labelPosition = dynamicBuffer[k];
                            for (int l = 0; l < dynamicBuffer2.Length; l += 4)
                            {
                                int2 material = dynamicBuffer2[l].m_Material;
                                int index = math.select(material.x, material.y, labelPosition.m_IsUnderground);
                                array.ElementAt(index).m_VertexCount += 4;
                            }
                        }
                    }
                }

                int num = 0;
                for (int m = 0; m < m_SubMeshCount; m++)
                {
                    num += array[m].m_VertexCount;
                }

                Mesh.MeshData meshData = m_NameMeshData[0];
                NativeArray<VertexAttributeDescriptor> attributes = new NativeArray<VertexAttributeDescriptor>(7, Allocator.Temp, NativeArrayOptions.UninitializedMemory);
                attributes[0] = new VertexAttributeDescriptor(VertexAttribute.Position, VertexAttributeFormat.Float32, 3, 0);
                attributes[1] = new VertexAttributeDescriptor(VertexAttribute.Normal);
                attributes[2] = new VertexAttributeDescriptor(VertexAttribute.Color, VertexAttributeFormat.UNorm8, 4);
                attributes[3] = new VertexAttributeDescriptor(VertexAttribute.TexCoord0, VertexAttributeFormat.Float32, 4);
                attributes[4] = new VertexAttributeDescriptor(VertexAttribute.TexCoord1, VertexAttributeFormat.Float32, 4);
                attributes[5] = new VertexAttributeDescriptor(VertexAttribute.TexCoord2, VertexAttributeFormat.Float32, 4);
                attributes[6] = new VertexAttributeDescriptor(VertexAttribute.TexCoord3, VertexAttributeFormat.Float32, 4);
                meshData.SetVertexBufferParams(num, attributes);
                meshData.SetIndexBufferParams((num >> 2) * 6, IndexFormat.UInt32);
                attributes.Dispose();

                num = 0;
                meshData.subMeshCount = m_SubMeshCount;
                for (int n = 0; n < m_SubMeshCount; n++)
                {
                    ref SubMeshData reference = ref array.ElementAt(n);
                    meshData.SetSubMesh(n, new SubMeshDescriptor
                    {
                        firstVertex = num,
                        indexStart = (num >> 2) * 6,
                        vertexCount = reference.m_VertexCount,
                        indexCount = (reference.m_VertexCount >> 2) * 6,
                        topology = MeshTopology.Triangles
                    }, MeshUpdateFlags.DontValidateIndices | MeshUpdateFlags.DontNotifyMeshUsers | MeshUpdateFlags.DontRecalculateBounds);
                    num += reference.m_VertexCount;
                    reference.m_VertexCount = 0;
                    reference.m_Bounds = new Bounds3(float.MaxValue, float.MinValue);
                }

                NativeArray<LabelVertexData> vertexData = meshData.GetVertexData<LabelVertexData>();
                NativeArray<uint> indexData = meshData.GetIndexData<uint>();
                LabelVertexData value = default;
                for (int num2 = 0; num2 < m_Chunks.Length; num2++)
                {
                    ArchetypeChunk archetypeChunk2 = m_Chunks[num2];
                    if (archetypeChunk2.Has(ref m_HiddenType))
                    {
                        continue;
                    }

                    NativeArray<LabelExtents> nativeArray = archetypeChunk2.GetNativeArray(ref m_LabelExtentsType);
                    NativeArray<PrefabRef> nativeArray2 = archetypeChunk2.GetNativeArray(ref m_PrefabRefType);
                    NativeArray<Temp> nativeArray3 = archetypeChunk2.GetNativeArray(ref m_TempType);
                    BufferAccessor<LabelPosition> bufferAccessor3 = archetypeChunk2.GetBufferAccessor(ref m_LabelPositionType);
                    BufferAccessor<LabelVertex> bufferAccessor4 = archetypeChunk2.GetBufferAccessor(ref m_LabelVertexType);
                    for (int num3 = 0; num3 < bufferAccessor4.Length; num3++)
                    {
                        LabelExtents labelExtents = nativeArray[num3];
                        DynamicBuffer<LabelPosition> dynamicBuffer3 = bufferAccessor3[num3];
                        DynamicBuffer<LabelVertex> dynamicBuffer4 = bufferAccessor4[num3];
                        Color32 color = m_FallbackColor;
                        Color32 selectedColor = m_FallbackSelectedColor;
                        if (nativeArray2.Length > num3)
                        {
                            PrefabRef prefabRef = nativeArray2[num3];
                            if (m_NetNameData.TryGetComponent(prefabRef.m_Prefab, out NetNameData netNameData))
                            {
                                color = netNameData.m_Color;
                                selectedColor = netNameData.m_SelectedColor;
                            }
                        }

                        if (nativeArray3.Length != 0
                            && (nativeArray3[num3].m_Flags & (TempFlags.Create | TempFlags.Delete | TempFlags.Select | TempFlags.Modify | TempFlags.Replace)) != 0)
                        {
                            color = selectedColor;
                        }

                        float num4 = math.length(math.max(-labelExtents.m_Bounds.min, labelExtents.m_Bounds.max));
                        for (int num5 = 0; num5 < dynamicBuffer3.Length; num5++)
                        {
                            LabelPosition labelPosition2 = dynamicBuffer3[num5];
                            float3 float5 = new float3(labelPosition2.m_HalfLength, 0f, 0f);
                            float2 xy = labelPosition2.m_Curve.a.xy;
                            float2 xy2 = labelPosition2.m_Curve.b.xy;
                            float4 float6 = new float4(labelPosition2.m_Curve.c, labelPosition2.m_Curve.a.z);
                            float4 float7 = new float4(labelPosition2.m_Curve.d, labelPosition2.m_Curve.b.z);
                            float3 float8 = MathUtils.Position(labelPosition2.m_Curve, 0.5f);
                            float num6 = num4 * labelPosition2.m_MaxScale;
                            Bounds3 bounds = new Bounds3(float8 - num6, float8 + num6);
                            SubMeshDescriptor subMeshDescriptor = default;
                            int num7 = -1;
                            for (int num8 = 0; num8 < dynamicBuffer4.Length; num8 += 4)
                            {
                                int2 material2 = dynamicBuffer4[num8].m_Material;
                                int num9 = math.select(material2.x, material2.y, labelPosition2.m_IsUnderground);
                                ref SubMeshData reference2 = ref array.ElementAt(num9);
                                if (num9 != num7)
                                {
                                    subMeshDescriptor = meshData.GetSubMesh(num9);
                                    reference2.m_Bounds |= bounds;
                                    num7 = num9;
                                }

                                int num10 = subMeshDescriptor.firstVertex + reference2.m_VertexCount;
                                int num11 = subMeshDescriptor.indexStart + (reference2.m_VertexCount >> 2) * 6;
                                reference2.m_VertexCount += 4;
                                indexData[num11] = (uint)num10;
                                indexData[num11 + 1] = (uint)(num10 + 1);
                                indexData[num11 + 2] = (uint)(num10 + 2);
                                indexData[num11 + 3] = (uint)(num10 + 2);
                                indexData[num11 + 4] = (uint)(num10 + 3);
                                indexData[num11 + 5] = (uint)num10;
                                for (int num12 = 0; num12 < 4; num12++)
                                {
                                    LabelVertex labelVertex = dynamicBuffer4[num8 + num12];
                                    value.m_Position = labelVertex.m_Position;
                                    value.m_Normal = float5;
                                    value.m_Color = new Color32(
                                        (byte)(labelVertex.m_Color.r * color.r >> 8),
                                        (byte)(labelVertex.m_Color.g * color.g >> 8),
                                        (byte)(labelVertex.m_Color.b * color.b >> 8),
                                        (byte)(labelVertex.m_Color.a * color.a >> 8));
                                    value.m_UV0 = new float4(labelVertex.m_UV0, xy);
                                    value.m_UV1 = new float4(labelPosition2.m_MaxScale, labelVertex.m_UV1.y, xy2);
                                    value.m_UV2 = float6;
                                    value.m_UV3 = float7;
                                    vertexData[num10 + num12] = value;
                                }
                            }
                        }
                    }
                }

                for (int num13 = 0; num13 < m_SubMeshCount; num13++)
                {
                    SubMeshDescriptor subMesh = meshData.GetSubMesh(num13);
                    subMesh.bounds = RenderingUtils.ToBounds(array[num13].m_Bounds);
                    meshData.SetSubMesh(num13, subMesh, MeshUpdateFlags.DontValidateIndices | MeshUpdateFlags.DontNotifyMeshUsers | MeshUpdateFlags.DontRecalculateBounds);
                }

                array.Dispose();
            }
        }
    }
}
