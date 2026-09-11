using System.Runtime.InteropServices;
using Colossal.Serialization.Entities;
using Unity.Entities;

namespace BelzontAdr
{
    [StructLayout(LayoutKind.Sequential, Size = 1)]
    public struct ADRNameAggregate : IComponentData, IQueryTypeParameter, IEmptySerializable
    {
    }
}
