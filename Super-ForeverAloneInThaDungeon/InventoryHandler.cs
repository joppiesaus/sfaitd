using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Super_ForeverAloneInThaDungeon.Graphics;

namespace Super_ForeverAloneInThaDungeon
{
    public class InventoryHandler
    {
        public int SelectedItemIndex { get; set; }
        public int SelectedActionIndex { get; set; }


        Point invPrevPoint = new Point();
        int invLowestY = 0;

        private readonly List<DisplayItem> _items;
        private DisplayItem _itemDescription = new DisplayItem(new Point(), new Point());

        public InventoryItem SelectedItem { get { return _player.Inventory[SelectedItemIndex]; } }

        private readonly Player _player;
        private readonly Drawer _drawer;
        private readonly int _mapWidth;

        public InventoryHandler(Player player, Drawer drawer, int width)
        {
            _player = player;
            _drawer = drawer;
            _mapWidth = width;

            _items = new List<DisplayItem>();
        }

        public void DrawInventory()
        {

            _drawer.ClearScreen();
            _items.Clear();
            invPrevPoint = Point.Origin;

            if (_player.ItemCount > 0)
            {
                foreach (var item in _player.Inventory)
                {
                    ConsoleColor color = item == SelectedItem
                        ? Constants.SelectedItemBorderColor
                        : Constants.ItemBorderColor;
                    
                    DrawProcess(item, 0, color);
                }
               

                DrawDescription();
            }
            else
            {
                _drawer.WriteCenter("Your Inventory is empty");
            }
        }

        public void MakeBlackSpace(DisplayItem item)
        {
            _drawer.DrawRectangle(item.Origin, new Size(item.Width, item.Height), ConsoleColor.Black);
        }

        public void DrawProcess(InventoryItem item, int i, ConsoleColor color)
        {
            int graphicItemWidth = item.Visual.GetLength(1) + 2;

            if (invPrevPoint.X + graphicItemWidth >= Console.BufferWidth)
            {
                invPrevPoint.X = 0;
                invPrevPoint.Y = invLowestY + 1;
            }
            
            Point begin = new Point(invPrevPoint.X, invPrevPoint.Y);

            DrawItem(item, color, invPrevPoint);

            // TODO : find other way to get the dimension
            int graphicItemHeight = Constants.GenerateReadableString(item.Name, item.Visual.GetLength(1)).Length + item.Visual.GetLength(0) + 1;

            _items.Add(new DisplayItem(begin, begin.AddX(graphicItemWidth).AddY(graphicItemHeight)));

            invPrevPoint.X += graphicItemWidth + 1;
            if (_items[i].EndY > invLowestY) invLowestY = _items[i].EndY;
        }

        public void DrawItem(InventoryItem item, ConsoleColor borderColor, Point origin)
        {
            int innerWidth = item.Visual.GetLength(1);
            string[] title = Constants.GenerateReadableString(item.Name, innerWidth);
            int height = item.Visual.GetLength(0) + title.Length + 2;


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

            for (int y = 0; y < item.Visual.GetLength(0); y++)
            {
                _drawer.Draw(Constants.yWall, borderColor, new Point(origin.X, origin.Y + title.Length + y));

                // TODO : Use string builder
                string line = "";
                for (int x = 0; x < innerWidth; x++)
                    line += item.Visual[y, x];

                _drawer.Write(line, item.Color, new Point(origin.X + 1, origin.Y + title.Length + y));

                _drawer.Draw(Constants.yWall, borderColor, new Point(origin.X + 1 + innerWidth, origin.Y + title.Length + y));
            }
            
            // draw lower bar
            int endLineIndex = origin.Y + title.Length + item.Visual.GetLength(0);
            _drawer.Write(bottomBar.ToString(), borderColor, new Point(origin.X, endLineIndex));
        }

