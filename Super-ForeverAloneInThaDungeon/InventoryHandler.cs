using System;
using System.Linq;
using System.Text;
using System.Threading;
using Super_ForeverAloneInThaDungeon.Display;
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
            string[] title = Constants.GenerateReadableString(item.name, innerWidth);
            int height = item.image.GetLength(0) + title.Length + 2;

            _drawer.DrawRectangle(origin, new Size(innerWidth, height), ConsoleColor.Black);

            StringBuilder topBar = new StringBuilder(Constants.lupWall.ToString());
            StringBuilder bottomBar = new StringBuilder(Constants.ldownWall.ToString());

            for (int i = 0; i < innerWidth; i++)
            {
                topBar.Append(Constants.xWall);
                bottomBar.Append(Constants.xWall);
            }

            topBar.Append(Constants.rupWall.ToString());
            bottomBar.Append(Constants.rdownWall.ToString());

            _drawer.Write(topBar.ToString(), borderColor, origin);

            
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
                                           
            InventoryItem selectedItem = _player.inventory[SelectedItemIndex];
            string[] itemDescription = Constants.GenerateReadableString(selectedItem.description, Constants.invDescriptionWidth - 4);
            
            int itemInnerWidth = selectedItem.image.GetLength(1);

            bool lefty = _items[SelectedItemIndex].Origin.X + Constants.invDescriptionWidth >= _mapWidth;

            var selectedAction = selectedItem.actions[SelectedActionIndex];
            var remainingActions = selectedItem.actions.Count(a => a != selectedAction);

            string[] selectedActionDescriptionLines = Constants.GenerateReadableString(
                        selectedAction.Description,
                        Constants.invDescriptionWidth - 9 - selectedAction.Action.Length
                    );

            int width = Constants.invDescriptionWidth;

            int height = 4 + itemDescription.Length + selectedItem.Details.Length + selectedActionDescriptionLines.Length + remainingActions;

            int originX = lefty ? _items[SelectedItemIndex].Origin.X - Constants.invDescriptionWidth + itemInnerWidth + 2 : _items[SelectedItemIndex].Origin.X;

            Point descriptionBlockOrigin = new Point(originX, _items[SelectedItemIndex].EndY - 1);

            _drawer.DrawRectangle(descriptionBlockOrigin, new Size(width, height), ConsoleColor.Black);
            
            char[] bar = new char[Constants.invDescriptionWidth];

            for (int i = 0; i < Constants.invDescriptionWidth; i++)
            {
                bar[i] = Constants.xWall;
            }

            int thatStupidCharacterThatBreaksEverything;

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
// Y1

            
            for (int y = 0; y < itemDescription.Length; y++)
            {

                _drawer.Draw(Constants.yWall, Constants.invSelItemBorderColor, new Point(cursorLeft, cursorTop));

                cursorLeft += 1;
                
                _drawer.Write(itemDescription[y], ConsoleColor.Gray, new Point(cursorLeft, cursorTop));

                cursorLeft = originX + Constants.invDescriptionWidth - 1;
                _drawer.Draw(Constants.yWall, Constants.invSelItemBorderColor, new Point(cursorLeft, cursorTop));

                cursorTop++;
                cursorLeft = originX;
            }
// Y1 + description.length
            _drawer.Draw(Constants.yWall, Constants.invSelItemBorderColor, new Point(cursorLeft, cursorTop));
            cursorLeft = originX + Constants.invDescriptionWidth - 1;
            _drawer.Draw(Constants.yWall, Constants.invSelItemBorderColor, new Point(cursorLeft, cursorTop));
            cursorTop++;
            cursorLeft = originX;
