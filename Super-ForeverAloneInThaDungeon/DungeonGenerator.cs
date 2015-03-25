using System;

namespace Super_ForeverAloneInThaDungeon
{
    partial class Game
    {
        /// <summary>
        /// Generates dungeons using the default algorithm
        /// </summary>
        /// <param name="nRooms">Number of rooms</param>
        /// <returns>Array of dungeons</returns>
        Room[] generateDungeons(ushort nRooms)
        {
            Room[] dungeons = new Room[nRooms];

            // Start with one dungeon where the player spawns.
            dungeons[0] = new Room(
                new Point(
                    tiles.GetLength(0) / 2 + ran.Next(-10, 11),
                    tiles.GetLength(1) / 2 + ran.Next(-10, 11)
                )
            );
            dungeons[0].AppendName();
            dungeons[0].Generate(ref tiles);

            for (ushort currentRoom = 1; currentRoom < nRooms;)
            {
                // Current corridor construction
                CorridorConstruct cc = null;

                // Corridor adding
                while (true) // If not possible... heheheheheheheh
                {
                    // 1: Pick any wall of any room
                    cc = dungeons[ran.Next(0, currentRoom)].GetRandomCorridorBuildPoint();

                    // 2: Check if possible, if not, try again
                    cc.length = (byte)ran.Next(6, 16);
                    if (canBuildCorridorHere(cc))
                    {
                        // Don't add the corridor yet - there may be a case where it's surrounded by other rooms.
                        cc.beginIsDungeon = (sbyte)cc.direction;
                        break;
                    }
                }


                // Feature adding

                // Add a room to the corridor
                for (byte d = 0; d < 5; d++)
                {
                    // Choose a random point & direction for a path from the corridor to a new room
                    CorridorConstruct pathToRoom = new CorridorConstruct();
                    pathToRoom.direction = cc.direction > 1 ? (byte)ran.Next(0, 2) : (byte)ran.Next(2, 4);
                    pathToRoom.length = (byte)ran.Next(3, 8);

                    Point pathToRoomIncrement = getIncrementByDirection(pathToRoom.direction);
                    Point ccIncrement = getIncrementByDirection(cc.direction);
                    pathToRoom.origin = new Point(
                        cc.origin.X + ran.Next(1, cc.length + 1) * ccIncrement.X,
                        cc.origin.Y + ran.Next(1, cc.length + 1) * ccIncrement.Y
                    );

                    if (!canBuildCorridorHere(pathToRoom)) { continue; }

                    Room room = roomPlanner.GetRoom();

                    // If room can be build, build corridor, room, and path.
                    if (connectRoomToPath(ref room, ref pathToRoom, pathToRoomIncrement))
                    {
                        room.Generate(ref tiles);
                        constructCorridor(pathToRoom, pathToRoomIncrement);
                        constructCorridor(cc, ccIncrement);

                        room.AppendName();
                        dungeons[currentRoom++] = room;
                        break;
                    }
                }
            }

            return dungeons;
        }

        void makeRandomDoorAt(Point p, sbyte dir)
        {
            /* 2/3  spawning
             * if
             * if 1/5
             *      1/2 locked but kickable
             *      1/2 open
             * else
             *      1/3 not locked not kickable
             */
            if (Game.ran.Next(0, 3) != 0)
            {
                Door door = new Door(dir > 1 ? true : false);
                //door.SlamInFaceOf(peter);

                if (Game.ran.Next(0, 5) != 0)
                {
                    if (Game.ran.Next(0, 3) == 0)
                    {
                        door.Kickable = false;
                    }
                }
                else if (Game.ran.Next(0, 2) == 0)
                {
                    door.Locked = true;
                }
                else
                {
                    door.Open = true;
                }

                tiles[p.X, p.Y] = door;
            }
        }

        void constructCorridor(CorridorConstruct c, Point increment)
        {
            for (byte i = 0; i <= c.length; i++)
                tiles[c.origin.X + increment.X * i, c.origin.Y + increment.Y * i] = new Tile(TileType.Corridor);
            if (c.beginIsDungeon != -1)
            {
                makeRandomDoorAt(c.origin, c.beginIsDungeon);
            }
            if (c.endIsDungeon != -1)
            {
                makeRandomDoorAt(new Point(c.origin.X + increment.X * c.length, c.origin.Y + increment.Y * c.length), c.endIsDungeon);
            }
        }

        /// <summary>
        /// Checks if a room can be build on a path from a corridor
        /// </summary>
        bool connectRoomToPath(ref Room r, ref CorridorConstruct construct, Point increment)
        {
            Point p = new Point(
                construct.origin.X + increment.X * construct.length,
                construct.origin.Y + increment.Y * construct.length
            );

            for (byte i = 0; i < 8; i++)
            {
                Point size = new Point(
                    Game.ran.Next(r.GenerationSize.where.X, r.GenerationSize.where.Y),
                    Game.ran.Next(r.GenerationSize.end.X, r.GenerationSize.end.Y)
                );

                r.ConnectToCorridorConstruct(p, size, construct.direction);

                if (canRoomBeBuild(r))
                {
                    construct.endIsDungeon = (sbyte)construct.direction;
                    return true;
                }
            }
            return false;
        }

