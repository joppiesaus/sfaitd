using System;

namespace Super_ForeverAloneInThaDungeon
{
    class CorridorConstruct
    {
        public Point origin;
        public byte length, direction;
        public sbyte beginIsDungeon = -1, endIsDungeon = -1; // Directions relative to the dungeon wall(0 for up of the room). -1 for nothing.
    }

    class Dimension2D
    {
        public Point where, end;
        public Dimension2D() { }
        public Dimension2D(Point w, Point e) { where = w; end = e; }
    }

    partial class Game
    {
        // used for Breadth-First Search (Node)
        class BFSN
        {
            public int x, y;
            public ushort score;

            public BFSN(int xp, int yp, ushort tileScore)
            {
                this.x = xp;
                this.y = yp;
                this.score = tileScore;
            }
        }

        class LineWriter
        {
            // POD + constructor
            class Node
            {
                public ushort x, prevLength;

                public Node(ushort posX)
                {
                    x = posX;
                }
            }

            Node[] nodes;


            // last element will stick to right border
            public LineWriter(ushort[] locations)
            {
                this.nodes = new Node[locations.Length];

                for (int i = 0; i < nodes.Length; i++)
                    nodes[i] = new Node(locations[i]);
            }

            public void Draw(string[] data)
            {                
                // print all the stuff at locations
                for (int i = 0; i < data.Length - 1; i++)
                {
                    Console.CursorLeft = nodes[i].x;
                    if (nodes[i].prevLength > data[i].Length)
                    {
                        Console.Write(data[i]);
                        for (int a = 0; a < nodes[i].prevLength - data[i].Length; a++) Console.Write(' ');
                    }
                    else
                    {
                        Console.Write(data[i]);
                    }

                    nodes[i].prevLength = (ushort)data[i].Length;
                }

                // last one sticks to right border
                byte n = (byte)(data.Length - 1);

                if (nodes[n].prevLength > data[n].Length)
                {
                    Console.CursorLeft = Console.WindowWidth - nodes[n].prevLength;
                    for (int a = 0; a < nodes[n].prevLength - data[n].Length; a++) Console.Write(' ');
                    Console.Write(data[n]);
                }
                else
                {
                    Console.CursorLeft = Console.WindowWidth - data[n].Length;
                    Console.Write(data[n]);
                }

                nodes[n].prevLength = (ushort)data[n].Length;
            }
        }
    }
}
