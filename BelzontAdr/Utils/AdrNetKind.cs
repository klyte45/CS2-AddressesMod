using Colossal.Entities;
using Game.Net;
using Game.Prefabs;
using Unity.Entities;

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
    }
}
