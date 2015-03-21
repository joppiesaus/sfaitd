using System;

using Super_ForeverAloneInThaDungeon.Spells;

namespace Super_ForeverAloneInThaDungeon
{
    static class SpellGenerator
    {
        /// <summary>
        /// Does the same as GenerateRandomSpellEffects, but chooses the number of effects himself.
        /// </summary>
        public static SpellEffect[] GenerateMultiple()
        {
            byte count = 1;

            // I have no idea what I'm doing
            int chance = 999;
            int less = 8;
            while (Game.ran.Next(0, 999) < chance)
            {
                less *= 9;
                less -= 7 * count;
                chance -= less;
                count++;
            }


            SpellEffect[] effects = new SpellEffect[count];

            for (int i = 0; i < count; i++)
            {
                effects[i] = Generate();
            }

            return effects;
        }

        /// <summary>
        /// Generates fully Game.random spell effects including rarity.
        /// </summary>
        public static SpellEffect Generate()
        {
            int n = Game.ran.Next(0, 1000);
            if (n < 30)
            {
                return GenerateRareSpell();
            }
            else if (n < 222)
            {
                return GenerateUncommonSpell();
            }
            else
            {
                return GenerateCommonSpell();
            }
        }

        public static SpellEffect GenerateCommonSpell()
        {
            switch (Game.ran.Next(0, 2))
            {
                default:
                    return new Heal(Game.ran.Next(3, 8));
                case 1:
                    return new StrengthBoost(Game.ran.Next(2, 5));
            }
        }

        public static SpellEffect GenerateUncommonSpell()
        {
            switch (Game.ran.Next(0, 1))
            {
                default:
                    return new MaxHealthBoost(Game.ran.Next(10, 34) / 10);
            }
        }

        public static SpellEffect GenerateRareSpell()
        {
            switch (Game.ran.Next(0, 3))
            {
                default:
                    return new HitPenalty(Game.ran.Next(15, 21));

                case 2:
                    return new TemporaryImmunity(Game.ran.Next(10, 14 + Game.ran.Next(0, 7)));
            }
        }

        public static SpellEffect[] GenerateMultipleCommon(int n = 2)
        {
            SpellEffect[] eff = new SpellEffect[n];

            for (int i = 0; i < n; i++)
            {
                eff[i] = GenerateCommonSpell();
            }

            return eff;
        }

        public static SpellEffect[] GenerateMultipleUncommon(int n = 2)
        {
            SpellEffect[] eff = new SpellEffect[n];

            for (int i = 0; i < n; i++)
            {
                eff[i] = GenerateUncommonSpell();
            }

            return eff;
        }

        public static SpellEffect[] GenerateMultipleRare(int n = 2)
        {
            SpellEffect[] eff = new SpellEffect[n];

            for (int i = 0; i < n; i++)
            {
                eff[i] = GenerateRareSpell();
            }

            return eff;
        }

        public static SpellEffect[] GenerateRandomSpellEffects(int n = 2)
        {
            SpellEffect[] eff = new SpellEffect[n];

            for (int i = 0; i < n; i++)
            {
                eff[i] = Generate();
            }
            return eff;
        }
    }
}
