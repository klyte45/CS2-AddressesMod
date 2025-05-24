using Colossal.Entities;
using System;
using Unity.Entities;

namespace BelzontAdr.Bridge
{
    [Obsolete("Don't reference methods on this class directly. Always use reverse patch to access them, and don't use this mod DLL as hard dependency of your own mod.", true)]
    public static class RoadMarkerInfoBridge
    {
        public static (Colossal.Hash128 routeDataIndex, int routeDirection, int numericCustomParam1, int numericCustomParam2) DesconstructRoadMarkerData(Entity e)
            => !World.DefaultGameObjectInjectionWorld.EntityManager.TryGetComponent<ADRHighwayMarkerData>(e, out var component)
                ? default
                : (component.routeDataIndex, (int)component.routeDirection, component.numericCustomParam1, component.numericCustomParam2);

        public static (string prefix, string suffix, string fullName) GetHighwayRouteNamings(Colossal.Hash128 routeIndex) => ("XX", "000", "<TBD>");
    }
}