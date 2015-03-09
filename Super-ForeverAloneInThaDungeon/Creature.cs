using System;

namespace Super_ForeverAloneInThaDungeon
{
    enum CreatureMoveMode
    {
        FollowPlayer,
        Random,
        Stationary
    }

    class Creature : Tile
    {
        public bool processed = false;

        public int maxHealth = 0;
        public int health = 0;
        public int money = 0;
        public ushort searchRange = 0;
        public Point damage = new Point(1, 4);

        public bool friendly = false;   // if true, creature doesn't attack/heal

        bool playerInRange = false;
        public CreatureMoveMode moveMode = CreatureMoveMode.Random; // how the creature behaves


        public Tile lastTile = new Tile(TileType.Air);

        // Likelyness of hitting something
        public ushort hitLikelyness = 0;

        // Likelyness of getting hit in a form of a penalty
        short? hitPenalty = null; // 1000 = max, 0 = min
        public bool HasHitPenalty
        {
            get { return hitPenalty != null; }
        }
        public short HitPenalty
        {
            get { return HasHitPenalty ? (short)hitPenalty : (short)0; }
            set { this.hitPenalty = value; }
        }


        public Creature()
        {
            walkable = true;
        }

        // Include if (t is Pickupable) when you need to do multiple checks!
        public void onTileEncounter(ref Tile t)
        {
            if (t.tiletype == TileType.Money)
            {
                money += ((Money)t).money;
                t = new Tile(((Pickupable)t).replaceTile);
            }
        }

        /// <summary>
        /// Attacks this creature
        /// </summary>
        /// <param name="dmg">Damage</param>
        /// <param name="t">"Grave" tile, place for drops</param>
        /// <returns>Creature is dead</returns>
        public bool doDamage(int dmg, ref Tile t)
        {
            health -= dmg;

            if (health <= 0)
            {
                t = new Money(this.money);
                ((Pickupable)t).replaceTile = lastTile.tiletype;
                t.needsToBeDrawn = true;
                return true;
            }
            return false;
        }

        /// <summary>
        /// Heal this creature
        /// </summary>
        /// <param name="amnt">amount to heal<param>
        /// <returns>Effective points healed</returns>
        public int heal(int amnt)
        {
            int h = health + amnt;
            if (h > maxHealth) h = maxHealth;

            h -= health;

            health += h;
            return h;
        }

        public int hit(ref Random ran, ref Player p)
        {
            if (ran.Next(0, 1001) > hitLikelyness - p.HitPenalty)
            {
                return 0;
            }

            int dmg = ran.Next(damage.X, damage.Y + 1);

            Tile t = (Tile)p;
            if (p.doDamage(dmg, ref t)) p = null;

            return dmg;
        }

        public virtual ushort getXp(ref Random ran)
        {
            return 0;
        }



        public void UpdatePlayerDiscovery(bool action)
        {
            if (action)
            {
                if (!playerInRange)
                    OnPlayerDiscovery();
            }
            else
            {
                if (playerInRange)
                    OnPlayerLeave();
            }
        }

        public virtual void OnPlayerDiscovery()
        {
            moveMode = CreatureMoveMode.FollowPlayer;
        }

        public virtual void OnPlayerLeave()
        {
            moveMode = CreatureMoveMode.Random;
        }

        public virtual void OnPlayerAttack()
        {
        }
    }



    class Snake : Creature
    {
        public Snake(ref Random ran, int lvl = 0)
            : base()
        {
            health = maxHealth = ran.Next(6, 10) + lvl;
            money = ran.Next(1, 5); 
            tiletype = TileType.Snake;
            drawChar = 'S';
            color = ConsoleColor.DarkGreen;
            damage = new Point(2 + lvl, 4 + lvl);
            searchRange = 5;

            if (lvl <= 10)
            {
                float f = (float)lvl / 2.0f;
                hitLikelyness = (ushort)((95 * f - f * f * 10) * 13 + 301);
            }
            else hitLikelyness = 900;

            hitLikelyness += (ushort)ran.Next(-20, 21);
        }

        public override ushort getXp(ref Random ran)
        {
            return (ushort)ran.Next(0, damage.X / 2 + maxHealth / 5);
        }
    }

    class Goblin : Creature
    {
        public Goblin(ref Random ran, int lvl = 0)
            : base()
        {
            money = ran.Next(2, 7);
            health = maxHealth = ran.Next(8, 13) + lvl;
            searchRange = (ushort)(6 + lvl / 3);
            tiletype = TileType.Goblin;
            drawChar = Constants.chars[3];
            color = ConsoleColor.DarkRed;
            damage = new Point(2 + lvl, 4 + lvl + ran.Next(0, lvl));
            
            if (lvl <= 8)
            {
                float f = (float)lvl / 2.0f;
                hitLikelyness = (ushort)((120 * f - f * f * 12) * 13 + 300);
            }
            else hitLikelyness = 800;
        }

        public override ushort getXp(ref Random ran)
        {
            return (ushort)(ran.Next(0, 2 + maxHealth / 10 + damage.Y - damage.X));
        }
    }
}
