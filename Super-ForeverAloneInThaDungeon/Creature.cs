using System;
using Super_ForeverAloneInThaDungeon.Graphics;

namespace Super_ForeverAloneInThaDungeon
{
    // mix of FOLLOW, FRIENDLY, ETC
    // TODO: implement
    public enum CreatureAIMode
    {
        Friendly,
        Neutral,
        Angry
    }

    public class Creature : Tile
    {
        public bool processed = false;

        public int maxHealth = 0;
        public int health = 0;
        public int money = 0;
        public ushort searchRange = 0;
        public Point damage = new Point(1, 4);

        // IMPLEMENT
        //public CreatureAIMode creatureAI = CreatureAIMode.Angry;

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
            Walkable = true;
        }

        // Include if (t is Pickupable) when you need to do multiple checks!
        public void onTileEncounter(ref Tile t)
        {
            if (t.Type == TileType.Money)
            {
                money += ((Money)t).money;
                t = new Tile(((Pickupable)t).replaceTile);
            }
        }

        /// <summary>
        /// DIE YOU ANIMAL!
        /// </summary>
        /// <param name="dmg">HOW HARD DO YOU WANT TO DIE ?</param>
        /// <param name="t">Where do I need to dig your grave ?</param>
        /// <returns>Is he dead now?</returns>
        public bool doDamage(int dmg, ref Tile t)
        {
            health -= dmg;

            if (health <= 0)
            {
                t = new Money(this.money);
                ((Pickupable)t).replaceTile = lastTile.Type;
                t.NeedsRefresh = true;
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

        public void move(Tile old)
        {
            this.lastTile = old;
        }
    }



    class Snake : Creature
    {
        public Snake(ref Random ran, int lvl = 0)
            : base()
        {
            health = maxHealth = ran.Next(6, 10) + lvl;
            money = ran.Next(1, 5); 
            Type = TileType.Snake;
            RepresentationInLight = 'S';
            Color = ConsoleColor.DarkGreen;
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
            Type = TileType.Goblin;
            RepresentationInLight = Constants.chars[3];
            Color = ConsoleColor.DarkRed;
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
            return (ushort)(ran.Next(1, 3 + maxHealth / 10) + ran.Next(0, damage.Y - damage.X));
        }
    }
}
