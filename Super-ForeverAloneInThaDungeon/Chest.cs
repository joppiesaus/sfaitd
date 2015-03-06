using System;

namespace Super_ForeverAloneInThaDungeon
{
    class Chest : Tile
    {
        public InventoryItem[] contents;

        public Chest(ref Random ran, InventoryItem[] cont = null)
        {
            this.tiletype = TileType.Chest;
            this.drawChar = Constants.chars[5];
            this.color = ConsoleColor.DarkYellow;
            this.walkable = true;

            this.contents = cont == null ? GenerateRandomLoot(ref ran, ran.Next(ran.Next(0, 2), 4)) : cont;
        }


        public static InventoryItem[] GenerateRandomLoot(ref Random ran, int n)
        {
            InventoryItem[] items = new InventoryItem[n];

            for (int i = 0; i < n; i++)
            {
                switch (ran.Next(0, 3))
                {
                    case 0:
                        items[i] = Constants.dagger;
                        break;
                    case 1:
                        items[i] = Constants.spear;
                        break;
                    case 2:
                        items[i] = (new Scroll(SpellGenerator.GenerateMultiple(ref ran))).getInvItem(ref ran);
                        break;
                }
            }

            return items;
        }
    }
}
