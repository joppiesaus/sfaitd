using System;

namespace Super_ForeverAloneInThaDungeon
{
    class PopupWindow
    {
        public const ConsoleColor DEFAULT_BORDERCOLOR = ConsoleColor.Yellow;
        public const ConsoleColor DEFAULT_TEXTCOLOR = ConsoleColor.White;
        public const short DEFAULT_WIDTH = 40;

        public DisplayItem item;

        public PopupWindow(string text, short width = DEFAULT_WIDTH, ConsoleColor borderColor = DEFAULT_BORDERCOLOR, ConsoleColor textColor = DEFAULT_TEXTCOLOR, byte addLines = 0)
        {
            string[] lines = Constants.GenerateReadableString(text, width - 4);
            this.item = new DisplayItem(new Point(), width, (short)(lines.Length + ++addLines + 3));

            item.CenterScreen();
            Game.MakeBlackSpace(item);
            

            // construct bars
            char[] bar = new char[width];
            for (short i = 0; i < width; i++)
                bar[i] = Constants.xWall;
            bar[0] = Constants.lupWall;
            bar[--width] = Constants.rupWall;

            // draw upper bar
            Console.SetCursorPosition(item.pos.X, item.pos.Y);
            Console.ForegroundColor = borderColor;
            Console.Write(bar);

            Console.CursorLeft = item.pos.X;
            Console.CursorTop++;

            // extra line
            Console.Write(Constants.yWall);
            Console.CursorLeft = item.EndX - 1;
            Console.Write(Constants.yWall);
            Console.CursorLeft = item.pos.X;
            Console.CursorTop++;

            // draw lines
            for (byte i = 0; i < lines.Length; i++)
            {
                Console.Write(Constants.yWall);
                Console.CursorLeft++;

                Console.ForegroundColor = textColor;
                Console.Write(lines[i]);

                Console.ForegroundColor = borderColor;

                Console.CursorLeft = item.EndX - 1;
                Console.Write(Constants.yWall);
                Console.CursorTop++;
                Console.CursorLeft = item.pos.X;
            }

            // draw additional empty lines
            for (byte i = 0; i < addLines; i++)
            {
                Console.Write(Constants.yWall);
                Console.CursorLeft = item.EndX - 1;
                Console.Write(Constants.yWall);
                Console.CursorLeft = item.pos.X;
                Console.CursorTop++;
            }

            // draw lower bar
            bar[0] = Constants.ldownWall;
            bar[width] = Constants.rdownWall;
            Console.Write(bar);
        }
    }

    class PopupWindowEnterText : PopupWindow
    {
        public PopupWindowEnterText(string enterMessage, short width = DEFAULT_WIDTH, 
            ConsoleColor borderColor = DEFAULT_BORDERCOLOR, ConsoleColor textColor = DEFAULT_TEXTCOLOR)
            : base(enterMessage, width, borderColor, textColor, 2)
        {
            // Empty
        }

        public string Act(ConsoleColor color = ConsoleColor.Green)
        {
            Console.CursorTop = item.EndY - 3;
            Console.CursorLeft = item.pos.X + 2;
            Console.ForegroundColor = color;
            return Console.ReadLine();
        }
    }

    class PopupWindowMessage : PopupWindow
    {
        public PopupWindowMessage(string message, short width = DEFAULT_WIDTH, ConsoleColor borderColor = DEFAULT_BORDERCOLOR, ConsoleColor textColor = DEFAULT_TEXTCOLOR)
            : base(message, width, borderColor, textColor)
        {
            // Empty
        }

        public void Act()
        {
            Console.CursorTop = Console.BufferHeight - 1;
            Console.CursorLeft = 0;
            Console.ReadKey();
        }
    }

    class PopupWindowYesNo : PopupWindow
    {
        public PopupWindowYesNo(string message, short width = DEFAULT_WIDTH, ConsoleColor borderColor = DEFAULT_BORDERCOLOR, ConsoleColor textColor = DEFAULT_TEXTCOLOR)
            : base(message, width, borderColor, textColor, 3)
        {
            Console.CursorTop = item.EndY - 4;
            Console.CursorLeft = item.pos.X + 2;
            Console.ForegroundColor = ConsoleColor.Gray;
            Console.Write('(');
            Console.ForegroundColor = ConsoleColor.Green;
            Console.Write('y');
            Console.ForegroundColor = ConsoleColor.Gray;
            Console.Write(")es/(");
            Console.ForegroundColor = ConsoleColor.Red;
            Console.Write('n');
            Console.ForegroundColor = ConsoleColor.Gray;
            Console.Write(")o?");
        }

        public bool Act()
        {
            Console.ForegroundColor = ConsoleColor.Gray;
            Console.CursorTop = item.EndY - 3;
            Console.CursorLeft = item.pos.X + 2;

            while (true)
            {
                ConsoleKeyInfo keyInfo = Console.ReadKey();
                if (keyInfo.Key == ConsoleKey.Escape) return false;

                switch (keyInfo.KeyChar)
                {
                    case 'y':
                    case 'Y':
                        return true;
                    case 'n':
                    case 'N':
                        return false;
                }
            }
        }
    }
}
