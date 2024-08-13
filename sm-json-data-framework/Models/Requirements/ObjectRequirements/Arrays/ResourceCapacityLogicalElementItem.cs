using sm_json_data_framework.InGameStates;
using sm_json_data_framework.Models.Items;
using sm_json_data_framework.Models.Raw.Requirements.ObjectRequirements.Arrays;
using sm_json_data_framework.Options;
using sm_json_data_framework.Rules;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml.Linq;

namespace sm_json_data_framework.Models.Requirements.ObjectRequirements.Arrays
{
    /// <summary>
    /// An element within a <see cref="ResourceCapacityLogicalElement"/> logical element. It describes one resource for which Samus must have a given capacity.
    /// </summary>
    public class ResourceCapacityLogicalElementItem : AbstractModelElement<UnfinalizedResourceCapacityLogicalElementItem, ResourceCapacityLogicalElementItem>
    {
        public ResourceCapacityLogicalElementItem(UnfinalizedResourceCapacityLogicalElementItem sourceElement, Action<ResourceCapacityLogicalElementItem> mappingsInsertionCallback, ModelFinalizationMappings mappings)
            : base(sourceElement, mappingsInsertionCallback)
        {
            Resource = sourceElement.Resource;
            Count = sourceElement.Count;
        }

        /// <summary>
        /// The resource type that Samus must have a capacity for.
        /// </summary>
        public RechargeableResourceEnum Resource { get; }

        /// <summary>
        /// How much of the resource Samus must have capacity for.
        /// </summary>
        public int Count { get; }

        protected override void PropagateLogicalOptions(ReadOnlyLogicalOptions logicalOptions, SuperMetroidModel model)
        {
            // Nothing to do here
        }

        public override bool CalculateLogicallyRelevant(SuperMetroidModel model)
        {
            // Capacity cannot be below 0, so anything that isn't above 0 is always met and completely irrelevant
            return Count > 0;
        }

        /// <summary>
        /// Returns whether the provided InGameState meets the requirements of this ResourceCapacityElement.
        /// </summary>
        /// <param name="inGameState">The InGameState to test</param>
        /// <returns></returns>
        public bool IsFulfilled(ReadOnlyInGameState inGameState)
        {
            return inGameState.ResourceMaximums.GetAmount(Resource) >= Count;
        }
    }

    public class UnfinalizedResourceCapacityLogicalElementItem : AbstractUnfinalizedModelElement<UnfinalizedResourceCapacityLogicalElementItem, ResourceCapacityLogicalElementItem>
    {
        public RechargeableResourceEnum Resource { get; set; }
        public int Count { get; set; }

        public UnfinalizedResourceCapacityLogicalElementItem()
        {

        }

        public UnfinalizedResourceCapacityLogicalElementItem(RawResourceCapacityLogicalElementItem rawResourceCapacityElement, LogicalElementCreationKnowledgeBase knowledgeBase)
        {
            Resource = rawResourceCapacityElement.Type;
            Count = rawResourceCapacityElement.Count;
        }

        protected override ResourceCapacityLogicalElementItem CreateFinalizedElement(UnfinalizedResourceCapacityLogicalElementItem sourceElement, Action<ResourceCapacityLogicalElementItem> mappingsInsertionCallback, ModelFinalizationMappings mappings)
        {
            return new ResourceCapacityLogicalElementItem(sourceElement, mappingsInsertionCallback, mappings);
        }
    }
}
