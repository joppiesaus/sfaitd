using System.Net.NetworkInformation;
using System.Runtime.CompilerServices;

namespace Super_ForeverAloneInThaDungeon.Graphics
{
    public struct Point
    {
        public int X;
        public int Y;

        public static Point Origin { get { return new Point(0, 0);} }

        public Point(int x, int y)
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

        public Point AddX(int x)
        {
            return new Point(X + x, Y);
        }

        public Point AddY(int y)
        {
            return new Point(X, Y + y);
        }

    }

    public class DisplayItem
    {
        public Point Origin { get; set; }
        public Size Size { get; set; }
        
        public int Width { get { return Size.Width; } }
        public int Height { get { return Size.Height; } }

        public Point End
        {
            get { return new Point(Origin.X + Width, Origin.Y + Height); }
        }

        public int EndX
        {
            get { return Origin.X + Width; }
            
        }

        public int EndY
        {
            get { return Origin.Y + Height; }
        }

        public DisplayItem(Point origin, int width, int height)
        {
            Origin = origin;
            Size = new Size(width, height);
            
        }

        public DisplayItem(Point origin, Size size)
            : this(origin, origin.AddX(size.Width).AddY(size.Height))
        {
        }

        public DisplayItem(Point begin, Point end)
        {
            Origin = begin;
            Size = new Size((end.X - begin.X), (end.Y - begin.Y));
        }

        public void CenterScreen()
        {
            Origin = new Point(System.Console.BufferWidth / 2 - Width / 2, System.Console.BufferHeight / 2 - Height / 2);
        }

        public void Center(int wdt, int hgt)
        {
            Origin = new Point(wdt / 2 - Width / 2, hgt / 2 - Height / 2);
        }
    }
}
