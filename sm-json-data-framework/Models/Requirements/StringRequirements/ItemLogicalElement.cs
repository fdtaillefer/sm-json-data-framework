using sm_json_data_framework.Models.InGameStates;
using sm_json_data_framework.Models.Items;
using System;
using System.Collections.Generic;
using System.Text;

namespace sm_json_data_framework.Models.Requirements.StringRequirements
{

    public class ItemLogicalElement : AbstractStringLogicalElement
    {
        private Item Item { get; set; }

        public ItemLogicalElement(Item item)
        {
            Item = item;
        }

        public override bool IsFulfilled(SuperMetroidModel model, InGameState inGameState, int times = 1, bool usePreviousRoom = false)
        {
            return inGameState.HasItem(Item);
        }
    }
}
