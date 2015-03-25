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

        public virtual InventoryItem GenerateInvItem()
        {
            return null;
        }
    }

    class ItemPickupable : Pickupable
    {
        public InventoryItem item;

        public ItemPickupable()
        {
            this.drawChar = Constants.chars[8];
            this.replaceTile = TileType.Air;
        }
        public ItemPickupable(InventoryItem _item)
        {
            this.drawChar = Constants.chars[8];
            this.replaceTile = TileType.Air;
            this.item = _item;
        }

        public override InventoryItem GenerateInvItem()
        {
            return item;
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