        public void DrawDescription()
        {
            InventoryItem selectedItem = _player.Inventory[SelectedItemIndex];
            string[] itemDescription = Constants.GenerateReadableString(selectedItem.Description, Constants.invDescriptionWidth - 4);
            
            int itemInnerWidth = selectedItem.Visual.GetLength(1);

            bool lefty = _items[SelectedItemIndex].Origin.X + Constants.invDescriptionWidth >= _mapWidth;

            var selectedAction = selectedItem.Actions[SelectedActionIndex];
            var remainingActions = selectedItem.Actions.Count(a => a != selectedAction);

            string[] selectedActionDescriptionLines = Constants.GenerateReadableString(
                        selectedAction.Description,
                        Constants.invDescriptionWidth - 9 - selectedAction.Action.Length
                    );

            int width = Constants.invDescriptionWidth;

            int height = 4 + itemDescription.Length + selectedItem.Details.Count + selectedActionDescriptionLines.Length + remainingActions;

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

            _drawer.Write(bar, Constants.SelectedItemBorderColor, descriptionBlockOrigin);
            cursorTop++;

            foreach (var descriptionParts in itemDescription)
            {
                _drawer.Draw(Constants.yWall, Constants.SelectedItemBorderColor, new Point(cursorLeft, cursorTop));

                cursorLeft += 1;
                
                _drawer.Write(descriptionParts, ConsoleColor.Gray, new Point(cursorLeft, cursorTop));

                cursorLeft = originX + Constants.invDescriptionWidth - 1;
                _drawer.Draw(Constants.yWall, Constants.SelectedItemBorderColor, new Point(cursorLeft, cursorTop));

                cursorTop++;
                cursorLeft = originX;
            }

            _drawer.Draw(Constants.yWall, Constants.SelectedItemBorderColor, new Point(cursorLeft, cursorTop));
            cursorLeft = originX + Constants.invDescriptionWidth - 1;
            _drawer.Draw(Constants.yWall, Constants.SelectedItemBorderColor, new Point(cursorLeft, cursorTop));
            cursorTop++;
            cursorLeft = originX;

            if (selectedItem.Details != null)
            {
                int valueLocation = 0;

                for (int i = 0; i < selectedItem.Details.Count; i++)
                    if (selectedItem.Details[i].Label.Length > valueLocation) 
                        valueLocation = selectedItem.Details[i].Label.Length;

                valueLocation += originX + 4;

                for (int i = 0; i < selectedItem.Details.Count; i++)
                {
                    _drawer.Draw(Constants.yWall, Constants.SelectedItemBorderColor, new Point(cursorLeft, cursorTop));
                    cursorLeft++;
                    _drawer.Write(selectedItem.Details[i].Label + ':', selectedItem.Details[i].LabelColor, new Point(cursorLeft, cursorTop));

                    cursorLeft = valueLocation;
                    _drawer.Write(selectedItem.Details[i].Value, selectedItem.Details[i].ValueColor, new Point(cursorLeft, cursorTop));

                    cursorLeft = originX + Constants.invDescriptionWidth - 1;
                    _drawer.Draw(Constants.yWall, Constants.SelectedItemBorderColor, new Point(cursorLeft, cursorTop));

                    cursorTop++;
                    cursorLeft = originX;
                }

                _drawer.Draw(Constants.yWall, Constants.SelectedItemBorderColor, new Point(cursorLeft, cursorTop));
                cursorLeft = originX + Constants.invDescriptionWidth - 1;
                _drawer.Draw(Constants.yWall, Constants.SelectedItemBorderColor, new Point(cursorLeft, cursorTop));
                cursorTop++;
                cursorLeft = originX;
            }

            for (int i = 0; i < selectedItem.Actions.Count; i++)
            {
                _drawer.Draw(Constants.yWall, Constants.SelectedItemBorderColor, new Point(cursorLeft, cursorTop));
                cursorLeft++;

                if (i == SelectedActionIndex)
                {

                    cursorLeft++;
                    _drawer.Draw(Constants.chars[4], ConsoleColor.DarkRed, new Point(cursorLeft, cursorTop));
                    cursorLeft += 3;

                    _drawer.Write(selectedItem.Actions[i].Action, ConsoleColor.Magenta, new Point(cursorLeft, cursorTop));
                    cursorLeft += selectedItem.Actions[i].Action.Length;
                    cursorLeft += 2;

                    int x = cursorLeft;
                    string[] desc = Constants.GenerateReadableString(
                        selectedItem.Actions[i].Description,
                        Constants.invDescriptionWidth - (x - originX) - 2
                    );


                    if (desc.Length > 1)
                    {
                        Console.ForegroundColor = Constants.SelectedItemBorderColor;

                        for (int y = 0; y < desc.Length - 1; y++)
                        {
                            cursorLeft = x;

                            _drawer.Write(desc[y], ConsoleColor.Gray, new Point(cursorLeft, cursorTop));

                            cursorLeft = originX + Constants.invDescriptionWidth - 1;

                            _drawer.Draw(Constants.yWall, Constants.SelectedItemBorderColor, new Point(cursorLeft, cursorTop));

                            cursorLeft = originX;
                            cursorTop++;

                            _drawer.Draw(Constants.yWall, Constants.SelectedItemBorderColor, new Point(cursorLeft, cursorTop));
                        }
                    }


                    cursorLeft = x;

                    _drawer.Write(desc[desc.Length - 1], ConsoleColor.Gray, new Point(cursorLeft, cursorTop));
                }
                else
                {
                    cursorLeft += 4;
                    _drawer.Write(selectedItem.Actions[i].Action, ConsoleColor.Red, new Point(cursorLeft, cursorTop));
                }

                cursorLeft = originX + Constants.invDescriptionWidth - 1;
                _drawer.Draw(Constants.yWall, Constants.SelectedItemBorderColor, new Point(cursorLeft, cursorTop));
                cursorLeft = originX;
                cursorTop++;
            }

            bar[0] = Constants.ldownWall;
            bar[Constants.invDescriptionWidth - 1] = Constants.rdownWall;
            bar[thatStupidCharacterThatBreaksEverything] = Constants.xWall;

            _drawer.Write(bar, Constants.SelectedItemBorderColor, new Point(cursorLeft, cursorTop));

            int previousEndY = _itemDescription.EndY;

            _itemDescription = new DisplayItem(descriptionBlockOrigin, new Point(Constants.invDescriptionWidth, cursorTop + 1));
        }

        public void PreviousAction()
        {
            if (--SelectedActionIndex < 0)
            {
                SelectedActionIndex = SelectedItem.Actions.Count - 1;
            }

            DrawInventory();
        }

        public void NextAction()
        {
            if (++SelectedActionIndex >= SelectedItem.Actions.Count)
            {
                SelectedActionIndex = 0;
            }

            DrawInventory();
        }

        public void DoSelectedInventoryAction()
        {
            // if an item gets added while the other is being removed,
            // it will crash because _items doesn't get updated while the Inventory does.
            int idiCount = _player.ItemCount;

            // Do the action, check if needs to be destroyed
            // It seems kindof redundant passing in the arguments you use to call the function itself
            if (_player.Inventory[SelectedItemIndex].Actions[SelectedActionIndex].Act(_player, SelectedItemIndex))
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
            DrawInventory();

        }

        public void PreviousItem()
        {
            if (SelectedItemIndex == 0) return;
            --SelectedItemIndex;
            DrawInventory();
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
                    DrawItem(_player.Inventory[i], i == SelectedItemIndex ? Constants.SelectedItemBorderColor : Constants.ItemBorderColor, _items[i].Origin);
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
