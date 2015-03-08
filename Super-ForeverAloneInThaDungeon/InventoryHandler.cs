using System;
using System.Text;
using System.Threading;
using Super_ForeverAloneInThaDungeon.Graphics;

namespace Super_ForeverAloneInThaDungeon
{
    public class InventoryHandler
    {
        public int SelectedItemIndex { get; set; }
        public int SelectedActionIndex { get; set; }

        bool invDidDescDraw = false;

        Point invPrevPoint = new Point();
        int invLowestY = 0;

        private readonly DisplayItem[] _items;
        private DisplayItem _itemDescription = new DisplayItem(new Point(), new Point());

        public InventoryItem SelectedItem { get { return _player.inventory[SelectedItemIndex]; } }

        private readonly Player _player;
        private readonly Drawer _drawer;
        private readonly int _mapWidth;

        public InventoryHandler(Player player, Drawer drawer, int width)
        {
            _player = player;
            _drawer = drawer;
            _mapWidth = width;

            _items = new DisplayItem[Constants.invCapacity];
        }

        public void DrawInventory(int fromIndex = 0)
        {
            if (_player.ItemCount > 0)
            {
                invLowestY = 0;

                if (fromIndex == 0)
                {
                    for (int i = 0; i < SelectedItemIndex; i++)
                        inv_drawProcess(_player.inventory[i], i, Constants.invItemBorderColor);
                    inv_drawProcess(_player.inventory[SelectedItemIndex], SelectedItemIndex, Constants.invSelItemBorderColor);
                    for (int i = SelectedItemIndex + 1; i < _player.ItemCount; i++)
                        inv_drawProcess(_player.inventory[i], i, Constants.invItemBorderColor);
                }
                else
                {
                    if (fromIndex <= SelectedItemIndex)
                    {
                        for (int i = 0; i < SelectedItemIndex; i++)
                            inv_drawProcess(_player.inventory[i], i, Constants.invItemBorderColor);
                        inv_drawProcess(_player.inventory[SelectedItemIndex], SelectedItemIndex, Constants.invSelItemBorderColor);
                        for (int i = SelectedItemIndex + 1; i < _player.ItemCount; i++)
                            inv_drawProcess(_player.inventory[i], i, Constants.invItemBorderColor);
                    }
                    else
                    {
                        for (int i = fromIndex; i < _player.ItemCount; i++)
                            inv_drawProcess(_player.inventory[i], i, Constants.invItemBorderColor);
                    }
                }

                // Draw two times. Very redundant, but used for the background.
                DrawDescription();
                DrawDescription();

                invPrevPoint = new Point();
            }
            else
            {
                // If the description was shown and it's the last item, it should be removed!
                if (invDidDescDraw)
                {
                    MakeBlackSpace(_itemDescription);
                }
                _drawer.WriteCenter("Your inventory is empty");
            }
        }

        public void MakeBlackSpace(DisplayItem item)
        {
            _drawer.DrawRectangle(item.Origin, new Size(item.Width, item.Height), ConsoleColor.Black);
        }

        public void inv_drawProcess(InventoryItem item, int i, ConsoleColor color)
        {
            int graphicItemWidth = item.image.GetLength(1) + 2;

            if (invPrevPoint.X + graphicItemWidth >= Console.BufferWidth)
            {
                invPrevPoint.X = 0;
                invPrevPoint.Y = invLowestY + 1;
            }
            
            Point begin = new Point(invPrevPoint.X, invPrevPoint.Y);

            DrawItem(item, color, invPrevPoint);

            // TODO : find other way to get the dimension
            int graphicItemHeight = Constants.GenerateReadableString(item.name, item.image.GetLength(1)).Length + item.image.GetLength(0) + 1;

            _items[i] = new DisplayItem(begin, begin.AddX(graphicItemWidth).AddY(graphicItemHeight));

            invPrevPoint.X += graphicItemWidth + 1;
            if (_items[i].EndY > invLowestY) invLowestY = _items[i].EndY;
        }

