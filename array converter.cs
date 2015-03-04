using System;
using System.IO;

namespace Super_ForeverAloneInThaDungeon
{
    class Acrabadabra
    {
        public Acrabadabra()
        {
            thinggy();
        }

        void thinggy()
        {
            Console.Clear();
            Console.WriteLine("Path?");
            Console.Write(Directory.GetCurrentDirectory() + "\\");
            string s = Console.ReadLine();
            string path = Directory.GetCurrentDirectory() + "\\" + s;
            string result = "";

            using (StreamReader read = new StreamReader(path)) result = read.ReadToEnd();

            string end = "new char[,] { {";
            bool first = true;

            foreach (char c in result.ToCharArray())
            {
                if (c == '\n')
                {
                    first = true;
                    end += " },";
                    end += Environment.NewLine + '\t';
                    end += "{ ";
                    continue;
                }
                else if (c == '\r') continue;
                else if (!first)
                {
                    end += ",";
                }
                else first = false;

                if (c == '\\') end += String.Format("'{0}\\'", c);
                else end += String.Format("'{0}'", c);
            }

            end += "} };";

            Console.Write(end);

            path = Directory.GetCurrentDirectory() + "\\" + s + "_output.txt";

            Console.WriteLine();
            Console.WriteLine("Saving to: " + path);

            if (!File.Exists(path)) using (File.Create(path)) { }
            using (StreamWriter write = new StreamWriter(path)) write.Write(end);

            Console.ReadLine();
        }
    }
}
