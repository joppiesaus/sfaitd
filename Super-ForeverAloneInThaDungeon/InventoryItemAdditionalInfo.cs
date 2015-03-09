using System;

namespace Super_ForeverAloneInThaDungeon
{
    public class ItemDetail
    {
        public string Label { get; set; }
        public string Value { get; set; }
        public ConsoleColor LabelColor { get; set; }
        public ConsoleColor ValueColor { get; set; }
        
        public ItemDetail(string label, string value, ConsoleColor labelColor = ConsoleColor.DarkCyan, ConsoleColor valueColor = ConsoleColor.White)
        {
            Label = label;
            Value = value;
            LabelColor = labelColor;
            ValueColor = valueColor;
        }
    }
}
