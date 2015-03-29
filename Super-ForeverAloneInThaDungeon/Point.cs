namespace Super_ForeverAloneInThaDungeon
{
    struct Point
    {
        public int X, Y;

        public Point(int x = 0, int y = 0)
        {
            this.X = x;
            this.Y = y;
        }

        public bool Same(int x, int y)
        {
            return (X == x && Y == y);
        }
        public bool Same(Point p)
        {
            return (X == p.X && Y == p.Y);
        }

        public override string ToString()
        {
            return "Point { X: " + X + " Y: " + Y + " }";
        }

        public void Add(Point p)
        {
            X += p.X;
            Y += p.Y;
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

        /// <summary>
        /// Positions this displayitem in the middle of screen
        /// </summary>
        public void CenterScreen()
        {
            this.pos = new Point(System.Console.BufferWidth / 2 - width / 2, System.Console.BufferHeight / 2 - height / 2);
        }

        /// <summary>
        /// Positions this displayitem in the given dimensions(origin 0,0)
        /// </summary>
        /// <param name="wdt">width of dimension</param>
        /// <param name="hgt">height of dimension</param>
        public void Center(int wdt, int hgt)
        {
            this.pos = new Point(wdt / 2 - width / 2, hgt / 2 - height / 2);
        }

        public void Add(DisplayItem item)
        {
            if (pos.X > item.pos.X)
            {
                pos.X = item.pos.X;
                EndX = pos.X + width;
            }
            if (item.EndX > EndX)
            {
                EndX = item.EndX;
            }

            if (pos.Y > item.pos.Y)
            {
                pos.Y = item.pos.Y;
                EndY = pos.Y + height;
            }
            if (item.EndY > EndY)
            {
                EndY = item.EndY;
            }
        }
    }
}
