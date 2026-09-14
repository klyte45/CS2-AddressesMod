using Colossal.Entities;
using System;
using Unity.Entities;

namespace BelzontAdr.Bridge
{
    [Obsolete("Don't reference methods on this class directly. Always use reverse patch to access them, and don't use this mod DLL as hard dependency of your own mod.", true)]
    public static class RoadMarkerInfoBridge
    {
        private static AdrHighwayRoutesSystem m_highwaySystem;

        /// <summary>
        /// Deconstructs highway context for a marker entity.
        /// Route id + direction resolve from the owning ADR aggregate
        /// (marker → attached edge → <see cref="ADRNameAggregated"/> → <see cref="ADRNameAggregateHighwayData"/>).
        /// Custom numeric params still come from visual-only <see cref="ADRHighwayMarkerData"/>.
        /// When the edge has no ADR highway association, route/direction are defaults.
        /// </summary>
        public static (Colossal.Hash128 routeDataIndex, int routeDirection, int numericCustomParam1, int numericCustomParam2) DesconstructRoadMarkerData(Entity e)
        {
            var em = World.DefaultGameObjectInjectionWorld.EntityManager;
            if (!em.TryGetComponent<ADRHighwayMarkerData>(e, out var component))
            {
                return default;
            }

            Colossal.Hash128 routeId = default;
            int routeDirection = (int)ADRNameAggregateHighwayData.RouteDirection.NONE;
            if (AdrHighwayMarkerWEFn.TryResolveAdrHighwayFromMarker(e, out _, out _, out _, out var highway))
            {
                routeId = highway.highwayDataId;
                routeDirection = (int)highway.routeDirection;
            }

            return (routeId, routeDirection, component.numericCustomParam1, component.numericCustomParam2);
        }

        public static (string prefix, string suffix, string fullName) GetHighwayRouteNamings(Colossal.Hash128 routeIndex)
        {
            m_highwaySystem ??= World.DefaultGameObjectInjectionWorld.GetOrCreateSystemManaged<AdrHighwayRoutesSystem>();
            return m_highwaySystem.TryGetHighwayData(routeIndex, out var data)
                ? (data.prefix, data.suffix, data.name)
                : ("??", "???", "??????????????????????????");
        }
    }
}
