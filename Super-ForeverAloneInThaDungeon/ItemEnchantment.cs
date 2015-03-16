using System;

namespace Super_ForeverAloneInThaDungeon.Enchantments
{
    abstract class ItemEnchantment
    {
        public abstract string Name { get; }
        public abstract string HelpDescription { get; }

        int value;

        public ItemEnchantment() { }
        public ItemEnchantment(int val)
        {
            this.value = val;
        }

        public abstract void Apply(ref WorldObject obj);

        public virtual IIAI GenerateInventoryInfo()
        {
            return new IIAI(Name, ConsoleColor.Red);
        }
    }

    class ItemEnchantmentFire : ItemEnchantment
    {
        public override string Name { get { return "Fire Aspect"; } }
        public override string HelpDescription { get { return "Makes target able to set things on fire"; } }

        public override void Apply(ref WorldObject obj)
        {
            obj.SetOnFire();
            EventRegister.RegisterFire("you", obj);
        }
    }
}
