using System;

namespace Super_ForeverAloneInThaDungeon.Levels
{
    interface IRoomEntry
    {
        Point Chance { get; }

        Room RoomToBuild();
    }

    class RoomEntry<T> : IRoomEntry where T : Room, new()
    {
        Point chance;
        public Point Chance { get { return chance; } } // 0 is a chance too!

        public RoomEntry(Point _chance)
        {
            this.chance = _chance;
        }
        public RoomEntry(int chance, int outof)
        {
            this.chance = new Point(chance, outof);
        }

        public Room RoomToBuild()
        {
            return new T();
        }
    }

    // FUTURE: Add bias
    class RoomPlanner
    {
        public IRoomEntry[] entries;

        public Room GetRoom()
        {
            for (int i = 0; i < entries.Length - 1; i++)
            {
                if (Game.ran.Next(0, entries[i].Chance.Y) <= entries[i].Chance.X)
                {
                    return entries[i].RoomToBuild();
                }
            }
            return entries[entries.Length - 1].RoomToBuild();
        }
    }
}
