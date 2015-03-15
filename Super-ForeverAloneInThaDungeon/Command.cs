using System;

namespace Super_ForeverAloneInThaDungeon
{
    partial class Game
    {
        // Maybe another command system in the future?
        void ReadCommand()
        {
            PopupWindowEnterText pWnd = new PopupWindowEnterText("Enter command", 60);
            string cmdText = pWnd.Act();

            DisplayItem clearItem = pWnd.item; // What dimensions should be redrawn?
            

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
                    Game.Message("Redrawed succesfully");
                    redrawNoMatterWhatState();
                    return;
                
                case "HIGHSCORES":
                    if (!Highscores.HasLoaded) Highscores.Load();
                    Highscores.Display();
                    redrawNoMatterWhatState();
                    return;

                case "ME":
                    if (args.Length > 0)
                    {
                        switch (args[0].ToUpper())
                        {
                            case "RENAME":
                            case "NAME":
                                if (args.Length == 1)
                                {
                                    PopupWindowEnterText newNameWnd = new PopupWindowEnterText(
                                        "To what do you want to change your name?(leave blank to not change)", 50, ConsoleColor.Red);
                                    clearItem.Add(newNameWnd.item);

                                    string name = newNameWnd.Act();

                                    if (!string.IsNullOrWhiteSpace(name))
                                    {
                                        ((Player)tiles[playerPos.X, playerPos.Y]).name = name;
                                        Message("Name changed to " + name);
                                    }
                                }
                                else
                                {
                                    if (!string.IsNullOrEmpty(args[1]))
                                    {
                                        ((Player)tiles[playerPos.X, playerPos.Y]).name = args[1];
                                        Message("Name changed to " + args[1]);
                                    }
                                }
                                break;
                        }
                    }
                    break;
                

                case "THIS":
                    if (args.Length > 0)
                    {
                        switch (args[0].ToUpper())
                        {
                            case "WEBSITE":
                                Game.Message("function1.nl/p/sfaitd/");
                                break;
                            case "GIT":
                                Game.Message("github.com/joppiesaus/sfaitd/");
                                break;
                            case "PATH":
                                Game.Message(Constants.EXE_DIR);
                                break;
                            case "VERSION":
                                Game.Message("Version " + System.Reflection.Assembly.GetExecutingAssembly().GetName().Version);
                                break;
                            case "CREDITS":
                                Game.Message("nope");
                                break;
                        }
                    }
                    break;

                default :
                    Message("Unknown command");
                    break;

                // TEMPORARY HACK!
                /*case "KILL":
                    Tile player = tiles[playerPos.X, playerPos.Y];

                    for (int x = 0; x < tiles.GetLength(0); x++)
                        for (int y = 0; y < tiles.GetLength(1); y++)
                            if (tiles[x, y] is Creature) tiles[x, y] = ((Creature)tiles[x, y]).lastTile;

                    tiles[playerPos.X, playerPos.Y] = player;
                    break;*/
            }


            switch (state)
            {
                default:
                    wipeDisplayItem(clearItem);
                    draw();
                    break;
                case State.Inventory:
                    inv_wipePopup(clearItem);
                    drawInfoBar();
                    break;
            }
        }

        void redrawNoMatterWhatState()
        {
            switch (state)
            {
                default:
                    reDrawDungeon();
                    break;
                case State.Inventory:
                    drawInventory();
                    break;
            }
        }
    }
}
