using sm_json_data_framework.Models.InGameStates;
using System;
using System.Collections.Generic;
using System.Text;

namespace sm_json_data_framework.Models.Requirements.ObjectRequirements.Integers
{
    /// <summary>
    /// An abstract superclass for the many AbstractObjectLogicalElementWithNumericalIntegerValues that happen to inflict damage on Samus.
    /// </summary>
    public abstract class AbstractDamageNumericalValueLogicalElement: AbstractObjectLogicalElementWithNumericalIntegerValue
    {
        /// <summary>
        /// Calculates the amount of damage that fulfilling this logical element will inflict on Samus.
        /// </summary>
        /// <param name="model">A model that can be used to obtain data about the current game configuration.</param>
        /// <param name="inGameState">The in-game state to evaluate</param>
        /// <param name="times">The number of consecutive times that Samus will take this damage.</param>
        /// <param name="usePreviousRoom">If true, uses the last known room state at the previous room instead of the current room to answer
        /// (whenever in-room state is relevant).</param>
        /// <returns>The calculated amount of damage</returns>
        public abstract int CalculateDamage(SuperMetroidModel model, InGameState inGameState, int times = 1, bool usePreviousRoom = false);

        public override InGameState AttemptFulfill(SuperMetroidModel model, InGameState inGameState, int times = 1, bool usePreviousRoom = false)
        {
            int damage = CalculateDamage(model, inGameState, times: times, usePreviousRoom: usePreviousRoom);
            if (inGameState.IsResourceAvailable(ConsumableResourceEnum.ENERGY, damage))
            {
                inGameState = inGameState.Clone();
                inGameState.ConsumeResource(ConsumableResourceEnum.ENERGY, damage);
                return inGameState;
            }
            else
            {
                return null;
            }
        }
    }
}