        public void DrawItem(InventoryItem item, ConsoleColor borderColor, Point origin)
        {
            int innerWidth = item.image.GetLength(1);

            StringBuilder topBar = new StringBuilder(Constants.lupWall.ToString());
            StringBuilder bottomBar = new StringBuilder(Constants.ldownWall.ToString());

            for (int i = 0; i < innerWidth; i++)
            {
                topBar.Append(Constants.xWall);
                bottomBar.Append(Constants.xWall);
            }

            topBar.Append(Constants.rupWall.ToString());
            bottomBar.Append(Constants.rdownWall.ToString());

            _drawer.Write(topBar.ToString(), borderColor, origin); // 1

            

            string[] title = Constants.GenerateReadableString(item.name, innerWidth);
            
            for (int y = 0; y < title.Length; y++)
            {
                _drawer.Draw(Constants.yWall, borderColor, new Point(origin.X, origin.Y + y + 1));

                int x = origin.X + innerWidth / 2 - title[y].Length / 2 + 1;
                _drawer.Write(title[y], ConsoleColor.White, new Point(x, origin.Y + y));

                _drawer.Draw(Constants.yWall, borderColor, new Point(origin.X + innerWidth + 1, origin.Y + y + 1));
            }

            for (int y = 0; y < item.image.GetLength(0); y++)
            {
                _drawer.Draw(Constants.yWall, borderColor, new Point(origin.X, origin.Y + title.Length + y));

                // TODO : Use string builder
                string line = "";
                for (int x = 0; x < innerWidth; x++)
                    line += item.image[y, x];

                _drawer.Write(line, item.color, new Point(origin.X + 1, origin.Y + title.Length + y));

                _drawer.Draw(Constants.yWall, borderColor, new Point(origin.X + 1 + innerWidth, origin.Y + title.Length + y));
            }
            
            // draw lower bar
            int endLineIndex = origin.Y + title.Length + item.image.GetLength(0);
            _drawer.Write(bottomBar.ToString(), borderColor, new Point(origin.X, endLineIndex));
        }

