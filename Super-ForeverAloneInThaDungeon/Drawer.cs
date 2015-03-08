using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Cryptography;
using System.Threading;
using Super_ForeverAloneInThaDungeon.Display;
using Super_ForeverAloneInThaDungeon.Graphics;

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
                    map[x, y].Draw(this, x, y);
                }
            }

            DrawInfoBar((Player)map[playerPosition.X, playerPosition.Y], floor, room, message, map.GetLength(1));
        }

        public void ReDrawDungeon(Tile[,] map, Point playerPosition, uint floor, Room room, string message)
        {
            Console.Clear();
            Console.SetCursorPosition(0, 0);
            for (int x = 0; x < map.GetLength(0); x++)
            {
                for (int y = 0; y < map.GetLength(1); y++)
                {
                    map[x, y].NeedsRefresh = true;
                }
            }

            Draw(map, true, playerPosition, floor, room, message);
        }

        public void DrawMessages(string location, string message, int position)
        {
            string[] msgs = message.Split('\n');

            for (int i = 1; i < msgs.Length - 1; i++)
            {
                _secondeInfoLine.Draw(position, new string[] { msgs[i] + "--Press space--", location });
                // TODO : Handle multiple messages
            }

            _secondeInfoLine.Draw(position, new string[] { msgs[msgs.Length - 1], location });
        }

        public void DrawInfoBar(Player player, uint floor, Room room, string defaultMessage, int infoBarPosition)
        {
            _firstInfoLine.Draw(infoBarPosition, new string[]
            {
                "Health: " + player.health + '(' + player.maxHealth + ')',
                "xp: " + player.xp + '/' + player.reqXp + "(lvl " + player.level + ')',
                player.meleeWeapon == null
                    ? "str: " + player.damage.X
                    : "str: " + player.damage.X + '+' + player.meleeWeapon.damage.X + '-' + player.meleeWeapon.damage.Y,
                "Floor: -" + floor,
                "Money: " + player.money
            });

            string location = "";

            if (player.lastTile.Type != TileType.Air) location = player.lastTile.Type.ToString();
            else
            {
                location = room == null ? "O_o What the hell did you do this game?" : room.name;
            }

            DrawMessages(location, defaultMessage, infoBarPosition + 1);

        }


        public void Draw(char representation, ConsoleColor color, Point position)
        {
            Screen.Write(new Symbol(representation, color, position));
        }

        public void Write(IEnumerable<char> line, ConsoleColor color, Point position)
        {
            Screen.Write(line, color, ConsoleColor.Black, position);
        }

        public void Write(IEnumerable<char> line, ConsoleColor foreground, ConsoleColor background, int position)
        {
            Screen.Write(line, foreground, background, new Point(0, position));
        }

        public void Write(IEnumerable<char> line, ConsoleColor foreground, ConsoleColor background, Point position)
        {
            Screen.Write(line, foreground, background, position);
        }

        public void DrawRectangle(Point origin, Size size, ConsoleColor color)
        {
            for (int lineIndex = 0; lineIndex < size.Height; lineIndex ++)
            {
                Screen.Write(Enumerable.Repeat(' ', size.Width), color, color, origin.AddY(lineIndex));                
            }
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