using System;

namespace Super_ForeverAloneInThaDungeon
{
    // Not sure if I had to do this...
    class Pickupable : Tile
    {
        public Tile replaceTile;

        public Pickupable()
        {
            walkable = true;
            this.replaceTile = new Tile(TileType.Air);
        }
        public Pickupable(Tile _replaceTile)
        {
            walkable = true;
            this.replaceTile = _replaceTile;
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
            this.replaceTile = new Tile(TileType.Air);
        }
        public ItemPickupable(InventoryItem _item)
        {
            this.drawChar = Constants.chars[8];
            this.replaceTile = new Tile(TileType.Air);
            this.item = _item;
        }

        public ItemPickupable(Tile _replaceTile)
        {
            this.drawChar = Constants.chars[8];
            this.replaceTile = _replaceTile;
        }
        public ItemPickupable(InventoryItem _item, Tile _replaceTile)
        {
            this.drawChar = Constants.chars[8];
            this.replaceTile = _replaceTile;
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
