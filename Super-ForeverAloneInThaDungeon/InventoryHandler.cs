using System;

namespace Super_ForeverAloneInThaDungeon
{
    // Refractor count: 23
    partial class Game
    {
        int invSelItem;
        int invActionSel;

        bool invDidDescDraw = false;

        Point invPrevPoint = new Point();
        int invLowestY = 0;

        DisplayItem[] invDItems;
        DisplayItem invDescription = new DisplayItem(new Point(), new Point());

        // I feel sooo redundant. It's not that easy you know. I could just have an array of "chars", but drawing one char is wayyy much slower than 10.
        /// <summary>
        /// Draws inventory
        /// </summary>
        /// <param name="fromIndex">Start drawing from an selected index</param>
        void drawInventory(int fromIndex = 0 /*from what item it should start drawing from*/)
        {
            Player p = (Player)tiles[playerPos.X, playerPos.Y];

            if (p.nInvItems > 0)
            {
                invLowestY = 0;

                if (fromIndex == 0)
                {
                    for (int i = 0; i < invSelItem; i++)
                        inv_drawProcess(p.inventory[i], i, Constants.invItemBorderColor);
                    inv_drawProcess(p.inventory[invSelItem], invSelItem, Constants.invSelItemBorderColor);
                    for (int i = invSelItem + 1; i < p.nInvItems; i++)
                        inv_drawProcess(p.inventory[i], i, Constants.invItemBorderColor);
                }
                else
                {
                    if (fromIndex <= invSelItem)
                    {
                        for (int i = 0; i < invSelItem; i++)
                            inv_drawProcess(p.inventory[i], i, Constants.invItemBorderColor);
                        inv_drawProcess(p.inventory[invSelItem], invSelItem, Constants.invSelItemBorderColor);
                        for (int i = invSelItem + 1; i < p.nInvItems; i++)
                            inv_drawProcess(p.inventory[i], i, Constants.invItemBorderColor);
                    }
                    else
                    {
                        for (int i = fromIndex; i < p.nInvItems; i++)
                            inv_drawProcess(p.inventory[i], i, Constants.invItemBorderColor);
                    }
                }

                // Draw two times. Very redundant, but used for the background.
                inv_drawDescription();
                inv_drawDescription();

                invPrevPoint = new Point();
            }
            else
            {
                // If the description was shown and it's the last item, it should be removed!
                if (invDidDescDraw)
                {
                    MakeBlackSpace(invDescription);
                    invDescription = new DisplayItem(new Point(), 0, 0);
                }
                WriteCenter("Your inventory is empty");
            }
        }


        /// <summary>
        /// Filters all player inventory items based on a lambda
        /// </summary>
        /// <param name="filter">Filter function</param>
        /// <returns>Array of indexes</returns>
        ushort[] filterInventoryItems(Func<InventoryItem, bool> filter)
        {
            Player p = (Player)tiles[playerPos.X, playerPos.Y];

            ushort index = 0;
            ushort[] items = new ushort[p.nInvItems];

            for (ushort i = 0; i < p.nInvItems; i++)
            {
                if (filter(p.inventory[i]))
                {
                    items[index++] = i;
                }
            }

            Array.Resize(ref items, index);
            return items;
        }

