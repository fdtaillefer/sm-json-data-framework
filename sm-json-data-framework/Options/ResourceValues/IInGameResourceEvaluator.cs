using sm_json_data_framework.InGameStates;
using System;
using System.Collections.Generic;
using System.Text;

namespace sm_json_data_framework.Options.ResourceValues
{
    /// <summary>
    /// An interface that can assign a value to snapshots of in-game resources, for comparison purposes.
    /// </summary>
    public interface IInGameResourceEvaluator
    {
        /// <summary>
        /// Calculates a value for the provided ReadOnlyResourceCount. Assumes that dead states (energy of 0) won't show up.
        /// </summary>
        /// <param name="resources">The resource count to calculate a value for</param>
        /// <returns>The calculated value</returns>
        int CalculateValue(ReadOnlyResourceCount resources);
    }
}
