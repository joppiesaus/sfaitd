using System;
using Super_ForeverAloneInThaDungeon.Graphics;

namespace Super_ForeverAloneInThaDungeon
{
    public class PopupWindow
    {
        public DisplayItem item;

        public PopupWindow(short width, string text, ConsoleColor borderColor = ConsoleColor.Yellow, ConsoleColor textColor = ConsoleColor.White, byte addLines = 0)
        {
            string[] lines = Constants.GenerateReadableString(text, width - 4);
            this.item = new DisplayItem(new Point(), width, (short)(lines.Length + ++addLines + 3));

            item.CenterScreen();
            //_drawer.MakeBlackSpace(item);
            

            // construct bars
            char[] bar = new char[width];
            for (short i = 0; i < width; i++)
                bar[i] = Constants.xWall;
            bar[0] = Constants.lupWall;
            bar[--width] = Constants.rupWall;

            // draw upper bar
            Console.SetCursorPosition(item.Origin.X, item.Origin.Y);
            Console.ForegroundColor = borderColor;
            Console.Write(bar);

            Console.CursorLeft = item.Origin.X;
            Console.CursorTop++;

            // extra line
            Console.Write(Constants.yWall);
            Console.CursorLeft = item.EndX - 1;
            Console.Write(Constants.yWall);
            Console.CursorLeft = item.Origin.X;
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
            }

            Console.CursorLeft = item.Origin.X;

            // draw additional empty lines
            for (byte i = 0; i < addLines; i++)
            {
                Console.Write(Constants.yWall);
                Console.CursorLeft = item.EndX - 1;
                Console.Write(Constants.yWall);
                Console.CursorLeft = item.Origin.X;
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
        public PopupWindowEnterText(short width, string enterMessage, ConsoleColor borderColor = ConsoleColor.Yellow, ConsoleColor textColor = ConsoleColor.White)
            : base(width, enterMessage, borderColor, textColor, 2)
        {
            Console.CursorTop -= 2;
            Console.CursorLeft = item.Origin.X + 2;
        }
    }
}
