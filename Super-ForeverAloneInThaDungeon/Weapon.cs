using System;

using Super_ForeverAloneInThaDungeon.Enchantments;

namespace Super_ForeverAloneInThaDungeon
{
    class WeaponItem : InventoryItem
    {
        public Weapon weapon;

        public WeaponItem(Weapon superWeapon, string desc, char[,] img, ConsoleColor clr = ConsoleColor.Gray)
        {
            this.weapon = superWeapon;

            this.name = superWeapon.name;
            this.description = desc;
            this.image = img;
            this.color = clr;

            this.actions = new InventoryAction[] { new InventoryActionYield(), new InventoryActionDrop() };

            int extraInfoLength = 1;
            if (superWeapon is Throwable) extraInfoLength++;
            if (superWeapon.enchantments != null) extraInfoLength += superWeapon.enchantments.Length + 1;

            this.extraInfo = new IIAI[extraInfoLength];

            int eic = 0; // ExtraInfoCount

            extraInfo[eic++] = new IIAID("Deals", superWeapon.damage.X + "-" + superWeapon.damage.Y);

            if (superWeapon is Throwable)
            {
                extraInfo[eic++] = new IIAID("Range", ((Throwable)superWeapon).range.ToString());
            }

            if (superWeapon.enchantments != null)
            {
                extraInfo[eic++] = new IIAIH("Enchantments", ConsoleColor.Blue);

                for (int i = 0; i < superWeapon.enchantments.Length; i++)
                {
                    extraInfo[eic++] = superWeapon.enchantments[i].GenerateInventoryInfo();
                }
            }
        }
    }

    class Weapon
    {
        public ItemEnchantment[] enchantments = null;

        public Point damage;
        public string name;

        public Weapon() { enchantments = new ItemEnchantment[0]; }
        public Weapon(ItemEnchantment ench)
        {
            enchantments = new ItemEnchantment[] { ench };
        }
        public Weapon(ItemEnchantment[] ench)
        {
            enchantments = ench;
        }

        public void Enchant(ItemEnchantment enc)
        {
            // Inneficient, but the best for the RAM
            Array.Resize(ref enchantments, enchantments.Length + 1);

            enchantments[enchantments.Length - 1] = enc;
        }

        public override string ToString()
        {
            return name;
        }

        public int Attack(ref Creature c)
        {
            int beginHealth = c.health;

            return 0;
        }
    }

    class Throwable : Weapon
    {
        public ushort hitChance;
        public byte range;
    }


    class Dagger : Weapon
    {
        public Dagger()
        {
            damage = new Point(1, 3);
            name = "Dagger";

            enchantments = new ItemEnchantment[] { new ItemEnchantmentFire() };
        }
    }

    class Spear : Throwable
    {
        public Spear()
        {
            hitChance = 780;
            range = 3;
            damage = new Point(3, 7);
            name = "Spear";
        }
    }
}
