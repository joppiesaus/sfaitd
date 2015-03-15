using System;

namespace Super_ForeverAloneInThaDungeon
{
    class WorldObject : Tile
    {
        public bool destroyed = false;

        public virtual void SetOnFire()
        {
            destroyed = true;
            Game.Message("Your burned away " + tiletype.ToString() + '!');
        }
    }
}