        /// <summary>
        /// Lets the user select an inventory item based on a filter
        /// </summary>
        /// <param name="selected">Array of player inventory indexes</param>
        /// <returns>Index of item</returns>
        int selectInventoryItem(ushort[] selected)
        {
            // ugly way to solve this problem
            invActionSel = 0;
            invSelItem = 0;
            invPrevPoint = new Point();
            invLowestY = 0;

            Player p = (Player)tiles[playerPos.X, playerPos.Y];

            InventoryItem[] items = new InventoryItem[selected.Length];
            for (int i = 0; i < items.Length; i++)
            {
                items[i] = p.inventory[selected[i]];
                items[i].actions = new InventoryAction[] { new InventoryActionGraphicalSelect() };
            }

            inv_drawProcess(items[0], 0, Constants.invSelItemBorderColor);
            for (int i = 1; i < items.Length; i++)
            {
                inv_drawProcess(items[i], i, Constants.invItemBorderColor);
            }

            inv_drawDescription(items[0]);
            inv_drawDescription(items[0]);

            int ret = -2;

            while (ret == -2)
            {
                switch (Console.ReadKey().Key)
                {
                    case ConsoleKey.Escape:
                        ret = -1;
                        break;

                    case ConsoleKey.LeftArrow:
                        if (--invSelItem == -1) invSelItem = items.Length - 1;

                        invDidDescDraw = false;
                        MakeBlackSpace(invDescription);

                        for (int i = 0; i < items.Length; i++)
                        {
                            if (Collides(invDescription, invDItems[i]))
                            {
                                drawInvItem(items[i], Constants.invItemBorderColor, invDItems[i].pos);
                            }
                        }

                        drawInvItem(items[invSelItem], Constants.invSelItemBorderColor, invDItems[invSelItem].pos);
                        inv_drawDescription(items[invSelItem]);
                        inv_drawDescription(items[invSelItem]);
                        break;
                    case ConsoleKey.RightArrow:
                        if (++invSelItem == items.Length) invSelItem = 0;

                        invDidDescDraw = false;
                        MakeBlackSpace(invDescription);

                        for (int i = 0; i < items.Length; i++)
                        {
                            if (Collides(invDescription, invDItems[i]))
                            {
                                drawInvItem(items[i], Constants.invItemBorderColor, invDItems[i].pos);
                            }
                        }

                        drawInvItem(items[invSelItem], Constants.invSelItemBorderColor, invDItems[invSelItem].pos);
                        inv_drawDescription(items[invSelItem]);
                        inv_drawDescription(items[invSelItem]);
                        break;

                    case ConsoleKey.Enter:
                        ret = selected[invSelItem];
                        break;
                }
            }

            invDidDescDraw = false;
            invPrevPoint = new Point();
            invLowestY = 0;
            return ret;
        }

        void inv_drawProcess(InventoryItem item, int i, ConsoleColor clr)
        {
            int width = item.image.GetLength(1) + 2;

            if (invPrevPoint.X + width >= Console.BufferWidth)
            {
                invPrevPoint.X = 0;
                invPrevPoint.Y = invLowestY + 1;
            }
            
            Point begin = new Point(invPrevPoint.X, invPrevPoint.Y);
            drawInvItem(item, clr, invPrevPoint);

            invDItems[i] = new DisplayItem(begin, new Point(Console.CursorLeft, Console.CursorTop + 1));

            invPrevPoint.X += width + 1;
            if (Console.CursorTop > invLowestY) invLowestY = Console.CursorTop;
        }

        void drawInvItem(InventoryItem item, ConsoleColor borderColor, Point origin)
        {
            int innerWidth = item.image.GetLength(1);

            // generate lower and upper bar
            string up = Constants.lupWall.ToString();
            string down = Constants.ldownWall.ToString();

            for (int i = 0; i < innerWidth; i++)
            {
                up += Constants.xWall;
                down += Constants.xWall;
            }

            up += Constants.rupWall.ToString();
            down += Constants.rdownWall.ToString();

            // write upper bar
            Console.ForegroundColor = borderColor;
            Console.CursorLeft = origin.X;
            Console.CursorTop = origin.Y;
            Console.Write(up);
            Console.CursorLeft = origin.X;
            Console.CursorTop++;

            string[] title = Constants.GenerateReadableString(item.name, innerWidth);
            
            // draw title
            for (int y = 0; y < title.Length; y++)
            {
                Console.ForegroundColor = borderColor;
                Console.Write(Constants.yWall);

                Console.ForegroundColor = ConsoleColor.White;

                string t = title[y];
                Console.CursorLeft = origin.X + innerWidth / 2 - t.Length / 2 + 1;
                Console.Write(t);

                Console.CursorLeft = origin.X + innerWidth + 1;
                Console.ForegroundColor = borderColor;
                Console.Write(Constants.yWall);
                Console.CursorTop++;
                Console.CursorLeft = origin.X;
            }

            // draw image
            char[] lineBuf = new char[innerWidth];

            for (int y = 0; y < item.image.GetLength(0); y++)
            {
                Console.ForegroundColor = borderColor;
                Console.Write(Constants.yWall);

                for (int x = 0; x < innerWidth; x++)
                    lineBuf[x] = item.image[y, x];

                Console.ForegroundColor = item.color;
                Console.Write(lineBuf);

                Console.ForegroundColor = borderColor;
                Console.Write(Constants.yWall);
                Console.CursorLeft = origin.X;
                Console.CursorTop++;
            }

            // draw lower bar
            Console.Write(down);
        }

