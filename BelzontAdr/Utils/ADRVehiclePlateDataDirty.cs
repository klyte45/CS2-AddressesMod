using System;

namespace BelzontAdr
{
    public static class ADRTimeUtils
    {
        public static int ToMonthsEpoch(this DateTime dt)
        {
            return (dt.Year * 12) + dt.Month;
        }

    }
}
