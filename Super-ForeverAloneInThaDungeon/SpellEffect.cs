using System;

namespace Super_ForeverAloneInThaDungeon.Spells
{
    abstract class SpellEffect
    {
        protected int value;

        public abstract string Name { get; }
        public abstract string HelpDescription { get; }

        public virtual bool LessIsGood { get { return false; } }
        public virtual ConsoleColor LabelColor { get { return ConsoleColor.DarkCyan; } }

        public SpellEffect() { }
        public SpellEffect(int val) { this.value = val; }

        public virtual IIAI GenerateInventoryInfo()
        {
            return new IIAID(Name, ToString(), LabelColor, GetColor());
        }

        public abstract void Apply(ref Creature c);


        public override string ToString()
        {
            return value < 0 ? value.ToString() : '+' + value.ToString();
        }
        public static string ToString(int val)
        {
            return val < 0 ? val.ToString() : '+' + val.ToString();
        }

        public ConsoleColor GetColor()
        {
            return (LessIsGood ? value <= 0 : value >= 0) ? ConsoleColor.Green : ConsoleColor.Red;
        }

        public static string ToPercent(int val)
        {
            return val > 0 ? '+' + Constants.ToPercent((short)val) : Constants.ToPercent((short)val);
        }
    }

    abstract class SpellEffectPercent : SpellEffect
    {
        public SpellEffectPercent(int val) { this.value = val; }

        public override IIAI GenerateInventoryInfo()
        {
            return new IIAID(Name, ToPercent(value), LabelColor, GetColor());
        }
    }

    abstract class SpellEffectTemporaryEffect : SpellEffect
    {
        public SpellEffectTemporaryEffect(int val) { this.value = val; }

        public override IIAI GenerateInventoryInfo()
        {
            return new IIAID(Name, ToString() + " moves", LabelColor, GetColor());
        }
    }



    class HitPenalty : SpellEffectPercent
    {
        public override string Name { get { return "Hard to hit"; } }
        public override string HelpDescription { get { return "When applied, the target creature has value percentage points less chance of getting hit"; } }

        public override ConsoleColor LabelColor { get { return ConsoleColor.Yellow; } }
        public override bool LessIsGood { get { return true; } }

        public HitPenalty(int val) : base(-val) { }


        public override void Apply(ref Creature c)
        {
            if (c.hitPenalty == null) c.hitPenalty = 0;
            c.hitPenalty -= (short)value;
            if ((short)c.hitPenalty > 400) c.hitPenalty = 400;
            Game.Message("Your chance of getting hit has decreased by " + Constants.ToPercent((short)-value));
        }
    }

    class MaxHealthBoost : SpellEffect
    {
        public override string Name { get { return "Enhanced Body"; } }
        public override string HelpDescription { get { return "Player's max health is increased by the value"; } }

        public MaxHealthBoost(int val) : base(val) { }

        public override void Apply(ref Creature c)
        {
            c.maxHealth += (ushort)value;
            if (c is Player) Game.Message("Your max health has been increased by " + value);
            else Game.Message("The " + c + " casted " + Name + ", max health has been increased by " + value);
        }
    }

    class Heal : SpellEffect
    {
        public override string Name { get { return "Heal"; } }
        public override string HelpDescription { get { return "Players's health is increased by the value"; } }

        public override ConsoleColor LabelColor { get { return ConsoleColor.Red; } }

        public Heal(int val) : base(val) { }

        public override void Apply(ref Creature c)
        {
            int amount = c.heal(value);
            if (c is Player) Game.Message("Your health has been increased by " + amount);
            else Game.Message("The " + c + " healed himself " + amount);
        }
    }

    class StrengthBoost : SpellEffect
    {
        public override string Name { get { return "Bear Strength"; } }
        public override string HelpDescription { get { return "Creature's strength is increased by the value"; } }

        public override ConsoleColor LabelColor { get { return ConsoleColor.DarkGreen; } } 

        public StrengthBoost(int val) : base(val) { }

        public override void Apply(ref Creature c)
        {
            c.damage.X += value;
            c.damage.Y += value;
            if (c is Player) Game.Message("Your strength has been increased by " + value);
        }
    }

    class TemporaryImmunity : SpellEffectTemporaryEffect
    {
        public override string Name { get { return "Temporary Immunity"; } }
        public override string HelpDescription { get { return "Creature's immune to all special attacks for a few moves"; } }

        public override ConsoleColor LabelColor { get { return ConsoleColor.White; } }


        public TemporaryImmunity(int val = 10) : base(val) { }

        public override void Apply(ref Creature c)
        {
            c.AddTemporaryEffect(new TemporaryEffectAllImmune((ushort)value));
        }
    }
}
