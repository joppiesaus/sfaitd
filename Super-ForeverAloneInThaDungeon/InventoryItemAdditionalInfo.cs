using System;

namespace Super_ForeverAloneInThaDungeon
{
    // InventoryItemAdditionalInfo
    // This class gives extra information on the selected inventory item
    // IIAID("Hunger", "100%", ConsoleColor.Green, ConsoleColor.Yellow) creates in the inventory description (green text)Hunger: (yellow text)100%
    // IIAI("Stick made of Wood", ConsoleColor.Blue) creates in the inventory description (blue text)Stick made of Wood
    
    /// <summary>
    /// label: DerpEffect IV
    /// </summary>
    class IIAI
    {
        public string label;
        public ConsoleColor lColor;

        protected IIAI() { }
        public IIAI(string lbl, ConsoleColor lblColor = ConsoleColor.DarkCyan)
        {
            this.label = lbl;
            this.lColor = lblColor;
        }
    }

    /// <summary>
    /// Header: ------ Effects ------
    /// </summary>
    class IIAIH : IIAI
    {
        public IIAIH(string lbl, ConsoleColor lblColor = ConsoleColor.DarkCyan)
            :base(lbl, lblColor)
        { }
    }
    
    /// <summary>
    /// label, value: Hunger: 100%
    /// </summary>
    class IIAID : IIAI
    {
        public string value;
        public ConsoleColor vColor;

        public IIAID(string lbl, string val, ConsoleColor lblColor = ConsoleColor.DarkCyan, ConsoleColor vColor = ConsoleColor.White)
        {
            this.label = lbl;
            this.value = val;
            this.lColor = lblColor;
            this.vColor = vColor;
        }
    }
}
