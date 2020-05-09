using sm_json_data_framework.Models.InGameStates;
using sm_json_data_framework.Models.Items;
using System;
using System.Collections.Generic;
using System.Text;

namespace sm_json_data_framework.Models.Requirements.ObjectRequirements.Integers
{
    /// <summary>
    /// A logical element which requires Samus to spend some frames grappled to one of Draygon's turrets.
    /// </summary>
    public class DraygonElectricityFrames : AbstractDamageNumericalValueLogicalElement
    {
        public override int CalculateDamage(SuperMetroidModel model, InGameState inGameState, int times = 1, bool usePreviousRoom = false)
        {
            return model.Rules.CalculateElectricityGrappleDamage(inGameState, Value) * times;
        }

        public override IEnumerable<Item> GetDamageReducingItems(SuperMetroidModel model, InGameState inGameState)
        {
            return model.Rules.GetElectricityGrappleDamageReducingItems(model, inGameState);
        }
    }
}
