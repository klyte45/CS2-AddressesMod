using Belzont.Utils;
using Game.Prefabs;
using System;
using System.Collections.Generic;
using Unity.Entities;

namespace BelzontAdr
{
    [ComponentMenu("Addresses/", new Type[]
{
    typeof(StaticObjectPrefab)
})]
    public class ADRHighwayPassingThroughMarker : ComponentBase
    {
        public override void GetArchetypeComponents(HashSet<ComponentType> components)
        {
            components.Add(ComponentType.ReadWrite<ADRHighwayPassingThroughMarkerData>());
        }

        public override void GetPrefabComponents(HashSet<ComponentType> components)
        {
        }
    }
}
