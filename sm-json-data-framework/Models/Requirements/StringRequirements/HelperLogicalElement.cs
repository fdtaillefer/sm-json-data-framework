using sm_json_data_framework.Models.Helpers;
using sm_json_data_framework.Models.InGameStates;
using sm_json_data_framework.Models.Techs;
using sm_json_data_framework.Options;
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
        public int Tries => AppliedLogicalOptions?.NumberOfTries(Helper) ?? LogicalOptions.DefaultNumberOfTries;

        private UnfinalizedHelperLogicalElement InnerElement { get; set; }

        public HelperLogicalElement(UnfinalizedHelperLogicalElement innerElement, Action<HelperLogicalElement> mappingsInsertionCallback, ModelFinalizationMappings mappings)
            : base(innerElement, mappingsInsertionCallback)
        {
            InnerElement = innerElement;
            Helper = InnerElement.Helper.Finalize(mappings);
        }

        public override bool IsNever()
        {
            return Helper.Requires.IsNever();
        }

        /// <summary>
        /// The helper whose logical requirements must be fulfilled for this logical element.
        /// </summary>
        public Helper Helper {get;}

        protected override ExecutionResult ExecuteUseful(SuperMetroidModel model, ReadOnlyInGameState inGameState, int times = 1, int previousRoomCount = 0)
        {
            return Helper.Requires.Execute(model, inGameState, times: times * Tries, previousRoomCount: previousRoomCount);
        }

        protected override bool PropagateLogicalOptions(ReadOnlyLogicalOptions logicalOptions)
        {
            Helper.ApplyLogicalOptions(logicalOptions);

            // This becomes impossible if the helper itself becomes impossible
            return Helper.UselessByLogicalOptions;
        }

        protected override bool CalculateLogicallyNever()
        {
            // This is impossible if the helper itself is impossible
            return Helper.LogicallyNever;
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

        public override bool IsNever()
        {
            return Helper.Requires.IsNever();
        }
    }
}
