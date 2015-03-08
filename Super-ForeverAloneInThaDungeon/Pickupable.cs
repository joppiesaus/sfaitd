using System;

namespace Super_ForeverAloneInThaDungeon
{
    // Not sure if I had to do this...
    class Pickupable : Tile
    {
        public TileType replaceTile = TileType.Air;

        public Pickupable()
        {
            Walkable = true;
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
            Walkable = true;
            Type = TileType.Money;
            money = (short)value;
            RepresentationInLight = '*';
            Color = ConsoleColor.Yellow;
        }
    }
}
