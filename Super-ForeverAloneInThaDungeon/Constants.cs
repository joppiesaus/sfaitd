using System;
using System.Text;

namespace Super_ForeverAloneInThaDungeon
{
    static class Constants
    {
        public const byte playerLookRadius = 6;//15;

        public const byte invCapacity = 50;
        public const byte invDescriptionWidth = 0x20;

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
            yWallToXWallBothSides = _g(202);
            yWallWithRightXWall = _g(204);
            yWallWithLeftXWall = _g(185);

            chars = new char[] { _g(166), // scrollChar
                _g(30),                   // startChar
                _g(31),                   // exitChar
                _g(147),                  // goblinChar
                _g(16),                   // >ish char
                _g(220),                  // chestChar
            };
        }

        public const ushort SFLAG_NEEDS_CHECK = ushort.MaxValue - 1; // I'm pretty sure that the score wont be that long

        public static readonly string EXE_DIR = AppDomain.CurrentDomain.BaseDirectory;
        public const string HIGHSCORES_FILE_NAME = "SuperForeverAloneInThaDungeon.com";


        public const ConsoleColor invItemBorderColor = ConsoleColor.DarkGreen;
        public const ConsoleColor invSelItemBorderColor = ConsoleColor.Yellow;

        public static readonly Encoding enc = Encoding.GetEncoding(437);

        public readonly static char[] chars; // used to display special characters, like a scroll.
        public readonly static char yWall, xWall, lupWall, ldownWall, rupWall, rdownWall, lSplitWall, rSplitWall, yWallToXWallBothSides, yWallWithRightXWall, yWallWithLeftXWall;

        public static readonly string[] dungeonNameParts = new string[] { "Prisons", "Dungeon", "Tunnels", "Chamber", "Tombs", "Cells", "Lair", "Cave", "Catacombs", "Caverns" };
        public readonly static string[] namefwords = new string[] { "Witty", "Ancient", "Evil", "Heartless", "Furious", "Screaming", "Mighty", "Infamous", "Loved", "Isolated", "Withered", "Magic", "Undead", "Mystic", "Dark", "Black", "Creepy" };
        public readonly static string[] nameswords = new string[] { "Orc", "Elf", "Prisoner", "Dragon", "Angel", "Demon", "Giant", "Ghost", "Priest", "Cobra", "Warrior", "Phantom", "Hunter", "Phoenix", "Widow", "Mage", "Warlock" };
        public readonly static string[] namewords = new string[] { "Blindness", "Resurrection", "Power", "Acceptance", "Tears", "Undead", "Lies", "Fear", "Disbelieve", "Anger", "Mistakes", "Judgement" };

        // "Macro"
        static char _g(byte val)
        {
            return enc.GetChars(new byte[] { val })[0];
        }

        // random language characters
        public readonly static char[] rlangChars = new char[] { 'a', 'o', 'e', 'u', 'j', 'k', 'n', 'q', 'p', 'w',
            _g(230), _g(208), _g(244), _g(255), _g(232), _g(209), _g(190), _g(207)  };

        /*public readonly static InventoryItem[] invItems = {
            new InventoryItem("Empty", "Empty", new char[,] {{'\\',' ',' ',' ',' ',' ','/'},{' ','\\',' ',' ',' ','/',' '},{' ',' ','\\',' ','/',' ',' '},{' ',' ',' ','X',' ',' ',' '},
	        { ' ',' ','/',' ','\\',' ',' ' },{ ' ','/',' ',' ',' ','\\',' ' },{ '/',' ',' ',' ',' ',' ','\\'}}, ConsoleColor.DarkRed),};*/


        public static readonly char[,] emptyScrollImage = new char[,] { {' ','_','_','_','_','_','_','_','_','_','_',' ',' ',' ',' ',' ' },
            { '(',')','_','_','_','_','_','_','_','_','_',')',' ',' ',' ',' ' },{ ' ','\\',' ',' ',' ',' ',' ',' ',' ',' ',' ',' ','\\',' ',' ',' ' },
	        { ' ',' ','\\',' ',' ',' ',' ',' ',' ',' ',' ',' ',' ','\\',' ',' ' },{ ' ',' ',' ','\\','_','_','_','_','_','_','_','_','_','_','\\',' ' },
	        { ' ',' ',' ','(',')','_','_','_','_','_','_','_','_','_','_',')'} };

        // ranged weapons
        public static readonly WeaponItem spear = new WeaponItem(new Spear(),
            "The spear is a weapon with short range you can throw to creatures. It's very effective against big creatures!",
            new char[,] { {' ',' ',' ',' ',' ','/','|' },{ ' ',' ',' ',' ','/',' ','|' },
            { ' ',' ',' ',' ','/','/',' ' },{ ' ',' ',' ','/','/',' ',' ' },{ ' ',' ','/','/',' ',' ',' ' },{ ' ','/','/',' ',' ',' ',' '} });

