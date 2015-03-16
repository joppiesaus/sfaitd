﻿using System;

namespace Super_ForeverAloneInThaDungeon
{
    static class EventRegister
    {
        public static void RegisterAttack(Creature from, Creature to, int dmg)
        {
            Game.Message(string.Format("{0} {1} {2}", from.InlineName(), Constants.GetCreatureDamageInWords(dmg), to.InlineName()).CapitalizeFirstLetter());
        }

        public static void RegisterKill(WorldObject from, WorldObject to)
        {
            Game.Message(string.Format("{0} killed {1}", from.InlineName(), to.InlineName()).CapitalizeFirstLetter());
        }
        public static void RegisterDeath(WorldObject to, string by)
        {
            Game.Message(string.Format("{0} got killed by {1}!", to, by).CapitalizeFirstLetter()); 
        }

        public static void RegisterFire(string from, WorldObject to)
        {
            Game.Message(string.Format("{0} set {1} on fire!", from, to).CapitalizeFirstLetter());
        }
    }
}
