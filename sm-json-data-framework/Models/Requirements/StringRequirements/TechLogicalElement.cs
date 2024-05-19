using sm_json_data_framework.Models.GameFlags;
using sm_json_data_framework.Models.InGameStates;
using sm_json_data_framework.Models.Techs;
using sm_json_data_framework.Options;
using System;
using System.Collections.Generic;
using System.Text;

namespace sm_json_data_framework.Models.Requirements.StringRequirements
{
    /// <summary>
    /// A logical element that is fulfilled by fulfilling the requirements of a Tech.
    /// </summary>
    public class TechLogicalElement : AbstractStringLogicalElement<UnfinalizedTechLogicalElement, TechLogicalElement>
    {
        /// <summary>
        /// Number of tries the player is expected to take to execute the tech, as per applied logical options.
        /// </summary>
        public int Tries  => AppliedLogicalOptions?.NumberOfTries(Tech) ?? LogicalOptions.DefaultNumberOfTries;

        private UnfinalizedTechLogicalElement InnerElement { get; set; }

        public TechLogicalElement(UnfinalizedTechLogicalElement innerElement, Action<TechLogicalElement> mappingsInsertionCallback, ModelFinalizationMappings mappings)
            : base(innerElement, mappingsInsertionCallback)
        {
            InnerElement = innerElement;
            Tech = innerElement.Tech.Finalize(mappings);
        }

        /// <summary>
        /// The tech that must be executed to fulfill this logical element.
        /// </summary>
        public Tech Tech { get; }

        public override bool IsNever()
        {
            return false;
        }

        protected override ExecutionResult ExecuteUseful(SuperMetroidModel model, ReadOnlyInGameState inGameState, int times = 1, int previousRoomCount = 0)
        {
            return Tech.Requires.Execute(model, inGameState, times: times * Tries, previousRoomCount: previousRoomCount);
        }

        protected override bool PropagateLogicalOptions(ReadOnlyLogicalOptions logicalOptions)
        {
            Tech.ApplyLogicalOptions(logicalOptions);
            // This becomes impossible if the tech itself becomes impossible
            return Tech.UselessByLogicalOptions;
        }

        protected override bool CalculateLogicallyNever()
        {
            // This is impossible if the tech itself is impossible
            return Tech.LogicallyNever;
        }

        protected override bool CalculateLogicallyAlways()
        {
            // This is always possible if the tech itself also is
            return Tech.LogicallyAlways;
        }
    }

    public class UnfinalizedTechLogicalElement : AbstractUnfinalizedStringLogicalElement<UnfinalizedTechLogicalElement, TechLogicalElement>
    {
        public UnfinalizedTech Tech { get; set; }

        public UnfinalizedTechLogicalElement(UnfinalizedTech tech)
        {
            Tech = tech;
        }

        protected override TechLogicalElement CreateFinalizedElement(UnfinalizedTechLogicalElement sourceElement, Action<TechLogicalElement> mappingsInsertionCallback, ModelFinalizationMappings mappings)
        {
            return new TechLogicalElement(sourceElement, mappingsInsertionCallback, mappings);
        }

        public override bool IsNever()
        {
            return false;
        }
    }
}
