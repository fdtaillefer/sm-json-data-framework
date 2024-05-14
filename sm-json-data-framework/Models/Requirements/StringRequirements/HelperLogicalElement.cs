﻿using sm_json_data_framework.Models.Helpers;
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
        private UnfinalizedHelperLogicalElement InnerElement { get; set; }

        public HelperLogicalElement(UnfinalizedHelperLogicalElement innerElement, Action<HelperLogicalElement> mappingsInsertionCallback, ModelFinalizationMappings mappings)
            : base(innerElement, mappingsInsertionCallback)
        {
            InnerElement = innerElement;
            Helper = InnerElement.Helper.Finalize(mappings);
        }

        /// <summary>
        /// The helper whose logical requirements must be fulfilled for this logical element.
        /// </summary>
        public Helper Helper {get;}
    }

    public class UnfinalizedHelperLogicalElement : AbstractUnfinalizedStringLogicalElement<UnfinalizedHelperLogicalElement, HelperLogicalElement>
    {
        public UnfinalizedHelper Helper { get; set; }

        private int Tries { get; set; } = LogicalOptions.DefaultNumberOfTries;

        public UnfinalizedHelperLogicalElement(UnfinalizedHelper helper)
        {
            Helper = helper;
        }

        protected override HelperLogicalElement CreateFinalizedElement(UnfinalizedHelperLogicalElement sourceElement, Action<HelperLogicalElement> mappingsInsertionCallback,
            ModelFinalizationMappings mappings)
        {
            return new HelperLogicalElement(sourceElement, mappingsInsertionCallback, mappings);
        }

        protected override bool ApplyLogicalOptionsEffects(ReadOnlyLogicalOptions logicalOptions)
        {
            Tries = logicalOptions?.NumberOfTries(Helper) ?? LogicalOptions.DefaultNumberOfTries;

            Helper.ApplyLogicalOptions(logicalOptions);

            // This becomes impossible if the helper itself becomes impossible
            return Helper.UselessByLogicalOptions;
        }

        public override bool IsNever()
        {
            return false;
        }

        protected override ExecutionResult ExecuteUseful(UnfinalizedSuperMetroidModel model, ReadOnlyInGameState inGameState, int times = 1, int previousRoomCount = 0)
        {
            return Helper.Requires.Execute(model, inGameState, times: times * Tries, previousRoomCount: previousRoomCount);
        }
    }
}
