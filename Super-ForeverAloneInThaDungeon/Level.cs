using System;
using Super_ForeverAloneInThaDungeon.Levels;

namespace Super_ForeverAloneInThaDungeon
{
	class LevelPlanner
	{
        public RoomPlanner roomPlanner = new RoomPlanner();
        public CreatureSpawner creatureSpawner = new CreatureSpawner();

        public void UpdateSystems(int floor)
        {
            roomPlanner.entries = new IRoomEntry[]
            {
                new RoomEntry<RoomTreasureRoom>(new Point(0, 30)),
                new RoomEntry<Room>(new Point(0, 0))
            };

            creatureSpawner.initialEntries = new ICreatureEntry[]
            {
                new CreatureEntry<Snake>(1, 400, floor),
                new CreatureEntry<Goblin>(2, 900, floor),
                new CreatureEntry<Grunt>(1, 666, floor)
            };

            creatureSpawner.spawnEntries = new ICreatureEntry[]
            {
                new CreatureEntry<Snake>(1, 4, floor / 2),
                new CreatureEntry<Goblin>(1, 5, floor / 2),
                new CreatureEntry<Grunt>(1, 4, floor)
            };

            creatureSpawner.spawnRate = (ushort)(21 - Math.Log((Math.Max(floor, 0) - 1) * 9, 2) * 1.8);
        }
	}
}
