namespace Super_ForeverAloneInThaDungeon
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

    class TemporaryEffectHeal : TemporaryEffect
    {
        public TemporaryEffectHeal(ushort count, short amount) : base(count, amount) { }

        public override void Act(ref Creature c)
        {
            c.heal(amount);
        }
    }
}
