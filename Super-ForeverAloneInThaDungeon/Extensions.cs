
namespace Super_ForeverAloneInThaDungeon
{
    static class Extensions
    {
        // Lazyness level: Over 9000!
        public static bool Invert(this bool b)
        {
            /*if (b) return false;
            return true;*/
            /*return (b) ? false : true;*/
            /*return b ^= true;*/
            return !b;
        }

        public static string CapitalizeFirstLetter(this string s)
        {
            return s.Insert(0, s[0].ToString().ToUpper()).Remove(1, 1);
        }
    }
}
