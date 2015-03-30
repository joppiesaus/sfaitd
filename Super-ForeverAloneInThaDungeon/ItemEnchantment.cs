using System;

namespace Super_ForeverAloneInThaDungeon.Enchantments
{
    abstract class ItemEnchantment
    {
        public abstract string Name { get; }
        public abstract string HelpDescription { get; }

        public int value;

        public ItemEnchantment() { }
        public ItemEnchantment(int val)
        {
            this.value = val;
        }

        public abstract void Apply(ref WorldObject obj, Thing caller);

        public virtual IIAI GenerateInventoryInfo()
        {
            return new IIAI(Name, ConsoleColor.Red);
        }
    }

    class ItemEnchantmentFire : ItemEnchantment
    {
        public override string Name { get { return "Fire Aspect"; } }
        public override string HelpDescription { get { return "Makes target able to set things on fire"; } }

        public ItemEnchantmentFire(short val = 1) { this.value = val; }

        public override void Apply(ref WorldObject obj, Thing caller)
        {
            switch (obj.SetOnFire((short)value))
            {
                case 1:
                    EventRegister.RegisterFire(caller.InlineName, obj);
                    break;
                case 2:
                    if (caller is Player)
                    {
                        Game.Message("You try to set " + obj.InlineName + " on fire, but it's immune to it!");
                    }
                    else
                    {
                        Game.Message("The " + obj.InlineName + " tried to set you on fire, but you're immune to it!");
                    }
                    break;
            }
        }
    }
}
