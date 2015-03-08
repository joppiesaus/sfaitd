using System;
using Super_ForeverAloneInThaDungeon.Graphics;

namespace Super_ForeverAloneInThaDungeon
{
    public enum TileType
    {
        None,
        Air,
        VerticalWall,
        HorizontalWall,
        Money,
        Player,
        Corridor,
        Torch,
        Scroll,
        Snake,
        Goblin,
        Wizzard,
        Chest,
        Up,
        Down
    }

    public class Tile
    {
        public char RepresentationInLight { get; set; }

        public bool Walkable { get; set; }
        public bool Lighten { get; set; }
        public bool Discovered { get; set; }
        public bool NeedsRefresh { get; set; }

        protected ConsoleColor Color { get; set; }
        public TileType Type { get; set; }

        public const char DefaultTileInInFogCharacter = '.';
        public char ReprensentationInDark = DefaultTileInInFogCharacter;

        public ConsoleColor CurrentColor
        {
            get { return Lighten ? Color : ConsoleColor.DarkGray; }
        }

        public char CurrentRepresentation
        {
            get { return Lighten ? RepresentationInLight : ReprensentationInDark; }
        }

        public Tile()
        {
            NeedsRefresh = true;
        }

        public Tile(TileType type)
        {
            SetTileType(type);
        }

        public void SetTileType(TileType type)
        {
            NeedsRefresh = true;
            Type = type;

            switch (type)
            {
                default:
                    RepresentationInLight = ' ';
                    break;
                case TileType.None:
                    RepresentationInLight = ReprensentationInDark = ' ';
                    break;
                case TileType.Air:
                    Walkable = true;
                    RepresentationInLight = '.';
                    ReprensentationInDark = '.';
                    Color = ConsoleColor.Gray; 
                    break;
                case TileType.VerticalWall:
                    ReprensentationInDark = RepresentationInLight = Constants.yWall;
                    Color = ConsoleColor.Gray; 
                    break;
                case TileType.HorizontalWall:
                    ReprensentationInDark = RepresentationInLight = Constants.xWall;
                    Color = ConsoleColor.Gray; 
                    break;
                case TileType.Corridor:
                    RepresentationInLight = ReprensentationInDark = '#';
                    Color = ConsoleColor.White;
                    Walkable = true; 
                    break;
                case TileType.Up:
                    RepresentationInLight = Constants.chars[1];
                    Color = ConsoleColor.DarkCyan;
                    Walkable = true; 
                    break;
                case TileType.Down:
                    RepresentationInLight = Constants.chars[2];
                    Color = ConsoleColor.Green;
                    Walkable = true; 
                    break;
            }
        }

        public void Draw(Drawer drawer, int x, int y)
        {
            if (!NeedsRefresh) return;
            if (!Discovered) return;
            
            drawer.Draw(CurrentRepresentation, CurrentColor, new Point(x, y));

            NeedsRefresh = false;
        }

        public override string ToString()
        {
            return RepresentationInLight.ToString();
        }
    }
}
