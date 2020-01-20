using sm_json_data_framework.Models.InGameStates;
using System;
using System.Collections.Generic;
using System.Text;

namespace sm_json_data_framework.Models.Requirements.ObjectRequirements.Integers
{
    public class SpikeHits : AbstractObjectLogicalElementWithNumericalIntegerValue, IDamageRequirement
    {
        public override bool IsFulfilled(InGameState inGameState, bool usePreviousRoom = false)
        {
            int damage = CalculateDamage(inGameState.HasVariaSuit(), inGameState.HasGravitySuit());
            return inGameState.IsResourceAvailable(ConsumableResourceEnum.ENERGY, damage);
        }

        public virtual int CalculateDamage(bool hasVaria, bool hasGravity)
        {
            int baseDamage = 60;
            if (hasGravity)
            {
                return baseDamage / 4;
            }
            else if (hasVaria)
            {
                return baseDamage / 2;
            }
            else
            {
                return baseDamage;
            }
        }
    }
}
