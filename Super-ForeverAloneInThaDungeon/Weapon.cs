using System;
using Super_ForeverAloneInThaDungeon.Graphics;

namespace Super_ForeverAloneInThaDungeon
{
    public class WeaponItem : InventoryItem
    {
        public Weapon weapon;

        public WeaponItem(Weapon superWeapon, string desc, char[,] img, ConsoleColor clr = ConsoleColor.Gray)
        {
            this.weapon = superWeapon;

            this.name = superWeapon.name;
            this.description = desc;
            this.image = img;
            this.color = clr;

            this.Details = new ItemDetail[] { new ItemDetail("Deals", superWeapon.damage.X + "-" + superWeapon.damage.Y) };
            if (superWeapon is Throwable)
            {
                AddAdditionalInfo(new ItemDetail("Range", ((Throwable)superWeapon).range.ToString()));
            }

            this.actions = new InventoryAction[] { new InventoryActionYield(), new InventoryActionDrop() };
        }
    }

    public class Weapon
    {
        public Point damage;
        public string name;

        public override string ToString()
        {
            return name;
        }
    }

    public class Throwable : Weapon
    {
        public ushort hitChance;
        public byte range;
    }

    public class Dagger : Weapon
    {
        public Dagger()
        {
            damage = new Point(1, 3);
            name = "Dagger";
        }
    }

    public class Spear : Throwable
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