        public void DrawDescription()
        {
            
            MakeBlackSpace(_itemDescription);
            
            InventoryItem item = _player.inventory[SelectedItemIndex];
            int itemInnerWidth = item.image.GetLength(1);

            bool lefty = _items[SelectedItemIndex].Origin.X + Constants.invDescriptionWidth >= _mapWidth;

            int originX = lefty ? _items[SelectedItemIndex].Origin.X - Constants.invDescriptionWidth + itemInnerWidth + 2 : _items[SelectedItemIndex].Origin.X;

            Point descriptionBlockOrigin = new Point(originX, _items[SelectedItemIndex].EndY - 1);

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

            int cursorLeft = descriptionBlockOrigin.X;
            int cursorTop = descriptionBlockOrigin.Y;

            _drawer.Write(bar, Constants.invSelItemBorderColor, descriptionBlockOrigin);

            cursorTop++;

            string[] description = Constants.GenerateReadableString(item.description, Constants.invDescriptionWidth - 4);
            for (int y = 0; y < description.Length; y++)
            {

                _drawer.Draw(Constants.yWall, Constants.invSelItemBorderColor, new Point(cursorLeft, cursorTop));

                cursorLeft += 1;
                


                _drawer.Write(description[y], ConsoleColor.Gray, new Point(cursorLeft, cursorTop));

                cursorLeft = originX + Constants.invDescriptionWidth - 1;
                _drawer.Draw(Constants.yWall, Constants.invSelItemBorderColor, new Point(cursorLeft, cursorTop));


                cursorTop++;
                cursorLeft = originX;
            }

            _drawer.Draw(Constants.yWall, Constants.invSelItemBorderColor, new Point(cursorLeft, cursorTop));
            cursorLeft = originX + Constants.invDescriptionWidth - 1;
            _drawer.Draw(Constants.yWall, Constants.invSelItemBorderColor, new Point(cursorLeft, cursorTop));
            cursorTop++;
            cursorLeft = originX;

            if (item.extraInfo != null)
            {
                int valLoc = 0;

                for (int i = 0; i < item.extraInfo.Length; i++)
                    if (item.extraInfo[i].label.Length > valLoc) valLoc = item.extraInfo[i].label.Length;

                valLoc += originX + 4;

                for (int i = 0; i < item.extraInfo.Length; i++)
                {
                    _drawer.Draw(Constants.yWall, Constants.invSelItemBorderColor, new Point(cursorLeft, cursorTop));
                    cursorLeft++;
                    _drawer.Write(item.extraInfo[i].label + ':', item.extraInfo[i].lColor, new Point(cursorLeft, cursorTop));

                    cursorLeft = valLoc;
                    _drawer.Write(item.extraInfo[i].value, item.extraInfo[i].vColor, new Point(cursorLeft, cursorTop));

                    cursorLeft = originX + Constants.invDescriptionWidth - 1;
                    _drawer.Draw(Constants.yWall, Constants.invSelItemBorderColor, new Point(cursorLeft, cursorTop));

                    cursorTop++;
                    cursorLeft = originX;
                }


                _drawer.Draw(Constants.yWall, Constants.invSelItemBorderColor, new Point(cursorLeft, cursorTop));
                cursorLeft = originX + Constants.invDescriptionWidth - 1;
                _drawer.Draw(Constants.yWall, Constants.invSelItemBorderColor, new Point(cursorLeft, cursorTop));
                cursorTop++;
                cursorLeft = originX;
            }

            for (int i = 0; i < item.actions.Length; i++)
            {
                _drawer.Draw(Constants.yWall, Constants.invSelItemBorderColor, new Point(cursorLeft, cursorTop));
                cursorLeft++;

                if (i == SelectedActionIndex)
                {

                    cursorLeft++;
                    _drawer.Draw(Constants.chars[4], ConsoleColor.DarkRed, new Point(cursorLeft, cursorTop));
                    cursorLeft += 3;

                    _drawer.Write(item.actions[i].Action,ConsoleColor.Magenta, new Point(cursorLeft, cursorTop));
                    cursorLeft += item.actions[i].Action.Length;
                    cursorLeft += 2;

                    int x = cursorLeft;
                    string[] desc = Constants.GenerateReadableString(
                        item.actions[i].Description,
                        Constants.invDescriptionWidth - (x - originX) - 2
                    );

                    if (desc.Length > 1)
                    {
                        Console.ForegroundColor = Constants.invSelItemBorderColor;

                        for (int y = 0; y < desc.Length - 1; y++)
                        {
                            cursorLeft = x;

                            _drawer.Write(desc[y], ConsoleColor.Gray, new Point(cursorLeft, cursorTop));

                            cursorLeft = originX + Constants.invDescriptionWidth - 1;

                            _drawer.Draw(Constants.yWall, Constants.invSelItemBorderColor, new Point(cursorLeft, cursorTop));

                            cursorLeft = originX;
                            cursorTop++;

                            _drawer.Draw(Constants.yWall, Constants.invSelItemBorderColor, new Point(cursorLeft, cursorTop));
                        }
                    }


                    cursorLeft = x;

                    _drawer.Write(desc[desc.Length - 1], ConsoleColor.Gray, new Point(cursorLeft, cursorTop));
                }
                else
                {
                    cursorLeft += 4;
                    _drawer.Write(item.actions[i].Action, ConsoleColor.Red, new Point(cursorLeft, cursorTop));
                }

                cursorLeft = originX + Constants.invDescriptionWidth - 1;
                _drawer.Draw(Constants.yWall, Constants.invSelItemBorderColor, new Point(cursorLeft, cursorTop));
                cursorLeft = originX;
                cursorTop++;
            }

            bar[0] = Constants.ldownWall;
            bar[Constants.invDescriptionWidth - 1] = Constants.rdownWall;
            bar[thatStupidCharacterThatBreaksEverything] = Constants.xWall;

            _drawer.Write(bar, Constants.invSelItemBorderColor, new Point(cursorLeft, cursorTop));

            int previousEndY = _itemDescription.EndY;

            _itemDescription = new DisplayItem(descriptionBlockOrigin, new Point(Constants.invDescriptionWidth, cursorTop + 1));

            // if description border had decreased, draw the collided inventory _items again
            if (invDidDescDraw && (previousEndY > _itemDescription.EndY || _itemDescription.EndY - previousEndY > 1))
            {
                bool needed = false;
                for (int i = 0; i < _player.ItemCount; i++)
                {
                    if (_items[i].EndY > _itemDescription.EndY && inv_collides(_itemDescription, _items[i]))
                    {
                        needed = true;
                        DrawItem(_player.inventory[i], Constants.invItemBorderColor, _items[i].Origin);
                    }
                }
                if (needed)
                {
                    DrawDescription();
                    return;
                }
            }

            invDidDescDraw = true;
        }

