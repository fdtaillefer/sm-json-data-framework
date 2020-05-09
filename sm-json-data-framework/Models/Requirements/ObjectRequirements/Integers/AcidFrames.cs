using sm_json_data_framework.Models.InGameStates;
using sm_json_data_framework.Models.Items;
using System;
using System.Collections.Generic;
using System.Text;

namespace sm_json_data_framework.Models.Requirements.ObjectRequirements.Integers
{
    /// <summary>
    /// A logical element which requires Samus to spend some frames in acid.
    /// </summary>
    public class AcidFrames : AbstractDamageNumericalValueLogicalElement
    {
        public override int CalculateDamage(SuperMetroidModel model, InGameState inGameState, int times = 1, bool usePreviousRoom = false)
        {
            return model.Rules.CalculateAcidDamage(inGameState, Value) * times;
        }

        public override IEnumerable<Item> GetDamageReducingItems(SuperMetroidModel model, InGameState inGameState)
        {
            return model.Rules.GetAcidDamageReducingItems(model, inGameState);
        }
    }
}
