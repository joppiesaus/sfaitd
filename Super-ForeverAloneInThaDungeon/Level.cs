using System;
using Super_ForeverAloneInThaDungeon.Levels;

namespace Super_ForeverAloneInThaDungeon
{
	class LevelPlanner
	{
        public RoomPlanner roomPlanner = new RoomPlanner();
        public CreatureSpawner creatureSpawner = new CreatureSpawner();

        public void UpdateSystems(uint floor)
        {
            roomPlanner.entries = new IRoomEntry[]
            {
                new RoomEntry<RoomTreasureRoom>(new Point(0, 30)),
                new RoomEntry<Room>(new Point(0, 0))
            };
        }
	}
}
