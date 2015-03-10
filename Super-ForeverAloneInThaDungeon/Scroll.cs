using System;
using System.Collections.Generic;
using Super_ForeverAloneInThaDungeon.Spells;

namespace Super_ForeverAloneInThaDungeon
{
    class EffectItem : InventoryItem
    {
        public SpellEffect[] effects;

        public EffectItem(string nameee, string desc, char[,] img, ConsoleColor clr, SpellEffect[] fx, InventoryAction[] addActions = null)
        {
            Name = nameee;
            Description = desc;
            Visual = img;
            Color = clr;
            effects = fx;

            Details = new List<ItemDetail>(fx.Length + 1);
            Details.Add(new ItemDetail("Effects", "", ConsoleColor.Magenta));

            for (int i = 0; i < fx.Length; i++)
            {
                Details.Add(fx[i].GetInventoryInfo());
            }

            if (addActions == null)
            {
                Actions = new List<InventoryAction>() { new InventoryActionCast(), new InventoryActionDrop() };
            }
            else
            {
                Actions = new List<InventoryAction>(addActions.Length + 1);

                for (int i = 0; i < addActions.Length; i++)
                {
                    Actions[i] = addActions[i];
                }

                Actions[addActions.Length] = new InventoryActionDrop();
            }
        }
    }

    class Scroll : Pickupable
    {
        SpellEffect[] effects;

        public Scroll(SpellEffect[] fx)
        {
            this.RepresentationInLight = Constants.chars[0];
            this.Type = TileType.Scroll;
            this.Color = ConsoleColor.Magenta;

            this.effects = fx;
        }

        public override InventoryItem getInvItem(ref Random ran)
        {
            string scrollName = ran.Next(0, 2) == 0 ? 
                string.Format("\"Scroll of The {0} {1}\"",
                    Constants.namefwords[ran.Next(Constants.namefwords.Length)],
                    Constants.nameswords[ran.Next(Constants.nameswords.Length)]
                )
                : string.Format("\"Scroll of {0}\"", Constants.namewords[ran.Next(Constants.namewords.Length)]
            );

            char[,] img = new char[,] { {' ','_','_','_','_','_','_','_','_','_','_',' ',' ',' ',' ',' ' },
	        { '(',')','_','_','_','_','_','_','_','_','_',')',' ',' ',' ',' ' },
	        { ' ','\\',' ',' ',' ',' ',' ',' ',' ',' ',' ',' ','\\',' ',' ',' ' },
	        { ' ',' ','\\',' ',' ',' ',' ',' ',' ',' ',' ',' ',' ','\\',' ',' ' },
	        { ' ',' ',' ','\\','_','_','_','_','_','_','_','_','_','_','\\',' ' },
	        { ' ',' ',' ','(',')','_','_','_','_','_','_','_','_','_','_',')'} };

            // Write random text in the scroll's Visual
            // THE NUMBERS MASON
            // WHAT DO THEY MEAN
            byte xOff = 3;
            for (byte y = 2; y < 4; y++)
            {
                int length = xOff + ran.Next(2, 9);
                for (int i = xOff; i < length; i++)
                    img[y, i] = Constants.rlangChars[ran.Next(0, Constants.rlangChars.Length)];

                if (length < xOff + 5)
                {
                    int i = length + 1;
                    length += ran.Next(2, 5);
                    for (; i < length; i++)
                        img[y, i] = Constants.rlangChars[ran.Next(0, Constants.rlangChars.Length)];
                }
                xOff++;
            }

            /*return new InventoryItem(scrollName, "This scroll can do magical stuff", img, ForegroundColor, new ItemDetail[] { new ItemDetail("Effects", "???", ConsoleColor.Magenta) },
                new InventoryAction[] { new InventoryActionCast(), new InventoryActionDrop() });*/
            return new EffectItem(scrollName, "",
                img, Color, effects);
        }

        

    }
}
