using System;

namespace Super_ForeverAloneInThaDungeon
{
    // I question myself: Have I ever been this depressed while coding?
    // I've a lot to learn.
    // fiav-minutehs-latehrh
    // OH THIS IS SO FUN
    // another fiav-minutehs-laterh
    // this code is shit
    // i am shit
    partial class Game
    {
        public static readonly Random ran;

        enum State
        {
            Default, Inventory, Throwing
        }

        const TileType noneTile = TileType.None; // use ONLY for dungeon generation(educational :p) exception purposes.

        bool hack = false; // you can walk everywhere if true
        bool hackLighting = false;
        bool disableFight = false;
        bool drawOnceAfterInit = false; // That's for the blue bar-thinggy underneath.

        uint currentFloor = 0;

        // Environment
        LevelPlanner level = new LevelPlanner();
        Room[] rooms;
        Tile[,] tiles; // Ode to the mighty comma!
        ushort[,] scores; // For enemys searching the player

        // Div
        State state = State.Default;

        LineWriter infoLine1 = new LineWriter(new ushort[] { 0, 20, 40, 60, 0xfaef });
        LineWriter infoLine2 = new LineWriter(new ushort[] { 0, 0xdead });

        // Player
        Point playerPos = new Point(-1, -1);

        static string currentMessage = "";


        static Game()
        {
            ran = new Random();
        }

        /// <summary>
        /// Gets or sets your mother's car windscreenwiper.
        /// </summary>
        /// <param name="size">Default: 120, 50. Product of size can never be larger than 65536, because that can lead to breadth-first search problems</param>
        public Game(Point size)
        {
            //Console.CursorVisible = false;
            Console.OutputEncoding = Constants.enc;
            //Console.Title = "SuperForeverAloneInThaDungeon"; Player will set the name
            Console.SetWindowSize(size.X, size.Y + 3);
            Console.BufferHeight = Console.WindowHeight;

            tiles = new Tile[size.X, size.Y]; // !UNSAFE!
            scores = new ushort[size.X, size.Y]; // !ALSO UNSAFE!

            // inventory stuff
            invDItems = new DisplayItem[Constants.invCapacity];
        }


        // TODO: Make redrawing effecienter
        public void run()
        {
            Player p = new Player();
            Message("Welcome, " + p.name + '!'); // I could have just used Environment.UserName since the Player.name = Environment.UserName... :~)

            while (true)
            {
                currentFloor++;
                setDungeonToEmpty();

                level.UpdateSystems((int)currentFloor);

                // tweak this
                //rooms = createDungeons(15, new Room(new Point(4, 19), new Point(5, 15))); // Counted from 0 *trollface*
                rooms = generateDungeons(10);

                spawnPlayerInRoom(rooms[ran.Next(0, rooms.Length)], p);

                onPlayerMove(ref p); // make sure everything inits properly

                if (disableFight) p.walkable = false;

                

                // Make sure the dungeon starts nice and clean
                // MAKE EFFECIENTER???
                reDrawDungeon();

                gameLoop();


                p = (Player)tiles[playerPos.X, playerPos.Y];
            }
        }

        void gameLoop()
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
                        case ConsoleKey.OemPeriod: ReadCommand(); doNotCallDraw = true; break;
                        case ConsoleKey.Escape: Environment.Exit(0); break;
                        case ConsoleKey.R: return;
                        case ConsoleKey.K: tryKick(); break;
                        case ConsoleKey.LeftArrow: toAdd.X--; break;
                        case ConsoleKey.RightArrow: toAdd.X++; break;
                        case ConsoleKey.UpArrow: toAdd.Y--; break;
                        case ConsoleKey.DownArrow: toAdd.Y++; break;
                        case ConsoleKey.Tab: state = State.Inventory; drawInventory(); continue;
                        case ConsoleKey.F1: hack = hack.Invert(); break; //                                                 HACK TOGGLE HACK TOGGLE HACK
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
                            if (key == ConsoleKey.S && ((Player)tiles[playerPos.X, playerPos.Y]).RangedWeapon != null)
                            {
                                attackRanged(askDirection(), ((Player)tiles[playerPos.X, playerPos.Y]).RangedWeapon, playerPos);
                            }
                            break;
                    }

                    // Clear what key the user entered(prevent uglyness)
                    Console.CursorLeft = 0;
                    Console.Write(' ');
                    Console.CursorLeft = 0;

