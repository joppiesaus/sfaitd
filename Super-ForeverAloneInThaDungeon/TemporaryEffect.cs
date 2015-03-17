﻿namespace Super_ForeverAloneInThaDungeon
{
    // POD
    abstract class TemporaryEffect
    {
        public ushort moves;
        public short amount;

        public TemporaryEffect(ushort count, short _amount = 0)
        {
            moves = count;
            amount = _amount;
        }

        public abstract void Act(ref Creature c);
    }

    class TemporaryEffectBurn : TemporaryEffect
    {
        public TemporaryEffectBurn(ushort count, short amount = 1) : base(count, amount) { }

        public override void Act(ref Creature c)
        {
            if (c.HasImmunity(CreatureElement.Fire)) return;

            short n = c.HasWeakness(CreatureElement.Fire) ? (short)(amount * 2) : amount;

            bool didBurn = false;

            for (short i = 0; i < n; i++)
            {
                if (Game.ran.Next(0, 4) != 0)
                {
                    if (c.DoDirectDamage(1))
                    {
                        EventRegister.RegisterDeath(c, "fire");
                        return;
                    }
                    didBurn = true;
                }
            }

            if (didBurn)
            {
                EventRegister.RegisterBurn(c);
            }
        }
    }

    class TemporaryEffectHeal : TemporaryEffect
    {
        public TemporaryEffectHeal(ushort count, short amount) : base(count, amount) { }

        public override void Act(ref Creature c)
        {
            c.heal(amount);
        }
    }
}
