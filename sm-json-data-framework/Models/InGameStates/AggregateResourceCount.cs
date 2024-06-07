using sm_json_data_framework.Models.Items;
using sm_json_data_framework.Models.Requirements;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace sm_json_data_framework.Models.InGameStates
{
    /// <summary>
    /// A read-only resource count that is an aggregate of several other resource counts.
    /// All the values it returns correspond to the sum of all resource counts it is an aggregate of,
    /// and the values returned by this will stay up-to-date with changes in those instances.
    /// </summary>
    public class AggregateResourceCount : ReadOnlyResourceCount
    {
        protected List<ReadOnlyResourceCount> ResourceCounts { get; }

        public AggregateResourceCount(params ReadOnlyResourceCount[] resourceCounts)
        {
            ResourceCounts = new List<ReadOnlyResourceCount>(resourceCounts);
        }

        public bool Any(Predicate<int> resourcePredicate)
        {
            return Enum.GetValues<RechargeableResourceEnum>().Any(resource => resourcePredicate.Invoke(GetAmount(resource)));
        }

        /// <summary>
        /// Returns a non-aggregate clone of this AggregateResourceCount, initially with the same values as the original.
        /// This new instance will be completely disconnected from the resource counts this instance is an aggregate of.
        /// </summary>
        /// <returns></returns>
        public ResourceCount Clone()
        {
            return new ResourceCount(this);
        }

        public int GetAmount(RechargeableResourceEnum resource)
        {
            return ResourceCounts.Sum(count => count.GetAmount(resource));
        }

        public int GetAmount(ConsumableResourceEnum resource)
        {
            return ResourceCounts.Sum(count => count.GetAmount(resource));
        }

        public bool IsResourceAvailable(ConsumableResourceEnum resource, int quantity)
        {
            int actualAmount = GetAmount(resource);
            return ResourceCount.IsResourceAvailable(resource, quantity, actualAmount);
        }

        public override bool Equals(object obj)
        {
            if (obj is ReadOnlyResourceCount count)
            {
                foreach (RechargeableResourceEnum resource in Enum.GetValues(typeof(RechargeableResourceEnum)))
                {
                    if (GetAmount(resource) != count.GetAmount(resource))
                    {
                        return false;
                    }
                }
            }
            else
            {
                return false;
            }

            return true;
        }

        public override int GetHashCode()
        {
            HashCode hash = new();
            foreach (RechargeableResourceEnum resource in Enum.GetValues(typeof(RechargeableResourceEnum)))
            {
                hash.Add(GetAmount(resource));
            }
            return hash.ToHashCode();
        }
    }
}
