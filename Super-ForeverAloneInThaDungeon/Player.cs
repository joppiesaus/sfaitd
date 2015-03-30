using System;

namespace Super_ForeverAloneInThaDungeon
{
    class Player : Creature
    {
        public int nInvItems = 0;

        public InventoryItem[] inventory = new InventoryItem[Constants.invCapacity];

        public WeaponItem rWeaponItem = Constants.spear;
        public WeaponItem mWeaponItem;// = Constants.dagger;

        // "holding" weapons
        public override Throwable RangedWeapon
        {
            get { return rWeaponItem == null ? null : (Throwable)rWeaponItem.weapon; }
        }
        public override Weapon MeleeWeapon
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


        public override string InlineName
        {
            get { return "you"; }
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
            walkable = true;

            hitLikelyness = 400;

            AddInventoryItem(Constants.dagger);
            //AddInventoryItem(Constants.swedishMatches);
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

        public override int GetDamage()
        {
            return damage.X;
        }

        public override void AmplifyAttack(ref WorldObject target, ref int dmg, AttackMode mode)
        {
            if (mode == AttackMode.Melee && MeleeWeapon == null) dmg += this.damage.X;
        }

        public override void Attack(ref WorldObject target)
        {
            base.Attack(ref target);
            if (target is Creature) ((Creature)target).OnPlayerAttack();
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