                    if (toAdd.X != 0 || toAdd.Y != 0)
                    {
                        Point toCheck = new Point(playerPos.X + toAdd.X, playerPos.Y + toAdd.Y);
                        //if (/*isValidMove(toCheck) || hack*/)
                        //{
                            Point old = playerPos;                      // Tile that will appear(old tile under player)
                            Tile preCopy = tiles[toCheck.X, toCheck.Y]; // Tile where the player will move to

                            Player p = (Player)tiles[old.X, old.Y];

                            bool abort = false; // if true, move will be halted no matter if it's a possible move or not.

                            // TODO: if preCopy is WorldObject && isAttackable
                            if (preCopy is Creature)
                            {
                                p.Attack(ref tiles[toCheck.X, toCheck.Y]);
                                abort = true;
                            }
                            else if (preCopy.tiletype == TileType.Money)
                            {
                                // I knew I'd know the English grammar!
                                // I'm sick of these games with "You found 1 coins!"
                                int money = ((Money)preCopy).money;
                                string s = money == 1 ? "" : "s";
                                p.money += money;
                                Message("You found " + money + " coin" + s + '!');
                                preCopy = new Tile(((Pickupable)preCopy).replaceTile);
                            }
                            else if (preCopy is Pickupable)
                            {
                                // assuming all other pickupables are handled!
                                if (p.AddInventoryItem(((Pickupable)preCopy).GenerateInvItem()))
                                {
                                    Message("You found " + p.LastInventoryItem().name);
                                    preCopy = new Tile(((Pickupable)preCopy).replaceTile);
                                }
                                else Message(Constants.invFullMsg);
                            }
                            else if (preCopy is Chest)
                            {
                                // Don't allow to walk on chest
                                // (not needed)
                                abort = true;

                                Chest c = (Chest)preCopy;

                                int count = c.contents.Length;

                                Message("You explore the chest");
                                if (count > 0)
                                    for (int i = 0; i < c.contents.Length; i++)
                                    {
                                        if (p.AddInventoryItem(c.contents[i]))
                                        {
                                            Message("You found " + c.contents[i].name);
                                            count--;
                                        }
                                        else
                                        {
                                            Message("There's more, but you can't carry more");
                                            break;
                                        }
                                    }
                                else
                                    Message("The chest is empty");

                                if (count == 0)
                                    c.contents = new InventoryItem[0];
                                else
                                {
                                    InventoryItem[] items = new InventoryItem[count];

                                    int n = c.contents.Length - count;

                                    for (int i = 0; i < count; i++)
                                    {
                                        items[i] = c.contents[n + i];
                                    }

                                    c.contents = items;
                                }
                            }
                            else if (preCopy is Door)
                            {
                                Door door = (Door)preCopy;
                                if (!door.Open)
                                {
                                    ((Door)preCopy).TryOpen();
                                    abort = true;
                                }
                            }
                            else if (preCopy.tiletype == TileType.Down) return;


                            if (!abort && (isValidMove(toCheck) || hack))
                            {
                                tiles[toCheck.X, toCheck.Y].needsToBeDrawn = true;
                                preCopy.needsToBeDrawn = true;
                                tiles[old.X, old.Y].needsToBeDrawn = true;

                                tiles[toCheck.X, toCheck.Y] = tiles[old.X, old.Y];
                                Player plyr = (Player)tiles[toCheck.X, toCheck.Y];
                                tiles[old.X, old.Y] = plyr.lastTile;

                                plyr.lastTile = preCopy;
                                playerPos = toCheck;

                                onPlayerMove(ref plyr);
                            }
                        //}
                    }

