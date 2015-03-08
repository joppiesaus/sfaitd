using System;

namespace Super_ForeverAloneInThaDungeon
{
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
        bool drawOnceAfterInit = false; // That'content for the blue bar-thinggy underneath.

        uint currentFloor = 0;

        // Environment
        Room[] rooms;
        Tile[,] map; // Ode to the mighty comma!
        ushort[,] scores; // For enemys searching the player

        private Random _random;
        private State _state = State.Default;

        private Point _playerPosition = new Point(-1, -1);
        // TODO : remove static
        private static string _informationMessages = string.Empty;
        private readonly Drawer _drawer = new Drawer();

        

        /// <summary>
        /// Gets or sets your mother'content car windscreenwiper.
        /// </summary>
        /// <param name="size">Default: 120, 50. Product of size can never be larger than 65536, because that can lead to breadth-first search problems</param>
        public Game(Point size)
        {
            //Console.CursorVisible = false;
            Console.OutputEncoding = Constants.enc;
            Console.Title = "SuperForeverAloneInThaDungeon";
            Console.SetWindowSize(size.X, size.Y + 3);
            Console.BufferHeight = Console.WindowHeight;

            map = new Tile[size.X, size.Y]; // !UNSAFE!
            scores = new ushort[size.X, size.Y]; // !ALSO UNSAFE!

            // inventory stuff
            invDItems = new DisplayItem[Constants.invCapacity];

            _random = new Random();
        }


        // TODO: Make effecienter
        public void Run()
        {
            while (true)
            {
                Player p;
                if (_playerPosition.Same(-1, -1)) p = new Player();
                else p = (Player)map[_playerPosition.X, _playerPosition.Y];

                setDungeonToEmpty();

                // tweak this
                rooms = createDungeons(15, new Room(new Point(4, 19), new Point(5, 15))); // Counted from 0 *trollface*

                spawnPlayerInRoom(rooms[_random.Next(0, rooms.Length)], p);

                OnPlayerMove(ref p); // make sure everything inits properly

                Message("Welcome, " + p.name + "!"); // I could have just used Environment.UserName since the Player.name = Environment.UserName... :~)
                if (disableFight) p.walkable = false;


                currentFloor++;
                _drawer.ReDrawDungeon(map, _playerPosition, currentFloor, getDungeonAt(_playerPosition),
                string.IsNullOrEmpty(_informationMessages) ? GetDefaultInformationMessage() : _informationMessages);

                GameLoop();
            }
        }

