using System;
using System.Diagnostics;
using System.Text; // I know... I know... But I HAD to! I had no choice *cries like a depressed professor*... I wish it had only ONE namespace!

namespace Super_ForeverAloneInThaDungeon
{
    public class Game
    {
        // ugliest class I've ever written
        class DualWriter
        {
            ushort prevLength1 = 0;
            ushort prevLength2 = 0;
            
            public void dualWrite(string s1, string s2)
            {
                Console.Write(s1);
                if (s1.Length < prevLength1)
                    for (int i = 0; i < prevLength1 - s1.Length; i++) Console.Write(' ');
                
                prevLength1 = (ushort)s1.Length;

                if (s2.Length < prevLength2)
                {
                    Console.CursorLeft = Console.WindowWidth - prevLength2;
                    for (int i = 0; i < prevLength2 - s2.Length; i++) Console.Write(' ');
                }
                else Console.CursorLeft = Console.WindowWidth - s2.Length;

                prevLength2 = (ushort)s2.Length;

                Console.Write(s2);
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

        static TileType noneTile = TileType.None; // use ONLY for dungeon generation(educational :p) exception purposes.

        public static char[] chars;
        static char yWall, xWall, lupWall, ldownWall, rupWall, rdownWall, lSplitWall, rSplitWall;

        bool hack = false;
        bool disableFight = false;
        bool drawOnceAfterInit = true; // That's for the blue bar-thinggy underneath.

        // Environment
        Room[] rooms;

        // Player stuff
        sbyte damageMultiplier = 0;

        // Div
        Random ran;
        State state = State.Default;
        DualWriter dInfoWriter1 = new DualWriter();
        DualWriter dInfoWriter2 = new DualWriter(); // Why can't we just do old DualWriter(); ?

        // Player
        Point playerPos = new Point(-1, -1);

        string message = "Wow I didn't initiliaze apperantly...";

        Tile[,] tiles; // Ode to the mighty comma!

        /// <summary>
        /// Gets or sets your mother's car windscreenwiper.
        /// </summary>
        /// <param name="size">Default: 100, 30</param>
        public Game(Point size)
        {
            Console.OutputEncoding = Encoding.GetEncoding(437);
            Console.Title = "SuperForeverAloneInThaDungeon";
            Console.SetWindowSize(size.X, size.Y + 3);
            Console.BufferHeight = Console.WindowHeight;

            tiles = new Tile[size.X, size.Y]; // !UNSAFE!

            Encoding e = Encoding.GetEncoding(437);
            yWall = e.GetChars(new byte[] { 186 })[0];
            xWall = e.GetChars(new byte[] { 205 })[0];
            lupWall = e.GetChars(new byte[] { 201 })[0];
            ldownWall = e.GetChars(new byte[] { 200 })[0];
            rupWall = e.GetChars(new byte[] { 187 })[0];
            rdownWall = e.GetChars(new byte[] { 188 })[0];
            lSplitWall = e.GetChars(new byte[] { 204 })[0];
            rSplitWall = e.GetChars(new byte[] { 185 })[0];



            chars = new char[] { e.GetChars(new byte[] { 166 })[0], // scrollChar
                e.GetChars(new byte[] { 30 })[0],                   // startChar
                e.GetChars(new byte[] { 31 })[0]};                  // exitChar

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

                        Array.Resize(ref corrP, highestIndex + 1);
                        generateCorridor(corrP);
                        dungeons[a] = toBuild;
                        generateCorridor(cPoints);

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
            tiles[p.X, p.Y] = new Tile(TileType.Exit);

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
            p.lastTile = new Tile(TileType.Start);
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

            for (int i = where.X; i <= end.X; i++)
            {
                tiles[i, where.Y].setTile(TileType.Wall);
                tiles[i, where.Y].drawChar = xWall;
                tiles[i, end.Y].setTile(TileType.Wall);
                tiles[i, end.Y].drawChar = xWall;
            }

            for (int i = where.Y; i <= end.Y; i++)
            {
                tiles[where.X, i].setTile(TileType.Wall);
                tiles[where.X, i].drawChar = yWall;
                tiles[end.X, i].setTile(TileType.Wall);
                tiles[end.X, i].drawChar = yWall;
            }

            // corners
            tiles[where.X, where.Y].drawChar = lupWall;
            tiles[where.X, end.Y].drawChar = ldownWall;
            tiles[end.X, where.Y].drawChar = rupWall;
            tiles[end.X, end.Y].drawChar = rdownWall;

            where.X++;
            where.Y++;
            end.X--;
            end.Y--;

            for (int x = where.X; x <= end.X; x++)
                for (int y = where.Y; y <= end.Y; y++)
                {
                    if (ran.Next(0, 51) == 50) tiles[x, y] = new Money(ran.Next(1, 6));
                    else if (ran.Next(0, 1000) == 500) tiles[x, y] = new Scroll(ref ran);
                    else if (ran.Next(0, 801) == 213) tiles[x, y] = new Snake(ref ran);
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

        void generateCorridor(Point[] p, TileType t = TileType.Corridor)
        {
            for (int i = 0; i < p.Length; i++)
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
        void init()
        {
            ran = new Random();

            Player p;
            if (playerPos.same(-1, -1)) p = new Player();
            else p = (Player)tiles[playerPos.X, playerPos.Y];

            setDungeonToEmpty();

            rooms = createDungeons(10, new Room(new Point(4, 19), new Point(5, 15))); // Counted from 0 *trollface*
            spawnPlayerInRoom(rooms[ran.Next(0, rooms.Length)], p);

            message = "Welcome, " + ((Player)tiles[playerPos.X, playerPos.Y]).name + "!"; // I could have just used Environment.UserName since the Player.name = Environment.UserName... :~)
            if (disableFight) ((Player)tiles[playerPos.X, playerPos.Y]).walkable = false;

            draw();
            loop();
        }

        // TODO: CHANGE TO WHILE LOOP
        // never mind TEST IT
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
                        case ConsoleKey.Tab: state = State.Inventory; Console.Clear(); drawOnceAfterInit = true; continue;
                        case ConsoleKey.F1: hack = hack.invert(); break; //                                                 HACK TOGGLE HACK TOGGLE HACK
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
                        if (hack || isValidMove(toCheck))
                        {
                            Point old = playerPos;
                            Tile preCopy = tiles[toCheck.X, toCheck.Y];
                            preCopy.needsToBeDrawn = true;
                            tiles[old.X, old.Y].needsToBeDrawn = true;


                            Player p = (Player)tiles[old.X, old.Y];
                            p.onMove();

                            bool abort = false;

                            if (preCopy is Creature)
                            {
                                //processCreatureFight(ref tiles[toCheck.X, toCheck.Y]);
                                attackCreature(ref tiles[toCheck.X, toCheck.Y]);
                                abort = true;
                            }
                            else if (preCopy.tiletype == TileType.Money)
                            {
                                // I knew I'd know the English grammar!
                                int money = ((Money)preCopy).money;
                                string s = "";
                                if (money != 1) s = "s";
                                message = "You found " + money + " coin" + s + "!";
                                ((Player)tiles[old.X, old.Y]).money += money;
                                preCopy = new Tile(((Pickupable)preCopy).replaceTile);
                            }
                            else if (preCopy.tiletype == TileType.Scroll)
                            {
                                if (p.addInventoryItem(((Scroll)preCopy).invItem))
                                {
                                    message = "You have found " + ((Scroll)preCopy).scroll;
                                    preCopy = new Tile(((Pickupable)preCopy).replaceTile);
                                }
                                else message = Constants.invFullMsg;
                            }
                            else if (preCopy.tiletype == TileType.Exit) init();
                            else message = toCheck.ToString();

                            if (!abort)
                            {
                                tiles[toCheck.X, toCheck.Y] = tiles[old.X, old.Y];
                                Creature c = (Creature)tiles[toCheck.X, toCheck.Y];
                                tiles[old.X, old.Y] = c.lastTile;

                                c.lastTile = preCopy;
                                playerPos = toCheck;
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
                    Throwable t = ((Player)tiles[playerPos.X, playerPos.Y]).tWeapon;

                    switch (key)
                    {
                        // Think backwards here: If you need to go UP in an ARRAY, what do you need to do?
                        case ConsoleKey.UpArrow: handleThrowable(0, -1, t); break;
                        case ConsoleKey.DownArrow: handleThrowable(0, 1, t); break;
                        case ConsoleKey.LeftArrow: handleThrowable(-1, 0, t); break;
                        case ConsoleKey.RightArrow: handleThrowable(1, 0, t); break;
                        default: message = playerPos.ToString(); draw(); break;
                    }

                    state = State.Default;
                }
                #endregion
            }

            //loop();
        }

        void attackCreature(ref Tile creature)
        {
            Player p = (Player)tiles[playerPos.X, playerPos.Y];
            Creature c = (Creature)creature;

            int pdmg = ran.Next(p.damage.X, p.damage.Y) + damageMultiplier;

            //message = string.Format("{0} {1}", Constants.getPDamageInWords(pdmg), c.tiletype);
            pakMsg(string.Format("{0} {1}", Constants.getPDamageInWords(pdmg), c.tiletype));

            if (c.doDamage(pdmg, ref creature))
            {
                onDead(c.tiletype.ToString());
            }
        }

        void attackPlayer(ref Tile creature)
        {
            Creature c = (Creature)creature;
            Player p = (Player)tiles[playerPos.X, playerPos.Y];

            int cdmg = c.hit(ref ran, ref p);

            //message = string.Format("{0} {1}", c.tiletype, Constants.getCDamageInWords(cdmg));
            pakMsg(string.Format("{0} {1}", c.tiletype, Constants.getCDamageInWords(cdmg)));

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
                                Tile preCopy = tiles[p.X, p.Y];
                                preCopy.needsToBeDrawn = true;

                                tiles[p.X, p.Y] = tiles[x, y];
                                Creature c = (Creature)tiles[p.X, p.Y];
                                c.needsToBeDrawn = true;
                                c.processed = true;

                                tiles[x, y] = c.lastTile;
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
        void handleThrowable(sbyte x, sbyte y, Throwable t)
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
                    pakMsg(string.Format("{0} {1} with the Spear", Constants.getPDamageInWords(dmg), tiles[curPoint.X, curPoint.Y].tiletype));

                    Creature c = (Creature)tiles[curPoint.X, curPoint.Y];
                    if (c.doDamage(dmg, ref tiles[curPoint.X, curPoint.Y]))
                        onDead(c.tiletype.ToString());

                    processMonsters();
                    return;
                }
            }

            message = "I don't see any creature there!";
            draw();
        }
        
        Point getPointTowardsPlayer(int x, int y)
        {
            Point p = new Point(x,y);
            Creature c = (Creature)tiles[x, y];

            if (isInRangeOfPlayer(p, c.searchRange))
            {
                if (x < playerPos.X) p.X++;
                else if (x > playerPos.X) p.X--;
                if (y < playerPos.Y) p.Y++;
                else if (y > playerPos.Y) p.Y--;
            }
            else
            {
                p.X += ran.Next(-1, 2);
                p.Y += ran.Next(-1, 2);
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
            for (int x = 0; x < tiles.GetLength(0); x++) for (int y = 0; y < tiles.GetLength(1); y++) tiles[x, y].needsToBeDrawn = true;
            draw();
        }

        void draw()
        {
            for (int y = 0; y < tiles.GetLength(1); y++) // Changed x-y, because of the "Enter problem", but it fixed it self by removing one line.
            {
                for (int x = 0; x < tiles.GetLength(0); x++)
                {
                    if (tiles[x, y].needsToBeDrawn)
                    {
                        Console.SetCursorPosition(x, y);
                        Console.ForegroundColor = tiles[x, y].color;
                        Console.Write(tiles[x, y]);
                        tiles[x, y].needsToBeDrawn = false;
                    }
                    else if (tiles[x, y].needsToBeWiped)
                    {
                        Console.SetCursorPosition(x, y);
                        Console.Write(' ');
                        tiles[x, y].needsToBeWiped = false;
                    }
                }
            }
            

            Console.BackgroundColor = ConsoleColor.DarkBlue;
            Console.ForegroundColor = ConsoleColor.White;

            if (drawOnceAfterInit)
            {
                drawOnce(Console.WindowWidth * 2);
                drawOnceAfterInit = false;
            }

            Console.SetCursorPosition(0, tiles.GetLength(1));

            Player p = (Player)tiles[playerPos.X, playerPos.Y];

            dInfoWriter1.dualWrite("Health: " + p.health, "Money: " + p.money);
            if (p.lastTile.tiletype != TileType.Air) dInfoWriter2.dualWrite(message, p.lastTile.tiletype.ToString());
            else
            {
                string s = "O_o What the hell did you do this game?";
                //string name = getDungeonAt(playerPos).name;

                Room r = getDungeonAt(playerPos);
                if (r != null && r.name != null) s = r.name;

                dInfoWriter2.dualWrite(message, s);
            }

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
            
            for (int i = 0; i < item.image.GetLength(1) + 2; i++) line += xWall;

            string invNumber = name.ToString();
            string top = line;
            string bottom = line;

            int index = top.Length / 2;
            int insrt = invNumber.Length / 2;
            if (invNumber.Length == 1) top = top.Remove(index, 1).Insert(index, invNumber).Remove(0, invNumber.Length).Insert(0, lupWall.ToString()).Remove(top.Length - 1, 1) + rupWall;
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

                top = top.Remove(0, 1).Insert(0, lupWall.ToString()).Remove(top.Length - 2, 1) + rupWall;
            }

            bottom = bottom.Remove(0, 1).Insert(0, lSplitWall.ToString()).Remove(bottom.Length - 1, 1) + rSplitWall;

            for (int y = 0; y < item.image.GetLength(0); y++)
            {
                Console.ForegroundColor = item.color;
                for (int x = 0; x < item.image.GetLength(1); x++)
                {
                    Console.SetCursorPosition(orgin.X + 1 + x, y + 1);
                    Console.Write(item.image[y, x].ToString());
                }

                Console.ForegroundColor = ConsoleColor.White;
                Console.Write(yWall);

                Console.CursorLeft = orgin.X;
                Console.ForegroundColor = ConsoleColor.White;
                Console.Write(yWall);
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
                Console.Write(yWall);
                Console.ForegroundColor = color;
                Console.Write(lines[i]);
                Console.ForegroundColor = ConsoleColor.White;
                Console.Write(yWall);
            }

            Console.CursorTop++;
            Console.CursorLeft = orgin.X;
            Console.Write(bottom.Remove(0, 1).Insert(0, ldownWall.ToString()).Remove(bottom.Length - 1, 1) + rdownWall);
            

            Console.CursorLeft = orgin.X;
            Console.CursorTop = orgin.Y;
            //Console.Write(top);

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
            for (byte i = 0; i < p.inventory.Length; i++) drawInvItem(p.inventory[i], i);
            if (p.tWeapon != null) drawInvItem(Constants.invItems[p.tWeapon.invItemIndex], "Weapon");
            else drawInvItem(Constants.invItems[0], "Weapon");
        }
        #endregion

        #region "constant" messages methods
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

        void onDead(string name)
        {
            pakMsg("You have defeated " + name + "!");
        }

        void onPlayerDead()
        {
            message = "GAME OVER: R.I.P., " + ((Player)tiles[playerPos.X, playerPos.Y]).name + "!";
            draw();
            Console.ReadKey();
            Environment.Exit(0);
        }
        #endregion
    }

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
        public bool lighten = true;
        public bool needsToBeDrawn = true;
        public bool needsToBeWiped = false;
        public ConsoleColor color;
        public TileType tiletype;