        void inv_drawDescription(InventoryItem item = null)
        {
            // if this needs to change, make sure the background changes too
            if (invDidDescDraw)
            {
                MakeBlackSpace(invDescription);
            }

            if (item == null) item = ((Player)tiles[playerPos.X, playerPos.Y]).inventory[invSelItem];
            int itemInnerWidth = item.image.GetLength(1);

            bool lefty = invDItems[invSelItem].pos.X + Constants.invDescriptionWidth >= tiles.GetLength(0);
            int originX = lefty ? invDItems[invSelItem].pos.X - Constants.invDescriptionWidth + itemInnerWidth + 2 : invDItems[invSelItem].pos.X;

            Point begin = new Point(originX, invDItems[invSelItem].EndY - 1);

            // used to display the horizontal stuff(upper bar and lower bar)
            char[] bar = new char[Constants.invDescriptionWidth];

            // make a "template" for the bars
            for (int i = 0; i < Constants.invDescriptionWidth; i++)
            {
                bar[i] = Constants.xWall;
            }

            // rip in pece uncompressed downloaders
            int thatStupidCharacterThatBreaksEverything;

            // construct upper bar
            if (lefty)
            {
                thatStupidCharacterThatBreaksEverything = Constants.invDescriptionWidth - itemInnerWidth - 2;
                bar[thatStupidCharacterThatBreaksEverything] = Constants.yWallToXWallBothSides;
                bar[0] = Constants.lupWall;
                bar[Constants.invDescriptionWidth - 1] = Constants.yWallWithLeftXWall;
            }
            else
            {
                thatStupidCharacterThatBreaksEverything = itemInnerWidth + 1;
                bar[thatStupidCharacterThatBreaksEverything] = Constants.yWallToXWallBothSides;
                bar[0] = Constants.yWallWithRightXWall;
                bar[Constants.invDescriptionWidth - 1] = Constants.rupWall;
            }

            // draw the upper bar
            Console.CursorLeft = originX;
            Console.CursorTop = begin.Y;
            Console.ForegroundColor = Constants.invSelItemBorderColor;

            Console.Write(bar);

            Console.CursorTop++;
            Console.CursorLeft = originX;

            // draw the description
            string[] description = Constants.GenerateReadableString(item.description, Constants.invDescriptionWidth - 4);
            for (int y = 0; y < description.Length; y++)
            {
                Console.Write(Constants.yWall);
                Console.CursorLeft++;
                Console.ForegroundColor = ConsoleColor.Gray;

                Console.Write(description[y]);

                Console.ForegroundColor = Constants.invSelItemBorderColor;
                Console.CursorLeft = originX + Constants.invDescriptionWidth - 1;
                Console.Write(Constants.yWall);

                Console.CursorTop++;
                Console.CursorLeft = originX;
            }

            // extra line(wow what an effort)
            Console.Write(Constants.yWall);
            Console.CursorLeft = originX + Constants.invDescriptionWidth - 1;
            Console.Write(Constants.yWall);
            Console.CursorTop++;
            Console.CursorLeft = originX;

            // draw additional info
            if (item.extraInfo != null)
            {
                int valLoc = 0; // global location of the values displayed

                // calculate the max distance so everything looks neat
                for (int i = 0; i < item.extraInfo.Length; i++)
                    if (item.extraInfo[i] is IIAID && item.extraInfo[i].label.Length > valLoc) valLoc = item.extraInfo[i].label.Length;

                valLoc += originX + 4;

                for (int i = 0; i < item.extraInfo.Length; i++)
                {
                    Console.Write(Constants.yWall);

                    // If'its
                    if (item.extraInfo[i] is IIAID)
                    {
                        IIAID itemInfo = (IIAID)item.extraInfo[i];

                        Console.CursorLeft++;
                        Console.ForegroundColor = itemInfo.lColor;

                        Console.Write(itemInfo.label);
                        Console.CursorLeft = valLoc;
                        Console.ForegroundColor = itemInfo.vColor;
                        Console.Write(itemInfo.value);
                    }
                    else if (item.extraInfo[i] is IIAIH)
                    {
                        // Add an extra line
                        Console.CursorLeft = originX + Constants.invDescriptionWidth - 1;
                        Console.Write(Constants.yWall);
                        Console.CursorTop++;
                        Console.CursorLeft = originX;
                        Console.Write(Constants.yWall);

                        IIAIH header = (IIAIH)item.extraInfo[i];

                        // relative location
                        int relativeLoc = (Constants.invDescriptionWidth - 4) / 2 - header.label.Length / 2;
                        int rightLength = (Constants.invDescriptionWidth - 5) - header.label.Length - relativeLoc;
                        char[] buf = new char[--relativeLoc];

                        for (byte a = 0; a < relativeLoc; a++)
                            buf[a] = '-';

                        Console.CursorLeft++;
                        Console.ForegroundColor = ConsoleColor.DarkGray;
                        Console.Write(buf);
                        Console.CursorLeft++;

                        Console.ForegroundColor = header.lColor;
                        Console.Write(header.label);
                        Console.CursorLeft++;

                        buf = new char[rightLength];
                        for (byte a = 0; a < rightLength; a++)
                            buf[a] = '-';

                        Console.ForegroundColor = ConsoleColor.DarkGray;
                        Console.Write(buf);
                    }
                    else
                    {
                        Console.CursorLeft++;

                        Console.ForegroundColor = item.extraInfo[i].lColor;
                        Console.Write(item.extraInfo[i].label);
                    }
                    

                    Console.ForegroundColor = Constants.invSelItemBorderColor;
                    Console.CursorLeft = originX + Constants.invDescriptionWidth - 1;
                    Console.Write(Constants.yWall);

                    Console.CursorTop++;
                    Console.CursorLeft = originX;
                }

                // ... and an extra line
                Console.Write(Constants.yWall);
                Console.CursorLeft = originX + Constants.invDescriptionWidth - 1;
                Console.Write(Constants.yWall);
                Console.CursorTop++;
                Console.CursorLeft = originX;
            }

            for (int i = 0; i < item.actions.Length; i++)
            {
                Console.Write(Constants.yWall);

                // Draw the description of the action when needed
                if (i == invActionSel)
                {
                    // draw cursor on selected item
                    Console.CursorLeft++;
                    Console.ForegroundColor = ConsoleColor.DarkRed; // TODO: Not sure. Blue? Background color?
                    Console.Write(Constants.chars[4]);
                    Console.CursorLeft += 2;

                    Console.ForegroundColor = ConsoleColor.Magenta;
                    Console.Write(item.actions[i].Action);

                    Console.CursorLeft += 2;

                    int x = Console.CursorLeft;
                    string[] desc = Constants.GenerateReadableString(
                        item.actions[i].Description,
                        Constants.invDescriptionWidth - (x - originX) - 2
                    );

                    if (desc.Length > 1)
                    {
                        Console.ForegroundColor = Constants.invSelItemBorderColor;

                        for (int y = 0; y < desc.Length - 1; y++)
                        {
                            Console.CursorLeft = x;
                            Console.ForegroundColor = ConsoleColor.Gray; // FOR COLOR CHANGE CHECK

                            Console.Write(desc[y]);

                            Console.ForegroundColor = Constants.invSelItemBorderColor;
                            Console.CursorLeft = originX + Constants.invDescriptionWidth - 1;
                            Console.Write(Constants.yWall);

                            Console.CursorLeft = originX;
                            Console.CursorTop++;

                            Console.Write(Constants.yWall);
                        }
                    }

                    Console.CursorLeft = x;
                    Console.ForegroundColor = ConsoleColor.Gray; // THIS OUT TOO

                    // one line doesn't count
                    Console.Write(desc[desc.Length - 1]);
                }
                else
                {
                    Console.CursorLeft += 4;
                    Console.ForegroundColor = ConsoleColor.Red;
                    Console.Write(item.actions[i].Action);
                }

                Console.ForegroundColor = Constants.invSelItemBorderColor;
                Console.CursorLeft = originX + Constants.invDescriptionWidth - 1;
                Console.Write(Constants.yWall);
                Console.CursorLeft = originX;
                Console.CursorTop++;
            }

            // construct lower bar
            bar[0] = Constants.ldownWall;
            bar[Constants.invDescriptionWidth - 1] = Constants.rdownWall;
            bar[thatStupidCharacterThatBreaksEverything] = Constants.xWall;

            // draw lower bar
            Console.Write(bar);

            int endY = invDescription.EndY;

            invDescription = new DisplayItem(begin, new Point(Console.CursorLeft, Console.CursorTop + 1));

            // if description border had decreased, draw the collided inventory items again
            if (invDidDescDraw && (endY > invDescription.EndY || invDescription.EndY - endY > 1))
            {
                Player p = ((Player)tiles[playerPos.X, playerPos.Y]);
                bool needed = false;
                for (int i = 0; i < p.nInvItems; i++)
                {
                    if (invDItems[i].EndY > invDescription.EndY && Collides(invDescription, invDItems[i]))
                    {
                        needed = true;
                        drawInvItem(p.inventory[i], Constants.invItemBorderColor/*can never be selected item, no need to check*/, invDItems[i].pos);
                    }
                }
                if (needed)
                {
                    // ... and itself again
                    inv_drawDescription();
                    return;
                }
            }

            Console.ForegroundColor = ConsoleColor.White;
            Console.CursorLeft = 0;
            Console.CursorTop = Console.BufferHeight - 1;

            invDidDescDraw = true;
        }