        public void PreviousAction()
        {
            if (--SelectedActionIndex < 0)
            {
                SelectedActionIndex = SelectedItem.actions.Length - 1;
            }

            DrawDescription();
        }

        public void NextAction()
        {
            if (++SelectedActionIndex >= SelectedItem.actions.Length)
            {
                SelectedActionIndex = 0;
            }

            DrawDescription();
        }

        public void DoSelectedInventoryAction()
        {

            // if an item gets added while the other is being removed,
            // it will crash because _items doesn't get updated while the inventory does.
            int idiCount = _player.ItemCount;

            // Do the action, check if needs to be destroyed
            // It seems kindof redundant passing in the arguments you use to call the function itself
            if (_player.inventory[SelectedItemIndex].actions[SelectedActionIndex].Act(_player, SelectedItemIndex))
            {
                // destroy item
                for (int i = SelectedItemIndex; i < idiCount; i++)
                {
                    MakeBlackSpace(_items[i]);
                }
                _player.removeInventoryItem(SelectedItemIndex);

                if (SelectedItemIndex == _player.ItemCount && SelectedItemIndex != 0)
                    SelectedItemIndex--;

                DrawInventory();
            }

        }

        public void NextItem()
        {
            if (SelectedItemIndex >= _player.ItemCount - 1) return;
            ++SelectedItemIndex;
            ChangeItemSelection();

        }

        public void PreviousItem()
        {
            if (SelectedItemIndex == 0) return;
            --SelectedItemIndex;
            ChangeItemSelection();
        }

        private void ChangeItemSelection()
        {
            invDidDescDraw = false;
            MakeBlackSpace(_itemDescription);
            inv_handleCollision(_itemDescription);
            DrawItem(_player.inventory[SelectedItemIndex], Constants.invSelItemBorderColor, _items[SelectedItemIndex].Origin);
            DrawDescription();
            DrawDescription();
        }

        public void inv_wipePopup(DisplayItem item)
        {
            MakeBlackSpace(item);
            inv_handleCollision(item);
            if (inv_collides(item, _itemDescription))
            {
                DrawDescription();
            }
        }

        public void inv_handleCollision(DisplayItem item)
        {

            for (int i = 0; i < _player.ItemCount; i++)
            {
                if (inv_collides(item, _items[i]))
                {
                    DrawItem(_player.inventory[i], i == SelectedItemIndex ? Constants.invSelItemBorderColor : Constants.invItemBorderColor, _items[i].Origin);
                }
            }
        }

        public bool inv_collides(DisplayItem a, DisplayItem b)
        {

            // Constants.invDescriptionWidth is left out, because a's width needs to be called sometimes.
            // Labda's don't work as default parameters, so everything becomes a mess.
            // So that optimization cancels out.
            return (a.Origin.X + a.Width/*Constants.invDescriptionWidth*/ > b.Origin.X && a.Origin.X < b.Origin.X + b.Width  &&
                    a.Origin.Y + a.Height > b.Origin.Y && a.Origin.Y < b.Origin.Y + b.Height);
        }
    }
}