// Y2 + description.length
            if (selectedItem.Details != null)
            {
                int valueLocation = 0;

                for (int i = 0; i < selectedItem.Details.Length; i++)
                    if (selectedItem.Details[i].Label.Length > valueLocation) 
                        valueLocation = selectedItem.Details[i].Label.Length;

                valueLocation += originX + 4;

                for (int i = 0; i < selectedItem.Details.Length; i++)
                {
                    _drawer.Draw(Constants.yWall, Constants.invSelItemBorderColor, new Point(cursorLeft, cursorTop));
                    cursorLeft++;
                    _drawer.Write(selectedItem.Details[i].Label + ':', selectedItem.Details[i].LabelColor, new Point(cursorLeft, cursorTop));

                    cursorLeft = valueLocation;
                    _drawer.Write(selectedItem.Details[i].Value, selectedItem.Details[i].ValueColor, new Point(cursorLeft, cursorTop));

                    cursorLeft = originX + Constants.invDescriptionWidth - 1;
                    _drawer.Draw(Constants.yWall, Constants.invSelItemBorderColor, new Point(cursorLeft, cursorTop));

                    cursorTop++;
                    cursorLeft = originX;
                }
// Y2 + description.length + item.Details.Length

                _drawer.Draw(Constants.yWall, Constants.invSelItemBorderColor, new Point(cursorLeft, cursorTop));
                cursorLeft = originX + Constants.invDescriptionWidth - 1;
                _drawer.Draw(Constants.yWall, Constants.invSelItemBorderColor, new Point(cursorLeft, cursorTop));
                cursorTop++;
                cursorLeft = originX;
// Y3 + description.length + item.Details.Length
            }

            for (int i = 0; i < selectedItem.actions.Length; i++)
            {
                _drawer.Draw(Constants.yWall, Constants.invSelItemBorderColor, new Point(cursorLeft, cursorTop));
                cursorLeft++;

                if (i == SelectedActionIndex)
                {

                    cursorLeft++;
                    _drawer.Draw(Constants.chars[4], ConsoleColor.DarkRed, new Point(cursorLeft, cursorTop));
                    cursorLeft += 3;

                    _drawer.Write(selectedItem.actions[i].Action, ConsoleColor.Magenta, new Point(cursorLeft, cursorTop));
                    cursorLeft += selectedItem.actions[i].Action.Length;
                    cursorLeft += 2;

                    int x = cursorLeft;
                    string[] desc = Constants.GenerateReadableString(
                        selectedItem.actions[i].Description,
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
                    _drawer.Write(selectedItem.actions[i].Action, ConsoleColor.Red, new Point(cursorLeft, cursorTop));
                }
// Y3 + description.length + item.Details.Length + selectedAction.desc.Length + item.actions\selectedAction.Length
                cursorLeft = originX + Constants.invDescriptionWidth - 1;
                _drawer.Draw(Constants.yWall, Constants.invSelItemBorderColor, new Point(cursorLeft, cursorTop));
                cursorLeft = originX;
                cursorTop++;
// Y4 +  description.length + item.Details.Length + selectedAction.desc.Length + item.actions\selectedAction.Length
            }

            bar[0] = Constants.ldownWall;
            bar[Constants.invDescriptionWidth - 1] = Constants.rdownWall;
            bar[thatStupidCharacterThatBreaksEverything] = Constants.xWall;

            _drawer.Write(bar, Constants.invSelItemBorderColor, new Point(cursorLeft, cursorTop));

            int previousEndY = _itemDescription.EndY;

            _itemDescription = new DisplayItem(descriptionBlockOrigin, new Point(Constants.invDescriptionWidth, cursorTop + 1));

            // TODO : handle redraw on selected action change
            // if description border had decreased, draw the collided inventory _items again
            //if (invDidDescDraw && (previousEndY > _itemDescription.EndY || _itemDescription.EndY - previousEndY > 1))
            //{
            //    bool needed = false;
            //    for (int i = 0; i < _player.ItemCount; i++)
            //    {
            //        if (_items[i].EndY > _itemDescription.EndY && inv_collides(_itemDescription, _items[i]))
            //        {
            //            needed = true;
            //            DrawItem(_player.inventory[i], Constants.invItemBorderColor, _items[i].Origin);
            //        }
            //    }
            //    if (needed)
            //    {
            //        DrawDescription();
            //        return;
            //    }
            //}

            //invDidDescDraw = true;
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
