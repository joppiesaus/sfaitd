using System;
using System.Diagnostics;
using System.Text; // I know... I know... But I HAD to! I had no choice *cries like a depressed professor*... I wish it had only ONE namespace!

// Reading old code is fun!
namespace Super_ForeverAloneInThaDungeon
{
    /*
     * //////////
     * x add a light hack!
     * 
     * IDEAS:
     * x Locked doors
     * x exploring system, shadow and other stuff
     * x Special rooms with special enemys
     * x levels and abilities
     * x npcs
     * x show how many levels etc
     * x you must go down until you've found x, then you must go up and then YOU WIN!!!!11!14!
     * 
     * \//\/\/\/\/\/\//\/\/\/\/\/
     * 
     * BUGS:
     * x Goblins do weird stuff
     * x pathfinding is buggy when there are more than one options, so pick the option that makes the mob go to the player.
     * x Goblins have wrong image
     * x Creatures drop nonetile
     * x Sometimes things dont refresh properly
     */

    public class Game
    {
        // used for Breadth-First Search (Node)
        class BFSN
        {
            public int x, y;
            public ushort score;

            public BFSN(int xp, int yp, ushort tileScore)
            {
                this.x = xp;
                this.y = yp;
                this.score = tileScore;
            }
        }

        class LineWriter
        {
            // POD
            class Node
            {
                public ushort x, prevLength;
                public Node(ushort posX)
                {
                    x = posX;
                }
            }

            Node[] nodes;


            // last element will stick to right border
            public LineWriter(ushort[] locations)
            {
                this.nodes = new Node[locations.Length];
                
                for (int i = 0; i < nodes.Length; i++)
                    nodes[i] = new Node(locations[i]);
            }

            // FINISH
            // MAKE IT STICK to right wall too
            public void Draw(string[] data)
            {
                for (int i = 0; i < data.Length - 1; i++)
                {
                    Console.CursorLeft = nodes[i].x;
                    if (nodes[i].prevLength > data[i].Length)
                    {
                        Console.Write(data[i]);                        
                        for (int a = 0; a < nodes[i].prevLength - data[i].Length; a++) Console.Write(' ');
                    }
                    else
                    {
                        Console.Write(data[i]);
                    }

                    nodes[i].prevLength = (ushort)data[i].Length;
                }

                // last one sticks to right border
                byte n = (byte)(data.Length - 1);

                if (nodes[n].prevLength > data[n].Length)
                {
                    Console.CursorLeft = Console.WindowWidth - nodes[n].prevLength;
                    for (int a = 0; a < nodes[n].prevLength - data[n].Length; a++) Console.Write(' ');
                    Console.Write(data[n]);
                }
                else
                {
                    Console.CursorLeft = Console.WindowWidth - data[n].Length;
                    Console.Write(data[n]);
                }

                nodes[n].prevLength = (ushort)data[n].Length;
            }
        }

        class Room
        {
            public string name = "";
            public Point where, end;
            
            public Room(Point w, Point e)
            {
                this.where = w;
                this.end = e;
            }

            public void appendName(ref Random ran)
            {
                string dung = Constants.dungeonNameParts[ran.Next(0, Constants.dungeonNameParts.Length)];

                if (ran.Next(2) == 0)
                    this.name = string.Format("{0} of The {1} {2}", dung, Constants.namefwords[ran.Next(0, Constants.namefwords.Length)],
                        Constants.nameswords[ran.Next(0, Constants.nameswords.Length)]);
                else if (ran.Next(2) == 0)
                {
                    if (dung[dung.Length - 1] != 's') dung += 's';
                    this.name = string.Format("{0} of {1}", dung, Constants.namewords[ran.Next(0, Constants.namewords.Length)]);
                }
                else
                {
                    this.name = string.Format("The {0} {1}", Constants.namefwords[ran.Next(0, Constants.namewords.Length)], dung);
                }
            }
        }
        enum State
        {
            Default, Inventory, Combat, Pause, Throwing
        }

        const TileType noneTile = TileType.None; // use ONLY for dungeon generation(educational :p) exception purposes.

        bool hack = false; // you can walk everywhere if true
        bool hackLighting = false;
        bool disableFight = false;
        bool drawOnceAfterInit = false; // That's for the blue bar-thinggy underneath.

        uint currentFloor = 0;

        // Environment
        Room[] rooms;
        Tile[,] tiles; // Ode to the mighty comma!
        ushort[,] scores; // For enemys searching the player

        // Player stuff
        sbyte damageMultiplier = 0;

        // Div
        Random ran;
        State state = State.Default;

        LineWriter infoLine1 = new LineWriter(new ushort[] { 0, 20, 40, 0xfaef });
        LineWriter infoLine2 = new LineWriter(new ushort[] { 0, 0xdead });

        // Player
        Point playerPos = new Point(-1, -1);

        string message = "Wow I didn't initiliaze apperantly...";

        /// <summary>
        /// Gets or sets your mother's car windscreenwiper.
        /// </summary>
        /// <param name="size">Default: 120, 50. Product of size can never be larger than 65536, because that can lead to breadth-first search problems</param>
        public Game(Point size)
        {
            //Console.CursorVisible = false;
            Console.OutputEncoding = Constants.enc;
            Console.Title = "SuperForeverAloneInThaDungeon";
            Console.SetWindowSize(size.X, size.Y + 3);
            Console.BufferHeight = Console.WindowHeight;

            tiles = new Tile[size.X, size.Y]; // !UNSAFE!
            scores = new ushort[size.X, size.Y]; // !ALSO UNSAFE!

            init();
        }

