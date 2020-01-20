using sm_json_data_framework.Models.InGameStates;
using System;
using System.Collections.Generic;
using System.Text;

namespace sm_json_data_framework.Models.Requirements.ObjectRequirements.Integers
{
    public class AcidFrames : AbstractObjectLogicalElementWithNumericalIntegerValue, IDamageRequirement
    {
        public override bool IsFulfilled(InGameState inGameState, bool usePreviousRoom = false)
        {
            int damage = CalculateDamage(inGameState.HasVariaSuit(), inGameState.HasGravitySuit());
            return inGameState.IsResourceAvailable(ConsumableResourceEnum.ENERGY, damage);
        }

        public virtual int CalculateDamage(bool hasVaria, bool hasGravity)
        {
            if(hasGravity)
            {
                return Value * 3 / 8;
            }
            else if (hasVaria)
            {
                return Value * 3 / 4;
            }
            else
            {
                return Value * 6 / 4;
            }
        }
    }
}
