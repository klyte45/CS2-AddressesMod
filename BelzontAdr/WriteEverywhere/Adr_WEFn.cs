using System.Collections.Generic;
using Unity.Entities;

namespace BelzontAdr
{
    public static class Adr_WEFn
    {
        private static int CheckCurrentPropIs(Dictionary<string, string> vars, string value) => vars.TryGetValue("$typ", out var type) && type == value ? 1 : 0;
        public static int GetCurrentLayoutSelected(Entity e, Dictionary<string, string> vars) => int.TryParse(vars.TryGetValue("$cl#", out var value) ? value : "-1", out int result) ? result : -2;
        public static int CheckCurrentPropIsDiamondSign(Entity e, Dictionary<string, string> vars) => CheckCurrentPropIs(vars, "dmd");
        public static int CheckCurrentPropIsHorizontalSign(Entity e, Dictionary<string, string> vars) => CheckCurrentPropIs(vars, "hrt");
        public static int CheckCurrentPropIsOctogonalSign(Entity e, Dictionary<string, string> vars) => CheckCurrentPropIs(vars, "oct");
        public static int CheckCurrentPropIsRoundSign(Entity e, Dictionary<string, string> vars) => CheckCurrentPropIs(vars, "rnd");
        public static int CheckCurrentPropIsShieldSign(Entity e, Dictionary<string, string> vars) => CheckCurrentPropIs(vars, "shd");
        public static int CheckCurrentPropIsSquareSign(Entity e, Dictionary<string, string> vars) => CheckCurrentPropIs(vars, "sqr");
        public static int CheckCurrentPropIsTriangleDownSign(Entity e, Dictionary<string, string> vars) => CheckCurrentPropIs(vars, "tdw");
        public static int CheckCurrentPropIsTriangleUpSign(Entity e, Dictionary<string, string> vars) => CheckCurrentPropIs(vars, "tup");
        public static int CheckCurrentPropIsVerticalSign(Entity e, Dictionary<string, string> vars) => CheckCurrentPropIs(vars, "vrt");

    }
}