                    if (!doNotCallDraw)
                    {
                        update();
                        processMonsters();
                        draw();
                    }
                }
                #endregion
                #region inventory state
                else if (state == State.Inventory)
                {
                    ConsoleKey key = Console.ReadKey().Key;

                    if (((Player)tiles[playerPos.X, playerPos.Y]).nInvItems > 0)
                        switch (key)
                        {
                            case ConsoleKey.LeftArrow:
                                inv_changeSelectedItem(invSelItem - 1);
                                break;
                            case ConsoleKey.RightArrow:
                                inv_changeSelectedItem(invSelItem + 1);
                                break;
                            case ConsoleKey.UpArrow:
                                if (--invActionSel < 0)
                                    invActionSel = ((Player)tiles[playerPos.X, playerPos.Y]).inventory[invSelItem].actions.Length - 1;
                                inv_drawDescription();
                                break;
                            case ConsoleKey.DownArrow:
                                if (++invActionSel >= ((Player)tiles[playerPos.X, playerPos.Y]).inventory[invSelItem].actions.Length)
                                    invActionSel = 0;
                                inv_drawDescription();
                                break;
                            case ConsoleKey.Enter:
                                doSelectedInventoryAction();
                                break;

                            case ConsoleKey.OemPeriod: ReadCommand(); break;
                            case ConsoleKey.Tab: state = State.Default; reDrawDungeon(); continue;
                            case ConsoleKey.Escape: Environment.Exit(0); break;
                        }
                    else
                        switch (key)
                        {
                            case ConsoleKey.OemPeriod: ReadCommand(); break;
                            case ConsoleKey.Tab: state = State.Default; reDrawDungeon(); continue;
                            case ConsoleKey.Escape: Environment.Exit(0); break;
                        }
                }
                #endregion
            }
        }

        /// <summary>
        /// Updates all WorldObjects
        /// </summary>
        void update()
        {
            level.creatureSpawner.Update(rooms, ref tiles);

            for (int x = 0; x < tiles.GetLength(0); x++)
                for (int y = 0; y < tiles.GetLength(1); y++)
                {
                    if (tiles[x, y] is WorldObject)
                    {
                        WorldObject obj = (WorldObject)tiles[x, y];

                        obj.Update();

                        if (obj.destroyed)
                        {
                            obj.Drop(ref tiles[x, y]);
                        }
                    }
                }
        }


        #region div
        void processMonsters()
        {
            ((Creature)tiles[playerPos.X, playerPos.Y]).processed = true;

            for (int x = 0; x < tiles.GetLength(0); x++) for (int y = 0; y < tiles.GetLength(1); y++)
                {
                    if (tiles[x, y] is Creature && !((Creature)tiles[x, y]).processed)
                    {
                        Point p = getPointTowardsPlayer(x, y);

                        if (isValidMove(p) && !p.Same(x, y))
                        {
                            if (tiles[p.X, p.Y].tiletype == TileType.Player)
                            {
                                ((Creature)tiles[x, y]).Attack(ref tiles[playerPos.X, playerPos.Y]);
                            }
                            else //if (!(tiles[p.X, p.Y] is Creature)) // for 
                            {
                                Tile preCopy = tiles[p.X, p.Y]; // target tile

                                tiles[p.X, p.Y] = tiles[x, y];
                                Creature c = (Creature)tiles[p.X, p.Y];

                                if (preCopy.lighten)
                                {
                                    c.discovered = true; // TODO: Find better method then discovered?
                                    c.lighten = true;
                                    c.needsToBeDrawn = true;
                                }
                                c.notLightenChar = preCopy.notLightenChar;
                                c.processed = true;

                                tiles[x, y] = c.lastTile;
                                if (preCopy.lighten) tiles[x, y].needsToBeDrawn = true;
                                c.OnTileEncounter(ref preCopy);
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

            if (((WorldObject)tiles[playerPos.X, playerPos.Y]).destroyed)
            {
                gameOver();
            }
        }


        void attackRanged(Point direction, Throwable t, Point origin)
        {
            Point curPoint = origin;
            for (byte i = 0; i < t.range; i++)
            {
                curPoint.X += direction.X;
                curPoint.Y += direction.Y;

                if (isInScreen(curPoint) && tiles[curPoint.X, curPoint.Y] is WorldObject)
                {
                    ((Creature)tiles[origin.X, origin.Y]).Attack(ref tiles[curPoint.X, curPoint.Y], AttackMode.Ranged);
                    return;
                }
            }
            Message("Nothing to target!");
        }

        void generateScoreGrid(Point p)
        {
            int count = 0; // this value will never be reached I think. It's mathematical worst-case, which is practically impossible in this scenario.
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
            scores[p.X, p.Y] = 0; // origin/target = 0

            // browse trough them, mark the scores
            while (index >= 0)
            {
                BFSN cTile = list[index--];
                ushort nscore = (ushort)(cTile.score + 1);

                if (cTile.x + 1 < tiles.GetLength(0) && scores[cTile.x + 1, cTile.y] == Constants.SFLAG_NEEDS_CHECK)
                {
                    scores[cTile.x + 1, cTile.y] = nscore;
                    list[++index] = new BFSN(cTile.x + 1, cTile.y, nscore);
                }
                if (cTile.x > 0 && scores[cTile.x - 1, cTile.y] == Constants.SFLAG_NEEDS_CHECK)
                {
                    scores[cTile.x - 1, cTile.y] = nscore;
                    list[++index] = new BFSN(cTile.x - 1, cTile.y, nscore);
                }
                if (cTile.y + 1 < tiles.GetLength(1) && scores[cTile.x, cTile.y + 1] == Constants.SFLAG_NEEDS_CHECK)
                {
                    scores[cTile.x, cTile.y + 1] = nscore;
                    list[++index] = new BFSN(cTile.x, cTile.y + 1, nscore);
                }
                if (cTile.y > 0 && scores[cTile.x, cTile.y - 1] == Constants.SFLAG_NEEDS_CHECK)
                {
                    scores[cTile.x, cTile.y - 1] = nscore;
                    list[++index] = new BFSN(cTile.x, cTile.y - 1, nscore);
                }
            }
        }

        void onPlayerMove(ref Player p)
        {
            p.onMove();

            // lighting stuff
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


        Point getPointTowardsPlayer(int x, int y)
        {
            Point p = new Point(x, y);
            Creature c = (Creature)tiles[x, y];

            c.UpdatePlayerDiscovery(isInRangeOfPlayer(p, c.searchRange));

            switch (c.moveMode)
            {
                case CreatureMoveMode.FollowPlayer:
                    Point preferredDir = new Point(playerPos.X + (x > playerPos.X ? -1 : 1), playerPos.Y + (y > playerPos.Y ? 1 : -1));

                    Point[] points = new Point[] {
                        // diagonal moves
                        new Point(x - 1, y - 1),
                        new Point(x + 1, y + 1),
                        new Point(x - 1, y + 1),
                        new Point(x + 1, y - 1),
                        // horizontal/vertical moves
                        new Point(x - 1, y),
                        new Point(x + 1, y),
                        new Point(x, y - 1),
                        new Point(x, y + 1)
                    };
                    ushort least = ushort.MaxValue;
                    byte n = 0xff;

                    // this has preferences.
                    for (byte i = 0; i < points.Length; i++)
                    {
                        int scor = scores[points[i].X, points[i].Y];
                        if (scores[points[i].X, points[i].Y] < least)
                        {
                            least = scores[points[i].X, points[i].Y];
                            n = i;
                        }
                    }

                    if (scores[preferredDir.X, preferredDir.Y] <= least)
                    {
                        return preferredDir;
                    }

                    if (n != 0xff)
                    {
                        p = points[n];
                    }
                    break;

                case CreatureMoveMode.Random:
                    c.UpdatePlayerDiscovery(false);
                    points = new Point[5];
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
                    break;
            }

            return p;
        }
        #region boolean expressions
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
        #endregion

        #region popups
        void wipeDisplayItem(DisplayItem d)
        {
            for (int y = d.pos.Y; y < d.EndY; y++)
                for (int x = d.pos.X; x < d.EndX; x++)
                    tiles[x, y].needsToBeDrawn = true;

            char[] buffer = new char[d.width];
            for (short i = 0; i < d.width; i++)
            {
                buffer[i] = ' ';
            }

            Console.CursorTop = d.pos.Y;
            Console.CursorLeft = d.pos.X;
            for (int y = d.pos.Y; y < d.EndY; y++)
            {
                Console.CursorLeft = d.pos.X;
                Console.Write(buffer);
                Console.CursorTop++;
            }
        }
        #endregion

        #region "constant" messages methods
        public static void Message(string s)
        {
            currentMessage += '\n' + s ;
        }

        string getDefaultMessage()
        {
            return playerPos.ToString();
        }
        #endregion

        #region player thinggies
        Point askDirection()
        {
            Message("Which direction?");
            drawInfoBar();
            Point p = Constants.GetDirectionByKey(Console.ReadKey().Key);
            if (p.Same(0, 0))
            {
                Message("Never mind.");
                drawInfoBar();
            }
            return p;
        }

        void tryKick()
        {
            Point dir = askDirection();
            if (!dir.Same(0, 0))
            {
                Tile t = tiles[playerPos.X + dir.X, playerPos.Y + dir.Y];
                if (t is WorldObject)
                {
                    ((WorldObject)t).Kick();
                }
                else
                {
                    Message("Nothing to kick there");
                }
            }
        }
        #endregion

        void gameOver()
        {
            // rest in rip player
            Message("GAME OVER: R.I.P. " + ((Player)tiles[playerPos.X, playerPos.Y]).name + '!');
            draw();
            Console.ReadKey();

            Player p = (Player)tiles[playerPos.X, playerPos.Y];

            // display and update highscores
            Highscores.Add(p.name, (int)-currentFloor, (int)p.level, (int)p.money);
            Highscores.Display();

            Environment.Exit(0);
        }
    }
}
