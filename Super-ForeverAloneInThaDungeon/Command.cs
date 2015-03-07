using System;

namespace Super_ForeverAloneInThaDungeon
{
    partial class Game
    {
        void ReadCommand()
        {
            DisplayItem dispItem = (new PopupWindowEnterText(40, "Enter command")).item;
            Console.ForegroundColor = ConsoleColor.Green;
            string cmdText = Console.ReadLine();

            string[] args = cmdText.Split(' ');
            string command = args[0].ToUpper();

            if (args.Length > 1)
            {
                string[] newArgs = new string[args.Length - 1];
                for (int i = newArgs.Length; i > 0; i--)
                {
                    newArgs[i - 1] = args[i];
                }
                args = newArgs;
            }
            else
            {
                args = new string[0];
            }

            switch (command)
            {
                case "EXIT":
                    Environment.Exit(0);
                    break;

                case "SUICIDE":
                    onPlayerDead();
                    break;

                case "REDRAW":
                case "DRAW":
                    switch (state)
                    {
                        default:
                            reDrawDungeon();
                            break;
                        case State.Inventory:
                            drawInventory();
                            break;
                    }
                    Game.msg("Redrawed succesfully");
                    return;

                case "THIS":
                    if (args.Length > 0)
                    {
                        switch (args[0].ToUpper())
                        {
                            case "WEBSITE":
                                Game.msg("function1.nl/p/sfaitd/");
                                break;
                            case "PATH":
                                Game.msg(Constants.EXE_DIR);
                                break;
                            case "VERSION":
                                Game.msg("Version: " + System.Reflection.Assembly.GetExecutingAssembly().GetName().Version);
                                break;
                            case "CREDITS":
                                Game.msg("nope");
                                break;
                        }
                    }
                    break;
            }

            
            switch (state)
            {
                default:
                    wipeDisplayItem(dispItem);
                    draw();
                    break;
                case State.Inventory:
                    inv_wipePopup(dispItem);
                    break;
            }
        }
    }
}
