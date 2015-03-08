namespace Super_ForeverAloneInThaDungeon.Graphics
{
    public struct Size
    {
        public int Width { get; private set; }
        public int Height { get; private set; }

        public Size(int width, int height) : this()
        {
            Width = width;
            Height = height;
        }
    }
}
