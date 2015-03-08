namespace Super_ForeverAloneInThaDungeon
{
    public struct Point
    {
        public int X;
        public int Y;

        public Point(int x = 0, int y = 0)
        {
            X = x;
            Y = y;
        }

        // TODO : override equals instead
        public bool Same(int x, int y)
        {
            return (X == x && Y == y);
        }

        public override string ToString()
        {
            return "Point { X: " + X + " Y: " + Y + " }";
        }
    }

    public class DisplayItem
    {
        public Point Position { get; set; }
        public short Width { get; set; }
        public short Height { get; set; }

        public Point End
        {
            get { return new Point(Position.X + Width, Position.Y + Height); }
        }
        public int EndX
        {
            get { return Position.X + Width; }
            set { Width = (short)(value - Position.X); }
        }
        public int EndY
        {
            get { return Position.Y + Height; }
            set { Height = (short)(value - Position.Y); }
        }

        public DisplayItem(Point p, short w, short h)
        {
            Position = p;
            Width = w;
            Height = h;
        }
        public DisplayItem(Point begin, Point end)
        {
            Position = begin;

            Width = (short)(end.X - begin.X);
            Height = (short)(end.Y - begin.Y);
        }

        public void CenterScreen()
        {
            Position = new Point(System.Console.BufferWidth / 2 - Width / 2, System.Console.BufferHeight / 2 - Height / 2);
        }

        public void Center(int wdt, int hgt)
        {
            Position = new Point(wdt / 2 - Width / 2, hgt / 2 - Height / 2);
        }
    }
}