        /// <summary>
        /// Checks if corridor can be build
        /// </summary>
        bool canBuildCorridorHere(CorridorConstruct c)
        {
            Point increment = getIncrementByDirection(c.direction);

            if (!isInScreen(new Point(c.origin.X + increment.X * c.length, c.origin.Y + increment.Y * c.length))) return false;

            for (byte i = c.length; i > 1; i--)
            {
                if (tiles[c.origin.X + increment.X * i, c.origin.Y + increment.Y * i].tiletype != TileType.None) return false;
            }
            return true;
        }

        /// <summary>
        /// Checks if room can be build
        /// </summary>
        bool canRoomBeBuild(Room r)
        {
            // Check corners and middle
            if (r.where.X < 0 ||
                r.end.X >= tiles.GetLength(0) ||
                r.where.Y < 0 ||
                r.end.Y >= tiles.GetLength(1) ||
                tiles[r.where.X, r.where.Y].tiletype != TileType.None ||
                tiles[r.end.X, r.where.Y].tiletype != TileType.None ||
                tiles[r.where.X, r.end.Y].tiletype != TileType.None ||
                tiles[r.end.X, r.end.Y].tiletype != TileType.None ||
                tiles[r.where.X + (r.end.X - r.where.X) / 2, r.where.Y + (r.end.Y - r.where.Y) / 2].tiletype != TileType.None) return false;

            // Check vertical walls
            for (int y = r.where.Y + 1; y < r.end.Y; y++)
            {
                if (tiles[r.where.X, y].tiletype != TileType.None ||
                    tiles[r.end.X, y].tiletype != TileType.None) return false;
            }

            // Check horizontal walls
            for (int x = r.where.X + 1; x < r.end.X; x++)
            {
                if (tiles[x, r.where.Y].tiletype != TileType.None ||
                    tiles[x, r.end.Y].tiletype != TileType.None) return false;
            }

            // I am pretty sure there's no dungeon here
            // Everything needs to be connected, that's why I'm sure even thought not everything is checked.
            return true;
        }

        void setDungeonToEmpty()
        {
            for (int a = 0; a < tiles.GetLength(0); a++)
                for (int b = 0; b < tiles.GetLength(1); b++)
                    tiles[a, b] = new Tile(noneTile);
        }

        void spawnPlayerInRoom(Room r, Player p)
        {
            Point where = r.where;
            Point end = r.end;

            where.X++;
            where.Y++;
            end.X--;
            end.Y--;

            playerPos = new Point(-1, -1);
            while (!isValidMove(playerPos))
                playerPos = new Point(ran.Next(where.X, end.X + 1), ran.Next(where.Y, end.Y + 1));

            p.needsToBeDrawn = true;
            p.lastTile = new Tile(TileType.Up);
            tiles[playerPos.X, playerPos.Y] = p;
        }

        static byte getOppositeDirection(byte dir)
        {
            switch (dir)
            {
                default: return 1;
                case 1: return 0;
                case 2: return 3;
                case 3: return 2;
            }
        }

        static byte[] getPossibleCorridorDirections(byte dir)
        {
            switch (dir)
            {
                default: return new byte[] { 2, 3 };
                case 2:
                case 3: return new byte[] { 0, 1 };
            }
        }

        static Point getIncrementByDirection(byte dir)
        {
            switch (dir)
            {
                default: return new Point(0, -1);
                case 1: return new Point(0, 1);
                case 2: return new Point(-1, 0);
                case 3: return new Point(1, 0);
            }
        }

        Room getDungeonAt(Point p)
        {
            // not effecient
            for (int i = 0; i < rooms.Length; i++)
                if (p.X >= rooms[i].where.X && p.X <= rooms[i].end.X && p.Y >= rooms[i].where.Y && p.Y <= rooms[i].end.Y) return rooms[i];
            return null;
        }

        /// <summary>
        /// Is itz clears to you?
        /// </summary>
        /// <param name="r">Boxs to sitz in</param>
        /// <param name="direction">Favoritez direction to lie down: 0 = up, 1 = down, 2 = left, 3 = right</param>
        /// <returns>How happy catz is!</returns>
        static Point getRandomWallPoint(Room r, byte direction)
        {
            switch (direction)
            {
                default: return new Point(ran.Next(r.where.X + 1, r.end.X), r.where.Y); // Return up
                case 1: return new Point(ran.Next(r.where.X + 1, r.end.X), r.end.Y); // Down,
                case 2: return new Point(r.where.X, ran.Next(r.where.Y + 1, r.end.Y)); // Left...
                case 3: return new Point(r.end.X, ran.Next(r.where.Y + 1, r.end.Y)); // Right!
            }
        }
    }
}
