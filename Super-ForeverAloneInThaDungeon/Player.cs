using System;

namespace Super_ForeverAloneInThaDungeon
{
    class Player : Creature
    {
        public int nInvItems = 0;

        public InventoryItem[] inventory = new InventoryItem[Constants.invCapacity];

        public WeaponItem rWeaponItem;//= Constants.spear;
        public WeaponItem mWeaponItem;// = Constants.dagger;

        // "holding" weapons
        public Throwable RangedWeapon
        {
            get { return rWeaponItem == null ? null : (Throwable)rWeaponItem.weapon; }
        }
        public Weapon MeleeWeapon
        {
            get { return mWeaponItem == null ? null : mWeaponItem.weapon; }
        }

        public string name;
        public uint xp = 0;
        public uint reqXp = 1;
        public uint level = 1;

        private byte hMoves = 0;

        public readonly bool[,] circle = Constants.GenerateCircle(Constants.playerLookRadius);

        public void Rename(string _name)
        {
            this.name = _name;
            Console.Title = "SuperForeverAloneInThaDungeon - " + _name;
        }


        public override string InlineName()
        {
            return "you";
        }

        public Player()
            : base()
        {
            Rename(Environment.UserName);
            health = maxHealth = 12;

            tiletype = TileType.Player;
            lastTile = new Tile(TileType.Up);
            drawChar = '☺';
            color = ConsoleColor.Magenta;
            damage = new Point(3, 0); // x is the strength

            hitLikelyness = 400;

            AddInventoryItem(Constants.dagger);
            AddInventoryItem(Constants.sword);
            AddInventoryItem(Constants.swedishMatches);
            AddInventoryItem((new Scroll(SpellGenerator.GenerateMultiple()).GenerateInvItem()));
            AddInventoryItem((new Scroll(SpellGenerator.GenerateMultiple()).GenerateInvItem()));
            AddInventoryItem((new Scroll(SpellGenerator.GenerateMultiple()).GenerateInvItem()));
            AddInventoryItem((new Scroll(SpellGenerator.GenerateMultiple()).GenerateInvItem()));
            AddInventoryItem((new Scroll(SpellGenerator.GenerateMultiple()).GenerateInvItem()));
        }

        public bool AddInventoryItem(InventoryItem item)
        {
            if (nInvItems >= inventory.Length) return false;
            inventory[nInvItems++] = item;
            return true;
        }

        public void RemoveInventoryItem(int n)
        {
            // order matters here
            nInvItems--;
            for (; n < nInvItems;)
            {
                inventory[n] = inventory[++n];
            }
            inventory[nInvItems] = null;
        }

        // unused
        public InventoryItem SwapInventoryItem(int n, InventoryItem item)
        {
            InventoryItem ret = inventory[n];
            inventory[n] = item;
            return ret;
        }

        public InventoryItem LastInventoryItem()
        {
            return inventory[nInvItems - 1];
        }

        public void onMove()
        {
            if (health >= maxHealth) return;
            hMoves++;
            if (hMoves >= 4)
            {
                health++;
                hMoves = 0;
            }
        }

        public override int GetDamage(AttackMode aMode)
        {
            return aMode == AttackMode.Melee ? damage.X : 0;
        }

        public override void Drop(ref Tile t)
        {
            // Stop making InvalidCastOperation happen
            // It's not going to happen
        }

        public override void OnKill(Creature target)
        {
            xp += target.GetXp();

            while (xp >= reqXp)
            {
                levelUp();
                Game.Message("You are now level " + level + '!');
            }
        }

        public override void AmplifyAttack(ref WorldObject target, ref int damage, AttackMode aMode)
        {
            amplifyAttack(ref target, ref damage, aMode == AttackMode.Melee ? MeleeWeapon : RangedWeapon);
        }

        void amplifyAttack(ref WorldObject target, ref int damage, Weapon weapon)
        {
            if (weapon != null)
            {
                damage += Game.ran.Next(weapon.damage.X, weapon.damage.Y + 1);

                if (weapon.enchantments.Length > 0)
                    for (int i = 0; i < weapon.enchantments.Length; i++)
                    {
                        weapon.enchantments[i].Apply(ref target);
                    }
            }
        }

        public void levelUp()
        {

            xp -= reqXp;
            level++;

            ////////
            reqXp = reqXp * 2;
            hitLikelyness += (ushort)(75 - hitLikelyness / 100);

            maxHealth += 4;
            damage.X += 1;
            damage.Y += 2;
        }
    }
}