        // melee weapons
        public static readonly WeaponItem dagger = new WeaponItem(new Dagger(),
            "A simple knife!", new char[,] { {' ','_','_','_','_','_','.','_','_',' ' }, { ' ','`','-','-','-','-',';','=','=','\'' } });

        public static readonly WeaponItem sword = new WeaponItem(new Sword(), "A sword", new char[,] { {' ',' ','/','\\',' ',' ' },{ ' ',' ','|','|',' ',' ' },{ ' ',' ','|','|',' ',' ' },
        { ' ',' ','|','|',' ',' ' },{ ' ',' ','|','|',' ',' ' },{ ' ',' ','|','|',' ',' ' },{ ' ',' ','|','|',' ',' ' },{ 'o','=','=','=','=','o' },{ ' ',' ',')','(',' ',' ' },
        { ' ',' ','(',')',' ',' '} }, ConsoleColor.White);

        // items
        public static readonly ItemInventoryItem swedishMatches = new ItemInventoryItem(new SwedishMatches(), "Swedish matches manufactured at Uddevalla to set everything on fire with!", 
            new char[,] { {'(',')','=','=','=','=','=','=','=','=','='} }, ConsoleColor.Red);


        public const string invFullMsg = "Inventory full!";

        #region methods
        public static Point GetDirectionByKey(ConsoleKey key)
        {
            switch (key)
            {
                case ConsoleKey.UpArrow: return new Point(0, -1);
                case ConsoleKey.DownArrow: return new Point(0, 1);
                case ConsoleKey.LeftArrow: return new Point(-1, 0);
                case ConsoleKey.RightArrow: return new Point(1, 0);
            }
            return new Point(0, 0);
        }

        public static bool[,] GenerateCircle(int radius)
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

        public static string GetCreatureDamageInWords(int dmg)
        {
            if (dmg < 0) return "healed " + dmg + " to";
            else if (dmg == 0)
            {
                switch (Game.ran.Next(0, 4))
                {
                    default: return "missed";
                    case 1: return "barely missed";
                    case 2: return "completely missed";
                    case 3: return "didn't hit";
                }
            }
            else if (dmg < 6) return Game.ran.Next(0, 2) == 1 ? "did a simple hit on" : "did a minor hit on";
            else return Game.ran.Next(0, 2) == 1 ? "did a great hit on" : "injured";
        }

        public static string GetCreatureBlockMessage()
        {
            switch (Game.ran.Next(0, 2))
            {
                default:
                    return "blocked";
                case 1:
                    return "blocked brutely";
            }
        }

        // makes a string break into pieces
        public static string[] GenerateReadableString(string input, int lineLength)
        {
            string[] sInput = input.Split(' ');
            string[] lines = new string[sInput.Length/*input.Length / lineLength + 4*/];

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
                            lines[a] = currentLine;
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
                    lines[a] = currentLine;
                    a++;
                    currentLine = "";
                    i--;
                }
            }

            if (currentLine != "")
            {
                lines[a] = currentLine;
                a++;
            }

            Array.Resize(ref lines, a);
            return lines;
        }

        // converts 340 to 34.0%
        public static string ToPercent(short val)
        {
            string s = val.ToString();
            return s.Insert(s.Length - 1, ".") + '%';
        }

        // ???
        // I don't know what format yet
        public static string ToMysteriousNumeralSystem(uint val)
        {
            return val.ToString();
        }

        public static string GenerateRandomName()
        {
            return Game.ran.Next(0, 3) == 0 ?
                namefwords[Game.ran.Next(namefwords.Length)] + " " + nameswords[Game.ran.Next(nameswords.Length)] :
                namewords[Game.ran.Next(namewords.Length)];
        }

        public static char[,] GenerateRandomScrollImage()
        {
            char[,] img = emptyScrollImage;

            // Write random text in the scroll's image
            // THE NUMBERS MASON
            // WHAT DO THEY MEAN
            byte xOff = 3;
            for (byte y = 2; y < 4; y++)
            {
                int length = xOff + Game.ran.Next(2, 9);
                for (int i = xOff; i < length; i++)
                    img[y, i] = Constants.rlangChars[Game.ran.Next(0, Constants.rlangChars.Length)];

                if (length < xOff + 5)
                {
                    int i = length + 1;
                    length += Game.ran.Next(2, 5);
                    for (; i < length; i++)
                        img[y, i] = Constants.rlangChars[Game.ran.Next(0, Constants.rlangChars.Length)];
                }
                xOff++;
            }

            return img;
        }

        /*public static void AddElement(ref ushort elements, CreatureElement e)
        {
            elements |= (ushort)e;
        }
        public static void RemoveElement(ref ushort elements, CreatureElement e)
        {
            elements ^= (ushort)e;
        }
        public static bool CheckElement(ushort elements, CreatureElement e)
        {
            return ((elements & (ushort)e) == (ushort)e);
        }*/
        #endregion
    }
}
