using System;

namespace Super_ForeverAloneInThaDungeon
{
    enum CreatureMoveMode
    {
        FollowPlayer,
        Random,
        Stationary
    }

    enum AttackMode
    {
        Melee, Ranged
    }

    class Creature : WorldObject
    {
        const byte EFFECT_CAPACITY = 100;

        TemporaryEffect[] tEffects = new TemporaryEffect[EFFECT_CAPACITY];
        ushort nTeffects = 0;

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
        public short? hitPenalty = null; // 1000 = max, 0 = min

        public Creature()
        {
            walkable = true;
            attackable = true;
        }

        protected override Tile GenerateDrop()
        {
            return new Money(this.money);
        }

        public override void SetOnFire()
        {
            if (DoDirectDamage(1)) EventRegister.RegisterDeath(this, "fire");
        }

        public void OnTileEncounter(ref Tile t)
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
        /// <returns>Creature is dead</returns>
        public override bool DoDirectDamage(int dmg)
        {
            health -= dmg;

            if (health <= 0)
            {
                destroyed = true;
                return true;
            }
            return false;
        }

        /// <summary>
        /// Makes creature drop all it's stuff and replace itself, dead or alive.
        /// </summary>
        /// <param name="t">"Grave" tile, place for drops</param>
        public override void Drop(ref Tile t)
        {
            // probably both always will be true
            bool lighten = t.lighten;
            bool discovered = lighten || t.discovered;

            t = GenerateDrop();

            if (t is Pickupable)
            {
                ((Pickupable)t).replaceTile = lastTile.tiletype;
            }
            else if (t == null)
            {
                t = lastTile;
            }

            t.discovered = discovered;
            t.lighten = lighten;
            t.needsToBeDrawn = true;
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

        /// <summary>
        /// When attacking, creatures can defend
        /// </summary>
        /// <remarks>This function ALSO HANDLES MESSAGES WHEN BLOCKED.</remarks>
        /// <param name="Game.ran">Random input</param>
        /// <returns>If it defended or not</returns>
        public virtual bool TryDefend(AttackMode aMode)
        {
            return false;
        }

        /// <summary>
        /// When attacking, initiative can amplify it's attack(think of potions, weapons, etc)
        /// </summary>
        /// <param name="Game.ran">Random input</param>
        public virtual void AmplifyAttack(ref WorldObject target, AttackMode aMode)
        {
        }

        public virtual int GetDamage(AttackMode mode)
        {
            return Game.ran.Next(damage.X, damage.Y + 1);
        }

        /// <summary>
        /// When the creature killed something, this is called
        /// </summary>
        /// <param name="target">Target creature which is killed</param>
        public virtual void OnKill(Creature target)
        {
        }

        public override void Attack(ref Tile target, AttackMode attackMode = AttackMode.Melee)
        {
            Creature t = (Creature)target;

            int dmg = 0;

            if (Game.ran.Next(0, 1001) <= hitLikelyness - (t.hitPenalty == null ? 0 : Game.ran.Next(0, (short)t.hitPenalty + 1)))
            {
                if (t.TryDefend(attackMode))
                {
                    return;
                }
                else
                {
                    dmg = GetDamage(attackMode);
                }

                WorldObject wo = (WorldObject)target;
                AmplifyAttack(ref wo, attackMode);
            }

            EventRegister.RegisterAttack(this, t, dmg);

            t.DoDirectDamage(dmg);

            if (t.destroyed)
            {
                EventRegister.RegisterKill(this, t);

                OnKill(t);

                t.Drop(ref target);
            }
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

        /// <remarks>DON'T FORGET to call base.update in other variants!</remarks>
        public override void Update()
        {
            for (ushort i = 0; i < nTeffects; i++)
            {
                // WHAT NOW??/1/!/11//1/1/1/J34IO;LRHJ SDFJUIH
                //tEffects[i].Act(ref this);
                if ( 0==-- tEffects[i].moves)
                    removeTemporaryEffect(i);
            }
        }

        #region temporary effects
        public bool AddTemporaryEffect(TemporaryEffect effect)
        {
            if (nTeffects == EFFECT_CAPACITY) return false;
            tEffects[nTeffects++] = effect;
            return true;
        }

        void removeTemporaryEffect(ushort i)
        {
            tEffects[i] = tEffects[nTeffects - 1];
            tEffects[nTeffects--] = null;
        }
        #endregion
    }



    class Snake : Creature
    {
        public Snake(int lvl = 0)
            : base()
        {
            health = maxHealth = Game.ran.Next(6, 10) + lvl;
            money = Game.ran.Next(1, 5); 
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

            hitLikelyness += (ushort)Game.ran.Next(-20, 21);
        }

        public override ushort GetXp()
        {
            return (ushort)Game.ran.Next(0, damage.X / 2 + maxHealth / 5);
        }
    }

    class Goblin : Creature
    {
        public Goblin(int lvl = 0)
            : base()
        {
            money = Game.ran.Next(2, 7);
            health = maxHealth = Game.ran.Next(8, 13) + lvl;
            searchRange = (ushort)(6 + lvl / 3);
            tiletype = TileType.Goblin;
            drawChar = Constants.chars[3];
            color = ConsoleColor.DarkRed;
            damage = new Point(2 + lvl, 4 + lvl + Game.ran.Next(0, lvl));
            
            if (lvl <= 8)
            {
                float f = (float)lvl / 2.0f;
                hitLikelyness = (ushort)((120 * f - f * f * 12) * 13 + 300);
            }
            else hitLikelyness = 800;
        }

        public override ushort GetXp()
        {
            return (ushort)(Game.ran.Next(0, 2 + maxHealth / 10 + damage.Y - damage.X));
        }
    }
}
