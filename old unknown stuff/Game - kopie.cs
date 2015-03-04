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
            Default, Inventory, Combat, Pause, ThrowingSpear
        }

        public static char[] chars;
        static char yWall, xWall, lupWall, ldownWall, rupWall, rdownWall, lSplitWall, rSplitWall;

        bool hack = true;
        bool disableFight = false;
        bool drawOnceAfterInit = true;

        // Environment
        Room[] rooms;

        // Player stuff
        string currentWeapon = "Spear";
        sbyte damageMultiplier = 0;

        // Div
        Random ran;
        State state = State.Default;
        DualWriter dInfoWriter1 = new DualWriter();
        DualWriter dInfoWriter2 = new DualWriter(); // Why can't we just do old DualWriter(); ?

        // Player
        Point playerPos;

        string message = "Wow I didn't initiliaze apperantly...";

        Tile[,] tiles;

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

        // TODO: rewrite whole system
        // OPTION: Generate rooms first, then scatter bonus thinggys around the whole map.
        #region Dungeon generation stuff
        Room[] createDungeons(ushort howManyRooms)
        {
            Point spawnRoomGenPoint = new Point(tiles.GetLength(0) / 2 + ran.Next(-10, 5), tiles.GetLength(1) / 2 + ran.Next(-10, 5));
            Room spawnRoom = new Room(spawnRoomGenPoint, new Point(spawnRoomGenPoint.X + ran.Next(4, 12), spawnRoomGenPoint.Y + ran.Next(4, 8)));
            spawnRoom.appendName(ref ran);
            generateRoom(spawnRoom);

            Room[] dungeons = new Room[howManyRooms + 1];
            dungeons[0] = spawnRoom;
            int a = 0;

            Tile[,] oldTiles = tiles;

            // "The for loop is the loop you are going to use the most" PFFFFFFFTSSJHHHHHHHHHHHH WHAHAHAHAHh
            for (int i = 0; i < 1; /*howManyRooms;*/ i++)
            {
                /*byte direction = 0;
                int cLength = 0;
                Point dungeonConnectPoint;
                Room d;

                while (true)
                {
                    d = dungeons[ran.Next(0, a + 1)];

                    // You might think: Why do you work with bytes instead of enums? Well I think it's easier if you have just 4 values do do it
                    // with codes, rather than Direction direction = Direction.North. Also, you can pick random ones easier.
                    direction = (byte)ran.Next(0, 4);
                    dungeonConnectPoint = getRandomWallPoint(d, direction);
                    cLength = ran.Next(4, 7);
                    if (generateTiles(dungeonConnectPoint, direction, cLength)) break;
                }

                //Point p = getRandomCorridorRoomBuildPoint(direction, cLength, dungeonConnectPoint, ran.Next(3, 7));

                bool doAgain = true;
                while (doAgain)
                {
                    Point p = new Point(-1, -1);
                    while (p.same(-1, -1))
                        p = getRandomCorridorRoomBuildPoint(direction, cLength, dungeonConnectPoint, ran.Next(3, 7));

                    Room planningRoom = null;
                    for (byte b = 0; b < 10; b++)
                    {
                        if (planningRoom == null)
                            planningRoom = generateRoomAtCorridor(p, direction, new Point(ran.Next(5, 12), ran.Next(4, 8)));
                        else
                        {
                            planningRoom.appendName(ref ran);
                            dungeons[a] = planningRoom;
                            a++;
                            doAgain = false;
                            break;
                        }
                    }
                }*/
                a++;
            }


            Array.Resize(ref dungeons, a);
            return dungeons;
        }

        void setDungeonToEmpty()
        {
            for (int a = 0; a < tiles.GetLength(0); a++)
                for (int b = 0; b < tiles.GetLength(1); b++)
                    tiles[a, b] = new Tile(TileType.None);
        }

        bool canRoomBePlaced(Room r)
        {
            if (!isInScreen(r.where) || !isInScreen(r.end)) return false;
            for (int x = r.where.X; x < r.end.X; x++) for (int y = r.where.Y; y < r.end.Y; y++)
                    if (tiles[x, y].tiletype != TileType.None) return false;
            return true;
        }

        void spawnPlayerInRoom(Room r)
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

            tiles[playerPos.X, playerPos.Y] = new Player();
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

        Room generateRoomAtCorridor(Point c, byte dir, Point size)
        {
            Room r;

            switch (dir)
            {
                default: r = new Room(c, new Point(c.X + size.X, c.Y + size.Y)); break;
                case 1: r = new Room(new Point(c.X - size.X, c.Y - size.Y), c); break;
                case 2: r = new Room(c, new Point(c.X + size.X, c.Y + size.Y)); break;
                case 3: r = new Room(new Point(c.X - size.X, c.Y - size.Y), c); break;
            }

            r.appendName(ref ran);
            if (canRoomBePlaced(r)) generateRoom(r);

            return r;
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
            // What else did you expected
            switch (dir)
            {
                default: return new byte[] { 2, 3 };
                case 2: case 3: return new byte[] { 0, 1 };
            }
        }

        int getDifference(Point p, Room r, byte dir)
        {
            switch (dir)
            {
                default: return p.X - r.where.X;
                case 2: case 3: return p.Y - r.where.Y;
            }
        }

        // TODO: Random at generateTiles?
        Point getRandomCorridorRoomBuildPoint(byte cDir, int length, Point p, int cLength = 3, TileType t = TileType.Corridor)
        {
            Point inc = getIncrementByDirection(cDir);

            for (int i = 0; i < length; i++)
            {
                int l = ran.Next(i + 1, length);
                Point cp = new Point(p.X + (inc.X * l), p.Y + (inc.Y * l));
                byte dir = getPossibleCorridorDirections(cDir)[ran.Next(0, 2)]; // Length is constant: 2

                if (generateTiles(cp, dir, cLength, t))
                {
                    Point a = getIncrementByDirection(dir);
                    return new Point(cp.X + (a.X * cLength), cp.Y + (a.Y * cLength));
                }
            }

            return new Point(-1, -1);
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

        bool generateTiles(Point p, byte dir, int howMany, TileType t = TileType.Corridor)
        {
            // Algorithm:
            // 1: Check if everything is possible
            // if so, set everything and return true, otherwise return false.

            Point d = getIncrementByDirection(dir);
            Point current = p;
            Point[] points = new Point[howMany];

            for (int i = 0; i < howMany; i++)
            {
                points[i] = current;
                current.X += d.X;
                current.Y += d.Y;
            }

            if (!isInScreen(current)) return false;
            
            // Skip first one because that one is usually a wall, that needs to be replaced.
            for (int i = 1; i < howMany; i++)
                if (tiles[points[i].X, points[i].Y].tiletype != TileType.None) return false;

            for (int i = 0; i < howMany; i++)
                tiles[points[i].X, points[i].Y] = new Tile(t);

            return true;
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
            Stopwatch sw = new Stopwatch();
            sw.Start();
            ran = new Random();

            setDungeonToEmpty();

            /*Room dungeon1 = new Room(new Point(ran.Next(0, 20), ran.Next(0, 10)), new Point(ran.Next(32, tiles.GetLength(0)), ran.Next(12, tiles.GetLength(1))), ref ran);

            genererateRoom(dungeon1);

            rooms = new Room[] { dungeon1 };*/
            rooms = createDungeons(8);
            spawnPlayerInRoom(rooms[ran.Next(0, rooms.Length)]);

            message = "Welcome, " + ((Player)tiles[playerPos.X, playerPos.Y]).name + "!"; // I could have just used Environment.UserName since the Player.name = Environment.UserName... :~)
            if (disableFight) ((Player)tiles[playerPos.X, playerPos.Y]).walkable = false;

            sw.Stop();
            message = sw.Elapsed.ToString();

            draw();
            loop();
        }

        void loop()
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
                    case ConsoleKey.Tab: state = State.Inventory; Console.Clear(); drawOnceAfterInit = true; loop(); return;
                    case ConsoleKey.S: if (currentWeapon == "Spear") state = State.ThrowingSpear; else doNotCallDraw = true; loop(); return;
                    default: doNotCallDraw = true; break;
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
                            int money = ((Money)preCopy).money;
                            string s = "";
                            if (money != 1) s = "s";
                            message = "You found " + money + " coin" + s + "!";
                            ((Player)tiles[old.X, old.Y]).money += money;
                            preCopy = new Tile(TileType.Air);
                        }
                        else if (preCopy.tiletype == TileType.Scroll)
                        {
                            if (p.addInventoryItem(((Scroll)preCopy).invItem))
                            {
                                message = "You have found " + ((Scroll)preCopy).scroll;
                                preCopy = new Tile(TileType.Air);
                            }
                            else message = Constants.invFullMsg;
                        }
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
                    case ConsoleKey.Tab: state = State.Default; Console.Clear(); reDrawDungeon(); loop(); return;
                    case ConsoleKey.Escape: Environment.Exit(0); break;
                }
            }
            #endregion
            #region ThrowingSpear state
            else if (state == State.ThrowingSpear)
            {
                message = "Which direction?";
                draw();
                ConsoleKey key = Console.ReadKey().Key;

                switch (key)
                {
                    // Think backwards here: If you need to go UP in an ARRAY, what do you need to do?
                    case ConsoleKey.UpArrow: handleSpear(0, -1); break;
                    case ConsoleKey.DownArrow: handleSpear(0, 1); break;
                    case ConsoleKey.LeftArrow: handleSpear(-1, 0); break;
                    case ConsoleKey.RightArrow: handleSpear(1, 0); break;
                    default: message = playerPos.ToString(); draw(); break;
                }

                state = State.Default;
            }
            #endregion

            loop();
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
        void handleSpear(sbyte x, sbyte y)
        {
            Point curPoint = playerPos;
            for (byte i = 0; i < Constants.spearRange; i++) // Just saved you 24 bit of RAM!
            {
                curPoint.X += x;
                curPoint.Y += y;

                if (isInScreen(curPoint)) if (tiles[curPoint.X, curPoint.Y] is Creature)
                {
                    int dmg = ran.Next(Constants.spearMinDmg + damageMultiplier, Constants.spearMaxDmg + damageMultiplier);

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
            drawInvItem(Constants.spear, currentWeapon);
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

    class Scroll : Tile
    {
        // I just wrote a special program for this so you can easily calculate that.
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

    class Money : Tile
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

        public void onTileEncounter(ref Tile t)
        {
            if (t.tiletype == TileType.Money)
            {
                money += ((Money)t).money;
                t = new Tile(TileType.Air);
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

        public string name = Environment.UserName;
        public uint xp = 0;
        public uint level = 0;
        public int maxHealth = 25;

        private byte hMoves = 0;

        public Player()
        {
            tiletype = TileType.Player;
            drawChar = '☺';
            color = ConsoleColor.Magenta;
            damage = new Point(1, 5);
            walkable = true;
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
        public Snake(ref Random ran)
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
        { "Orc", "Elf", "Prisoner", "Dragon", "Angel", "Demon", "Giant", "Ghost", "Cobra", "Warrior", "Phantom", "Hunter", "Phoenix", "Widow", "Mage", "Warlock" };
        public readonly static string[] namewords = new string[] { "Blindness", "Resurrection", "Power", "Acceptance", "Tears", "Undead", "Lies", "Fear", "Disbelieve", "Anger" };

        public const string invFullMsg = "Inventory full!";
        public readonly static InventoryItem spear = new InventoryItem("Spear", "The most glorious weapon in the entire dungeon!",
        new char[,] { {' ',' ',' ',' ',' ',' ','/','|' },{ ' ',' ',' ',' ',' ','/',' ','|' },
        { ' ',' ',' ',' ',' ','/','/',' ' },{ ' ',' ',' ',' ','/','/',' ',' ' },{ ' ',' ',' ','/','/',' ',' ',' ' },{ ' ',' ','/','/',' ',' ',' ',' '} }, ConsoleColor.Gray);

        // Weapons
        public const byte spearRange = 3; // three tiles
        public const byte spearMinDmg = 5;
        public const byte spearMaxDmg = 10;

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
            else return "did a great hit on you";
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
    }
}
