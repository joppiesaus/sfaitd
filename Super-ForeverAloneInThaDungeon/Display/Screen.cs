using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using Super_ForeverAloneInThaDungeon.Graphics;

namespace Super_ForeverAloneInThaDungeon.Display
{
    public static class Screen
    {
        private static readonly BlockingCollection<Symbol> Queue = new BlockingCollection<Symbol>();

        static Screen()
        {
            var thread = new Thread(Run) { IsBackground = true };
            thread.Start();
        }

        private static void Run()
        {
            while (true)
            {
                var symbol = Queue.Take();
                Console.ForegroundColor = symbol.ForegroundColor;
                Console.BackgroundColor = symbol.BackgroundColor;
                Console.SetCursorPosition(symbol.Position.X, symbol.Position.Y);
                Console.Write(symbol.Value);    
            }
        }

        public static void Write(Symbol symbol)
        {
            Queue.Add(symbol);
        }

        public static void Write(IEnumerable<char> line, ConsoleColor foreground, ConsoleColor background, int position)
        {
            Write(line, foreground, background, new Point(0, position));
        }

        public static void Write(IEnumerable<char> line, ConsoleColor foreground, ConsoleColor background, Point position)
        {
            for (int i = 0; i < line.Count(); i++)
            {
                Queue.Add(new Symbol(line.ElementAt(i), foreground, background, position.AddX(i)));
            }
        }
    }
}
