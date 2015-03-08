using System;

namespace Super_ForeverAloneInThaDungeon
{
    class Program
    {
        static void Main(string[] args)
        {
            Game g;
            while (true)
            {
                try
                {
                    g = new Game(new Point(120, 47));
                    break;
                }
                catch
                {
                    Console.ForegroundColor = ConsoleColor.Red;
                    Console.WriteLine("Error: Console bounds out of range");
                    Console.ForegroundColor = ConsoleColor.Gray;
                    Console.WriteLine("The console window wants to be bigger than your screen.");
                    Console.WriteLine("To fix this problem, right-click the program's icon(left above),");
                    Console.WriteLine("Go to properties, then pick tab \"font\",");
                    Console.WriteLine("and then set the weight to something lower(Rasterfont 8x12 recommended)");
                    Console.WriteLine("If you are in CMD, clearing the screen may fix this too.");
                    Console.WriteLine("Press any key to try again, or escape to quit.");

                    if (Console.ReadKey().Key == ConsoleKey.Escape) return;
                }
            }
            g.Run();
        }
    }
}