        #region Dungeon generation stuff
        // Keep in mind that maxvalue is the maxvalue + 1 at all times!
        Room[] createDungeons(ushort howManyRooms, Room size,
            int cSpawnTries = 100, int bSpawnTries = 100, byte corrMinLength = 5, byte corrMaxLength = 15,
            byte cbMinLength = 5, byte cbMaxLength = 10)
        {
            Point spawnRoomGenPoint = new Point(tiles.GetLength(0) / 2 + ran.Next(-10, 5), tiles.GetLength(1) / 2 + ran.Next(-10, 5));
            Room spawnRoom = new Room(spawnRoomGenPoint, new Point(spawnRoomGenPoint.X + ran.Next(4, 12), spawnRoomGenPoint.Y + ran.Next(4, 8)));
            spawnRoom.appendName(ref ran);
            generateRoom(spawnRoom);

            Room[] dungeons = new Room[howManyRooms + 1];
            dungeons[0] = spawnRoom;
            int a = 1;

            for (int i = 0; i < howManyRooms; i++)
            {
                Room r;
                Point cWhere = new Point(-1, -1);
                int cEnd = 0;
                Point[] corrP = { };
                byte dir = 0;

                bool fail = true;
                for (int t = 0; t < cSpawnTries; t++) // cSpawnTries: how many tries to generate a corridor on a random wall
                {
                    r = dungeons[ran.Next(0, a)]; // 0,a --> owl eyes?
                    dir = (byte)ran.Next(0, 4);
                    cWhere = getRandomWallPoint(r, dir);
                     
                    corrP = getCorridorPoints(cWhere, dir, ran.Next(corrMinLength, corrMaxLength)); // corrMinLength, corrMaxLength: Length of corridor

                    if (corrP == null) continue;
                    cEnd = corrP.Length - 1;

                    if (!corrP[cEnd].same(-1, -1))
                    {
                        fail = false;
                        break;
                    }
                }

                if (fail) break;

                // Algorithm:
                // 1: Check if there's room for both corridor and the room itself
                // 2: Build both if so.
                //      when building, build the corridor needed.
                // goto 1
                int highestIndex = 0;
                for (int t = 0; t < bSpawnTries; t++) // bSpawnTries: How many tries to attach a room on a corridor
                {
                    int cIndex = ran.Next(0, corrP.Length);
                    Point connPoint = corrP[cIndex];


                    byte[] nDirArr = getPossibleCorridorDirections(dir);
                    byte nDir;

                    if (cEnd == cIndex)
                    {
                        nDir = getOppositeDirection(dir);
                    }
                    else nDir = nDirArr[ran.Next(0, nDirArr.Length)];


                    // cbMinLength, cbMaxLength: Length of connection between room and corridor
                    Point[] cPoints = getCorridorPoints(connPoint, getOppositeDirection(nDir), ran.Next(cbMinLength, cbMaxLength));
                    if (cPoints == null) continue;

                    // new Point(ran.Next(4, 19), ran.Next(5, 15))
                    // size: size of room
                    Room toBuild = generateRoom(cPoints[cPoints.Length - 1], new Point(ran.Next(size.where.X, size.where.Y), ran.Next(size.end.X, size.end.Y)), nDir, true, true);

                    if (toBuild != null)
                    {
                        if (cIndex > highestIndex) highestIndex = cIndex;

                        //Array.Resize(ref corrP, highestIndex + 1);
                        generateCorridor(corrP, highestIndex + 1);
                        dungeons[a] = toBuild;
                        generateCorridor(cPoints, cPoints.Length);

                        a++;

                        if (a == dungeons.Length)
                        {
                            i = howManyRooms;
                            break;
                        }
                    }
                }
            }

            if (a != dungeons.Length) Array.Resize(ref dungeons, a);

            // Add exit
            Point p = getRandomPointInRoom(dungeons[ran.Next(0, dungeons.Length)]);
            tiles[p.X, p.Y] = new Tile(TileType.Down);

            return dungeons;
        }

        void setDungeonToEmpty()
        {
            for (int a = 0; a < tiles.GetLength(0); a++)
                for (int b = 0; b < tiles.GetLength(1); b++)
                    tiles[a, b] = new Tile(noneTile);
        }

