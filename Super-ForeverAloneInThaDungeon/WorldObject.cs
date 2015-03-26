using System;

namespace Super_ForeverAloneInThaDungeon
{
    // 2complex4me
    class WorldObject : Tile
    {
        public bool destroyed = false;
        public bool attackable = false;
        public bool transparent = true;

        // Name in the form of "the Snake attacked you"
        public virtual string InlineName
        {
            get { return "the " + this.tiletype; }
        }

        public virtual void Attack(ref Tile target, AttackMode aMode)
        {
            // TODO: Register attack?
        }

        public virtual void Hit(int dmg) { }

        public virtual ushort GetXp()
        {
            return 0;
        }

        /// <summary>
        /// Attacks this
        /// </summary>
        /// <returns>destroyed</returns>
        public virtual bool DoDirectDamage(int dmg)
        {
            return false;
        }

        /// <summary>
        /// Generates drop. Returns null if no special drop.
        /// </summary>
        /// <param name="ran"></param>
        /// <returns>Tile or null</returns>
        protected virtual Tile GenerateDrop()
        {
            return null;
        }

        /// <summary>
        /// Makes this drop all it's stuff and replace itself, dead or alive.
        /// </summary>
        /// <param name="dmg">Damage</param>
        /// <param name="t">"Grave" tile, place for drops</param>
        public virtual void Drop(ref Tile t)
        {
            // probably both always will be true
            bool lighten = t.lighten;
            bool discovered = lighten || t.discovered;

            t = GenerateDrop();

            if (t is Pickupable)
            {
                ((Pickupable)t).replaceTile = TileType.Air;
            }
            else if (t == null)
            {
                t = new Tile(TileType.Air);
            }

            t.discovered = discovered;
            t.lighten = lighten;
            t.needsToBeDrawn = true;
        }

        /// <summary>
        /// Updates this object
        /// </summary>
        public virtual void Update()
        {
        }

        /// <summary>
        /// Kick this object. Player-only.
        /// </summary>
        public virtual void Kick()
        {
            Game.Message("You kicked " + InlineName + ", but no effect.");
        }

        /// <summary>
        /// Sets this object on fire
        /// 0 = not/already burning
        /// 1 = burning
        /// 2 = not burning because of immunity
        /// </summary>
        /// <returns>If on fire</returns>
        public virtual byte SetOnFire(short amount = 1)
        {
            destroyed = true;
            // !UNSAFE! TEMPORARY SHIZZZZZZZZ
            Game.Message("You burned away " + tiletype.ToString() + '!');
            return 1;
        }
    }
}
