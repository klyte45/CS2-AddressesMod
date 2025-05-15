using System.Collections.Generic;

namespace BelzontAdr
{
    public static class Adr_WEFn
    {
        public static int GetCurrentLayoutSelected(Dictionary<string, string> vars) => int.TryParse(vars.TryGetValue("$cl#", out var value) ? value : "-1", out int result) ? result : -2;
    }
}
