using System;
using System.Diagnostics;
using System.Linq;
using System.Text;
using Super_ForeverAloneInThaDungeon.Display;

namespace Super_ForeverAloneInThaDungeon
{
    partial class Game
    {
        // used for Breadth-First Search (Node)
        public class BFSN
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

        public class Node
        {
            public ushort StartPosition { get; set; }
            public ushort PreviousLength { get; set; }

            public Node(ushort startPosition)
            {
                StartPosition = startPosition;
            }
        }

        public class LineWriter
        {
            private readonly Node[] _nodes;
            
            public LineWriter(ushort[] locations)
            {
                _nodes = locations.Select(l => new Node(l)).ToArray();
            }

            public void Draw(int position, string[] data)
            {
                char[] buffer = new char[Console.WindowWidth];
                int bufferIndex;

                for (int i = 0; i < Console.WindowWidth; i++)
                {
                    buffer[i] = ' ';
                }

                for (int i = 0; i < data.Length - 1; i++)
                {
                    bufferIndex = _nodes[i].StartPosition;
                    if (_nodes[i].PreviousLength > data[i].Length)
                    {
                        foreach (var content in data[i])
                        {
                            buffer[bufferIndex++] = content;    
                        }
                        
                        for (int a = 0; a < _nodes[i].PreviousLength - data[i].Length; a++)
                        {
                            buffer[bufferIndex++] = ' ';
                        }
                    }
                    else
                    {
                        foreach (var content in data[i])
                        {
                            buffer[bufferIndex++] = content;
                        }
                    }

                    _nodes[i].PreviousLength = (ushort)data[i].Length;
                }

                byte n = (byte)(data.Length - 1);

                if (_nodes[n].PreviousLength > data[n].Length)
                {
                    bufferIndex = buffer.Length - _nodes[n].PreviousLength;
                    for (int a = 0; a < _nodes[n].PreviousLength - data[n].Length; a++)
                    {
                        buffer[bufferIndex++] = ' ';
                    }

                    foreach (var content in data[n])
                    {
                        buffer[bufferIndex++] = content;
                    }
                }
                else
                {
                    bufferIndex = buffer.Length - data[n].Length;

                    foreach (var content in data[n])
                    {
                        buffer[bufferIndex++] = content;
                    }
                }
                
                _nodes[n].PreviousLength = (ushort)data[n].Length;

                string line = new string(buffer);
                
                // TODO : Use drawer instead of calling screen directly
                Screen.Write(line, ConsoleColor.White, ConsoleColor.DarkBlue, position);
            }
        }
    }
}
