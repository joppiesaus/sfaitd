﻿using System;

namespace Super_ForeverAloneInThaDungeon
{
    // I question myself: Have I ever been this depressed while coding?
    // I've a lot to learn.
    partial class Game
    {
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

        // Div
        Random ran;
        State state = State.Default;

        LineWriter infoLine1 = new LineWriter(new ushort[] { 0, 20, 40, 60, 0xfaef });
        LineWriter infoLine2 = new LineWriter(new ushort[] { 0, 0xdead });

        // Player
        Point playerPos = new Point(-1, -1);

        static string currentMessage = "";

        

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

            // inventory stuff
            invDItems = new InventoryDisplayItem[Constants.invCapacity];

            ran = new Random();

            game();
        }


        // TODO: Make effecienter
        void game()
        {
            while (true)
            {
                Player p;
                if (playerPos.same(-1, -1)) p = new Player();
                else p = (Player)tiles[playerPos.X, playerPos.Y];

                setDungeonToEmpty();

                // tweak this
                rooms = createDungeons(15, new Room(new Point(4, 19), new Point(5, 15))); // Counted from 0 *trollface*

                spawnPlayerInRoom(rooms[ran.Next(0, rooms.Length)], p);

                onPlayerMove(ref p); // make sure everything inits properly

                msg("Welcome, " + p.name + "!"); // I could have just used Environment.UserName since the Player.name = Environment.UserName... :~)
                if (disableFight) p.walkable = false;


                currentFloor++;


                // Make sure the dungeon starts nice and clean
                reDrawDungeon();

                gameLoop();
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
                        case ConsoleKey.Escape: Environment.Exit(0); break;
                        case ConsoleKey.OemPeriod: ReadCommand(); doNotCallDraw = true; break;
                        case ConsoleKey.R: return;
                        case ConsoleKey.LeftArrow: toAdd.X--; break;
                        case ConsoleKey.RightArrow: toAdd.X++; break;
                        case ConsoleKey.UpArrow: toAdd.Y--; break;
                        case ConsoleKey.DownArrow: toAdd.Y++; break;
                        case ConsoleKey.Tab: state = State.Inventory; drawInventory(); continue;
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
                            if (key == ConsoleKey.S && ((Player)tiles[playerPos.X, playerPos.Y]).rangedWeapon != null)
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
                                msg("You found " + money + " coin" + s + '!');
                                preCopy = new Tile(((Pickupable)preCopy).replaceTile);
                            }
                            // TODO: Make available for other items
                            else if (preCopy is Pickupable)
                            {
                                // assuming all other pickupables are handled!
                                if (p.addInventoryItem(((Pickupable)preCopy).getInvItem(ref ran)))
                                {
                                    msg("You found " + p.lastInventoryItem().name);
                                    preCopy = new Tile(((Pickupable)preCopy).replaceTile);
                                }
                                else msg(Constants.invFullMsg);
                            }
                            else if (preCopy is Chest)
                            {
                                // Don't allow to walk on chest
                                abort = true;

                                Chest c = (Chest)preCopy;

                                int count = c.contents.Length;

                                msg("You explore the chest");
                                if (count > 0)
                                    for (int i = 0; i < c.contents.Length; i++)
                                    {
                                        if (p.addInventoryItem(c.contents[i]))
                                        {
                                            msg("You found " + c.contents[i].name);
                                            count--;
                                        }
                                        else
                                        {
                                            msg("There's more, but you can't carry more");
                                        }
                                    }
                                else
                                    msg("The chest is empty");

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
                            else if (preCopy.tiletype == TileType.Down) return;


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
                    //drawInventory();
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
                                drawInventory();
                                break;

                            case ConsoleKey.Tab: state = State.Default; /*Console.Clear();*/ reDrawDungeon(); continue;
                            case ConsoleKey.Escape: Environment.Exit(0); break;
                        }
                    else
                        switch (key)
                        {
                            case ConsoleKey.Tab: state = State.Default; /*Console.Clear();*/ reDrawDungeon(); continue;
                            case ConsoleKey.Escape: Environment.Exit(0); break;
                        }
                }
                #endregion
                #region Throwing state
                else if (state == State.Throwing)
                {
                    msg("Which direction?");
                    draw();
                    Player p = (Player)tiles[playerPos.X, playerPos.Y];
                    Throwable t = p.rangedWeapon;

                    switch (Console.ReadKey().Key)
                    {
                        // Think backwards here: If you need to go UP in an ARRAY, what do you need to do?
                        case ConsoleKey.UpArrow: handleThrowable(0, -1, p.rangedWeapon, ref p); break;
                        case ConsoleKey.DownArrow: handleThrowable(0, 1, p.rangedWeapon, ref p); break;
                        case ConsoleKey.LeftArrow: handleThrowable(-1, 0, p.rangedWeapon, ref p); break;
                        case ConsoleKey.RightArrow: handleThrowable(1, 0, p.rangedWeapon, ref p); break;
                    }

                    draw();
                    state = State.Default;
                }
                #endregion
            }
        }


        #region div
        // player attacks creature
        void attackCreature(ref Tile creature)
        {
            Player p = (Player)tiles[playerPos.X, playerPos.Y];
            Creature c = (Creature)creature;

            int pdmg = 
                ran.Next(0, 1001) <= p.hitLikelyness - c.HitPenalty ? (
                    p.meleeWeapon == null ? p.damage.X : p.damage.X + ran.Next(p.meleeWeapon.damage.X, p.meleeWeapon.damage.Y + 1)
                )
                : 0
            ;

            msg(string.Format("{0} {1}", Constants.getPDamageInWords(pdmg, ref ran), c.tiletype));

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

            msg(string.Format("{0} {1}", c.tiletype, Constants.getCDamageInWords(cdmg, ref ran)));

            if (p == null) onPlayerDead();
        }

        void processMonsters()
        {
            ((Creature)tiles[playerPos.X, playerPos.Y]).processed = true;

            for (int x = 0; x < tiles.GetLength(0); x++) for (int y = 0; y < tiles.GetLength(1); y++)
                {
                    if (tiles[x, y] is Creature && !((Creature)tiles[x, y]).processed)
                    {
                        Point p = getPointTowardsPlayer(x, y);

                        if (isValidMove(p) && !p.same(x, y))
                        {
                            if (tiles[p.X, p.Y].tiletype == TileType.Player)
                            {
                                attackPlayer(ref tiles[x, y]);
                            }
                            else if (!(tiles[p.X, p.Y] is Creature))
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

        // TODO: Make accesable for enemys too
        void handleThrowable(sbyte x, sbyte y, Throwable t, ref Player p)
        {
            Point curPoint = playerPos;
            for (byte i = 0; i < t.range; i++) // Just saved you 24 bit of RAM!
            {
                curPoint.X += x;
                curPoint.Y += y;

                if (isInScreen(curPoint)) if (tiles[curPoint.X, curPoint.Y] is Creature)
                    {
                        int dmg = ran.Next(t.damage.X, t.damage.Y + 1);

                        //message = string.Format("{0} {1} with the Spear", Constants.getPDamageInWords(dmg), tiles[curPoint.X, curPoint.Y].tiletype);
                        msg(string.Format("{0} {1} with the {2}", Constants.getPDamageInWords(dmg, ref ran), tiles[curPoint.X, curPoint.Y].tiletype, t.ToString()));

                        Creature c = (Creature)tiles[curPoint.X, curPoint.Y];
                        if (c.doDamage(dmg, ref tiles[curPoint.X, curPoint.Y]))
                            onDead(ref p, c);

                        processMonsters();
                        return;
                    }
            }

            msg("I don't see any creature there!");
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

            if (isInRangeOfPlayer(p, c.searchRange))
            {
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

        #region environment
        void giveXp(ref Player p, ushort amnt)
        {
            p.xp += amnt;
            while (p.xp >= p.reqXp)
            {
                p.levelUp();
                msg("you are now level " + p.level + '!');
            }
        }
        #endregion



        #region "constant" messages methods
        public static void msg(string s)
        {
            currentMessage += '\n' + s ;
        }

        string getDefaultMessage()
        {
            return playerPos.ToString();
        }

        void onDead(ref Player p, Creature c)
        {
            msg("you have defeated " + c.tiletype + "!");
            giveXp(ref p, c.getXp(ref ran));
        }

        void onPlayerDead()
        {
            // rest in rip player
            msg("GAME OVER: R.I.P. " + ((Player)tiles[playerPos.X, playerPos.Y]).name + '!');
            draw();
            Console.ReadKey();

            Player p = (Player)tiles[playerPos.X, playerPos.Y];

            // display and update highscores
            Highscores.Load();
            Highscores.Add(p.name, (int)-currentFloor, (int)p.level, (int)p.money);
            Highscores.Display();
            Console.ReadKey();

            Environment.Exit(0);
        }
        #endregion
    }
}
