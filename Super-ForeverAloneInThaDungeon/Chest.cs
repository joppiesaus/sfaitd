using System;

namespace Super_ForeverAloneInThaDungeon
{
    class Chest : WorldObject
    {
        public InventoryItem[] contents;

        public Chest(InventoryItem[] cont = null)
        {
            this.tiletype = TileType.Chest;
            this.drawChar = Constants.chars[5];
            this.color = ConsoleColor.DarkYellow;
            this.walkable = false;

            this.contents = cont == null ? GenerateRandomLoot(Game.ran.Next(Game.ran.Next(0, 2), 4)) : cont;
        }

        protected override Tile GenerateDrop()
        {
            return contents.Length > 0 ? new ItemPickupable(contents[Game.ran.Next(0, contents.Length)]) : new Tile(TileType.Air);
        }

        public override void Kick()
        {
            // 1/3 chance to break and drop an random pickubable item on the ground
            if (Game.ran.Next(0, 3) == 0)
            {
                Game.Message("You kicked " + this.InlineName + ", it breaks!");
                destroyed = true;
            }
            else
            {
                Game.Message("You kicked " + this.InlineName);
            }
        }

        public static InventoryItem[] GenerateRandomLoot(int n)
        {
            InventoryItem[] items = new InventoryItem[n];

            for (int i = 0; i < n; i++)
            {
                switch (Game.ran.Next(0, 3))
                {
                    case 0:
                        items[i] = Constants.sword;
                        break;
                    case 1:
                        items[i] = Constants.spear;
                        break;
                    case 2:
                        items[i] = (new Scroll(SpellGenerator.GenerateMultiple())).GenerateInvItem();
                        break;
                }
            }

            return items;
        }
    }
}
