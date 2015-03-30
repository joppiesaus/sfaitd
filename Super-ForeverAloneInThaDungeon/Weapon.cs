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
            if (superWeapon.enchantments.Length > 0) extraInfoLength += superWeapon.enchantments.Length + 1;

            this.extraInfo = new IIAI[extraInfoLength];

            int eic = 0; // ExtraInfoCount

            extraInfo[eic++] = new IIAID("Deals", superWeapon.damage.X + "-" + superWeapon.damage.Y);

            if (superWeapon is Throwable)
            {
                extraInfo[eic++] = new IIAID("Range", ((Throwable)superWeapon).range.ToString());
            }

            if (superWeapon.enchantments.Length > 0)
            {
                extraInfo[eic++] = new IIAIH("Enchantments", ConsoleColor.Blue);

                for (int i = 0; i < superWeapon.enchantments.Length; i++)
                {
                    extraInfo[eic++] = superWeapon.enchantments[i].GenerateInventoryInfo();
                }
            }
        }
    }

    class Weapon : Thing
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

        public void AmplifyAttack(Thing caller, ref WorldObject target, ref int dmg)
        {
            dmg += Game.ran.Next(damage.X, damage.Y + 1);

            for (int i = 0; i < enchantments.Length; i++)
            {
                enchantments[i].Apply(ref target, caller);
            }
        }

        public override string ToString()
        {
            return name;
        }

        public override string InlineName
        {
            get
            {
                return "the " + name;
            }
        }
    }

    class Throwable : Weapon
    {
        public ushort hitChance;
        public byte range;

        public void Attack(Creature caller, ref WorldObject target)
        {
            Creature t = (Creature)target;

            int dmg = 0;

            if (Game.ran.Next(0, 1001) <= hitChance - (t.hitPenalty == null ? 0 : Game.ran.Next(0, (short)t.hitPenalty + 1)))
            {
                if (t.TryDefend(AttackMode.Ranged))
                {
                    return;
                }

                AmplifyAttack((Thing)caller, ref target, ref dmg);
                caller.AmplifyAttack(ref target, ref dmg, AttackMode.Ranged);
            }

            EventRegister.RegisterAttack(this, t, dmg);

            t.DoDirectDamage(dmg);

            if (t.destroyed)
            {
                EventRegister.RegisterKill(this, t);

                caller.OnKill(t);

                Tile tile = (Tile)target;
                t.Drop(ref tile);
            }
        }
    }


    class Dagger : Weapon
    {
        public Dagger()
        {
            damage = new Point(1, 3);
            name = "Dagger";

            Enchant(new ItemEnchantmentFire());
        }
    }

    class Sword : Weapon
    {
        public Sword()
        {
            damage = new Point(3, 7);
            name = "Sword";
        }
    }

    class Spear : Throwable
    {
        public Spear()
        {
            hitChance = 789;
            range = 3;
            damage = new Point(3, 7);
            name = "Spear";
        }
    }
}
