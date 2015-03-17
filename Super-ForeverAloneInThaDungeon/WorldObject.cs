﻿using System;

namespace Super_ForeverAloneInThaDungeon
{
    // 2complex4me
    class WorldObject : Tile
    {
        public bool destroyed = false;
        public bool attackable = false;

        // Name in the form of "the Snake attacked you"
        public virtual string InlineName()
        {
            return "the " + this.tiletype;
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

        public virtual void SetOnFire()
        {
            destroyed = true;
            // !UNSAFE!
            Game.Message("You burned away " + tiletype.ToString() + '!');
        }
    }
}