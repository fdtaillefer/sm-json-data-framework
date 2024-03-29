using sm_json_data_framework.Models.InGameStates;
using sm_json_data_framework.Models.Items;
using System;
using System.Collections.Generic;
using System.Text;

namespace sm_json_data_framework.Models.Requirements.StringRequirements
{
    /// <summary>
    /// A logical element that is fulfilled by previously obtaining an item.
    /// </summary>
    public class ItemLogicalElement : AbstractStringLogicalElement
    {
        private Item Item { get; set; }

        public ItemLogicalElement(Item item)
        {
            Item = item;
        }

        public override ExecutionResult Execute(SuperMetroidModel model, InGameState inGameState, int times = 1, int previousRoomCount = 0)
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
    }
}