        void doSelectedInventoryAction()
        {
            Player p = (Player)tiles[playerPos.X, playerPos.Y];

            if (p.inventory[invSelItem].actions[invActionSel] is InventoryActionInteract)
            {
                InventoryActionInteract action = (InventoryActionInteract)p.inventory[invSelItem].actions[invActionSel];

                object target;

                switch (action.command)
                {
                    case InventoryAction.PreExecuteCommand.SelectDirection:
                        state = State.Default;
                        Message("Which direction?");
                        reDrawDungeon();

                        Point dir = Constants.GetDirectionByKey(Console.ReadKey().Key);
                        if (!dir.Same(0, 0))
                        {
                            Point targetPoint = new Point(playerPos.X + dir.X, playerPos.Y + dir.Y);

                            target = tiles[targetPoint.X, targetPoint.Y];
                            if (action.Interact(ref p, invSelItem, ref target))
                            {
                                p.RemoveInventoryItem(invSelItem);

                                if (invSelItem == p.nInvItems && invSelItem != 0)
                                    invSelItem--;
                            }
                            tiles[targetPoint.X, targetPoint.Y] = (Tile)target;

                            draw();
                        }
                        break;

                    case InventoryAction.PreExecuteCommand.SelectItem:
                        clearInventoryScreen();
                        Message("Select item");
                        drawInfoBar();

                        int sel = invSelItem;
                        target = selectInventoryItem(filterInventoryItems(((InventoryActionSelect)action).filterFunc));
                        if ((int)target != -1) action.Interact(ref p, sel, ref target);

                        clearInventoryScreen();
                        drawInfoBar();

                        drawInventory();
                        break;
                }
            }
            else
            {
                // if an item gets added while the other is being removed,
                // it will crash because invDItems doesn't get updated while the inventory does.
                int idiCount = p.nInvItems;

                // Do the action, check if needs to be destroyed
                // It seems kindof redundant passing in the arguments you use to call the function itself
                if (p.inventory[invSelItem].actions[invActionSel].Act(ref p, invSelItem))
                {
                    // destroy item
                    for (int i = invSelItem; i < idiCount; i++)
                    {
                        MakeBlackSpace(invDItems[i]);
                    }
                    p.RemoveInventoryItem(invSelItem);

                    // == seems unsafe, but if you mess arround with it it'll crash <i>anyway</i>
                    if (invSelItem == p.nInvItems && invSelItem != 0)
                        invSelItem--;

                    drawInventory(invSelItem);
                }

                drawInventory();
                drawInfoBar();
            }
        }

