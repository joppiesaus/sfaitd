using System;
using Super_ForeverAloneInThaDungeon.Spells;

namespace Super_ForeverAloneInThaDungeon
{
    abstract class InventoryAction
    {
        public enum PreExecuteCommand
        {
            SelectDirection,
            CombineTwo,
            Select,
        }

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
        /// <param name="p">Player</param>
        /// <returns>If the item needs to be destroyed</returns>
        public abstract bool Act(ref Player p, int itemIndex);
    }

    class InventoryActionDrop : InventoryAction
    {
        public override string Action { get { return "Drop"; } }
        public override string Description { get { return "Drop this item"; } }

        public InventoryActionDrop() : base() { }

        public override bool Act(ref Player p, int itemIndex)
        {
            return true;
        }
    }

    class InventoryActionCast : InventoryAction
    {
        public override string Action { get { return "Cast"; } }
        public override string Description { get { return "Cast this spell"; } }

        public InventoryActionCast() : base() { }

        public override bool Act(ref Player p, int itemIndex)
        {
            SpellEffect[] effects = ((EffectItem)p.inventory[itemIndex]).effects;

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

        public override bool Act(ref Player p, int itemIndex)
        {
            if (((WeaponItem)p.inventory[itemIndex]).weapon is Throwable)
            {
                if (p.rWeaponItem != null) p.AddInventoryItem(p.rWeaponItem);
                p.rWeaponItem = (WeaponItem)p.inventory[itemIndex];
            }
            else
            {
                if (p.mWeaponItem != null) p.AddInventoryItem(p.mWeaponItem);
                p.mWeaponItem = (WeaponItem)p.inventory[itemIndex];
            }

            Game.Message("now holding " + p.inventory[itemIndex].name + '!');
            return true;
        }
    }


    class InventoryActionInteract : InventoryAction
    {
        public override string Action { get { return "Use"; } }
        public override string Description { get { return "Interact with something with this item"; } }

        public readonly PreExecuteCommand command;

        public InventoryActionInteract(PreExecuteCommand cmd)
            : base()
        {
            this.command = cmd;
        }

        public override bool Act(ref Player p, int itemIndex)
        {
            throw new InvalidOperationException("Method cannot be used in InventoryActionInteract, use Interact instead.");
        }

        public virtual bool Interact(ref Player p, int itemIndex, ref object target)
        {
            return ((ItemInventoryItem)p.inventory[itemIndex]).item.Interact(ref p, ref target);
        }
    }

    class InventoryActionSelect : InventoryAction
    {
        public override string Action { get { return "Select"; } }
        public override string Description { get { return "Select this item"; } }

        public override bool Act(ref Player p, int itemIndex)
        {
            throw new NotImplementedException("Do not use the method Act in InventoryActionSelect!");
        }
    }

    // Unfinished
    class InventoryActionCombine : InventoryActionInteract
    {
        public override string Action { get { return "Combine"; } }
        public override string Description { get { return "Combine two of the same type"; } }

        public Func<InventoryItem, bool> filterFunction;
    }
}
