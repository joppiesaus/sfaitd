using System;

namespace Super_ForeverAloneInThaDungeon
{
    /// You have the room class
    /// Which generates rooms
    /// What rooms are choosen depends on which level you are
    /// The rooms are generated and filled in with room specific objects, items and creatures. Depends on which level and player and stuff
    /// Then there are other objects, items and creatures sprinkled over every room. Depends on level stuff too.
    /// Different levels, different monsters, different generation.
    class Room
    {
        public virtual Dimension2D GenerationSize { get { return new Dimension2D(new Point(5, 15), new Point(5, 15)); } }

        public string name = "";
        public Point where, end;

        public Room() { }
        public Room(Point w, Point e)
        {
            this.where = w;
            this.end = e;
        }
        /// <summary>
        /// Generates the room dimensions on its own
        /// </summary>
        public Room(Point pos)
        {
            int width = Game.ran.Next(GenerationSize.where.X, GenerationSize.where.Y);
            int height = Game.ran.Next(GenerationSize.end.X, GenerationSize.end.Y);
            this.where = new Point(pos.X - width / 2, pos.Y - height / 2);
            this.end = new Point(pos.X + width / 2, pos.Y + height / 2);
        }

        /// <summary>
        /// Constructs the dimensions of this room. Doesn't do anything else.
        /// </summary>
        public void Construct(Point w, Point e)
        {
            this.where = w;
            this.end = e;
        }

        public void AppendName()
        {
            string dung = Constants.dungeonNameParts[Game.ran.Next(0, Constants.dungeonNameParts.Length)];

            if (Game.ran.Next(2) == 0)
                this.name = string.Format("{0} of The {1} {2}", dung, Constants.namefwords[Game.ran.Next(0, Constants.namefwords.Length)],
                    Constants.nameswords[Game.ran.Next(0, Constants.nameswords.Length)]);
            else if (Game.ran.Next(2) == 0)
            {
                if (dung[dung.Length - 1] != 's') dung += 's';
                this.name = string.Format("{0} of {1}", dung, Constants.namewords[Game.ran.Next(0, Constants.namewords.Length)]);
            }
            else
            {
                this.name = string.Format("The {0} {1}", Constants.namefwords[Game.ran.Next(0, Constants.namewords.Length)], dung);
            }
        }

        /// <summary>
        /// Create a random point to start to build a corridor.
        /// </summary>
        public virtual CorridorConstruct GetRandomCorridorBuildPoint()
        {
            CorridorConstruct c = new CorridorConstruct();
            c.direction = (byte)Game.ran.Next(0, 4);
            
            switch (c.direction)
            {
                    // up
                case 0:
                    c.origin = new Point(Game.ran.Next(where.X + 1, end.X), where.Y);
                    break;

                    // down
                case 1:
                    c.origin = new Point(Game.ran.Next(where.X + 1, end.X), end.Y);
                    break;
                    
                    // left
                case 2:
                    c.origin = new Point(where.X, Game.ran.Next(where.Y + 1, end.Y));
                    break;

                    // right
                case 3:
                    c.origin = new Point(end.X, Game.ran.Next(where.Y + 1, end.Y));
                    break;
            }

            return c;
        }

        /// <summary>
        /// Generates this room.
        /// </summary>
        public virtual void Generate(ref Tile[,] tiles)
        {
            // horizontal
            for (int i = where.X; i <= end.X; i++)
            {
                tiles[i, where.Y] = new Wall(Constants.xWall);
                tiles[i, end.Y] = new Wall(Constants.xWall);
            }

            // vertical
            for (int i = where.Y; i <= end.Y; i++)
            {
                tiles[where.X, i] = new Wall(Constants.yWall);
                tiles[end.X, i] = new Wall(Constants.yWall);
            }

            // corners
            tiles[where.X, where.Y].drawChar = Constants.lupWall;
            tiles[where.X, end.Y].drawChar = Constants.ldownWall;
            tiles[end.X, where.Y].drawChar = Constants.rupWall;
            tiles[end.X, end.Y].drawChar = Constants.rdownWall;

            for (int x = where.X + 1; x < end.X; x++)
                for (int y = where.Y + 1; y < end.Y; y++)
                    tiles[x, y] = new Tile(TileType.Air);
        }

