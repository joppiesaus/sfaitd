﻿using System;

namespace Super_ForeverAloneInThaDungeon
{
    // TODO: Test and implement
    class Door : WorldObject
    {
        byte flags = 4;

        public bool Cursed
        {
            get { return (flags & 1) == 1; }
            set
            {
                if (value)
                {
                    flags |= 1;
                }
                else
                {
                    flags &= (byte)(1 ^ byte.MaxValue);
                }
            }
        }

        public bool Locked
        {
            get { return (flags & 2) == 2; }
            set
            {
                if (value)
                {
                    flags |= 2;
                }
                else
                {
                    flags &= (byte)(2 ^ byte.MaxValue);
                }
            }
        }

        public bool Kickable
        {
            get { return (flags & 4) == 4; }
            set
            {
                if (value)
                {
                    flags |= 4;
                }
                else
                {
                    flags &= (byte)(4 ^ byte.MaxValue);
                }
            }
        }

        public bool Open
        {
            get { return (flags & 8) == 8; }
            set
            {
                if (value)
                {
                    flags |= 8;
                }
                else
                {
                    flags &= (byte)(8 ^ byte.MaxValue);
                }
            }
        }

        /// <summary>
        /// true for x, false for y
        /// </summary>
        public bool Orientation
        {
            get { return (flags & 16) == 16; }
            set
            {
                if (value)
                {
                    flags |= 16;
                }
                else
                {
                    flags &= (byte)(16 ^ byte.MaxValue);
                }
            }
        }

        public Door(bool x)
        {
            this.Orientation = x;

            this.walkable = true;
            this.color = ConsoleColor.DarkYellow;
            this.notLightenChar = this.drawChar = x ? Constants.xDoor : Constants.yDoor;
            this.tiletype = TileType.Other;
        }

        /// <summary>
        /// Player-only
        /// </summary>
        public void Kick()
        {
            if (Open)
            {
                Game.Message("The door is already open!");
                return;
            }
            
            if (Kickable)
            {
                // 1/4 it won't go open even if it's possible,
                // 1/2 it opens(so it's closable)
                // 1/4 it breaks
                switch (Game.ran.Next(0, 4))
                {
                    case 0:
                        Game.Message("The door broke into pieces.");
                        this.destroyed = true;
                        return;
                    case 1:
                    case 2:
                        Game.Message("The door slammed open with great force!");
                        this.Toggle();
                        return;
                }
            }
            else if (Cursed)
            {
                Game.Message("You tried to kick the door open, but it's magically blocked!");
                return;
            }
            Game.Message("You tried to kick the door open, but it won't budge!");
        }

        /// <summary>
        /// Try to open the door. Player-only.
        /// </summary>
        public void TryOpen()
        {
            if (Locked)
            {
                Game.Message("It's locked!");
            }
            else if (Cursed)
            {
                Game.Message("You can't get your hands on the door, it must be cursed!");
            }
            else
            {
                if (Game.ran.Next(0, 5) == 0)
                {
                    Game.Message("You try to open the door with great force, but it's very stiff!");
                }
                else
                {
                    this.Toggle();
                    Game.Message("You opened the door.");
                }
            }
        }

        /// <summary>
        /// Toggles this door. Doesn't matter if it's cursed, broken...
        /// </summary>
        public void Toggle()
        {
            Locked = false;
            Open.Invert();
            Orientation.Invert();
            drawChar = notLightenChar = Orientation ? Constants.xWall : Constants.yWall;
        }

        /// <summary>
        /// Drops a broken door, which is practically air.
        /// </summary>
        protected override Tile GenerateDrop()
        {
            Tile t = new Tile(TileType.Air);
            t.drawChar = notLightenChar = Constants.chars[7];
            t.color = ConsoleColor.DarkYellow;
            return t;
        }
    }
}
