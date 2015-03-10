using System;

namespace Super_ForeverAloneInThaDungeon.Graphics
{
    public struct Size
    {
        public int Width { get; private set; }
        public int Height { get; private set; }

        public Size(int width, int height) : this()
        {
            if (width < 0)
            {
                    throw new ArgumentException(string.Format("Width shoud be > 0, actual {0}", width));
            }

            if (height < 0)
            {
                throw new ArgumentException(string.Format("Height shoud be > 0, actual {0}", height));
            }

            Width = width;
            Height = height;
        }
    }
}
