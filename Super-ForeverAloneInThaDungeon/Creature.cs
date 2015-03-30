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

    enum CreatureElement
    {
        Fire = 1,
        Magic = 2,
    }

    class Creature : WorldObject
    {
        // I like SCREAMING_CAPSLOCK
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

        public ushort weaknesses, immunities = 0;


        public Tile lastTile = new Tile(TileType.Air);

        // Likelyness of hitting something
        public ushort hitLikelyness = 0;

        // Likelyness of getting hit in a form of a penalty
        public short? hitPenalty = null; // 1000 = max, 0 = min


        public virtual Throwable RangedWeapon
        {
            get { return null; }
        }
        public virtual Weapon MeleeWeapon
        {
            get { return null; }
        }


        public Creature(int lvl = 0)
        {
            walkable = false;
            attackable = true;
        }

        protected override Tile GenerateDrop()
        {
            return new Money(this.money);
        }

        public void OnTileEncounter(ref Tile t)
        {
            if (t.tiletype == TileType.Money)
            {
                money += ((Money)t).money;
                t = new Tile(((Pickupable)t).replaceTile);
            }
        }

        public override void Kick()
        {
            if (DoDirectDamage(1))
            {
                EventRegister.RegisterPlayerKillBy(this, "kicked {0} to death!");
            }
            else
            {
                Game.Message("You kicked " + this.InlineName + '.');
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
        /// <returns>If it defended or not</returns>
        public virtual bool TryDefend(AttackMode aMode)
        {
            return false;
        }

        /// <summary>
        /// When attacking, initiative can amplify it's attack(think of potions, etc). Weapons have a seperate method for that.
        /// </summary>
        public virtual void AmplifyAttack(ref WorldObject target, ref int dmg, AttackMode mode)
        {
        }

        /// <summary>
        /// When the creature killed something, this is called
        /// </summary>
        /// <param name="target">Target creature which is killed</param>
        public virtual void OnKill(Creature target)
        {
        }

        /// <summary>
        /// Gets the damage dealt by hand by this creature.
        /// </summary>
        public virtual int GetDamage()
        {
            return Game.ran.Next(damage.X, damage.Y + 1);
        }

        
        /// <param name="target"></param>
        /// <param name="attackMode"></param>
        public override void Attack(ref WorldObject target)
        {
            Creature t = (Creature)target;

            int dmg = 0;

            if (Game.ran.Next(0, 1001) <= hitLikelyness - (t.hitPenalty == null ? 0 : Game.ran.Next(0, (short)t.hitPenalty + 1)))
            {
                if (t.TryDefend(AttackMode.Melee))
                {
                    return;
                }

                AmplifyAttack(ref target, ref dmg, AttackMode.Melee);

                if (MeleeWeapon == null) dmg = GetDamage();
                else MeleeWeapon.AmplifyAttack(this, ref target, ref dmg);
            }

            EventRegister.RegisterAttack(this, t, dmg);

            t.DoDirectDamage(dmg);

            if (t.destroyed)
            {
                EventRegister.RegisterKill(this, t);

                OnKill(t);

                Tile tile = (Tile)target;
                t.Drop(ref tile);
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

#region move modes
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
#endregion

        /// <summary>
        /// Updates this creature.
        /// </summary>
        /// <remarks>DON'T FORGET to call base.update in other variants!</remarks>
        public override void Update()
        {
            for (ushort i = 0; i < nTeffects; i++)
            {
                // You can't pass in this as a reference, because it's a keyword, not a variable.
                // You can do it with reference classes however.
                Creature sillycompiler = this;
                tEffects[i].Act(ref sillycompiler);

                // Try it yourself:
                //bool diditwork = Object.ReferenceEquals(sillycompiler, this);


                // I present to you: Code art!
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

        /// <summary>
        /// Checks if a temporaryeffect is in the array
        /// </summary>
        /// <param name="type">Type of temporary effect</param>
        /// <returns>index in array or -1 if not found</returns>
        public int HasTemporaryEffect(Type type)
        {
            for (ushort i = 0; i < nTeffects; i++)
                if (tEffects[i].GetType() == type) return i;
            return -1;
        }

        void removeTemporaryEffect(ushort i)
        {
            tEffects[i] = tEffects[nTeffects - 1];
            tEffects[nTeffects--] = null;
        }
        #endregion

        #region special attacks
        /* All these method assume you call the EventRegister! */
        public override byte SetOnFire(short amount)
        {
            if (HasImmunity(CreatureElement.Fire))
            {
                return 2;
            }
            else
            {
                int n = HasTemporaryEffect(typeof(TemporaryEffectBurn));
                if (n == -1)
                {
                    AddTemporaryEffect(new TemporaryEffectBurn((ushort)Game.ran.Next(8, 11 + amount * 2), amount));
                    return 1;
                }
                else
                {
                    tEffects[n].moves = Math.Max(tEffects[n].moves, (ushort)Game.ran.Next(8, 11 + amount * 2));
                    return 0;
                }
            }
        }
        #endregion

        #region conditionals
        public bool HasWeakness(CreatureElement weakness)
        {
            return ((weaknesses & (ushort)weakness) == (ushort)weakness);
        }
        public void AddWeakness(CreatureElement weakness)
        {
            weaknesses |= (ushort)weakness;
        }
        public void RemoveWeakness(CreatureElement weakness)
        {
            weaknesses &= (ushort)((ushort)weakness ^ ushort.MaxValue);
        }

        public bool HasImmunity(CreatureElement immunity)
        {
            return ((immunities & (ushort)immunity) == (ushort)immunity);
        }
        public void AddImmunity(CreatureElement immunity)
        {
            immunities |= (ushort)immunity;
        }
        public void RemoveImmunity(CreatureElement immunity)
        {
            immunities &= (ushort)((ushort)immunity ^ ushort.MaxValue);
        }
        #endregion
    }



    class Snake : Creature
    {
        public Snake(int lvl = 0)
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
            return (ushort)(Game.ran.Next(0, 1 + maxHealth / 10 + damage.Y - damage.X));
        }
    }

    class Grunt : Creature
    {
        public Grunt(int lvl = 0) :base()
        {
            money = Game.ran.Next(1, 4);
            health = maxHealth = Game.ran.Next(3 + lvl / 2, 5 + lvl);
            searchRange = 3;
            tiletype = TileType.Grunt;
            drawChar = Constants.chars[6];
            color = ConsoleColor.Gray;
            damage = new Point(1, 2 + lvl / 2);
            moveMode = CreatureMoveMode.Stationary;
            hitLikelyness = (ushort)Game.ran.Next(600, 800);
        }

        public override ushort GetXp()
        {
            return (ushort)Game.ran.Next(Game.ran.Next(0, 2), 2);
        }

        public override void OnPlayerDiscovery()
        {
        }
        public override void OnPlayerAttack()
        {
            moveMode = CreatureMoveMode.FollowPlayer;
        }
        public override void OnPlayerLeave()
        {
            moveMode = CreatureMoveMode.Random;
        }
    }
}
