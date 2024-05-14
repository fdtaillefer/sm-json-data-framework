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
        public string Name { get { return InnerElement.Name; } }
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

        protected override bool ApplyLogicalOptionsEffects(ReadOnlyLogicalOptions logicalOptions)
        {
            // Logical options have no power here
            return false;
        }
    }
}
