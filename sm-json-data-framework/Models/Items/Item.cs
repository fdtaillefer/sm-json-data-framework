using sm_json_data_framework.Models.Enemies;
using sm_json_data_framework.Options;
using System;
using System.Collections.Generic;
using System.Text;

namespace sm_json_data_framework.Models.Items
{
    /// <summary>
    /// Represents an item, regardless of whether this is explicitly an item in the game or just implicitly an item.
    /// </summary>
    public class Item : AbstractModelElement<UnfinalizedItem, Item>
    {
        private UnfinalizedItem InnerElement { get; set; }

        public Item(UnfinalizedItem innerElement, Action<Item> mappingsInsertionCallback)
            : base(innerElement, mappingsInsertionCallback)
        {
            InnerElement = innerElement;
        }

        /// <summary>
        /// The unique name of this item.
        /// </summary>
        public string Name => InnerElement.Name;

        protected override bool PropagateLogicalOptions(ReadOnlyLogicalOptions logicalOptions)
        {
            // Logical options have no power here
            return false;
        }

        protected override void UpdateLogicalProperties()
        {
            base.UpdateLogicalProperties();
            LogicallyNever = CalculateLogicallyNever();
            LogicallyAlways = CalculateLogicallyAlways();
        }

        public override bool CalculateLogicallyRelevant()
        {
            // An item that can't be obtained may as well not exist
            return !CalculateLogicallyNever();
        }

        /// <summary>
        /// If true, then this item is impossible to use given the current logical options, regardless of in-game state.
        /// </summary>
        public bool LogicallyNever { get; private set; }

        /// <summary>
        /// Calculates what the value of <see cref="LogicallyNever"/> should currently be.
        /// </summary>
        /// <returns></returns>
        protected bool CalculateLogicallyNever()
        {
            // Logical options can't currently take away items
            return false;
        }

        /// <summary>
        /// If true, then this item is always possible to use given the current logical options, regardless of in-game state.
        /// </summary>
        public bool LogicallyAlways { get; private set; }

        /// <summary>
        /// Calculates what the value of <see cref="LogicallyAlways"/> should currently be.
        /// </summary>
        /// <returns></returns>
        protected bool CalculateLogicallyAlways()
        {
            // Item is always usable if the game always starts with it
            return AppliedLogicalOptions.StartConditions.StartingInventory.HasItem(this);
        }

        /// <summary>
        /// If true, not only is this item always available given the current logical options, regardless of in-game state,
        /// but that availability is also guaranteed to cost no resources.
        /// </summary>
        public bool LogicallyFree => LogicallyAlways; // Just owning an item can never cost resources
    }

    public class UnfinalizedItem: AbstractUnfinalizedModelElement<UnfinalizedItem, Item>
    {
        public string Name { get; set; }

        public UnfinalizedItem() { }

        public UnfinalizedItem (string name)
        {
            Name = name;
        }

        protected override Item CreateFinalizedElement(UnfinalizedItem sourceElement, Action<Item> mappingsInsertionCallback, ModelFinalizationMappings mappings)
        {
            return new Item(sourceElement, mappingsInsertionCallback);
        }
    }
}
