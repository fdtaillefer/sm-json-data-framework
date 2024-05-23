using sm_json_data_framework.Models.Helpers;
using sm_json_data_framework.Models.InGameStates;
using sm_json_data_framework.Models.Techs;
using sm_json_data_framework.Options;
using sm_json_data_framework.Rules;
using System;
using System.Collections.Generic;
using System.Text;

namespace sm_json_data_framework.Models.Requirements.StringRequirements
{
    /// <summary>
    /// A logical element that is fulfilled by fulfilling the requirements of an inner Helper.
    /// </summary>
    public class HelperLogicalElement : AbstractStringLogicalElement<UnfinalizedHelperLogicalElement, HelperLogicalElement>
    {
        /// <summary>
        /// Number of tries the player is expected to take to execute the helper, as per applied logical options.
        /// </summary>
        public int Tries => AppliedLogicalOptions.NumberOfTries(Helper);

        public HelperLogicalElement(UnfinalizedHelperLogicalElement sourceElement, Action<HelperLogicalElement> mappingsInsertionCallback, ModelFinalizationMappings mappings)
            : base(sourceElement, mappingsInsertionCallback)
        {
            Helper = sourceElement.Helper.Finalize(mappings);
        }

        /// <summary>
        /// The helper whose logical requirements must be fulfilled for this logical element.
        /// </summary>
        public Helper Helper {get;}

        protected override ExecutionResult ExecuteUseful(SuperMetroidModel model, ReadOnlyInGameState inGameState, int times = 1, int previousRoomCount = 0)
        {
            return Helper.Requires.Execute(model, inGameState, times: times * Tries, previousRoomCount: previousRoomCount);
        }

        protected override void PropagateLogicalOptions(ReadOnlyLogicalOptions logicalOptions, SuperMetroidRules rules)
        {
            Helper.ApplyLogicalOptions(logicalOptions, rules);
        }

        protected override bool CalculateLogicallyNever(SuperMetroidRules rules)
        {
            // This is impossible if the helper itself is impossible
            return Helper.LogicallyNever;
        }

        protected override bool CalculateLogicallyAlways(SuperMetroidRules rules)
        {
            // This is always possible if the helper itself also is
            return Helper.LogicallyAlways;
        }

        protected override bool CalculateLogicallyFree(SuperMetroidRules rules)
        {
            // This is always possible if the helper itself also is
            return Helper.LogicallyFree;
        }
    }

    public class UnfinalizedHelperLogicalElement : AbstractUnfinalizedStringLogicalElement<UnfinalizedHelperLogicalElement, HelperLogicalElement>
    {
        public UnfinalizedHelper Helper { get; set; }

        public UnfinalizedHelperLogicalElement(UnfinalizedHelper helper)
        {
            Helper = helper;
        }

        protected override HelperLogicalElement CreateFinalizedElement(UnfinalizedHelperLogicalElement sourceElement, Action<HelperLogicalElement> mappingsInsertionCallback,
            ModelFinalizationMappings mappings)
        {
            return new HelperLogicalElement(sourceElement, mappingsInsertionCallback, mappings);
        }
    }
}
