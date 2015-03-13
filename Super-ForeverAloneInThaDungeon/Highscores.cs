using System;
using System.IO;

namespace Super_ForeverAloneInThaDungeon
{
    static class Highscores
    {
        public static bool HasLoaded
        {
            get { return nHighscores != -1; }
        }

        const byte MAX_HIGHSCORES = 10;

        class Highscore
        {
            public string name;
            public int floor, level, money;

            public Highscore(string nm, int flr, int lvl, int mny)
            {
                this.name = nm;
                this.floor = flr;
                this.level = lvl;
                this.money = mny;
            }
            

            // load constructor
            public Highscore(ref BinaryReader reader)
            {
                name = reader.ReadString();
                floor = reader.ReadInt32();
                level = reader.ReadInt32();
                money = reader.ReadInt32();
            }

            public void Save(ref BinaryWriter writer)
            {
                writer.Write(name);
                writer.Write(floor);
                writer.Write(level);
                writer.Write(money);
            }

            /// <summary>
            /// Compares two highscores based on index
            /// </summary>
            /// <param name="n">Index of highscore in Highscores.highscores</param>
            /// <returns>If inserted</returns>
            public bool Compare(byte n)
            {
                if (Compare(highscores[n]))
                {
                    AddHighscore(n, this);
                    return true;
                }
                return false;
            }

            private bool Compare(Highscore h)
            {
                if (h.level < level)
                {
                    return true;
                }
                else if (h.level == level)
                {
                    /*if (h.floor > floor)
                    {
                        return true;
                    }
                    else if (h.floor == floor)
                    {*/
                        return h.money < money ? true : false;
                    //}
                }
                return false;
            }
        }

        static Highscore[] highscores = new Highscore[MAX_HIGHSCORES];
        static short nHighscores = -1;
        static short lastAddedItem = -1;

        /// <summary>
        /// Tries to add an highscore. Also saves when needed.
        /// </summary>
        /// <returns>If it was able to add the highscore</returns>
        public static bool Add(string name, int floor, int level, int money)
        {
            // Lazy init
            if (nHighscores == -1)
            {
                Load();
            }

            Highscore highscore = new Highscore(name, floor, level, money);

            bool did = false;

            for (byte i = 0; i < nHighscores; i++)
            {
                if (highscore.Compare(i))
                {
                    did = true;
                    break;
                }
            }

            if (did)
            {
                Save();
                return true;
            }
            else if (nHighscores < MAX_HIGHSCORES)
            {
                lastAddedItem = nHighscores;
                highscores[nHighscores++] = highscore;
                Save();
                return true;
            }
            return false;
        }

        static void AddHighscore(byte n, Highscore h)
        {
            if (nHighscores < MAX_HIGHSCORES)
            {
                for (byte i = (byte)nHighscores; i > n; )
                {
                    highscores[i] = highscores[--i];
                }
                nHighscores++;
            }
            else
            {
                for (byte i = MAX_HIGHSCORES - 1; i > n; )
                {
                    highscores[i] = highscores[--i];
                }
            }

            lastAddedItem = n;
            highscores[n] = h;
        }

        /// <summary>
        /// Displays highscores on screen
        /// </summary>
        /// <remarks>Call only when highscores are loaded</remarks>
        public static void Display()
        {
            Console.Clear();
            Console.BackgroundColor = ConsoleColor.Black;
            Console.ForegroundColor = ConsoleColor.Red;
            Console.Write("--- Highscores ---");

            byte longestName = 0;
            for (byte i = 0; i < nHighscores; i++)
            {
                if (highscores[i].name.Length > longestName) longestName = (byte)highscores[i].name.Length;
            }

            if (lastAddedItem == -1)
            {
                for (byte i = 0; i < nHighscores; i++)
                    Draw(i, longestName);
            }
            else
            {
                for (byte i = 0; i < lastAddedItem; i++)
                    Draw(i, longestName);

                Console.BackgroundColor = ConsoleColor.DarkBlue;
                Draw((byte)lastAddedItem, longestName);
                Console.BackgroundColor = ConsoleColor.Black;

                for (byte i = (byte)(lastAddedItem + 1); i < nHighscores; i++)
                    Draw(i, longestName);
            }

            Console.ForegroundColor = ConsoleColor.Gray;
            Console.SetCursorPosition(0, Console.BufferHeight - 1);
            Console.Write("-- Press any key to continue --");
            Console.ReadKey();
        }

        static void Draw(byte n, byte nameLength)
        {
            Console.CursorLeft = 0;
            Console.CursorTop++;

            int offset = 4;

            switch (n)
            {
                default:
                    Console.ForegroundColor = ConsoleColor.Gray;
                    break;
                case 0:
                    Console.ForegroundColor = ConsoleColor.Yellow;
                    break;
                case 1:
                    Console.ForegroundColor = ConsoleColor.Cyan;
                    break;
                case 2:
                    Console.ForegroundColor = ConsoleColor.DarkRed;
                    break;
            }
            Console.Write("{0}: ", n + 1);

            Console.CursorLeft = offset;
            offset += nameLength + 3;

            Console.ForegroundColor = ConsoleColor.Yellow;
            Console.Write(highscores[n].name);
            
            Console.CursorLeft = offset;
            Console.ForegroundColor = ConsoleColor.Red;
            Console.Write("lvl: " + highscores[n].level);

            offset += 10;
            Console.CursorLeft = offset;
            Console.ForegroundColor = ConsoleColor.Green;
            Console.Write("flr: " + highscores[n].floor);

            offset += 11;
            Console.CursorLeft = offset;
            Console.ForegroundColor = ConsoleColor.DarkYellow;
            Console.Write("mny: " + highscores[n].money);
        }

        /// <summary>
        /// Loads highscores
        /// </summary>
        public static void Load()
        {
            string path = Constants.EXE_DIR + Constants.HIGHSCORES_FILE_NAME;

            if (File.Exists(path))
            {
                FileStream fs = new FileStream(path, FileMode.Open, FileAccess.Read);
                if (fs.CanRead && fs.Length > 0)
                {
                    BinaryReader reader = new BinaryReader(fs);
                    nHighscores = reader.ReadInt16();

                    for (int i = 0; i < nHighscores; i++)
                    {
                        highscores[i] = new Highscore(ref reader);
                    }
                    reader.Close();
                }
                fs.Dispose();
            }

            // Error checking
            if (nHighscores == -1) nHighscores = 0;
        }

        /// <summary>
        /// Saves highscores
        /// </summary>
        /// <returns>If was able to save correctly</returns>
        /// <remarks>Call only when highscores are loaded</remarks>
        public static bool Save()
        {
            FileStream fs = new FileStream(Constants.EXE_DIR + Constants.HIGHSCORES_FILE_NAME, FileMode.Create, FileAccess.Write);
            if (fs.CanWrite)
            {
                BinaryWriter writer = new BinaryWriter(fs);
                writer.Write(nHighscores);

                for (int i = 0; i < nHighscores; i++)
                {
                    highscores[i].Save(ref writer);
                }
                writer.Close();
                fs.Dispose();
                return true;
            }
            fs.Dispose();
            return false;
        }
    }
}
