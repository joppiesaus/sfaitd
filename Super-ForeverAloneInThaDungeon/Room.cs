using System;

namespace Super_ForeverAloneInThaDungeon
{
    class Room
    {
        public string name = "";
        public Point where, end;

        public Room(Point w, Point e)
        {
            this.where = w;
            this.end = e;
        }

        public void appendName()
        {
            string dung = Constants.dungeonNameParts[Game.ran.Next(0, Constants.dungeonNameParts.Length)];

            if (Game.ran.Next(2) == 0)
                this.name = string.Format("{0} of The {1} {2}", dung, Constants.namefwords[Game.ran.Next(0, Constants.namefwords.Length)],
                    Constants.nameswords[Game.ran.Next(0, Constants.nameswords.Length)]);
            else if (Game.ran.Next(2) == 0)
            {
                if (dung[dung.Length - 1] != 's') dung += 's';
                this.name = string.Format("{0} of {1}", dung, Constants.namewords[Game.ran.Next(0, Constants.namewords.Length)]);
            }
            else
            {
                this.name = string.Format("The {0} {1}", Constants.namefwords[Game.ran.Next(0, Constants.namewords.Length)], dung);
            }
        }
    }
}
