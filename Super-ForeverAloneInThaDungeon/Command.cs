using System;

namespace Super_ForeverAloneInThaDungeon
{
    partial class Game
    {
        void ReadCommand()
        {
            Console.CursorLeft = 2;
            string cmd = "";

            // read command
            while (true)
            {
                ConsoleKeyInfo info = Console.ReadKey();
                if (info.Key == ConsoleKey.Enter)
                {
                    break;
                }
                cmd += info.KeyChar;
            }
        }

        // TODO: Finish
    }
}
