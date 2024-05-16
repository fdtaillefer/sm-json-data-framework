using sm_json_data_framework.Options.ResourceValues;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace sm_json_data_framework.Models.InGameStates
{
    /// <summary>
    /// A Comparer that can compare two in-game states by their consumable resource count, based on an internal in-game resource evaluator.
    /// The "greater" in-game state is the one whose resource total is deemed more valuable according to that evaluator.
    /// </summary>
    public class InGameStateComparer : IComparer<ReadOnlyInGameState>
    {
        private IInGameResourceEvaluator ResourceEvaluator { get; set; }

        public InGameStateComparer(IInGameResourceEvaluator resourceEvaluator)
        {
            ResourceEvaluator = resourceEvaluator;
        }

        public int Compare(ReadOnlyInGameState x, ReadOnlyInGameState y)
        {
            return CalculateValue(x).CompareTo(CalculateValue(y));
        }

        /// <summary>
        /// Calculates a value to attribute to the provided InGameState when using this Comparer to compare InGameStates.
        /// </summary>
        /// <param name="inGameState">The InGameState to assign a value to</param>
        /// <returns></returns>
        private int CalculateValue(ReadOnlyInGameState inGameState)
        {
            // Give a negative value to null. It's decidedly less valuable than any existing state.
            if (inGameState == null)
            {
                return -1;
            }

            return ResourceEvaluator.CalculateValue(inGameState.Resources);
        }
    }
}