        void GameLoop()
        {
            while (true)
            {
                #region default _state
                if (_state == State.Default)
                {
                    ConsoleKey key = Console.ReadKey().Key;

                    Point toAdd = new Point();
                    bool doNotCallDraw = false;
                    switch (key)
                    {
                        case ConsoleKey.OemPeriod: ReadCommand(); doNotCallDraw = true; break;
                        case ConsoleKey.Escape: Environment.Exit(0); break;
                        case ConsoleKey.R: return;
                        case ConsoleKey.LeftArrow: toAdd.X--; break;
                        case ConsoleKey.RightArrow: toAdd.X++; break;
                        case ConsoleKey.UpArrow: toAdd.Y--; break;
                        case ConsoleKey.DownArrow: toAdd.Y++; break;
                        case ConsoleKey.Tab: _state = State.Inventory; drawInventory(); continue;
                        case ConsoleKey.F1: hack = !hack; break; //                                                 HACK TOGGLE HACK TOGGLE HACK
                        case ConsoleKey.F2: hackLighting = true;
                            for (int x = 0; x < map.GetLength(0); x++)
                                for (int y = 0; y < map.GetLength(1); y++)
                                    if (map[x, y].tiletype != TileType.None)
                                    {
                                        map[x, y].discovered = true;
                                        map[x, y].lighten = true;
                                        map[x, y].needsToBeDrawn = true;
                                    } break; ////////////////////////////////
                        default:
                            if (key == ConsoleKey.S && ((Player)map[_playerPosition.X, _playerPosition.Y]).rangedWeapon != null)
                            {
                                _state = State.Throwing;
                                continue;
                            }
                            doNotCallDraw = true;
                            break;
                    }

                    if (toAdd.X != 0 || toAdd.Y != 0)
                    {
                        Point toCheck = new Point(_playerPosition.X + toAdd.X, _playerPosition.Y + toAdd.Y);
                        if (isValidMove(toCheck) || hack)
                        {
                            Point old = _playerPosition;
                            Tile preCopy = map[toCheck.X, toCheck.Y]; // Tile where the player will move to
                            preCopy.needsToBeDrawn = true;
                            map[old.X, old.Y].needsToBeDrawn = true; //  Tile that will appear(old tile under player)

                            Player p = (Player)map[old.X, old.Y];

                            bool abort = false;

                            if (preCopy is Creature)
                            {
                                attackCreature(ref map[toCheck.X, toCheck.Y]);
                                abort = true;
                            }
                            else if (preCopy.tiletype == TileType.Money)
                            {
                                // I knew I'd know the English grammar!
                                int money = ((Money)preCopy).money;
                                string s = money != 1 ? "content" : "";
                                p.money += money;
                                Message("You found " + money + " coin" + s + '!');
                                preCopy = new Tile(((Pickupable)preCopy).replaceTile);
                            }
                            // TODO: Make available for other items
                            else if (preCopy is Pickupable)
                            {
                                // assuming all other pickupables are handled!
                                if (p.addInventoryItem(((Pickupable)preCopy).getInvItem(ref _random)))
                                {
                                    Message("You found " + p.lastInventoryItem().name);
                                    preCopy = new Tile(((Pickupable)preCopy).replaceTile);
                                }
                                else Message(Constants.invFullMsg);
                            }
                            else if (preCopy is Chest)
                            {
                                // Don't allow to walk on chest
                                abort = true;

                                Chest c = (Chest)preCopy;

                                int count = c.contents.Length;

                                Message("You explore the chest");
                                if (count > 0)
                                    for (int i = 0; i < c.contents.Length; i++)
                                    {
                                        if (p.addInventoryItem(c.contents[i]))
                                        {
                                            Message("You found " + c.contents[i].name);
                                            count--;
                                        }
                                        else
                                        {
                                            Message("There'content more, but you can't carry more");
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
                            else if (preCopy.tiletype == TileType.Down) return;


                            if (!abort)
                            {
                                map[toCheck.X, toCheck.Y] = map[old.X, old.Y];
                                Creature c = (Creature)map[toCheck.X, toCheck.Y];
                                map[old.X, old.Y] = c.lastTile;

                                c.lastTile = preCopy;
                                _playerPosition = toCheck;

                                Player plyr = (Player)map[toCheck.X, toCheck.Y];
                                OnPlayerMove(ref plyr);
                            }
                        }
                    }

                    if (!doNotCallDraw)
                    {
                        processMonsters();
                        _drawer.Draw(map, false, _playerPosition, currentFloor, getDungeonAt(_playerPosition),
                string.IsNullOrEmpty(_informationMessages) ? GetDefaultInformationMessage() : _informationMessages);
                        _informationMessages = string.Empty;
                    }
                }
                #endregion
                #region inventory _state
                else if (_state == State.Inventory)
                {
                    //drawInventory();
                    ConsoleKey key = Console.ReadKey().Key;

                    if (((Player)map[_playerPosition.X, _playerPosition.Y]).nInvItems > 0)
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
                                    invActionSel = ((Player)map[_playerPosition.X, _playerPosition.Y]).inventory[invSelItem].actions.Length - 1;
                                inv_drawDescription();
                                break;
                            case ConsoleKey.DownArrow:
                                if (++invActionSel >= ((Player)map[_playerPosition.X, _playerPosition.Y]).inventory[invSelItem].actions.Length)
                                    invActionSel = 0;
                                inv_drawDescription();
                                break;
                            case ConsoleKey.Enter:
                                doSelectedInventoryAction();
                                drawInventory();
                                break;

                            case ConsoleKey.Tab: 
                                _state = State.Default;
                                _drawer.ReDrawDungeon(map, _playerPosition, currentFloor, getDungeonAt(_playerPosition),
                string.IsNullOrEmpty(_informationMessages) ? GetDefaultInformationMessage() : _informationMessages); 
                                continue;

                            case ConsoleKey.Escape: 
                                Environment.Exit(0); 
                                break;
                        }
                    else
                        switch (key)
                        {
                            case ConsoleKey.Tab: 
                                _state = State.Default;
                                _drawer.ReDrawDungeon(map, _playerPosition, currentFloor, getDungeonAt(_playerPosition),
                string.IsNullOrEmpty(_informationMessages) ? GetDefaultInformationMessage() : _informationMessages); 
                                continue;
                            case ConsoleKey.Escape: Environment.Exit(0); break;
                        }
                }
                #endregion
                #region Throwing _state
                else if (_state == State.Throwing)
                {
                    Message("Which direction?");
                    _drawer.Draw(map, false, _playerPosition, currentFloor, getDungeonAt(_playerPosition),
                string.IsNullOrEmpty(_informationMessages) ? GetDefaultInformationMessage() : _informationMessages);
                    _informationMessages = string.Empty;
                    Player p = (Player)map[_playerPosition.X, _playerPosition.Y];
                    Throwable t = p.rangedWeapon;

                    switch (Console.ReadKey().Key)
                    {
                        // Think backwards here: If you need to go UP in an ARRAY, what do you need to do?
                        case ConsoleKey.UpArrow: handleThrowable(0, -1, p.rangedWeapon, ref p); break;
                        case ConsoleKey.DownArrow: handleThrowable(0, 1, p.rangedWeapon, ref p); break;
                        case ConsoleKey.LeftArrow: handleThrowable(-1, 0, p.rangedWeapon, ref p); break;
                        case ConsoleKey.RightArrow: handleThrowable(1, 0, p.rangedWeapon, ref p); break;
                    }

                    _drawer.Draw(map, false, _playerPosition, currentFloor, getDungeonAt(_playerPosition),
                string.IsNullOrEmpty(_informationMessages) ? GetDefaultInformationMessage() : _informationMessages);
                    _informationMessages = string.Empty;
                    _state = State.Default;
                }
                #endregion
            }
        }


        #region div
        // player attacks creature
        void attackCreature(ref Tile creature)
        {
            Player p = (Player)map[_playerPosition.X, _playerPosition.Y];
            Creature c = (Creature)creature;

            int pdmg = 
                _random.Next(0, 1001) <= p.hitLikelyness - c.HitPenalty ? (
                    p.meleeWeapon == null ? p.damage.X : p.damage.X + _random.Next(p.meleeWeapon.damage.X, p.meleeWeapon.damage.Y + 1)
                )
                : 0
            ;

            Message(string.Format("{0} {1}", Constants.getPDamageInWords(pdmg, ref _random), c.tiletype));

            if (c.doDamage(pdmg, ref creature))
            {
                OnPlayerDeath(ref p, c);
            }
        }

        void attackPlayer(ref Tile creature)
        {
            Creature c = (Creature)creature;
            Player p = (Player)map[_playerPosition.X, _playerPosition.Y];

            int cdmg = c.hit(ref _random, ref p);

            Message(string.Format("{0} {1}", c.tiletype, Constants.getCDamageInWords(cdmg, ref _random)));

            if (p == null) OnPlayerDeath();
        }

        void processMonsters()
        {
            ((Creature)map[_playerPosition.X, _playerPosition.Y]).processed = true;

            for (int x = 0; x < map.GetLength(0); x++) for (int y = 0; y < map.GetLength(1); y++)
                {
                    if (map[x, y] is Creature && !((Creature)map[x, y]).processed)
                    {
                        Point p = getPointTowardsPlayer(x, y);

                        if (isValidMove(p) && !p.Same(x, y))
                        {
                            if (map[p.X, p.Y].tiletype == TileType.Player)
                            {
                                attackPlayer(ref map[x, y]);
                            }
                            else if (!(map[p.X, p.Y] is Creature))
                            {
                                Tile preCopy = map[p.X, p.Y]; // target tile

                                map[p.X, p.Y] = map[x, y];
                                Creature c = (Creature)map[p.X, p.Y];
                                if (preCopy.lighten) c.needsToBeDrawn = true;
                                c.notLightenChar = preCopy.notLightenChar;
                                c.processed = true;

                                map[x, y] = c.lastTile;
                                if (preCopy.lighten) map[x, y].needsToBeDrawn = true;
                                c.onTileEncounter(ref preCopy);
                                c.lastTile = preCopy;
                            }
                        }
                    }
                }

            // Yes, I know, but come on! It'content a turn-based console game! This piece of code won't be noticed in performance!
            for (int x = 0; x < map.GetLength(0); x++) for (int y = 0; y < map.GetLength(1); y++)
                {
                    if (map[x, y] is Creature) 
                        ((Creature)map[x, y]).processed = false;
                }
        }

        // TODO: Make accesable for enemys too
        void handleThrowable(sbyte x, sbyte y, Throwable t, ref Player p)
        {
            Point curPoint = _playerPosition;
            for (byte i = 0; i < t.range; i++) // Just saved you 24 bit of RAM!
            {
                curPoint.X += x;
                curPoint.Y += y;

                if (isInScreen(curPoint)) if (map[curPoint.X, curPoint.Y] is Creature)
                    {
                        int dmg = _random.Next(t.damage.X, t.damage.Y + 1);

                        //message = string.Format("{0} {1} with the Spear", Constants.getPDamageInWords(dmg), map[curPoint.X, curPoint.Y].tiletype);
                        Message(string.Format("{0} {1} with the {2}", Constants.getPDamageInWords(dmg, ref _random), map[curPoint.X, curPoint.Y].tiletype, t.ToString()));

                        Creature c = (Creature)map[curPoint.X, curPoint.Y];
                        if (c.doDamage(dmg, ref map[curPoint.X, curPoint.Y]))
                            OnPlayerDeath(ref p, c);

                        processMonsters();
                        return;
                    }
            }

            Message("I don't see any creature there!");
        }

        private void GenerateScoreGrid(Point p)
        {
            int count = 0; // this value will never be reached I think. It'content mathematical worst-case, which is practically impossible in this scenario.
            int index = 0;

            // mark the grid which positions should be checked
            for (short x = 0; x < map.GetLength(0); x++)
                for (short y = 0; y < map.GetLength(1); y++)
                {
                    if (map[x, y].walkable)
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

                if (cTile.x + 1 < map.GetLength(0) && scores[cTile.x + 1, cTile.y] == Constants.SFLAG_NEEDS_CHECK)
                {
                    scores[cTile.x + 1, cTile.y] = nscore;
                    list[++index] = new BFSN(cTile.x + 1, cTile.y, nscore);
                }
                if (cTile.x > 0 && scores[cTile.x - 1, cTile.y] == Constants.SFLAG_NEEDS_CHECK)
                {
                    scores[cTile.x - 1, cTile.y] = nscore;
                    list[++index] = new BFSN(cTile.x - 1, cTile.y, nscore);
                }
                if (cTile.y + 1 < map.GetLength(1) && scores[cTile.x, cTile.y + 1] == Constants.SFLAG_NEEDS_CHECK)
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

        private void OnPlayerMove(ref Player p)
        {
            p.onMove();

            // lighting stuff
            if (!hackLighting)
            {
                bool[,] processed = new bool[(Constants.playerLookRadius + 2) * 2 - 1, (Constants.playerLookRadius + 2) * 2 - 1];

                for (int xa = 0; xa < p.circle.GetLength(0); xa++)
                {
                    int xb = _playerPosition.X + xa - Constants.playerLookRadius + 1;
                    for (int ya = 0; ya < p.circle.GetLength(1); ya++)
                    {
                        Point pos = new Point(xb, _playerPosition.Y + ya - Constants.playerLookRadius + 1);
                        if (p.circle[xa, ya] && isInScreen(pos))
                        {
                            if (map[pos.X, pos.Y].tiletype != TileType.None)
                                if (!map[pos.X, pos.Y].discovered)
                                {
                                    map[pos.X, pos.Y].discovered = true;
                                    map[pos.X, pos.Y].lighten = true;
                                    map[pos.X, pos.Y].needsToBeDrawn = true;
                                }
                                else if (!map[pos.X, pos.Y].lighten)
                                {
                                    map[pos.X, pos.Y].lighten = true;
                                    map[pos.X, pos.Y].needsToBeDrawn = true;
                                }
                            processed[xa + 1, ya + 1] = true;
                        }

                    }
                }

                for (int x = 0; x < processed.GetLength(0); x++)
                {
                    int xp = _playerPosition.X + x - Constants.playerLookRadius;
                    for (int y = 0; y < processed.GetLength(1); y++)
                    {
                        if (!processed[x, y])
                        {
                            int yp = _playerPosition.Y + y - Constants.playerLookRadius;
                            if (isInScreen(new Point(xp, yp)) && map[xp, yp].lighten)
                            {
                                map[xp, yp].lighten = false;
                                map[xp, yp].needsToBeDrawn = true;
                            }
                        }
                    }
                }
            }

            GenerateScoreGrid(_playerPosition);
        }


        Point getPointTowardsPlayer(int x, int y)
        {
            Point p = new Point(x, y);
            Creature c = (Creature)map[x, y];

            if (isInRangeOfPlayer(p, c.searchRange))
            {
                Point preferredDir = new Point(_playerPosition.X + (x > _playerPosition.X ? -1 : 1), _playerPosition.Y + (y > _playerPosition.Y ? 1 : -1));

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
                if (map[x + 1, y].walkable)
                    points[count++] = new Point(x + 1, y);
                if (map[x - 1, y].walkable)
                    points[count++] = new Point(x - 1, y);
                if (map[x, y + 1].walkable)
                    points[count++] = new Point(x, y + 1);
                if (map[x, y - 1].walkable)
                    points[count++] = new Point(x, y - 1);
                points[count++] = new Point(x, y); // and do nothing is also a chance

                p = points[_random.Next(0, count)];
            }

            return p;
        }
        #region boolean expressions
        bool isInRangeOfPlayer(Point p, int range)
        {
            Point difference = new Point(_playerPosition.X - p.X, _playerPosition.Y - p.Y);
            return (difference.X >= -range && difference.X <= range && difference.Y >= -range && difference.X <= range);
        }

        bool isValidMove(Point pos)
        {
            return (pos.X >= 0 && pos.Y >= 0 && pos.X < map.GetLength(0) && pos.Y < map.GetLength(1) && map[pos.X, pos.Y].walkable);
        }

        bool isInScreen(Point pos)
        {
            return pos.X >= 0 && pos.X < map.GetLength(0) && pos.Y >= 0 && pos.Y < map.GetLength(1);
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
                Message("you are now level " + p.level + '!');
            }
        }
        #endregion

        #region popups
        void wipeDisplayItem(DisplayItem d)
        {
            for (int y = d.Position.Y; y < d.EndY; y++)
                for (int x = d.Position.X; x < d.EndX; x++)
                    map[x, y].needsToBeDrawn = true;

            char[] buffer = new char[d.Width];
            for (short i = 0; i < d.Width; i++)
            {
                buffer[i] = ' ';
            }

            Console.CursorTop = d.Position.Y;
            Console.CursorLeft = d.Position.X;
            for (int y = d.Position.Y; y < d.EndY; y++)
            {
                Console.CursorLeft = d.Position.X;
                Console.Write(buffer);
                Console.CursorTop++;
            }
        }
        #endregion

        #region "constant" messages methods

        // TODO : remove static 
        public static void Message(string content)
        {
            _informationMessages += '\n' + content ;
        }

        private string GetDefaultInformationMessage()
        {
            return _playerPosition.ToString();
        }

        private void OnPlayerDeath(ref Player p, Creature c)
        {
            Message("you have defeated " + c.tiletype + "!");
            giveXp(ref p, c.getXp(ref _random));
        }

        private void OnPlayerDeath()
        {
            // rest in rip player
            Message("GAME OVER: R.I.P. " + ((Player)map[_playerPosition.X, _playerPosition.Y]).name + '!');
            _drawer.Draw(map, false, _playerPosition, currentFloor, getDungeonAt(_playerPosition),
                string.IsNullOrEmpty(_informationMessages) ? GetDefaultInformationMessage() : _informationMessages);
            _informationMessages = string.Empty;
            Console.ReadKey();

            Player p = (Player)map[_playerPosition.X, _playerPosition.Y];

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
