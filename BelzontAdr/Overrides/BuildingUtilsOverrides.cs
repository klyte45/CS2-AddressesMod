using Belzont.Utils;
using Colossal.Entities;
using Game.Buildings;
using Game.Net;
using Game.Tools;
using Unity.Entities;

namespace BelzontAdr
{
    public class BuildingUtilsOverrides : Redirector, IRedirectable
    {
        public void DoPatches(World world)
        {
            adrMainSystem = world.GetOrCreateSystemManaged<AdrMainSystem>();
            highwayRoutesSystem = world.GetOrCreateSystemManaged<AdrHighwayRoutesSystem>();

            var GetAddress = typeof(BuildingUtils).GetMethod("GetAddress", RedirectorUtils.allFlags, null, new[]
            {
                typeof(EntityManager),
                typeof(Entity),
                typeof(Entity),
                typeof(float),
                typeof(Entity).MakeByRefType(),
                typeof(int).MakeByRefType()
            }, null);
            AddRedirect(GetAddress, GetType().GetMethod(nameof(GetAddress_Override), RedirectorUtils.allFlags));
        }

        private static bool GetAddress_Override(ref bool __result, EntityManager entityManager, Entity entity, Entity edge, float curvePos, ref Entity road, ref int number)
        {
            if (entityManager.TryGetComponent<Temp>(edge, out var temp) && temp.m_Original != Entity.Null)
            {
                edge = temp.m_Original;
                curvePos = temp.m_CurvePosition;
            }

            if (AdrRoadAddressNumbering.TryGetAddress(
                    entityManager, adrMainSystem, highwayRoutesSystem, entity, edge, curvePos, out road, out number))
            {
                __result = true;
                return false;
            }

            __result = false;
            return false;
        }

        private static AdrMainSystem adrMainSystem;
        private static AdrHighwayRoutesSystem highwayRoutesSystem;
    }
}
