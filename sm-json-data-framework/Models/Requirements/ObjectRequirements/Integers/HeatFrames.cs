using sm_json_data_framework.Models.InGameStates;
using sm_json_data_framework.Models.Items;
using System;
using System.Collections.Generic;
using System.Text;

namespace sm_json_data_framework.Models.Requirements.ObjectRequirements.Integers
{
    /// <summary>
    /// A logical element which requires Samus to spend some frames in a heated room.
    /// </summary>
    public class HeatFrames : AbstractDamageNumericalValueLogicalElement
    {
        public HeatFrames()
        {

        }

        public HeatFrames(int frames)
        {
            Value = frames;
        }

        public override int CalculateDamage(SuperMetroidModel model, ReadOnlyInGameState inGameState, int times = 1, int previousRoomCount = 0)
        {
            int baseDamage = model.Rules.CalculateHeatDamage(inGameState, Value) * times;
            return (int)(baseDamage * model.LogicalOptions.HeatLeniencyMultiplier);
        }

        public override IEnumerable<Item> GetDamageReducingItems(SuperMetroidModel model, ReadOnlyInGameState inGameState)
        {
            return model.Rules.GetHeatDamageReducingItems(model, inGameState);
        }
    }
}