        void clearInventoryScreen()
        {
            Console.Clear();
            Console.BackgroundColor = ConsoleColor.DarkBlue;
            Console.CursorTop = tiles.GetLength(1);
            drawEmptySpaceOnce(Console.BufferWidth * 2);
            invDidDescDraw = false;
        }

        void inv_changeSelectedItem(int to)
        {
            Player p = (Player)tiles[playerPos.X, playerPos.Y];

            if (to < 0) to = p.nInvItems - 1;
            else if (to >= p.nInvItems) to = 0;

            invSelItem = to;

            invDidDescDraw = false;
            MakeBlackSpace(invDescription);
            inv_handleCollision(invDescription);
            drawInvItem(p.inventory[invSelItem], Constants.invSelItemBorderColor, invDItems[invSelItem].pos);
            inv_drawDescription();
            inv_drawDescription();
        }

        void inv_wipePopup(DisplayItem item)
        {
            MakeBlackSpace(item);
            inv_handleCollision(item);
            if (Collides(item, invDescription))
            {
                inv_drawDescription();
            }
        }

        void inv_handleCollision(DisplayItem item)
        {
            Player p = (Player)tiles[playerPos.X, playerPos.Y];

            for (int i = 0; i < p.nInvItems; i++)
            {
                if (Collides(item, invDItems[i]))
                {
                    drawInvItem(p.inventory[i], Constants.invItemBorderColor, invDItems[i].pos);
                }
            }
        }

        public static bool Collides(DisplayItem a, DisplayItem b)
        {
            // Constants.invDescriptionWidth is left out, because a's width needs to be called sometimes.
            // Labda's don't work as default parameters, so everything becomes a mess.
            // So that optimization cancels out.
            return (a.pos.X + a.width/*Constants.invDescriptionWidth*/ > b.pos.X && a.pos.X < b.pos.X + b.width/*Constants.invDescriptionWidth*/ &&
                    a.pos.Y + a.height > b.pos.Y && a.pos.Y < b.pos.Y + b.height);
        }



        public static void MakeBlackSpace(DisplayItem item)
        {
            Console.CursorTop = item.pos.Y;

            // writing an array of characters is wayyy faster than one character.
            char[] blank = new char[item.width];
            for (int i = 0; i < blank.Length; i++)
                blank[i] = ' ';

            for (int y = 0; y < item.height; y++)
            {
                Console.CursorLeft = item.pos.X;
                Console.Write(blank);
                Console.CursorTop++;
            }
        }
    }
}
