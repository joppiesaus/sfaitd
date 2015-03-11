﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Super_ForeverAloneInThaDungeon.Graphics;

namespace Super_ForeverAloneInThaDungeon
{
    public class InventoryHandler
    {
        private int _selectedItemIndex;
        private int _selectedActionIndex;


        private readonly List<DisplayItem> _items;
        private DisplayItem _itemDescription = new DisplayItem(new Point(), new Point());

        public InventoryItem SelectedItem { get { return _player.Inventory[_selectedItemIndex]; } }

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

        #region Graphic

        public void DrawInventory()
        {

            _drawer.ClearScreen();
            _items.Clear();

            if (_player.ItemCount > 0)
            {
                foreach (var item in _player.Inventory)
                {
                    ConsoleColor color = item == SelectedItem
                        ? Constants.SelectedItemBorderColor
                        : Constants.ItemBorderColor;
                    
                    DrawProcess(item, color);
                }
               
                // TODO : the description should be either drawn on the left side or the right, 
                // depending on the selected item location
                DrawDescription();
            }
            else
            {
                _drawer.WriteCenter("Your Inventory is empty");
            }
        }

        public void DrawProcess(InventoryItem item, ConsoleColor color)
        {
            // TODO : try to fit it on same line as previous item
            // if size is too small, change the line
            // TODO : change console buffer height to handle lot of items

            // TODO : find other way to get the dimension
            int graphicItemWidth = item.Visual.GetLength(1) + 2;
            int graphicItemHeight = Constants.GenerateReadableString(item.Name, item.Visual.GetLength(1)).Length + item.Visual.GetLength(0) + 1;

            DisplayItem previousItem = _items.Any() ? _items.Last() : null;

            if (previousItem == null)
            {
                DrawItem(item, color, Point.Origin);
                _items.Add(new DisplayItem(Point.Origin, new Size(graphicItemWidth, graphicItemHeight)));
                return;
            }

            Point origin = (previousItem.EndX + graphicItemWidth >= Console.WindowWidth)
                ? new Point(0, previousItem.EndY + 1)
                : new Point(previousItem.EndX, previousItem.Origin.Y);

            DrawItem(item, color, origin);
            _items.Add(new DisplayItem(origin, origin.AddX(graphicItemWidth).AddY(graphicItemHeight)));
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
            InventoryItem selectedItem = _player.Inventory[_selectedItemIndex];
            string[] itemDescription = Constants.GenerateReadableString(selectedItem.Description, Constants.invDescriptionWidth - 4);
            
            int itemInnerWidth = selectedItem.Visual.GetLength(1);

            bool lefty = _items[_selectedItemIndex].Origin.X + Constants.invDescriptionWidth >= _mapWidth;

            var selectedAction = selectedItem.Actions[_selectedActionIndex];
            var remainingActions = selectedItem.Actions.Count(a => a != selectedAction);

            string[] selectedActionDescriptionLines = Constants.GenerateReadableString(
                        selectedAction.Description,
                        Constants.invDescriptionWidth - 9 - selectedAction.Action.Length
                    );

            int width = Constants.invDescriptionWidth;

            int height = 4 + itemDescription.Length + selectedItem.Details.Count + selectedActionDescriptionLines.Length + remainingActions;

            int originX = lefty ? _items[_selectedItemIndex].Origin.X - Constants.invDescriptionWidth + itemInnerWidth + 2 : _items[_selectedItemIndex].Origin.X;

            Point descriptionBlockOrigin = new Point(originX, _items[_selectedItemIndex].EndY - 1);

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
                int valueLocation = selectedItem.Details.Max(d => d.Label.Length) + originX + 4;

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

                if (i == _selectedActionIndex)
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

            _itemDescription = new DisplayItem(descriptionBlockOrigin, new Size(Constants.invDescriptionWidth, cursorTop + 1));
        }

        #endregion

        #region Manipulation

        public void PreviousAction()
        {
            if (--_selectedActionIndex < 0)
            {
                _selectedActionIndex = SelectedItem.Actions.Count - 1;
            }

            DrawInventory();
        }

        public void NextAction()
        {
            if (++_selectedActionIndex >= SelectedItem.Actions.Count)
            {
                _selectedActionIndex = 0;
            }

            DrawInventory();
        }

        public void NextItem()
        {
            if (_selectedItemIndex >= _player.ItemCount - 1) return;
            ++_selectedItemIndex;
            DrawInventory();

        }

        public void PreviousItem()
        {
            if (_selectedItemIndex == 0) return;
            --_selectedItemIndex;
            DrawInventory();
        }

        public void DoSelectedInventoryAction()
        {
            // Do the action, check if needs to be destroyed
            // It seems kindof redundant passing in the arguments you use to call the function itself
            if (_player.Inventory[_selectedItemIndex].Actions[_selectedActionIndex].Act(_player, _selectedItemIndex))
            {
                
                _player.removeInventoryItem(_selectedItemIndex);

                if (_selectedItemIndex == _player.ItemCount && _selectedItemIndex != 0)
                    _selectedItemIndex--;

                DrawInventory();
            }

        }

        #endregion

        #region REMOVE

        public void inv_wipePopup(DisplayItem item)
        {
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
                    DrawItem(_player.Inventory[i], i == _selectedItemIndex ? Constants.SelectedItemBorderColor : Constants.ItemBorderColor, _items[i].Origin);
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

        #endregion
    }
}
