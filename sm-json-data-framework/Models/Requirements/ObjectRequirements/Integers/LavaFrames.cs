using sm_json_data_framework.Models.InGameStates;
using sm_json_data_framework.Models.Items;
using System;
using System.Collections.Generic;
using System.Text;

namespace sm_json_data_framework.Models.Requirements.ObjectRequirements.Integers
{
    /// <summary>
    /// A logical element which requires Samus to spend some frames in lava.
    /// </summary>
    public class LavaFrames : AbstractDamageNumericalValueLogicalElement
    {
        public override int CalculateDamage(SuperMetroidModel model, InGameState inGameState, int times = 1, int previousRoomCount = 0)
        {
            int baseDamage = model.Rules.CalculateLavaDamage(inGameState, Value) * times;
            return (int)(baseDamage * model.LogicalOptions.LavaLeniencyMultiplier);
        }

        public override IEnumerable<Item> GetDamageReducingItems(SuperMetroidModel model, InGameState inGameState)
        {
            return model.Rules.GetLavaDamageReducingItems(model, inGameState);
        }
    }
}
