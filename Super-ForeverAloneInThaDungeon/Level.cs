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
                new CreatureEntry<Snake>(1, (int)(444 - Math.Log(floor * 8, 2) * 5), floor),
                new CreatureEntry<Goblin>(2, 900, floor),
                new CreatureEntry<Grunt>(1, 666, floor)
            };

            creatureSpawner.spawnEntries = new ICreatureEntry[]
            {
                new CreatureEntry<Snake>(0, 2, floor / 2),
                new CreatureEntry<Goblin>(0, 3, floor / 2),
                new CreatureEntry<Grunt>(0, 2, floor)
            };

            creatureSpawner.spawnRate = (ushort)(22 - Math.Max(Math.Log((Math.Max(floor - 1, 0) / 2.5f) * 8, 2) * 1.7, 0));
        }
	}
}
