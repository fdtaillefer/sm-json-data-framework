using sm_json_data_framework.Models.Raw.Items;
using System;
using System.Collections.Generic;
using System.Text;

namespace sm_json_data_framework.Models.Items
{
    /// <summary>
    /// Represents an item that is explicitly identified as such in the game.
    /// </summary>
    public class InGameItem : Item
    {
        private UnfinalizedInGameItem InnerElement { get; set; }

        public InGameItem(UnfinalizedInGameItem innerElement, Action<Item> mappingsInsertionCallback) : base(innerElement, mappingsInsertionCallback)
        {
            InnerElement = InnerElement;
        }

        /// <summary>
        /// Hex value that represents this item.
        /// </summary>
        public string Data { get { return InnerElement.Data; } }
    }

    public class UnfinalizedInGameItem : UnfinalizedItem
    {
        public string Data { get; set; }

        public UnfinalizedInGameItem()
        {

        }

        public UnfinalizedInGameItem(RawInGameItem item): base(item.Name)
        {
            Data = item.Data;
        }

        public override InGameItem Finalize(ModelFinalizationMappings mappings)
        {
            return (InGameItem) base.Finalize(mappings);
        }

        protected override InGameItem CreateFinalizedElement(UnfinalizedItem sourceElement, Action<Item> mappingsInsertionCallback, ModelFinalizationMappings mappings)
        {
            return new InGameItem((UnfinalizedInGameItem)sourceElement, mappingsInsertionCallback);
        }
    }
}
