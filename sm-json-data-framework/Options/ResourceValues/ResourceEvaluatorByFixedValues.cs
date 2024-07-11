using sm_json_data_framework.InGameStates;
using sm_json_data_framework.Models.Requirements;
using System;
using System.Collections.Generic;
using System.Text;

namespace sm_json_data_framework.Options.ResourceValues
{
    /// <summary>
    /// An in-game resource evaluator that works by giving a fixed value to each resource and calculating a total.
    /// </summary>
    public class ResourceEvaluatorByFixedValues: IInGameResourceEvaluator
    {
        public ResourceEvaluatorByFixedValues(IDictionary<ConsumableResourceEnum, int> fixedResourceValues)
        {
            FixedResourceValues = new Dictionary<ConsumableResourceEnum, int>(fixedResourceValues);
        }

        /// <summary>
        /// A map that maps a consumable resource type to a fixed value, which can give resource types greater or smaller values relative to each other.
        /// It can be used by algorithms if they need to make a decision between several possible options to consume resources.
        /// </summary>
        private IDictionary<ConsumableResourceEnum, int> FixedResourceValues { get; set; }

        public int CalculateValue(ReadOnlyResourceCount resources)
        {
            // Give a negative value to null. It's decidedly less valuable than any existing state.
            if (resources == null)
            {
                return -1;
            }

            // We are assuming that dead states (0 energy) won't show up. If we wanted to support that, we'd have to check specifically for it and give it a negative value too.
            // (but greater than the value for null)
            int value = 0;
            foreach (ConsumableResourceEnum currentResource in Enum.GetValues<ConsumableResourceEnum>())
            {
                value += resources.GetAmount(currentResource) * FixedResourceValues[currentResource];
            }

            return value;
        }
    }
}
