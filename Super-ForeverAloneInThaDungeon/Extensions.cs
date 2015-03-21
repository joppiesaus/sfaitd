using Super_ForeverAloneInThaDungeon.Spells;
using System;

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

        /// <summary>
        /// Removes duplicates from a SpellEffect[]
        /// </summary>
        public static SpellEffect[] Compress(SpellEffect[] fx)
        {
            int length = fx.Length;

            for (int i = 0; i < length; i++)
            {
                for (int j = i + 1; j < length; j++)
                {
                    if (fx[i].GetType() == fx[j].GetType())
                    {
                        fx[i].value += fx[j].value;
                        fx[j--] = fx[--length];
                    }
                }
            }

            if (length != fx.Length)
            {
                SpellEffect[] arr = new SpellEffect[length];
                for (int i = 0; i < length; i++)
                {
                    arr[i] = fx[i];
                }
                return arr;
            }
            return fx;
        }

        /// <summary>
        /// Merges and compresses two SpellEffects[]
        /// </summary>
        public static SpellEffect[] Merge(this SpellEffect[] a, SpellEffect[] b)
        {
            SpellEffect[] c = new SpellEffect[a.Length + b.Length];
            for (int i = 0; i < a.Length; i++)
            {
                c[i] = a[i];
            }
            for (int i = 0; i < b.Length; i++)
            {
                c[i + a.Length] = b[i];
            }
            return Compress(c);
        }
    }
}
