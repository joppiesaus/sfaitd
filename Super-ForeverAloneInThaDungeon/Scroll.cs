using System;
using Super_ForeverAloneInThaDungeon.Spells;

namespace Super_ForeverAloneInThaDungeon
{
    class EffectItem : InventoryItem
    {
        public SpellEffect[] effects;

        public EffectItem(string _name, string desc, char[,] img, ConsoleColor clr, SpellEffect[] fx = null, InventoryAction[] addActions = null)
        {
            this.name = _name;
            this.description = desc;
            this.image = img;
            this.color = clr;

            this.effects = fx;

            extraInfo = new IIAI[fx.Length + 1];
            extraInfo[0] = new IIAIH("Effects", ConsoleColor.Magenta);

            for (int i = 0; i < fx.Length; i++)
            {
                extraInfo[i + 1] = fx[i].GenerateInventoryInfo();
            }

            if (addActions == null)
            {
                this.actions = new InventoryAction[] { new InventoryActionCast(), new InventoryActionDrop() };
            }
            else
            {
                this.actions = new InventoryAction[addActions.Length + 1];

                for (int i = 0; i < addActions.Length; i++)
                {
                    actions[i] = addActions[i];
                }

                this.actions[addActions.Length] = new InventoryActionDrop();
            }
        }
    }

    class Scroll : Pickupable
    {
        SpellEffect[] effects;

        public Scroll(SpellEffect[] fx = null)
        {
            this.drawChar = Constants.chars[0];
            this.tiletype = TileType.Scroll;
            this.color = ConsoleColor.Magenta;

            this.effects = fx;
        }

        public override InventoryItem GenerateInvItem()
        {
            return new EffectItem("Scroll of " + Constants.GenerateRandomName(), "Hurr durr im a scrol", Constants.GenerateRandomScrollImage(), color, effects);
        }

    }
}
