namespace Super_ForeverAloneInThaDungeon
{
    struct Point
    {
        public int X;
        public int Y;

        public Point(int x = 0, int y = 0)
        {
            this.X = x;
            this.Y = y;
        }

        public bool same(int x, int y)
        {
            return (X == x && Y == y);
        }

        public override string ToString()
        {
            return "Point { X: " + X + " Y: " + Y + " }";
        }
    }

    class DisplayItem
    {
        public Point pos;
        public short width, height;

        public Point End
        {
            get { return new Point(pos.X + width, pos.Y + height); }
        }
        public int EndX
        {
            get { return pos.X + width; }
            set { width = (short)(value - pos.X); }
        }
        public int EndY
        {
            get { return pos.Y + height; }
            set { height = (short)(value - pos.Y); }
        }

        public DisplayItem(Point p, short w, short h)
        {
            this.pos = p;
            this.width = w;
            this.height = h;
        }
        public DisplayItem(Point begin, Point end)
        {
            this.pos = begin;

            this.width = (short)(end.X - begin.X);
            this.height = (short)(end.Y - begin.Y);
        }

        public void CenterScreen()
        {
            this.pos = new Point(System.Console.BufferWidth / 2 - width / 2, System.Console.BufferHeight / 2 - height / 2);
        }

        public void Center(int wdt, int hgt)
        {
            this.pos = new Point(wdt / 2 - width / 2, hgt / 2 - height / 2);
        }
    }
}
