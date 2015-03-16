﻿using System;
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
            string scrollName = Game.ran.Next(0, 2) == 0 ? 
                string.Format("\"Scroll of The {0} {1}\"",
                    Constants.namefwords[Game.ran.Next(Constants.namefwords.Length)],
                    Constants.nameswords[Game.ran.Next(Constants.nameswords.Length)]
                )
                : string.Format("\"Scroll of {0}\"", Constants.namewords[Game.ran.Next(Constants.namewords.Length)]
            );

            char[,] img = new char[,] { {' ','_','_','_','_','_','_','_','_','_','_',' ',' ',' ',' ',' ' },
	        { '(',')','_','_','_','_','_','_','_','_','_',')',' ',' ',' ',' ' },
	        { ' ','\\',' ',' ',' ',' ',' ',' ',' ',' ',' ',' ','\\',' ',' ',' ' },
	        { ' ',' ','\\',' ',' ',' ',' ',' ',' ',' ',' ',' ',' ','\\',' ',' ' },
	        { ' ',' ',' ','\\','_','_','_','_','_','_','_','_','_','_','\\',' ' },
	        { ' ',' ',' ','(',')','_','_','_','_','_','_','_','_','_','_',')'} };

            // Write random text in the scroll's image
            // THE NUMBERS MASON
            // WHAT DO THEY MEAN
            byte xOff = 3;
            for (byte y = 2; y < 4; y++)
            {
                int length = xOff + Game.ran.Next(2, 9);
                for (int i = xOff; i < length; i++)
                    img[y, i] = Constants.rlangChars[Game.ran.Next(0, Constants.rlangChars.Length)];

                if (length < xOff + 5)
                {
                    int i = length + 1;
                    length += Game.ran.Next(2, 5);
                    for (; i < length; i++)
                        img[y, i] = Constants.rlangChars[Game.ran.Next(0, Constants.rlangChars.Length)];
                }
                xOff++;
            }

            /*return new InventoryItem(scrollName, "This scroll can do magical stuff", img, color, new IIAI[] { new IIAI("Effects", "???", ConsoleColor.Magenta) },
                new InventoryAction[] { new InventoryActionCast(), new InventoryActionDrop() });*/
            return new EffectItem(scrollName, "", img, color, effects);
        }

    }
}
