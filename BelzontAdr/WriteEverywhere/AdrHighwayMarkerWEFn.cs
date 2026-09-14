using Belzont.Utils;
using Colossal.Entities;
using Game.Objects;
using Game.Tools;
using System.Text;
using Unity.Entities;

namespace BelzontAdr
{
    /// <summary>
    /// WE formulae entry points for highway markers.
    /// Resolve path: marker → <see cref="Attached"/> edge → <see cref="ADRNameAggregated"/> →
    /// <see cref="ADRNameAggregateHighwayData"/> → highway registry.
    /// Markers stay visual-only; route/mileage/direction are not read from <see cref="ADRHighwayMarkerData"/>.
    /// </summary>
    public static class AdrHighwayMarkerWEFn
    {
        private static AdrHighwayRoutesSystem m_highwayRoutes;
        private static AdrMainSystem m_adrMain;

        private static AdrHighwayRoutesSystem HighwayRoutes =>
            m_highwayRoutes ??= World.DefaultGameObjectInjectionWorld.GetOrCreateSystemManaged<AdrHighwayRoutesSystem>();

        private static AdrMainSystem AdrMain =>
            m_adrMain ??= World.DefaultGameObjectInjectionWorld.GetOrCreateSystemManaged<AdrMainSystem>();

        /// <summary>
        /// Shared resolve used by WE formulae and <see cref="Bridge.RoadMarkerInfoBridge"/>.
        /// </summary>
        public static bool TryResolveAdrHighwayFromMarker(
            Entity marker,
            out Entity attachedEdge,
            out float curvePos,
            out Entity adrAggregate,
            out ADRNameAggregateHighwayData highway)
        {
            attachedEdge = Entity.Null;
            curvePos = 0f;
            adrAggregate = Entity.Null;
            highway = default;

            var em = World.DefaultGameObjectInjectionWorld.EntityManager;
            if (!TryGetAttachedNetEdge(em, marker, out attachedEdge, out curvePos))
            {
                return false;
            }

            if (!AdrRoadAddressNumbering.TryGetAdrAggregate(em, attachedEdge, out adrAggregate)
                || !em.TryGetComponent(adrAggregate, out highway))
            {
                return false;
            }

            return true;
        }

        public static Colossal.Hash128 GetHighwayDataId(Entity marker)
            => TryResolveAdrHighwayFromMarker(marker, out _, out _, out _, out var highway)
                ? highway.highwayDataId
                : default;

        public static int GetRouteDirection(Entity marker)
            => TryResolveAdrHighwayFromMarker(marker, out _, out _, out _, out var highway)
                ? (int)highway.routeDirection
                : (int)ADRNameAggregateHighwayData.RouteDirection.NONE;

        public static string GetRoutePrefix(Entity marker)
        {
            var id = GetHighwayDataId(marker);
            return id == default
                ? string.Empty
                : HighwayRoutes.TryGetHighwayData(id, out var data) ? data.prefix ?? string.Empty : string.Empty;
        }

        public static string GetRouteSuffix(Entity marker)
        {
            var id = GetHighwayDataId(marker);
            return id == default
                ? string.Empty
                : HighwayRoutes.TryGetHighwayData(id, out var data) ? data.suffix ?? string.Empty : string.Empty;
        }

        public static string GetRouteFullName(Entity marker)
        {
            var id = GetHighwayDataId(marker);
            return id == default
                ? string.Empty
                : HighwayRoutes.TryGetHighwayData(id, out var data) ? data.name ?? string.Empty : string.Empty;
        }

        public static string GetRouteDirectionLabel(Entity marker)
        {
            if (!TryResolveAdrHighwayFromMarker(marker, out _, out _, out _, out var highway)
                || highway.routeDirection == ADRNameAggregateHighwayData.RouteDirection.NONE)
            {
                return string.Empty;
            }

            return LocalizationExtensions.Translate($"K45::ADR.vuio[RouteDirection.{highway.routeDirection}]");
        }

        /// <summary>
        /// Formatted mileage at the marker attachment point (game unit system), or empty when not on an ADR highway.
        /// </summary>
        public static string GetMileageLabel(Entity marker)
        {
            if (!TryResolveAdrHighwayFromMarker(marker, out var edge, out var curvePos, out _, out _))
            {
                return string.Empty;
            }

            var em = World.DefaultGameObjectInjectionWorld.EntityManager;
            if (!AdrRoadAddressNumbering.TryGetDistanceAlongAdr(
                    em, AdrMain, HighwayRoutes, edge, curvePos, out float distanceAlong, out var hwCtx))
            {
                return string.Empty;
            }

            bool freedom = AdrRoadAddressNumbering.IsFreedomUnitSystem();
            float metersPerMajor = freedom ? (AdrRoadAddressNumbering.HalfMileMeters * 2f) : 1000f;
            float major = AdrRoadAddressNumbering.ComputeMileageMajorUnits(distanceAlong, hwCtx, metersPerMajor);
            return AdrRoadAddressNumbering.FormatMileageLabel(freedom, major);
        }

        /// <summary>
        /// Compact multi-line summary for ORIGINAL display: route id, mileage, direction.
        /// Empty lines omitted when ADR highway context is missing.
        /// </summary>
        public static string GetHighwaySummary(Entity marker)
        {
            var sb = new StringBuilder(64);
            string prefix = GetRoutePrefix(marker);
            string suffix = GetRouteSuffix(marker);
            string routeLine = $"{prefix}{suffix}".Trim();
            if (string.IsNullOrEmpty(routeLine))
            {
                routeLine = GetRouteFullName(marker);
            }

            if (!string.IsNullOrEmpty(routeLine))
            {
                sb.Append(routeLine);
            }

            string mileage = GetMileageLabel(marker);
            if (!string.IsNullOrEmpty(mileage))
            {
                if (sb.Length > 0) sb.Append('\n');
                sb.Append(mileage);
            }

            string direction = GetRouteDirectionLabel(marker);
            if (!string.IsNullOrEmpty(direction))
            {
                if (sb.Length > 0) sb.Append('\n');
                sb.Append(direction);
            }

            return sb.Length > 0 ? sb.ToString() : string.Empty;
        }

        private static bool TryGetAttachedNetEdge(EntityManager em, Entity marker, out Entity edge, out float curvePos)
        {
            edge = Entity.Null;
            curvePos = 0f;
            if (!em.TryGetComponent<Attached>(marker, out var attached) || attached.m_Parent == Entity.Null)
            {
                return false;
            }

            edge = attached.m_Parent;
            curvePos = attached.m_CurvePosition;
            if (em.TryGetComponent(edge, out Temp temp) && temp.m_Original != Entity.Null)
            {
                edge = temp.m_Original;
            }

            return em.Exists(edge);
        }
    }
}
