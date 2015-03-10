using System;
using System.Collections.Generic;
using System.Linq;
using Super_ForeverAloneInThaDungeon.Spells;

namespace Super_ForeverAloneInThaDungeon
{
    public class InventoryItem
    {
        public string Name { get; set; }
        public string Description { get; set; }
        public List<ItemDetail> Details { get; set; }
        public char[,] Visual { get; set; }
        public ConsoleColor Color;

        public List<InventoryAction> Actions;

        
        public InventoryItem() { }

        public InventoryItem(string name, string description, char[,] visual, ConsoleColor color,
            IEnumerable<ItemDetail> addInfo = null, IEnumerable<InventoryAction> invActions = null)
        {
            Name = name;
            Description = description;
            Visual = visual;
            Color = color;

            Details = addInfo == null ? new List<ItemDetail>() : addInfo.ToList();
            Actions = invActions == null ? new List<InventoryAction>() { new InventoryActionDrop() } : invActions.ToList();
        }
        
        public void AddAdditionalInfo(ItemDetail detail)
        {
            Details.Add(detail);
        }
    }

    public abstract class InventoryAction
    {
        public abstract string Action { get; }
        public abstract string Description { get; }

        protected int amnt;

        public InventoryAction(int amount = 0)
        {
            this.amnt = amount;
        }

        /// <summary>
        /// Do the action
        /// </summary>
        /// <param Name="p">Player</param>
        /// <returns>If the item needs to be destroyed</returns>
        public abstract bool Act(Player p, int itemIndex);
    }

    public class InventoryActionDrop : InventoryAction
    {
        public override string Action { get { return "Drop"; } }
        public override string Description { get { return "Drop this item"; } }

        public InventoryActionDrop() : base() { }

        public override bool Act( Player p, int itemIndex)
        {
            return true;
        }
    }

    class InventoryActionCast : InventoryAction
    {
        public override string Action { get { return "Cast"; } }
        public override string Description { get { return "Cast this spell"; } }

        public InventoryActionCast() : base() { }

        public override bool Act(Player p, int itemIndex)
        {
            SpellEffect[] effects = ((EffectItem)p.Inventory[itemIndex]).effects;

            Creature creature = (Creature)p;

            for (int i = 0; i < effects.Length; i++)
            {
                effects[i].Apply(ref creature);
            }

            return true;
        }
    }

    class InventoryActionYield : InventoryAction
    {
        public override string Action { get { return "Yield"; } }
        public override string Description { get { return "Hold this weapon in your hands"; } }

        public InventoryActionYield() : base() { }

        public override bool Act(Player p, int itemIndex)
        {
            if (((WeaponItem)p.Inventory[itemIndex]).weapon is Throwable)
            {
                if (p.rWeaponItem != null) p.addInventoryItem(p.rWeaponItem);
                p.rWeaponItem = (WeaponItem)p.Inventory[itemIndex];
            }
            else
            {
                if (p.mWeaponItem != null) p.addInventoryItem(p.mWeaponItem);
                p.mWeaponItem = (WeaponItem)p.Inventory[itemIndex];
            }

            Game.Message("now holding " + p.Inventory[itemIndex].Name + '!');
            return true;
        }
    }

    /*class InventoryActionHeal : InventoryAction
    {
        public override string Action { get { return "Heal"; } }
        public override string Description { get { return "Heal yourself " + amnt + " with this item"; } }

        public InventoryActionHeal(int amount) : base(amount) { }

        public override bool Act(ref Player p, int itemIndex)
        {
            int health = p.health + amnt;
            if (health > p.maxHealth) health = p.maxHealth;

            health -= p.health;

            p.health += health;

            Game.Message("you healed yourself " + health);
            return true;
        }
    }*/
}