        bool canRoomBePlaced(Room r)
        {
            if (!isInScreen(r.where) || !isInScreen(r.end)) return false;
            for (int x = r.where.X; x <= r.end.X; x++) for (int y = r.where.Y; y <= r.end.Y; y++)
                    if (tiles[x, y].tiletype != noneTile) return false;
            return true;
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

        // *scottisch accent* for moar info, gowto doubleudoubleudoubleudotfunction1dotnlslashpslashsfaitdslashinfoslashSARBdotPNG
        Room generateRoom(Point p, Point size, byte dir, bool doorAtP = true, bool placeMatters = true)
        {
            Room r;

            // This switch has a point
            switch (dir)
            {
                default:
                    int mSize = ran.Next(1, size.X);
                    r = new Room(
                        new Point(p.X - mSize, p.Y),
                        new Point(p.X + size.X - mSize, p.Y + size.Y));
                    break;
                case 1:
                    mSize = ran.Next(1, size.X);
                    r = new Room(
                        new Point(p.X - mSize, p.Y - size.Y),
                        new Point(p.X + size.X - mSize, p.Y));
                    break;
                case 2: 
                    mSize = ran.Next(1, size.Y);
                    r = new Room(
                        new Point(p.X, p.Y - mSize),
                        new Point(p.X + size.X, p.Y + size.Y - mSize));
                    break;
                case 3:
                    mSize = ran.Next(1, size.Y);
                    r = new Room(
                        new Point(p.X - size.X, p.Y - mSize),
                        new Point(p.X, p.Y + size.Y - mSize));
                    break;
            }

            if (placeMatters)
            {
                if (canRoomBePlaced(r)) generateRoom(r);
                else return null;
            }
            else generateRoom(r);

            if (doorAtP) tiles[p.X, p.Y] = new Tile(TileType.Corridor);


            r.appendName(ref ran);
            return r;
        }

        void generateRoom(Room r)
        {
            Point where = r.where;
            Point end = r.end;

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

            where.X++;
            where.Y++;
            end.X--;
            end.Y--;

            for (int x = where.X; x <= end.X; x++)
                for (int y = where.Y; y <= end.Y; y++)
                {
                    if (ran.Next(0, 49) == 0) tiles[x, y] = new Money(ran.Next(1, 6));
                    else if (ran.Next(0, 950) == 500) tiles[x, y] = new Scroll(ref ran);
                    else if (ran.Next(0, 801) < 2) tiles[x, y] = new Snake(ref ran);
                    else if (ran.Next(0, 1000) < 2) tiles[x, y] = new Goblin(ref ran);
                    else tiles[x, y].setTile(TileType.Air);
                }
        }

        Point getRandomPointInRoom(Room r)
        {
            r.where.X++;
            r.where.Y++;
            return new Point(ran.Next(r.where.X, r.end.X), ran.Next(r.where.Y, r.end.Y));
        }

        byte getOppositeDirection(byte dir)
        {
            switch (dir)
            {
                default: return 1;
                case 1: return 0;
                case 2: return 3;
                case 3: return 2;
            }
        }

        byte[] getPossibleCorridorDirections(byte dir)
        {
            switch (dir)
            {
                default: return new byte[] { 2, 3 };
                case 2: case 3: return new byte[] { 0, 1 };
            }
        }

        Point getIncrementByDirection(byte dir)
        {
            sbyte inX = 0;
            sbyte inY = 0;

            switch (dir)
            {
                default: inY--; break;
                case 1: inY++; break;
                case 2: inX--; break;
                case 3: inX++; break;
            }

            return new Point(inX, inY);
        }

        Point[] getCorridorPoints(Point p, byte dir, int howMany)
        {
            if (!isInScreen(p)) return null;

            Point d = getIncrementByDirection(dir);
            Point current = p;
            Point[] points = new Point[howMany];

            for (int i = 0; i < howMany; i++)
            {
                points[i] = current;
                current.X += d.X;
                current.Y += d.Y;
            }

            // Current is now the max, so we can easyally check if everything is right!
            if (!isInScreen(current)) return null;

            // Skip first one because that one is usually a wall, that needs to be replaced.
            for (int i = 1; i < howMany; i++)
                if (tiles[points[i].X, points[i].Y].tiletype != noneTile) return null;

            return points;
        }

        void generateCorridor(Point[] p, int max, TileType t = TileType.Corridor)
        {
            for (int i = 0; i < max; i++)
                tiles[p[i].X, p[i].Y] = new Tile(t);
        }

        Room getDungeonAt(Point p)
        {
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
        Point getRandomWallPoint(Room r, byte direction)
        {
            switch (direction)
            {
                default: return new Point(ran.Next(r.where.X + 1, r.end.X), r.where.Y); // Return up
                case 1: return new Point(ran.Next(r.where.X + 1, r.end.X), r.end.Y); // Down,
                case 2: return new Point(r.where.X, ran.Next(r.where.Y + 1, r.end.Y)); // Left...
                case 3: return new Point(r.end.X, ran.Next(r.where.Y + 1, r.end.Y)); // Right!
            }
        }
        #endregion

        #region div
        // TODO: Make effecienter
        void init()
        {
            ran = new Random();

            Player p;
            if (playerPos.same(-1, -1)) p = new Player();
            else p = (Player)tiles[playerPos.X, playerPos.Y];
            
            setDungeonToEmpty();

            // tweak this
            rooms = createDungeons(15, new Room(new Point(4, 19), new Point(5, 15))); // Counted from 0 *trollface*

            spawnPlayerInRoom(rooms[ran.Next(0, rooms.Length)], p);

            onPlayerMove(ref p); // make sure everything inits properly

            message = "Welcome, " + p.name + "!"; // I could have just used Environment.UserName since the Player.name = Environment.UserName... :~)
            if (disableFight) p.walkable = false;


            currentFloor++;


            // Make sure the dungeon starts nice and clean
            reDrawDungeon();
            
            loop();
        }

        void loop()
        {
            while (true)
            {
                #region default state
                if (state == State.Default)
                {
                    ConsoleKey key = Console.ReadKey().Key;

                    Point toAdd = new Point();
                    bool doNotCallDraw = false;
                    switch (key)
                    {
                        case ConsoleKey.Escape: Environment.Exit(0); break;
                        case ConsoleKey.R: init(); break;
                        case ConsoleKey.LeftArrow: toAdd.X--; break;
                        case ConsoleKey.RightArrow: toAdd.X++; break;
                        case ConsoleKey.UpArrow: toAdd.Y--; break;
                        case ConsoleKey.DownArrow: toAdd.Y++; break;
                        case ConsoleKey.Tab: state = State.Inventory; Console.Clear(); continue;
                        case ConsoleKey.F1: hack = hack.invert(); break; //                                                 HACK TOGGLE HACK TOGGLE HACK
                        case ConsoleKey.F2: hackLighting = true;
                            for (int x = 0; x < tiles.GetLength(0); x++)
                                for (int y = 0; y < tiles.GetLength(1); y++)
                                    if (tiles[x, y].tiletype != TileType.None)
                                {
                                    tiles[x, y].discovered = true;
                                    tiles[x, y].lighten = true;
                                    tiles[x, y].needsToBeDrawn = true;
                                } break; ////////////////////////////////
                        default:
                            if (key == ((Player)tiles[playerPos.X, playerPos.Y]).ThrowWeaponKey)
                            {
                                state = State.Throwing;
                                continue;
                            }
                            doNotCallDraw = true;
                            break;
                    }

                    if (toAdd.X != 0 || toAdd.Y != 0)
                    {
                        Point toCheck = new Point(playerPos.X + toAdd.X, playerPos.Y + toAdd.Y);
                        if (isValidMove(toCheck) || hack)
                        {
                            Point old = playerPos;
                            Tile preCopy = tiles[toCheck.X, toCheck.Y]; // Tile where the player will move to
                            preCopy.needsToBeDrawn = true;
                            tiles[old.X, old.Y].needsToBeDrawn = true; //  Tile that will appear(old tile under player)


                            Player p = (Player)tiles[old.X, old.Y];

                            bool abort = false;

                            if (preCopy is Creature)
                            {
                                attackCreature(ref tiles[toCheck.X, toCheck.Y]);
                                abort = true;
                            }
                            else if (preCopy.tiletype == TileType.Money)
                            {
                                // I knew I'd know the English grammar!
                                int money = ((Money)preCopy).money;
                                string s = money != 1 ? "s" : "";
                                p.money += money;
                                message = "you found " + money + " coin" + s + '!';
                                preCopy = new Tile(((Pickupable)preCopy).replaceTile);
                            }
                                // TODO: Make available for other items
                            else if (preCopy.tiletype == TileType.Scroll)
                            {
                                if (p.addInventoryItem(((Scroll)preCopy).invItem))
                                {
                                    message = "you found " + ((Scroll)preCopy).scroll;
                                    preCopy = new Tile(((Pickupable)preCopy).replaceTile);
                                }
                                else message = Constants.invFullMsg;
                            }
                            else if (preCopy.tiletype == TileType.Down) init();
                            else message = toCheck.ToString();


                            if (!abort)
                            {
                                tiles[toCheck.X, toCheck.Y] = tiles[old.X, old.Y];
                                Creature c = (Creature)tiles[toCheck.X, toCheck.Y];
                                tiles[old.X, old.Y] = c.lastTile;

                                c.lastTile = preCopy;
                                playerPos = toCheck;

                                Player plyr = (Player)tiles[toCheck.X, toCheck.Y];
                                onPlayerMove(ref plyr);
                            }
                        }
                    }

                    if (!doNotCallDraw)
                    {
                        processMonsters();
                        draw();
                    }
                }
                #endregion
                #region inventory state
                else if (state == State.Inventory)
                {
                    drawInventory();
                    ConsoleKey key = Console.ReadKey().Key;

                    switch (key)
                    {
                        case ConsoleKey.Tab: state = State.Default; Console.Clear(); reDrawDungeon(); continue;
                        case ConsoleKey.Escape: Environment.Exit(0); break;
                    }
                }
                #endregion
                #region ThrowingSpear state
                else if (state == State.Throwing)
                {
                    message = "Which direction?";
                    draw();
                    ConsoleKey key = Console.ReadKey().Key;
                    Player p = (Player)tiles[playerPos.X, playerPos.Y];
                    Throwable t = p.tWeapon;

                    switch (key)
                    {
                        // Think backwards here: If you need to go UP in an ARRAY, what do you need to do?
                        case ConsoleKey.UpArrow: handleThrowable(0, -1, p.tWeapon, ref p); break;
                        case ConsoleKey.DownArrow: handleThrowable(0, 1, p.tWeapon, ref p); break;
                        case ConsoleKey.LeftArrow: handleThrowable(-1, 0, p.tWeapon, ref p); break;
                        case ConsoleKey.RightArrow: handleThrowable(1, 0, p.tWeapon, ref p); break;
                        default: message = playerPos.ToString(); draw(); break;
                    }

                    state = State.Default;
                }
                #endregion
            }
        }

        void attackCreature(ref Tile creature)
        {
            Player p = (Player)tiles[playerPos.X, playerPos.Y];
            Creature c = (Creature)creature;

            int pdmg = ran.Next(p.damage.X, p.damage.Y) + damageMultiplier;

            //message = string.Format("{0} {1}", Constants.getPDamageInWords(pdmg), c.tiletype);
            pakMsg(string.Format("{0} {1}", Constants.getPDamageInWords(pdmg, ref ran), c.tiletype));

            if (c.doDamage(pdmg, ref creature))
            {
                onDead(ref p, c);
            }
        }

        void attackPlayer(ref Tile creature)
        {
            Creature c = (Creature)creature;
            Player p = (Player)tiles[playerPos.X, playerPos.Y];

            int cdmg = c.hit(ref ran, ref p);

            //message = string.Format("{0} {1}", c.tiletype, Constants.getCDamageInWords(cdmg));
            pakMsg(string.Format("{0} {1}", c.tiletype, Constants.getCDamageInWords(cdmg, ref ran)));

            if (p == null) onPlayerDead();
        }

        void processMonsters()
        {
            for (int x = 0; x < tiles.GetLength(0); x++) for (int y = 0; y < tiles.GetLength(1); y++)
                {
                    if (tiles[x, y] is Creature && tiles[x, y].tiletype != TileType.Player)
                    {
                        if (((Creature)tiles[x, y]).processed) continue;

                        Point p = getPointTowardsPlayer(x, y);

                        if (isValidMove(p) && !p.same(x, y))
                        {
                            if (tiles[p.X, p.Y].tiletype == TileType.Player && !disableFight)
                            {
                                attackPlayer(ref tiles[x, y]);
                            }
                            else
                            {
                                Tile preCopy = tiles[p.X, p.Y]; // target tile
                                

                                tiles[p.X, p.Y] = tiles[x, y];
                                Creature c = (Creature)tiles[p.X, p.Y];
                                if (preCopy.lighten) c.needsToBeDrawn = true;
                                c.notLightenChar = preCopy.notLightenChar;
                                c.processed = true;

                                tiles[x, y] = c.lastTile;
                                if (preCopy.lighten) tiles[x, y].needsToBeDrawn = true;
                                c.onTileEncounter(ref preCopy);
                                c.lastTile = preCopy;
                            }
                        }
                    }
                }

            // Yes, I know, but come on! It's a turn-based console game! This piece of code won't be noticed in performance!
            for (int x = 0; x < tiles.GetLength(0); x++) for (int y = 0; y < tiles.GetLength(1); y++)
                {
                    if (tiles[x, y] is Creature) ((Creature)tiles[x, y]).processed = false;
                }
        }

        // TODO: Not sure about processMonsters() on line 463
        void handleThrowable(sbyte x, sbyte y, Throwable t, ref Player p)
        {
            Point curPoint = playerPos;
            for (byte i = 0; i < t.range; i++) // Just saved you 24 bit of RAM!
            {
                curPoint.X += x;
                curPoint.Y += y;

                if (isInScreen(curPoint)) if (tiles[curPoint.X, curPoint.Y] is Creature)
                {
                    int dmg = ran.Next(t.damage.X + damageMultiplier, t.damage.Y + damageMultiplier);

                    //message = string.Format("{0} {1} with the Spear", Constants.getPDamageInWords(dmg), tiles[curPoint.X, curPoint.Y].tiletype);
                    pakMsg(string.Format("{0} {1} with the {2}", Constants.getPDamageInWords(dmg, ref ran), tiles[curPoint.X, curPoint.Y].tiletype, t.ToString()));

                    Creature c = (Creature)tiles[curPoint.X, curPoint.Y];
                    if (c.doDamage(dmg, ref tiles[curPoint.X, curPoint.Y]))
                        onDead(ref p, c);

                    processMonsters();
                    return;
                }
            }

            message = "I don't see any creature there!";
            draw();
        }

        void generateScoreGrid(Point p)
        {
            int count = 0; // this value will never be reached I think. It's mathematical worst-case.
            int index = 0;

            // mark the grid which positions should be checked
            for (short x = 0; x < tiles.GetLength(0); x++)
                for (short y = 0; y < tiles.GetLength(1); y++)
                {
                    if (tiles[x, y].walkable)
                    {
                        count++;
                        scores[x, y] = Constants.SFLAG_NEEDS_CHECK;
                    }
                    else
                    {
                        scores[x, y] = ushort.MaxValue;
                    }
                }

            // create a queue
            BFSN[] list = new BFSN[count];
            list[0] = new BFSN(p.X, p.Y, 0); // add first element

            // browse trough them, mark the scores
            while (index >= 0)
            {
                BFSN cTile = list[index];
                ushort nscore = (ushort)(cTile.score + 1);

                index--;                

                if (cTile.x + 1 < tiles.GetLength(0) && scores[cTile.x + 1, cTile.y] == Constants.SFLAG_NEEDS_CHECK)
                {
                    scores[cTile.x + 1, cTile.y] = cTile.score;
                    list[++index] = new BFSN(cTile.x + 1, cTile.y, nscore);
                }
                if (cTile.x > 0 && scores[cTile.x - 1, cTile.y] == Constants.SFLAG_NEEDS_CHECK)
                {
                    scores[cTile.x - 1, cTile.y] = cTile.score;
                    list[++index] = new BFSN(cTile.x - 1, cTile.y, nscore);
                }
                if (cTile.y + 1 < tiles.GetLength(1) && scores[cTile.x, cTile.y + 1] == Constants.SFLAG_NEEDS_CHECK)
                {
                    scores[cTile.x, cTile.y + 1] = cTile.score;
                    list[++index] = new BFSN(cTile.x, cTile.y + 1, nscore);
                }
                if (cTile.y > 0 && scores[cTile.x, cTile.y - 1] == Constants.SFLAG_NEEDS_CHECK)
                {
                    scores[cTile.x, cTile.y - 1] = cTile.score;
                    list[++index] = new BFSN(cTile.x, cTile.y - 1, nscore);
                }
            }
        }

        void onPlayerMove(ref Player p)
        {
            p.onMove();

            if (!hackLighting)
            {
                bool[,] processed = new bool[(Constants.playerLookRadius + 2) * 2 - 1, (Constants.playerLookRadius + 2) * 2 - 1];

                for (int xa = 0; xa < p.circle.GetLength(0); xa++)
                {
                    int xb = playerPos.X + xa - Constants.playerLookRadius + 1;
                    for (int ya = 0; ya < p.circle.GetLength(1); ya++)
                    {
                        Point pos = new Point(xb, playerPos.Y + ya - Constants.playerLookRadius + 1);
                        if (p.circle[xa, ya] && isInScreen(pos))
                        {
                            if (tiles[pos.X, pos.Y].tiletype != TileType.None)
                                if (!tiles[pos.X, pos.Y].discovered)
                                {
                                    tiles[pos.X, pos.Y].discovered = true;
                                    tiles[pos.X, pos.Y].lighten = true;
                                    tiles[pos.X, pos.Y].needsToBeDrawn = true;
                                }
                                else if (!tiles[pos.X, pos.Y].lighten)
                                {
                                    tiles[pos.X, pos.Y].lighten = true;
                                    tiles[pos.X, pos.Y].needsToBeDrawn = true;
                                }
                            processed[xa + 1, ya + 1] = true;
                        }
                        
                    }
                }

                for (int x = 0; x < processed.GetLength(0); x++)
                {
                    int xp = playerPos.X + x - Constants.playerLookRadius;
                    for (int y = 0; y < processed.GetLength(1); y++)
                    {
                        if (!processed[x, y])
                        {
                            int yp = playerPos.Y + y - Constants.playerLookRadius;
                            if (isInScreen(new Point(xp, yp)) && tiles[xp, yp].lighten)
                            {
                                tiles[xp, yp].lighten = false;
                                tiles[xp, yp].needsToBeDrawn = true;
                            }
                        }
                    }
                }
            }

            generateScoreGrid(playerPos);
        }


        // TODO: Make it so it doesn't have a preference.
        //       AND range specifier for better performance.
        Point getPointTowardsPlayer(int x, int y)
        {
            Point p = new Point(x,y);
            Creature c = (Creature)tiles[x, y];

            if (isInRangeOfPlayer(p, c.searchRange))
            {
                Point[] points = new Point[] {
                    new Point(x - 1, y),
                    new Point(x + 1, y),
                    new Point(x, y - 1),
                    new Point(x, y + 1)
                };
                ushort least = ushort.MaxValue;
                byte n = 0xff;

                // Good to know: This one has a preference to the left, then right, then up, then down.
                if (scores[points[0].X, points[0].Y] < least)
                {
                    least = scores[points[0].X, points[0].Y];
                    n = 0;
                }
                if (scores[points[1].X, points[1].Y] < least)
                {
                    least = scores[points[1].X, points[1].Y];
                    n = 1;
                }
                if (scores[points[2].X, points[2].Y] < least)
                {
                    least = scores[points[2].X, points[2].Y];
                    n = 2;
                }
                if (scores[points[3].X, points[3].Y] < least)
                {
                    least = scores[points[3].X, points[3].Y];
                    n = 3;
                }

                if (n != 0xff)
                {
                    p = points[n];
                }
            }
            else
            {
                Point[] points = new Point[5];
                byte count = 0;
                
                // who cares what index what direction is
                if (tiles[x + 1, y].walkable)
                    points[count++] = new Point(x + 1, y);
                if (tiles[x - 1, y].walkable)
                    points[count++] = new Point(x - 1, y);
                if (tiles[x, y + 1].walkable)
                    points[count++] = new Point(x, y + 1);
                if (tiles[x, y - 1].walkable)
                    points[count++] = new Point(x, y - 1);
                points[count++] = new Point(x, y); // and do nothing is also a chance

                p = points[ran.Next(0, count)];
            }

            return p;
        }

        bool isInRangeOfPlayer(Point p, int range)
        {
            Point difference = new Point(playerPos.X - p.X, playerPos.Y - p.Y);
            return (difference.X >= -range && difference.X <= range && difference.Y >= -range && difference.X <= range);
        }

        bool isValidMove(Point pos)
        {
            return (pos.X >= 0 && pos.Y >= 0 && pos.X < tiles.GetLength(0) && pos.Y < tiles.GetLength(1) && tiles[pos.X, pos.Y].walkable);
        }

        bool isInScreen(Point pos)
        {
            return pos.X >= 0 && pos.X < tiles.GetLength(0) && pos.Y >= 0 && pos.Y < tiles.GetLength(1);
        }
        #endregion

        #region drawMethods
        void reDrawDungeon()
        {
            Console.Clear();
            Console.SetCursorPosition(0, 0);
            drawOnceAfterInit = true;
            for (int x = 0; x < tiles.GetLength(0); x++) for (int y = 0; y < tiles.GetLength(1); y++) tiles[x, y].needsToBeDrawn = true;
            draw();
        }

        void draw()
        {
            for (int x = 0; x < tiles.GetLength(0); x++)
            {
                for (int y = 0; y < tiles.GetLength(1); y++)
                {
                    if (tiles[x, y].needsToBeDrawn)
                    {
                        Console.SetCursorPosition(x, y);
                        tiles[x, y].Draw();
                        tiles[x, y].needsToBeDrawn = false;
                    }
                }
            }

            Console.BackgroundColor = ConsoleColor.DarkBlue;
            Console.ForegroundColor = ConsoleColor.White;

            Console.SetCursorPosition(0, tiles.GetLength(1));

            if (drawOnceAfterInit)
            {
                drawOnce(Console.WindowWidth * 2);
                drawOnceAfterInit = false;
            }

            Player p = (Player)tiles[playerPos.X, playerPos.Y];

            infoLine1.Draw(new string[] { 
                "Health: " + p.health + '(' + p.maxHealth + ')',
                "xp: " + p.xp + "(lvl " + p.level + ')',
                "Floor: -" + currentFloor,
                "Money: " + p.money
            });

            string location = "";

            if (p.lastTile.tiletype != TileType.Air) location = p.lastTile.tiletype.ToString();
            else
            {
                Room r = getDungeonAt(playerPos);
                location = r == null ? "O_o What the hell did you do this game?" : r.name;
            }

            infoLine2.Draw(new string[] { message, location });


            Console.BackgroundColor = ConsoleColor.Black;
        }

        void drawOnce(int amount)
        {
            Point orgin = new Point(Console.CursorLeft, Console.CursorTop);
            for (int i = 0; i < amount; i++) Console.Write(' ');
            Console.SetCursorPosition(orgin.X, orgin.Y);
        }

        bool drawInvItem(InventoryItem item, object name, ConsoleColor color = ConsoleColor.Yellow)
        {
            Point orgin = new Point(Console.CursorLeft, Console.CursorTop);

            // Stop trying to make IndexOutOfRangeException happen. It's not going to happen.
            if (item.image.GetLength(0) + 1 + orgin.X > Console.WindowWidth || item.image.GetLength(1) + 1 + orgin.Y > Console.WindowHeight) return false;

            // I know this code won't be easy to understand, but hey, it works!
            string line = "";

            for (int i = 0; i < item.image.GetLength(1) + 2; i++) line += Constants.xWall;

            string invNumber = name.ToString();
            string top = line;
            string bottom = line;

            int index = top.Length / 2;
            int insrt = invNumber.Length / 2;
            if (invNumber.Length == 1) top = top
                .Remove(index, 1)
                .Insert(index, invNumber).Remove(0, invNumber.Length)
                .Insert(0, Constants.lupWall.ToString()).Remove(top.Length - 1, 1) + Constants.rupWall;
            else
            {
                top = top.Remove(index, insrt).Insert(index - insrt, invNumber);

                int leng = insrt;
                if (leng + index + invNumber.Length > top.Length) leng--;

                top = top.Remove(index + invNumber.Length, leng);
                if (leng != insrt) top = top.Remove(top.Length - 2, 1);

                // Uneven check
                if (name.ToString().Length % 2 != 0)
                {
                    top = top.Remove(0, 1);
                    insrt++;
                }

                top = top.Remove(0, 1).Insert(0, Constants.lupWall.ToString()).Remove(top.Length - 2, 1) + Constants.rupWall;
            }

            bottom = bottom.Remove(0, 1).Insert(0, Constants.lSplitWall.ToString()).Remove(bottom.Length - 1, 1) + Constants.rSplitWall;

            for (int y = 0; y < item.image.GetLength(0); y++)
            {
                Console.ForegroundColor = item.color;
                for (int x = 0; x < item.image.GetLength(1); x++)
                {
                    Console.SetCursorPosition(orgin.X + 1 + x, y + 1);
                    Console.Write(item.image[y, x].ToString());
                }

                Console.ForegroundColor = ConsoleColor.White;
                Console.Write(Constants.yWall);

                Console.CursorLeft = orgin.X;
                Console.ForegroundColor = ConsoleColor.White;
                Console.Write(Constants.yWall);
            }
            Console.CursorLeft = orgin.X;
            Console.CursorTop++;
            Console.Write(bottom);
            
            // Description stuff
            string[] lines = Constants.getReadableString(item.description, item.image.GetLength(1));
            for (int i = 0; i < lines.Length; i++)
            {
                Console.CursorTop++;
                Console.CursorLeft = orgin.X;
                Console.Write(Constants.yWall);
                Console.ForegroundColor = color;
                Console.Write(lines[i]);
                Console.ForegroundColor = ConsoleColor.White;
                Console.Write(Constants.yWall);
            }

            Console.CursorTop++;
            Console.CursorLeft = orgin.X;
            Console.Write(bottom.Remove(0, 1).Insert(0, Constants.ldownWall.ToString()).Remove(bottom.Length - 1, 1) + Constants.rdownWall);
            

            Console.CursorLeft = orgin.X;
            Console.CursorTop = orgin.Y;

            insrt = index - insrt;

            for (int i = 0; i < insrt; i++)
                Console.Write(top[i]);

            Console.ForegroundColor = color;

            for (int i = insrt; i < insrt + invNumber.Length; i++)
                Console.Write(top[i]);

            Console.ForegroundColor = ConsoleColor.White;

            for (int i = insrt + invNumber.Length; i < top.Length; i++)
                Console.Write(top[i]);

            return true;
        }

        void drawInventory()
        {
            Player p = (Player)tiles[playerPos.X, playerPos.Y];
            for (byte i = 0; i < p.nInvItems; i++) drawInvItem(p.inventory[i], i);
            if (p.tWeapon != null) drawInvItem(Constants.invItems[p.tWeapon.invItemIndex], "Weapon");
            else drawInvItem(Constants.invItems[0], "Weapon");
        }
        #endregion

        #region environment
        void giveXp(ref Player p, ushort amnt)
        {
            p.xp += amnt;
            if (p.xp > p.reqXp)
            {
                p.levelUp();
                pakMsg("you are now level " + p.level + '!');
            }
        }
        #endregion

        #region "constant" messages methods
        // "Press any key" message
        void pakMsg(string msg)
        {
            message = msg + " --Press any key--";
            draw();
            Console.ReadKey();
            message = getDefaultMessage();
            draw();
        }

        string getDefaultMessage()
        {
            return playerPos.ToString();
        }

        void onDead(ref Player p, Creature c)
        {
            giveXp(ref p, c.getXp(ref ran));
            pakMsg("you have defeated " + c.tiletype + "!");
        }

        void onPlayerDead()
        {
            message = "GAME OVER: R.I.P. " + ((Player)tiles[playerPos.X, playerPos.Y]).name + "!";
            draw();
            Console.ReadKey();
            Environment.Exit(0);
        }
        #endregion
    }

    #region items creatures and moar
    class InventoryItem
    {
        public string name, description;
        public char[,] image;
        public ConsoleColor color;

        public InventoryItem(string name, string description, char[,] image, ConsoleColor color)
        {
            this.name = name;
            this.description = description;
            this.image = image;
            this.color = color;
        }
    }

    class Tile
    {
        public char drawChar;
        public bool walkable = false;
        public bool lighten = false;
        public bool discovered = false;
        public bool needsToBeDrawn = true;
        public ConsoleColor color;
        public TileType tiletype;

        public char notLightenChar = '.';

        public Tile() { }
        public Tile(TileType tp/* = TileType.None*/)
        {
            setTile(tp);
        }

        public void setTile(TileType tp)
        {
            needsToBeDrawn = true;
            tiletype = tp;

            switch (tp)
            {
                default:
                    drawChar = ' ';
                    break;
                case TileType.None:
                    drawChar = notLightenChar = ' ';
                    break;
                case TileType.Air:
                    walkable = true;
                    drawChar = '.';
                    notLightenChar = '.';
                    color = ConsoleColor.Gray; break;
                case TileType.Wall:
                    //drawChar = '#';
                    color = ConsoleColor.Gray; break;
                case TileType.Corridor:
                    drawChar = notLightenChar = '#';
                    color = ConsoleColor.White;
                    walkable = true; break;
                case TileType.Up:
                    drawChar = Constants.chars[1];
                    color = ConsoleColor.DarkCyan;
                    walkable = true; break;
                case TileType.Down:
                    drawChar = Constants.chars[2];
                    color = ConsoleColor.Green;
                    walkable = true; break;
            }
        }

        public virtual void Draw()
        {
            if (discovered)
                if (lighten)
                {
                    Console.ForegroundColor = color;
                    Console.Write(drawChar);
                }
                else
                {
                    Console.ForegroundColor = ConsoleColor.DarkGray;
                    Console.Write(notLightenChar);
                }
        }

        public override string ToString()
        {
            return drawChar.ToString();
        }
    }

    class Wall : Tile
    {
        public Wall(char c)
            : base(TileType.Wall)
        {
            this.drawChar = c;
        }

        public override void Draw()
        {
            if (discovered)
                if (lighten)
                {
                    Console.ForegroundColor = color;
                    Console.Write(drawChar);
                }
                else
                {
                    Console.ForegroundColor = ConsoleColor.DarkGray;
                    Console.Write(drawChar);
                }
        }
    }

    // Not sure if I had to do this...
    class Pickupable : Tile
    {
        public TileType replaceTile = TileType.Air;
    }

    class Weapon
    {
        public uint invItemIndex = 0;
        public Point damage;
        protected string name;

        public override string ToString()
        {
            return name;
        }
    }
    class Throwable : Weapon
    {
        public byte range;
        public ConsoleKey key;
    }

    class Spear : Throwable
    {
        public Spear()
        {
            range = 3;
            damage = new Point(5, 10);
            key = ConsoleKey.S;
            invItemIndex = 1;
            name = "Spear";
        }
    }

    class Scroll : Pickupable
    {
        // I just wrote a special program for this so you can easily calculate that...
        // Now I think: Why not just a string?
        //readonly static char[,] inventoryScreenScroll = 
        public string scroll;

        public InventoryItem invItem;

        public Scroll(ref Random ran)
        {
            this.walkable = true;

            if (ran.Next(0, 2) == 1)
            {
                this.scroll = string.Format("\"The Scroll of The {0} {1}\"",
                    Constants.namefwords[ran.Next(Constants.namefwords.Length)], Constants.nameswords[ran.Next(Constants.nameswords.Length)]);
                this.color = ConsoleColor.DarkCyan;
            }
            else
            {
                this.scroll = string.Format("\"The scroll of {0}\"", Constants.namewords[ran.Next(Constants.namewords.Length)]);
                this.color = ConsoleColor.DarkMagenta;
            }
            
            this.drawChar = Constants.chars[0];
            this.tiletype = TileType.Scroll;

            char[,] img = new char[,] { {' ','_','_','_','_','_','_','_','_','_','_',' ',' ',' ',' ',' ' },
	        { '(',')','_','_','_','_','_','_','_','_','_',')',' ',' ',' ',' ' },
	        { ' ','\\',' ',' ',' ',' ',' ',' ',' ',' ',' ',' ','\\',' ',' ',' ' },
	        { ' ',' ','\\',' ',' ',' ',' ',' ',' ',' ',' ',' ',' ','\\',' ',' ' },
	        { ' ',' ',' ','\\','_','_','_','_','_','_','_','_','_','_','\\',' ' },
	        { ' ',' ',' ','(',')','_','_','_','_','_','_','_','_','_','_',')'} };

            byte xOff = 3;
            for (byte y = 2; y < 4; y++)
            {
                int length = xOff + ran.Next(2, 9);
                for (int i = xOff; i < length; i++)
                    img[y, i] = Constants.rlangChars[ran.Next(0, Constants.rlangChars.Length)];

                if (length < xOff + 5)
                {
                    int i = length + 1;
                    length += ran.Next(2, 5);
                    for (; i < length; i++)
                        img[y, i] = Constants.rlangChars[ran.Next(0, Constants.rlangChars.Length)];
                }
                xOff++;
            }

            invItem = new InventoryItem(scroll, scroll, img, color);
        }
    }

    class Money : Pickupable
    {
        public int money;

        public Money(int value)
        {
            walkable = true;
            tiletype = TileType.Money;
            money = value;
            drawChar = '*';
            color = ConsoleColor.Yellow;
        }
    }

    class Creature : Tile
    {
        public bool processed = false;

        public int health = 0;
        public int money = 0;
        public int searchRange = 5;
        public Point damage = new Point(1, 4);

        public Tile lastTile = new Tile(TileType.Air);

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
                ((Pickupable)t).replaceTile = lastTile.tiletype;
                t.needsToBeDrawn = true;
                return true;
            }
            return false;
        }

