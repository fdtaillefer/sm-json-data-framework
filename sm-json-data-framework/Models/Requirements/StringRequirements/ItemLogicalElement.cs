using sm_json_data_framework.Models.InGameStates;
using sm_json_data_framework.Models.Items;
using sm_json_data_framework.Options;
using System;
using System.Collections.Generic;
using System.Text;

namespace sm_json_data_framework.Models.Requirements.StringRequirements
{
    /// <summary>
    /// A logical element that is fulfilled by previously obtaining an item.
    /// </summary>
    public class ItemLogicalElement : AbstractStringLogicalElement<UnfinalizedItemLogicalElement, ItemLogicalElement>
    {
        private UnfinalizedItemLogicalElement InnerElement { get; set; }

        public ItemLogicalElement(UnfinalizedItemLogicalElement innerElement, Action<ItemLogicalElement> mappingsInsertionCallback, ModelFinalizationMappings mappings)
            : base(innerElement, mappingsInsertionCallback)
        {
            InnerElement = innerElement;
            Item = innerElement.Item.Finalize(mappings);
        }

        /// <summary>
        /// The item that Samus must havew to fulfill this logical element.
        /// </summary>
        public Item Item { get; }

        public override bool IsNever()
        {
            return false;
        }

        protected override ExecutionResult ExecuteUseful(SuperMetroidModel model, ReadOnlyInGameState inGameState, int times = 1, int previousRoomCount = 0)
        {
            if (inGameState.Inventory.HasItem(Item))
            {
                // Clone the In-game state to fulfill method contract
                ExecutionResult result = new ExecutionResult(inGameState.Clone());
                result.AddItemsInvolved(new[] { Item });
                return result;
            }
            else
            {
                return null;
            }
        }

        protected override bool PropagateLogicalOptions(ReadOnlyLogicalOptions logicalOptions)
        {
            // Nothing in logical options can alter this
            return false;
        }

        protected override bool CalculateLogicallyNever()
        {
            // This is impossible if the item can never be used
            return Item.LogicallyNever;
        }

        protected override bool CalculateLogicallyAlways()
        {
            // This is always possible if the item can always be used
            return Item.LogicallyAlways;
        }

        protected override bool CalculateLogicallyFree()
        {
            return Item.LogicallyFree;
        }
    }

    public class UnfinalizedItemLogicalElement : AbstractUnfinalizedStringLogicalElement<UnfinalizedItemLogicalElement, ItemLogicalElement>
    {
        public UnfinalizedItem Item { get; set; }

        public UnfinalizedItemLogicalElement(UnfinalizedItem item)
        {
            Item = item;
        }

        protected override ItemLogicalElement CreateFinalizedElement(UnfinalizedItemLogicalElement sourceElement, Action<ItemLogicalElement> mappingsInsertionCallback,
            ModelFinalizationMappings mappings)
        {
            return new ItemLogicalElement(sourceElement, mappingsInsertionCallback, mappings);
        }

        public override bool IsNever()
        {
            return false;
        }
    }
}