        public Tile(TileType tp = TileType.None)
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
                    drawChar = ' '; break;
                case TileType.Air:
                    walkable = true;
                    drawChar = '.'; 
                    color = ConsoleColor.DarkGray; break;
                case TileType.Wall:
                    drawChar = '#';
                    color = ConsoleColor.Gray; break;
                case TileType.Corridor:
                    drawChar = '#';
                    color = ConsoleColor.White;
                    walkable = true; break;
                case TileType.Start:
                    drawChar = Game.chars[1];
                    color = ConsoleColor.DarkCyan;
                    walkable = true; break;
                case TileType.Exit:
                    drawChar = Game.chars[2];
                    color = ConsoleColor.Green;
                    walkable = true; break;
            }
        }

        public override string ToString()
        {
            return drawChar.ToString();
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
        }
    }

    class Scroll : Pickupable
    {
        // I just wrote a special program for this so you can easily calculate that...
        // Now I think: Why not just a string?
        readonly static char[,] inventoryScreenScroll = new char[,] { {' ','_','_','_','_','_','_','_','_','_','_',' ',' ',' ',' ',' ' },
	        { '(',')','_','_','_','_','_','_','_','_','_',')',' ',' ',' ',' ' },
	        { ' ','\\',' ','~','~','~','~','~','~','~','~',' ','\\',' ',' ',' ' },
	        { ' ',' ','\\',' ','~','~','~','~','~','~',' ',' ',' ','\\',' ',' ' },
	        { ' ',' ',' ','\\','_','_','_','_','_','_','_','_','_','_','\\',' ' },
	        { ' ',' ',' ','(',')','_','_','_','_','_','_','_','_','_','_',')'} };
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
            
            this.drawChar = Game.chars[0];
            this.tiletype = TileType.Scroll;

            invItem = new InventoryItem(scroll, scroll, inventoryScreenScroll, color);
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

        public int health = 25;
        public int money = 0;
        public int searchRange = 7;
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

        public void move(Tile old)
        {
            this.lastTile = old;
        }
    }

    class Player : Creature
    {
        public InventoryItem[] inventory = new InventoryItem[0];
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
        public uint level = 0;
        public int maxHealth = 25;

        private byte hMoves = 0;

        public Player() : base()
        {
            health = maxHealth;
            tiletype = TileType.Player;
            lastTile = new Tile(TileType.Start);
            drawChar = '☺';
            color = ConsoleColor.Magenta;
            damage = new Point(1, 5);
        }

        public bool addInventoryItem(InventoryItem item)
        {
            if (inventory.Length >= 10) return false;
            Array.Resize(ref inventory, inventory.Length + 1);
            inventory[inventory.Length - 1] = item;
            return true;
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
            money = ran.Next(1, 5);
            tiletype = TileType.Snake;
            drawChar = 'S';
            color = ConsoleColor.DarkGreen;
            damage = new Point(1, 3);
        }
    }

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
        Wizzard,
        Start,
        Exit
    }


    static class Constants
    {
        public static readonly string[] dungeonNameParts = new string[] { "Prisons", "Dungeon", "Tunnels", "Chamber", "Tombs", "Cells", "Lair", "Cave", "Catacombs", "Caverns" };
        public readonly static string[] namefwords = new string[]
        { "Witty", "Ancient", "Evil", "Heartless", "Furious", "Screaming", "Mighty", "Infamous", "Loved", "Isolated", "Withered", "Magic", "Undead", "Mystic", "Dark", "Black", "Creepy"};
        public readonly static string[] nameswords = new string[]
        { "Orc", "Elf", "Prisoner", "Dragon", "Angel", "Demon", "Giant", "Ghost", "Priest", "Cobra", "Warrior", "Phantom", "Hunter", "Phoenix", "Widow", "Mage", "Warlock" };
        public readonly static string[] namewords = new string[] { "Blindness", "Resurrection", "Power", "Acceptance", "Tears", "Undead", "Lies", "Fear", "Disbelieve", "Anger" };

        public readonly static InventoryItem[] invItems = {
            new InventoryItem("Empty", "Empty", new char[,] {{'\\',' ',' ',' ',' ',' ','/'},{' ','\\',' ',' ',' ','/',' '},{' ',' ','\\',' ','/',' ',' '},{' ',' ',' ','X',' ',' ',' '},
	        { ' ',' ','/',' ','\\',' ',' ' },{ ' ','/',' ',' ',' ','\\',' ' },{ '/',' ',' ',' ',' ',' ','\\'}}, ConsoleColor.DarkRed),
            new InventoryItem("Spear", "The most glorious weapon in the entire dungeon!",
            new char[,] { {' ',' ',' ',' ',' ',' ','/','|' },{ ' ',' ',' ',' ',' ','/',' ','|' },
            { ' ',' ',' ',' ',' ','/','/',' ' },{ ' ',' ',' ',' ','/','/',' ',' ' },{ ' ',' ',' ','/','/',' ',' ',' ' },{ ' ',' ','/','/',' ',' ',' ',' '} }, ConsoleColor.Gray) };


        public const string invFullMsg = "Inventory full!";

        public static string getPDamageInWords(int dmg)
        {
            if (dmg == 0) return "You have tried to attack";
            else if (dmg > 0 && dmg < 5) return "You did a simple hit on";
            else if (dmg >= 5 && dmg < 10) return "You did a great hit on";
            else return "You did a critical hit on";
        }

        public static string getCDamageInWords(int dmg)
        {
            if (dmg < 0) return "has healed you with " + dmg;
            else if (dmg == 0) return "missed you";
            else if (dmg > 0 && dmg < 5) return "did a simple hit on you";
            else if (dmg >= 5 && dmg < 10) return "did a great hit on you";
            else return "did a critical hit on you";
        }

        public static string[] getReadableString(string input, int lineLength)
        {
            string[] sInput = input.Split(' ');
            string[] lines = new string[input.Length];

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
            /*return (b) ? false : true*/
            return !b;
        }
    }
}
