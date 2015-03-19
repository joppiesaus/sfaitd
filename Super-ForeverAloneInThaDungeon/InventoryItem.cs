using System;

namespace Super_ForeverAloneInThaDungeon
{
    class InventoryItem
    {
        public string name, description;
        public IIAI[] extraInfo;
        public char[,] image;
        public ConsoleColor color;

        public InventoryAction[] actions;

        public InventoryItem() { }
        public InventoryItem(string name, string description, char[,] image, ConsoleColor color,
            IIAI[] addInfo = null, InventoryAction[] invActions = null)
        {
            this.name = name;
            this.description = description;
            this.image = image;
            this.color = color;
            this.extraInfo = addInfo;

            this.actions = invActions == null ? new InventoryAction[] { new InventoryActionDrop() } : invActions;
        }

        // adds more info to this item
        // DO NOT USE WHEN NOT NEEDED, because it will call array.resize.
        public void AddAdditionalInfo(IIAI info)
        {
            Array.Resize(ref extraInfo, extraInfo.Length + 1);
            extraInfo[extraInfo.Length - 1] = info;
        }
    }
}
