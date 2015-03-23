using System;

namespace Super_ForeverAloneInThaDungeon
{
    // FUTURE: Add bias
    class RoomPlanner
    {
        interface IRoomEntry
        {
            Point Chance { get; }

            Room RoomToBuild();
        }

        class RoomEntry<T> : IRoomEntry where T : Room, new()
        {
            Point chance;
            public Point Chance { get { return chance; } }

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

        /*class RoomEntry
        {
            public Point chance;
            Room r;

            public RoomEntry(Point _chance, Room room)
            {
                this.chance = _chance;
                this.r = room;
            }

            public Room RoomToBuild()
            {
                return r.Clone();
            }
        }

        RoomEntry[] entries;

        public void Update(uint floor)
        {
            entries = new RoomEntry[] {
                new RoomEntry(new Point(1, 30), new TreasureRoom()),
                new RoomEntry(new Point(1, 1), new Room())
            };
        }*/

        IRoomEntry[] entries;

        public void Update(uint floor)
        {
            entries = new IRoomEntry[]
            {
                new RoomEntry<TreasureRoom>(new Point(1, 30)),
                new RoomEntry<Room>(new Point(1, 1))
            };
        }

        public Room GetRoom()
        {
            for (int i = 0; i < entries.Length - 1; i++)
            {
                if (Game.ran.Next(0, entries[i].Chance.Y) == entries[i].Chance.X)
                {
                    return entries[i].RoomToBuild();
                }
            }
            return entries[entries.Length - 1].RoomToBuild();
        }
    }
}
