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
        private static BlockingCollection<Symbol> _queue = new BlockingCollection<Symbol>();

        public static Size Size { get {  return new Size(Console.WindowWidth, Console.WindowHeight); } }

        static Screen()
        {
            var thread = new Thread(Run) { IsBackground = true };
            thread.Start();
        }

        private static void Run()
        {
            while (true)
            {
                var symbol = _queue.Take();
                Console.ForegroundColor = symbol.ForegroundColor;
                Console.BackgroundColor = symbol.BackgroundColor;
                Console.SetCursorPosition(symbol.Position.X, symbol.Position.Y);
                Console.Write(symbol.Value);    
            }
        }

        public static void Write(Symbol symbol)
        {
            _queue.Add(symbol);
        }

        public static void Write(IEnumerable<char> line, ConsoleColor foreground, ConsoleColor background, int position)
        {
            Write(line, foreground, background, new Point(0, position));
        }

        public static void Write(IEnumerable<char> line, ConsoleColor foreground, ConsoleColor background, Point position)
        {
            for (int i = 0; i < line.Count(); i++)
            {
                _queue.Add(new Symbol(line.ElementAt(i), foreground, background, position.AddX(i)));
            }
        }

        public static void Clear()
        {
            _queue.GetConsumingEnumerable(new CancellationToken(true));
            Console.ForegroundColor = Console.BackgroundColor = ConsoleColor.Black;
            Console.Clear();
        }
    }
}
