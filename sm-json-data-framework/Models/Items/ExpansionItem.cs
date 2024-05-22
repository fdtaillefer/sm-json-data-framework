using sm_json_data_framework.Models.Raw.Items;
using System;
using System.Collections.Generic;
using System.Text;

namespace sm_json_data_framework.Models.Items
{
    /// <summary>
    /// Represents an item that expands the available max amount of a resource.
    /// </summary>
    public class ExpansionItem : InGameItem
    {
        public ExpansionItem(UnfinalizedExpansionItem innerElement, Action<Item> mappingsInsertionCallback) : base(innerElement, mappingsInsertionCallback)
        {
            Resource = innerElement.Resource;
            ResourceAmount = innerElement.ResourceAmount;
        }

        /// <summary>
        /// The resource that this expansion item increases the capacity of.
        /// </summary>
        public RechargeableResourceEnum Resource { get; }

        /// <summary>
        /// The amount by which this item expands the capacity of its resource.
        /// </summary>
        public int ResourceAmount { get; }
    }

    public class UnfinalizedExpansionItem : UnfinalizedInGameItem
    {
        public RechargeableResourceEnum Resource { get; set; }

        public int ResourceAmount { get; set; }

        public UnfinalizedExpansionItem()
        {

        }

        public override ExpansionItem Finalize(ModelFinalizationMappings mappings)
        {
            return (ExpansionItem)base.Finalize(mappings);
        }

        public UnfinalizedExpansionItem(RawExpansionItem item): base(item)
        {
            Resource = item.Resource;
            ResourceAmount = item.ResourceAmount;
        }

        protected override ExpansionItem CreateFinalizedElement(UnfinalizedItem sourceElement, Action<Item> mappingsInsertionCallback, ModelFinalizationMappings mappings)
        {
            return new ExpansionItem((UnfinalizedExpansionItem)sourceElement, mappingsInsertionCallback);
        }
    }
}
