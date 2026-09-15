using Colossal.Entities;
using Game.Net;
using Game.Prefabs;
using Unity.Entities;
using UnityEngine;

namespace BelzontAdr
{
    /// <summary>
    /// Prefab-first net kind for ADR name-aggregates.
    /// Compatibility is exact enum equality (no TrainTram super-kind).
    /// </summary>
    public enum AdrNetKind : byte
    {
        None = 0,
        Road,
        Pathway,
        Train,
        Tram,
        Subway,
        Waterway,
        Taxiway
    }

    public static class AdrNetKindUtil
    {
        /// <summary>Face / text color for ADR NetNameData and ADR-style material clones (#FFFFFFC0).</summary>
        public static readonly Color32 AdrFaceColor32 = new(0xFF, 0xFF, 0xFF, 0xC0);

        public static readonly Color AdrFaceColor = new(1f, 1f, 1f, 0xC0 / 255f);

        /// <summary>
        /// Outline colors for ADR-style name-label material clones (RefsLibrary 23).
        /// <see cref="AdrNetKind.None"/> returns default (caller should not apply ADR outline).
        /// </summary>
        public static Color GetOutlineColor(AdrNetKind kind) => kind switch
        {
            AdrNetKind.Subway => HexOutline(0x00, 0x44, 0x00),
            AdrNetKind.Road => HexOutline(0x33, 0x33, 0x33),
            AdrNetKind.Pathway => HexOutline(0x66, 0x66, 0x66),
            AdrNetKind.Train => HexOutline(0x44, 0x22, 0x00),
            AdrNetKind.Tram => HexOutline(0x33, 0x00, 0x33),
            AdrNetKind.Waterway => HexOutline(0x00, 0x22, 0x44),
            AdrNetKind.Taxiway => HexOutline(0x33, 0x00, 0x1A),
            _ => default
        };

        private static Color HexOutline(byte r, byte g, byte b) => new(r / 255f, g / 255f, b / 255f, 1f);

        /// <summary>Non-road kinds that need ADR-owned NetNameData prefabs (Road uses vanilla AggregateNet).</summary>
        public static readonly AdrNetKind[] NonRoadKindsWithAdrNamePrefab =
        {
            AdrNetKind.Pathway,
            AdrNetKind.Train,
            AdrNetKind.Tram,
            AdrNetKind.Subway,
            AdrNetKind.Waterway,
            AdrNetKind.Taxiway
        };

        /// <summary>
        /// Resolve kind from an edge's prefab <c>*Data</c>. Never gates on edge <see cref="Road"/>.
        /// Order: RoadData → PathwayData → TrackData → WaterwayData → TaxiwayData.
        /// </summary>
        public static bool TryGetNetKind(EntityManager entityManager, Entity edge, out AdrNetKind kind)
        {
            kind = AdrNetKind.None;
            if (edge == Entity.Null
                || !entityManager.Exists(edge)
                || !entityManager.TryGetComponent(edge, out PrefabRef prefabRef)
                || prefabRef.m_Prefab == Entity.Null)
            {
                return false;
            }

            Entity prefab = prefabRef.m_Prefab;

            if (entityManager.HasComponent<RoadData>(prefab))
            {
                kind = AdrNetKind.Road;
                return true;
            }

            if (entityManager.HasComponent<PathwayData>(prefab))
            {
                kind = AdrNetKind.Pathway;
                return true;
            }

            if (entityManager.TryGetComponent(prefab, out TrackData trackData))
            {
                kind = MapTrackType(trackData.m_TrackType);
                return kind != AdrNetKind.None;
            }

            if (entityManager.HasComponent<WaterwayData>(prefab))
            {
                kind = AdrNetKind.Waterway;
                return true;
            }

            if (entityManager.HasComponent<TaxiwayData>(prefab))
            {
                kind = AdrNetKind.Taxiway;
                return true;
            }

            return false;
        }

        /// <summary>
        /// TrackTypes.Subway alone → Subway; Tram alone → Tram; Train or Train|Tram → Train.
        /// </summary>
        public static AdrNetKind MapTrackType(TrackTypes trackType)
        {
            bool train = (trackType & TrackTypes.Train) != 0;
            bool tram = (trackType & TrackTypes.Tram) != 0;
            bool subway = (trackType & TrackTypes.Subway) != 0;

            if (subway && !train && !tram)
            {
                return AdrNetKind.Subway;
            }

            if (tram && !train && !subway)
            {
                return AdrNetKind.Tram;
            }

            if (train)
            {
                return AdrNetKind.Train;
            }

            if (tram)
            {
                return AdrNetKind.Tram;
            }

            if (subway)
            {
                return AdrNetKind.Subway;
            }

            return AdrNetKind.None;
        }

        /// <summary>Locale id for the default display name of an aggregable kind (vanilla AggregateNet has no usable translation).</summary>
        public static string GetDefaultNameLocaleId(AdrNetKind kind) => kind switch
        {
            AdrNetKind.Road => "K45::ADR.main[netKind.Road]",
            AdrNetKind.Pathway => "K45::ADR.main[netKind.Pathway]",
            AdrNetKind.Train => "K45::ADR.main[netKind.Train]",
            AdrNetKind.Tram => "K45::ADR.main[netKind.Tram]",
            AdrNetKind.Subway => "K45::ADR.main[netKind.Subway]",
            AdrNetKind.Waterway => "K45::ADR.main[netKind.Waterway]",
            AdrNetKind.Taxiway => "K45::ADR.main[netKind.Taxiway]",
            _ => null
        };

        /// <summary>True when the edge classifies as <see cref="AdrNetKind.Road"/> via prefab <see cref="RoadData"/>.</summary>
        public static bool IsRoadEdge(EntityManager entityManager, Entity edge)
            => TryGetNetKind(entityManager, edge, out AdrNetKind kind) && kind == AdrNetKind.Road;

        /// <summary>
        /// Prefer stored <see cref="ADRNameAggregate.m_Kind"/>; otherwise classify <paramref name="refEdge"/>.
        /// </summary>
        public static bool TryResolveOwnerKind(
            EntityManager entityManager,
            Entity owner,
            Entity refEdge,
            out AdrNetKind kind)
        {
            kind = AdrNetKind.None;
            if (owner != Entity.Null
                && entityManager.Exists(owner)
                && entityManager.TryGetComponent(owner, out ADRNameAggregate host)
                && host.m_Kind != AdrNetKind.None)
            {
                kind = host.m_Kind;
                return true;
            }

            return refEdge != Entity.Null && TryGetNetKind(entityManager, refEdge, out kind);
        }
    }
}
