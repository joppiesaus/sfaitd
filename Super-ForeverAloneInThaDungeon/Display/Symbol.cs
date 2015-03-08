using System;
using Super_ForeverAloneInThaDungeon.Graphics;

namespace Super_ForeverAloneInThaDungeon.Display
{
    public class Symbol
    {
        public Point Position { get; private set; }
        public char Value { get; private set; }
        public ConsoleColor ForegroundColor { get; private set; }
        public ConsoleColor BackgroundColor { get; private set; }

        public Symbol(char value, ConsoleColor foregroundColor, Point position) :
            this(value, foregroundColor, ConsoleColor.Black, position)
        {
        }

        public Symbol(char value, ConsoleColor foregroundColor, ConsoleColor backgroundColor, Point position)
        {
            Value = value;
            ForegroundColor = foregroundColor;
            BackgroundColor = backgroundColor;
            Position = position;
        }
    }
}
