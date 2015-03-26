using System;

namespace Super_ForeverAloneInThaDungeon.Levels
{
    interface ICreatureEntry
    {
        Point Chance { get; }

        Creature CreatureToSpawn();
    }

    class CreatureEntry<T> : ICreatureEntry where T : Creature, new()
    {
        Point chance;
        public Point Chance { get { return chance; } } // 0 is a chance too!

        public CreatureEntry(Point _chance)
        {
            this.chance = _chance;
        }
        public CreatureEntry(int chance, int outof)
        {
            this.chance = new Point(chance, outof);
        }

        public Creature CreatureToSpawn()
        {
            return new T();
        }
    }

    class CreatureSpawner
    {
        public ICreatureEntry[] initialEntries;
        public ICreatureEntry[] spawnEntries;

        public ushort spawnRate = 10;
        ushort currentRate = 0;

        public void SprinkleSpawn(Room[] rooms, ref Tile[,] tiles)
        {
            // Do you even nest bro
            // (oh man sooo ineffecieentt)
            for (int i = 0; i < rooms.Length; i++)
                for (int x = rooms[i].where.X + 1; i < rooms[i].end.X; x++)
                    for (int y = rooms[i].where.Y + 1; i < rooms[i].end.Y; y++)
                        if (tiles[x, y].walkable)
                            for (int j = 0; j < initialEntries.Length; j++)
                                if (Game.ran.Next(0, initialEntries[j].Chance.Y) <= initialEntries[j].Chance.X)
                                {
                                    Tile t = tiles[x, y];
                                    Creature c = initialEntries[j].CreatureToSpawn();
                                    c.lastTile = t;
                                    tiles[x, y] = c;
                                    break;
                                }
        }

        public void Update(ref Tile[,] tiles)
        {
            if (++currentRate >= spawnRate)
            {

                currentRate = 0;
            }
        }
    }
}
