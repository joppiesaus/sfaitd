using System;

namespace Super_ForeverAloneInThaDungeon
{
    partial class Game
    {
        // Maybe another command system in the future?
        void ReadCommand()
        {
            PopupWindowEnterText pWnd = new PopupWindowEnterText("Enter command", 70);
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
            
            // How do I improve this so I can make help stuff quicker?
            switch (command)
            {
                case "EXIT":
                    Environment.Exit(0);
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

                case "UNYIELD":
                    if (args.Length > 0)
                    {
                        Player p = ((Player)tiles[playerPos.X, playerPos.Y]);
                        WeaponItem i = null;
                        bool did = false;

                        switch (args[0].ToLower())
                        {
                            case "melee":
                                i = p.mWeaponItem;
                                did = true;
                                break;
                            case "ranged":
                                i = p.rWeaponItem;
                                did = true;
                                break;
                        }

                        if (i != null)
                        {
                            p.AddInventoryItem(i);
                            Game.Message("You unyielded " + i.weapon.name);
                            i = null;
                        }
                        else if (did)
                        {
                            Game.Message("You don't have a " + args[0] + " weapon!");
                        }
                        else
                        {
                            Game.Message("Specify which type weapon you want to unyield(e.g. unyield melee)");
                        }
                    }
                    break;

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
                                        ((Player)tiles[playerPos.X, playerPos.Y]).Rename(name);
                                        Message("Name changed to " + name);
                                    }
                                }
                                else
                                {
                                    if (!string.IsNullOrEmpty(args[1]))
                                    {
                                        ((Player)tiles[playerPos.X, playerPos.Y]).Rename(args[1]);
                                        Message("Name changed to " + args[1]);
                                    }
                                }
                                break;
                            case "SUICIDE":
                                PopupWindowYesNo sureWnd = new PopupWindowYesNo("Are you sure?", 30, ConsoleColor.Cyan, ConsoleColor.Gray);
                                if (sureWnd.Act()) gameOver();
                                clearItem.Add(sureWnd.item);
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