        /// <summary>
        /// Sprinkles all room-specific items in this room.
        /// </summary>
        public virtual void Sprinkle(ref Tile[,] tiles, Player p, uint floor)
        {
            for (int x = where.X + 1; x < end.X; x++)
                for (int y = where.Y + 1; y < end.Y; y++)
                {
                    if (Game.ran.Next(0, 40) == 0) tiles[x, y] = new Money(Game.ran.Next(1, 6));
                    else if (Game.ran.Next(0, 909) == 0) tiles[x, y] = new Scroll(SpellGenerator.GenerateMultiple());
                    //else if (Game.ran.Next(0, 666) == 0) tiles[x, y] = new Grunt();
                    //else if (Game.ran.Next(0, 800) < 3) tiles[x, y] = new Snake();
                    //else if (Game.ran.Next(0, 900) < 2) tiles[x, y] = new Goblin();
                    else if (Game.ran.Next(0, 1250) == 0) tiles[x, y] = new Chest();
                }
        }

        public void ConnectToCorridorConstruct(Point p, Point size, byte direction)
        {
            switch (direction)
                {
                    default: // down
                        int offset = Game.ran.Next(1, size.X);
                        Construct(
                            new Point(p.X - offset, p.Y),
                            new Point(p.X + size.X - offset, p.Y + size.Y));
                        break;
                    case 0: // up
                        offset = Game.ran.Next(1, size.X);
                        Construct(
                            new Point(p.X - offset, p.Y - size.Y),
                            new Point(p.X + size.X - offset, p.Y));
                        break;
                    case 3: // right
                        offset = Game.ran.Next(1, size.Y);
                        Construct(
                            new Point(p.X, p.Y - offset),
                            new Point(p.X + size.X, p.Y + size.Y - offset));
                        break;
                    case 2: // left
                        offset = Game.ran.Next(1, size.Y);
                        Construct(
                            new Point(p.X - size.X, p.Y - offset),
                            new Point(p.X, p.Y + size.Y - offset));
                        break;
                }
        }

        /// <summary>
        /// Gets a random point in this room
        /// </summary>
        public Point GetRandomPointInRoom()
        {
            return new Point(
                Game.ran.Next(where.X + 1, end.X),
                Game.ran.Next(where.Y + 1, end.Y)
            );
        }

        /// <summary>
        /// Gets a valid walkable tile position in room
        /// </summary>
        public Point GetRandomValidPointInRoom(Tile[,] tiles)
        {
            Point p;

            do
            {
                p = new Point(
                    Game.ran.Next(where.X + 1, end.X),
                    Game.ran.Next(where.Y + 1, end.Y)
                );
            }
            while (tiles[p.X, p.Y].walkable);

            return p;
        }

        /// <summary>
        /// Attemps to spawn special creatures in this room on every CreatureSpawner.Update check
        /// </summary>
        /// <returns>If something spawned</returns>
        public virtual Creature SpawnRoomCreature()
        {
            return null;
        }
    }

    class RoomTreasureRoom : Room
    {
        public override void Sprinkle(ref Tile[,] tiles, Player p, uint floor)
        {
            for (int x = where.X + 1; x < end.X; x++)
                for (int y = where.Y + 1; y < end.Y; y++)
                {
                    if (Game.ran.Next(0, 20) == 0) tiles[x, y] = new Money(Game.ran.Next(1, 7));
                    else if (Game.ran.Next(0, 33) == 0)
                    {
                        switch (Game.ran.Next(0, 5))
                        {
                            case 0:
                            case 1:
                                tiles[x, y] = new Scroll(SpellGenerator.GenerateRandomSpellEffects(Game.ran.Next(2, 6)));
                                break;
                            case 2:
                                tiles[x, y] = new Scroll(SpellGenerator.GenerateMultipleUncommon(Game.ran.Next(1, 5)));
                                break;
                            case 3:
                                tiles[x, y] = new Scroll(SpellGenerator.GenerateMultipleRare(Game.ran.Next(1, 4)));
                                break;
                            case 4:
                                tiles[x, y] = new Scroll(SpellGenerator.GenerateMultipleCommon(Game.ran.Next(3, 8)));
                                break;
                        }
                    }
                    else if (Game.ran.Next(0, 500) == 0) tiles[x, y] = new Snake((int)(floor / 2));
                }
        }
    }

    //class RoundRoom : Room { }
}
