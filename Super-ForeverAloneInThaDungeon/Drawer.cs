using System;

namespace Super_ForeverAloneInThaDungeon
{
    public class Drawer
    {
        private readonly Game.LineWriter _firstInfoLine = new Game.LineWriter(new ushort[] { 0, 20, 40, 60, 0xfaef });
        private readonly Game.LineWriter _secondeInfoLine = new Game.LineWriter(new ushort[] { 0, 0xdead });


        public void Draw(Tile[,] map, bool drawOnceAfterInit, Point playerPosition, uint floor, Room room, string message)
        {
            for (int x = 0; x < map.GetLength(0); x++)
            {
                for (int y = 0; y < map.GetLength(1); y++)
                {
                    if (map[x, y].needsToBeDrawn)
                    {
                        Console.SetCursorPosition(x, y);
                        map[x, y].Draw();
                        map[x, y].needsToBeDrawn = false;
                    }
                }
            }

            if (drawOnceAfterInit)
            {
                Console.BackgroundColor = ConsoleColor.DarkBlue;
                Console.CursorLeft = 0;
                Console.CursorTop++;

                DrawOnce(Console.WindowWidth*2);
            }

            DrawInfoBar(map, playerPosition, floor, room, message);
        }


        public void ReDrawDungeon(Tile[,] map, Point playerPosition, uint floor, Room room, string message)
        {
            Console.Clear();
            Console.SetCursorPosition(0, 0);
            for (int x = 0; x < map.GetLength(0); x++)
            {
                for (int y = 0; y < map.GetLength(1); y++)
                {
                    map[x, y].needsToBeDrawn = true;
                }
            }

            Draw(map, true, playerPosition, floor, room, message);
        }

        public void DrawMessages(string location, string message)
        {

            string[] msgs = message.Split('\n');

            for (int i = 1; i < msgs.Length - 1; i++)
            {
                _secondeInfoLine.Draw(new string[] {msgs[i] + "--Press space--", location});

                Console.BackgroundColor = ConsoleColor.Black;
                while (true)
                {
                    ConsoleKey key = Console.ReadKey().Key;
                    if (key == ConsoleKey.Spacebar || key == ConsoleKey.Escape) break;
                }
                Console.CursorLeft = 0;
                Console.CursorTop--;
                Console.Write(' ');
                Console.CursorLeft = 0;
                Console.BackgroundColor = ConsoleColor.DarkBlue;
            }

            _secondeInfoLine.Draw(new string[] {msgs[msgs.Length - 1], location});
        }

        public void DrawInfoBar(Tile[,] map, Point playerPosition, uint floor, Room room, string defaultMessage)
        {
            Console.BackgroundColor = ConsoleColor.DarkBlue;
            Console.ForegroundColor = ConsoleColor.White;

            Console.SetCursorPosition(0, map.GetLength(1));

            Player p = (Player)map[playerPosition.X, playerPosition.Y];

            _firstInfoLine.Draw(new string[]
            {
                "Health: " + p.health + '(' + p.maxHealth + ')',
                "xp: " + p.xp + '/' + p.reqXp + "(lvl " + p.level + ')',
                p.meleeWeapon == null
                    ? "str: " + p.damage.X
                    : "str: " + p.damage.X + '+' + p.meleeWeapon.damage.X + '-' + p.meleeWeapon.damage.Y,
                "Floor: -" + floor,
                "Money: " + p.money
            });

            string location = "";

            if (p.lastTile.tiletype != TileType.Air) location = p.lastTile.tiletype.ToString();
            else
            {
                location = room == null ? "O_o What the hell did you do this game?" : room.name;
            }

            DrawMessages(location, defaultMessage);

            Console.BackgroundColor = ConsoleColor.Black;
        }

        public void DrawOnce(int amount)
        {
            Point orgin = new Point(Console.CursorLeft, Console.CursorTop);
            for (int i = 0; i < amount; i++) Console.Write(' ');
            Console.SetCursorPosition(orgin.X, orgin.Y);
        }

        /// <summary>
        /// Writes a message in the center of the screen
        /// </summary>
        /// <param name="s">String you want to display</param>
        public void WriteCenter(string s)
        {
            Console.CursorLeft = Console.BufferWidth/2 - s.Length/2;
            Console.CursorTop = Console.BufferHeight/2 - 2;
            Console.Write(s);
        }
    }
}