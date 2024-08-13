using sm_json_data_framework.InGameStates;
using sm_json_data_framework.Models.Items;
using sm_json_data_framework.Models.Requirements.ObjectRequirements.SubObjects;
using sm_json_data_framework.Models.Rooms;
using sm_json_data_framework.Options;
using sm_json_data_framework.Rules;
using sm_json_data_framework.Utils;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace sm_json_data_framework.Models.Requirements.ObjectRequirements.Arrays
{
    public class ResourceCapacityLogicalElement
        : AbstractArrayLogicalElement<UnfinalizedResourceCapacityLogicalElementItem, ResourceCapacityLogicalElementItem,
            UnfinalizedResourceCapacityLogicalElement, ResourceCapacityLogicalElement>
    {
        public ResourceCapacityLogicalElement(UnfinalizedResourceCapacityLogicalElement sourceElement, Action<ResourceCapacityLogicalElement> mappingsInsertionCallback, ModelFinalizationMappings mappings)
            : base(sourceElement, mappingsInsertionCallback, mappings)
        {
            ResourceCapacities = Value.ToDictionary(capacity => capacity.Resource);
        }

        /// <summary>
        /// The list of ResourceCapacityItem that Samus must fulfill in order to meet the requirements for this resource Capacity.
        /// </summary>
        public IReadOnlyDictionary<RechargeableResourceEnum, ResourceCapacityLogicalElementItem> ResourceCapacities { get; }

        protected override ResourceCapacityLogicalElementItem ConvertItem(UnfinalizedResourceCapacityLogicalElementItem sourceItem, ModelFinalizationMappings mappings)
        {
            return sourceItem.Finalize(mappings);
        }

        protected override ExecutionResult ExecutePossible(SuperMetroidModel model, ReadOnlyInGameState inGameState, int times = 1, int previousRoomCount = 0)
        {
            bool fulfilled = ResourceCapacities.Values.All(capacity => capacity.IsFulfilled(inGameState));
            if (fulfilled)
            {
                // Clone the InGameState to fulfill method contract
                return new ExecutionResult(inGameState.Clone());
            }
            else
            {
                // If this is not fulfilled, then execution fails
                return null;
            }
        }

        protected override void PropagateLogicalOptions(ReadOnlyLogicalOptions logicalOptions, SuperMetroidModel model)
        {
            foreach (ResourceCapacityLogicalElementItem resourceCapacityItem in ResourceCapacities.Values)
            {
                resourceCapacityItem.ApplyLogicalOptions(logicalOptions, model);
            }
        }

        protected override bool CalculateLogicallyNever(SuperMetroidModel model)
        {
            // If any resource capacity requires more than is possible to obtain, this is impossible to fulfill
            return ResourceCapacities
                .Values
                .WhereLogicallyRelevant()
                .Where(resourceCapacity =>
                {
                    int? maxCapacity = AppliedLogicalOptions.MaxPossibleAmount(resourceCapacity.Resource);
                    if (maxCapacity == null)
                    {
                        return false;
                    }
                    return resourceCapacity.Count > AppliedLogicalOptions.AvailableResourceInventory.ResourceMaximums.GetAmount(resourceCapacity.Resource);
                })
                .Any();
        }

        protected override bool CalculateLogicallyAlways(SuperMetroidModel model)
        {
            // If there's no requirement that is less than the starting capacity of the corresponding resource, this is always met
            return !ResourceCapacities
                .Values
                .WhereLogicallyRelevant()
                .Where(resourceCapacity => resourceCapacity.Count > AppliedLogicalOptions.StartConditions.StartingInventory.ResourceMaximums.GetAmount(resourceCapacity.Resource))
                .Any();
        }

        protected override bool CalculateLogicallyFree(SuperMetroidModel model)
        {
            // If there's no requirement that is less than the starting capacity of the corresponding resource, this is always met
            // (and also free since just having capacity consumes nothing)
            return CalculateLogicallyAlways(model);
        }
    }

    public class UnfinalizedResourceCapacityLogicalElement
        : AbstractUnfinalizedArrayLogicalElement<UnfinalizedResourceCapacityLogicalElementItem, ResourceCapacityLogicalElementItem,
            UnfinalizedResourceCapacityLogicalElement, ResourceCapacityLogicalElement>
    {
        public UnfinalizedResourceCapacityLogicalElement(IList<UnfinalizedResourceCapacityLogicalElementItem> items) : base(items)
        {

        }

        protected override ResourceCapacityLogicalElement CreateFinalizedElement(
            UnfinalizedResourceCapacityLogicalElement sourceElement, Action<ResourceCapacityLogicalElement> mappingsInsertionCallback, ModelFinalizationMappings mappings)
        {
            return new ResourceCapacityLogicalElement(sourceElement, mappingsInsertionCallback, mappings);
        }

        public override IEnumerable<string> InitializeReferencedLogicalElementProperties(UnfinalizedSuperMetroidModel model, UnfinalizedRoom room)
        {
            // No properties need to be handled here
            return Enumerable.Empty<string>();
        }
    }
}
