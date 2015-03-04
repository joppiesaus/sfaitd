using System;

namespace Super_ForeverAloneInThaDungeon
{
    // Not sure if I had to do this...
    class Pickupable : Tile
    {
        public TileType replaceTile = TileType.Air;

        public Pickupable()
        {
            walkable = true;
        }

        public virtual InventoryItem getInvItem(ref Random ran)
        {
            return null;
        }
    }

    class Money : Pickupable
    {
        public short money;

        public Money(int value)
        {
            walkable = true;
            tiletype = TileType.Money;
            money = (short)value;
            drawChar = '*';
            color = ConsoleColor.Yellow;
        }
    }
}