        public int hit(ref Random ran, ref Player p)
        {
            int dmg = ran.Next(damage.X, damage.Y);
            
            Tile t = (Tile)p;
            p.doDamage(dmg, ref t);

            if (t.tiletype != TileType.Player) p = null;

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

    class Player : Creature
    {
        public int nInvItems = 0;

        public InventoryItem[] inventory = new InventoryItem[10];
        // TODO: insert into inventory and use index instead
        public Throwable tWeapon = new Spear();
        public ConsoleKey ThrowWeaponKey
        {
            get
            {
                if (tWeapon != null) return tWeapon.key;
                return ConsoleKey.NoName;
            }
        }

        public string name = Environment.UserName;
        public uint xp = 0;
        public uint reqXp = 1;
        public uint level = 1;
        public int maxHealth = 10;

        private byte hMoves = 0;

        public readonly bool[,] circle = Constants.generateCircle(Constants.playerLookRadius);

        public Player() : base()
        {
            health = maxHealth;
            tiletype = TileType.Player;
            lastTile = new Tile(TileType.Up);
            drawChar = '☺';
            color = ConsoleColor.Magenta;
            damage = new Point(1, 5);

            Random ran = new Random();
            addInventoryItem((new Scroll(ref ran)).invItem);
        }

        public bool addInventoryItem(InventoryItem item)
        {
            if (nInvItems >= inventory.Length) return false;
            inventory[nInvItems++] = item;
            return true;
        }

        public void removeInventoryItem(int n)
        {
            for (int i = n; i < nInvItems; i++)
            {
                inventory[n] = inventory[n + 1];
            }
            inventory[--nInvItems] = null;
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
    }

    class Snake : Creature
    {
        public Snake(ref Random ran) : base()
        {
            health = ran.Next(6, 10);
            money = ran.Next(1, 5);
            tiletype = TileType.Snake;
            drawChar = 'S';
            color = ConsoleColor.DarkGreen;
            damage = new Point(1, 3);
        }

        public override ushort getXp(ref Random ran)
        {
            return (ushort)ran.Next(0, 1 + health / 5);
        }
    }

    class Goblin : Creature
    {
        public Goblin(ref Random ran) : base()
        {
            money = ran.Next(2, 7);
            health = ran.Next(8, 13);
            tiletype = TileType.Goblin;
            drawChar = Constants.chars[3];
            color = ConsoleColor.DarkRed;
            damage = new Point(2, 4);
            searchRange = 10;
        }

        public override ushort getXp(ref Random ran)
        {
            return (ushort)ran.Next(1, 3 + health / 10);
        }
    }
#endregion

    #region constants structs enums
    public struct Point
    {
        public int X;
        public int Y;

        public Point(int x = 0, int y = 0)
        {
            this.X = x;
            this.Y = y;
        }

        public bool same(int x, int y)
        {
            return (X == x && Y == y);
        }

        public override string ToString()
        {
            return "Point { X: " + X + " Y: " + Y + " }";
        }
    }

    // why did I decide combining polymorthism with enums
    enum TileType
    {
        None,
        Air,
        Wall,
        Money,
        Player,
        Corridor,
        Torch,
        Scroll,
        Snake,
        Goblin,
        Wizzard,
        Up,
        Down
    }


    static class Constants
    {
        public const byte playerLookRadius = 6;

        static Constants()
        {
            yWall = _g(186);
            xWall = _g(205);
            lupWall = _g(201);
            ldownWall = _g(200);
            rupWall = _g(187);
            rdownWall = _g(188);
            lSplitWall = _g(204);
            rSplitWall = _g(185);


            chars = new char[] { _g(166), // scrollChar
                _g(30),                   // startChar
                _g(31),                   // exitChar
                _g(147),                  // goblinChar
            };
        }

        public const ushort SFLAG_NEEDS_CHECK = ushort.MaxValue - 1; // I'm pretty sure that the score wont be that long

        public static readonly Encoding enc = Encoding.GetEncoding(437);

        public readonly static char[] chars; // used to display special characters, like a scroll.
        public readonly static char yWall, xWall, lupWall, ldownWall, rupWall, rdownWall, lSplitWall, rSplitWall;

        public static readonly string[] dungeonNameParts = new string[] { "Prisons", "Dungeon", "Tunnels", "Chamber", "Tombs", "Cells", "Lair", "Cave", "Catacombs", "Caverns" };
        public readonly static string[] namefwords = new string[]
        { "Witty", "Ancient", "Evil", "Heartless", "Furious", "Screaming", "Mighty", "Infamous", "Loved", "Isolated", "Withered", "Magic", "Undead", "Mystic", "Dark", "Black", "Creepy"};
        public readonly static string[] nameswords = new string[]
        { "Orc", "Elf", "Prisoner", "Dragon", "Angel", "Demon", "Giant", "Ghost", "Priest", "Cobra", "Warrior", "Phantom", "Hunter", "Phoenix", "Widow", "Mage", "Warlock" };
        public readonly static string[] namewords = new string[] { "Blindness", "Resurrection", "Power", "Acceptance", "Tears", "Undead", "Lies", "Fear", "Disbelieve", "Anger", "Mistakes", "Judgement" };

        // "Macro"
        static char _g(byte val)
        {
            return enc.GetChars(new byte[] { val })[0];
        }

        public readonly static char[] rlangChars = new char[] { 'a', 'o', 'e', 'u', 'j', 'k', 'n', 'q', 'p', 'w',
            _g(230), _g(208), _g(244), _g(255)  };

        public readonly static InventoryItem[] invItems = {
            new InventoryItem("Empty", "Empty", new char[,] {{'\\',' ',' ',' ',' ',' ','/'},{' ','\\',' ',' ',' ','/',' '},{' ',' ','\\',' ','/',' ',' '},{' ',' ',' ','X',' ',' ',' '},
	        { ' ',' ','/',' ','\\',' ',' ' },{ ' ','/',' ',' ',' ','\\',' ' },{ '/',' ',' ',' ',' ',' ','\\'}}, ConsoleColor.DarkRed),
            new InventoryItem("Spear", "The most glorious weapon in the entire dungeon!",
            new char[,] { {' ',' ',' ',' ',' ',' ','/','|' },{ ' ',' ',' ',' ',' ','/',' ','|' },
            { ' ',' ',' ',' ',' ','/','/',' ' },{ ' ',' ',' ',' ','/','/',' ',' ' },{ ' ',' ',' ','/','/',' ',' ',' ' },{ ' ',' ','/','/',' ',' ',' ',' '} }, ConsoleColor.Gray) };


        public const string invFullMsg = "Inventory full!";

        #region methods
        public static bool[,] generateCircle(int radius)
        {
            // http://webstaff.itn.liu.se/~stegu/circle/circlealgorithm.pdf
            int x = 0;
            int y = radius;
            int d = 5 - 4 * radius;
            int da = 12;
            int db = 20 - 8 * radius;

            bool[,] circle = new bool[radius * 2 - 1, radius * 2 - 1];

            // make an circle outline
            while (x < y)
            {
                if (d < 0)
                {
                    d += da;
                    db += 8;
                }
                else
                {
                    y--;
                    d += db;
                    db += 16;
                }
                da += 8;

                circle[++x, y] = true;
            }

            // fill the circle
            radius--;
            for (int a = -radius; a < radius; a++)
                for (int b = -radius; b < radius; b++)
                    if (a * a + b * b < radius * radius)
                        circle[a + radius, b + radius] = true;

            return circle;
        }

        public static string getPDamageInWords(int dmg, ref Random ran)
        {
            if (dmg == 0)
            {
                switch (ran.Next(0, 4))
                {
                    default: return "you missed";
                    case 1: return "you barely missed";
                    case 2: return "you completely missed";
                }
            }
            else if (dmg < 5) return ran.Next(0, 2) == 1 ? "you did a simple hit on" : "you did a minor hit on";
            else if (dmg < 10) return "you did a great hit on";
            else return ran.Next(0, 2) == 1 ? "you did a great hit on" : "you injured";
        }

        public static string getCDamageInWords(int dmg, ref Random ran)
        {
            if (dmg < 0) return "has healed you with " + dmg;
            else if (dmg == 0)
            {
                switch (ran.Next(0, 4))
                {
                    default: return "missed you";
                    case 1: return "barely missed you";
                    case 2: return "completely missed you";
                }
            }
            else if (dmg < 5) return ran.Next(0, 2) == 1 ? "did a simple hit on you" : "did a minor hit on you";
            else if (dmg < 10) return "did a great hit on you";
            else return ran.Next(0, 2) == 1 ? "did a critical hit on you" : "injured you";
        }

        public static string[] getReadableString(string input, int lineLength)
        {
            string[] sInput = input.Split(' ');
            string[] lines = new string[input.Length / lineLength + 4];

            string currentLine = "";
            int a = 0;

            for (int i = 0; i < sInput.Length; i++)
            {
                if (currentLine.Length + sInput[i].Length < lineLength)
                {
                    if (currentLine.Length > 0)
                        if (currentLine.Length + sInput[i].Length + 1 <= lineLength) currentLine += ' ';
                        else
                        {
                            // GOD DAMNIT! NEED TO DO IT TWICE
                            lines[a] = fillUpEmptySpace(currentLine, lineLength);
                            a++;
                            currentLine = "";
                            i--;
                        }
                    currentLine += sInput[i];
                }
                else if (currentLine.Length <= 0)
                {
                    lines[a] = sInput[i].Remove(lineLength, sInput[i].Length - lineLength);
                    sInput[i] = sInput[i].Remove(0, lineLength);
                    a++;
                    i--;
                }
                else
                {
                    lines[a] = fillUpEmptySpace(currentLine, lineLength);
                    a++;
                    currentLine = "";
                    i--;
                }
            }

            if (currentLine != "")
            {
                lines[a] = fillUpEmptySpace(currentLine, lineLength);
                a++;
            }


            Array.Resize(ref lines, a);
            return lines;
        }

        public static string fillUpEmptySpace(string s, int l, char c = ' ')
        {
            l -= s.Length;
            for (int i = 0; i < l; i++) s += c;
            return s;
        }

        // Lazyness level: Over 9000!
        public static bool invert(this bool b)
        {
            /*if (b) return false;
            return true;*/
            /*return (b) ? false : true;*/
            /*return b ^= true;*/
            return !b;
        }
        #endregion
    }
    #endregion
}
