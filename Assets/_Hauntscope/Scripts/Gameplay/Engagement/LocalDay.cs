using System;

namespace Hauntscope.Gameplay.Engagement
{
    public static class LocalDay
    {
        // yyyymmdd: compact, readable in the save file, ordered, and changes exactly at local midnight.
        public static int Key(DateTime date)
        {
            return date.Year * 10000 + date.Month * 100 + date.Day;
        }
    }
}
