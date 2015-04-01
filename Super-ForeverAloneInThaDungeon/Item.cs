using System;

namespace Super_ForeverAloneInThaDungeon
{
    class ItemInventoryItem : InventoryItem
    {
        public Item item;

        public ItemInventoryItem(Item _item, string desc, char[,] img, ConsoleColor clr = ConsoleColor.Gray)
        {
            this.item = _item;
            this.name = _item.Name;
            this.description = desc;
            this.image = img;
            this.color = clr;

            this.actions = new InventoryAction[] { new InventoryActionInteract(item.Command), new InventoryActionDrop() };
        }
    }

	abstract class Item
	{
        public abstract string Name { get; }
        public virtual InventoryAction.PreExecuteCommand Command { get { return InventoryAction.PreExecuteCommand.SelectDirection; } }

        /// <summary>
        /// Make this item interact with stuff.
        /// </summary>
        /// <param name="obj">Target creature/object</param>
        /// <returns>If it should be deleted from the inventory</returns>
        public abstract bool Interact(ref Player p, ref object obj);
	}


    // This item won't be in-game in "the endproduct" ofcourse...
    // But it'll always be hidden in the RAM!
    class SwedishMatches : Item
    {
        public override string Name { get { return "Säkerhets Tändstickor"; } }

        public override bool Interact(ref Player p, ref object obj)
        {
            if (obj is WorldObject)
            {
                WorldObject target = (WorldObject)obj;
                Tile t = (Tile)obj;
                target.Drop(ref t);
                obj = t;
                Game.Message("Look what you've done! You burned away the whole " + target.tiletype + '!');
            }
            else
            {
                Game.Message(obj.ToString() + " is not burnable! You wasted one! You silly!");
            }
            return false;
        }
    }
}
