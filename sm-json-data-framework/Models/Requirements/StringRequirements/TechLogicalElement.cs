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
            return Tech.Requires.Execute(model, inGameState, times: times * InnerElement.Tries, previousRoomCount: previousRoomCount);
        }
    }

    public class UnfinalizedTechLogicalElement : AbstractUnfinalizedStringLogicalElement<UnfinalizedTechLogicalElement, TechLogicalElement>
    {
        public UnfinalizedTech Tech { get; set; }

        /// <summary>
        /// Number of tries the player is expected to take to execute the tech, as per applied logical options.
        /// </summary>
        public int Tries { get; private set; } = LogicalOptions.DefaultNumberOfTries;

        public UnfinalizedTechLogicalElement(UnfinalizedTech tech)
        {
            Tech = tech;
        }

        protected override TechLogicalElement CreateFinalizedElement(UnfinalizedTechLogicalElement sourceElement, Action<TechLogicalElement> mappingsInsertionCallback, ModelFinalizationMappings mappings)
        {
            return new TechLogicalElement(sourceElement, mappingsInsertionCallback, mappings);
        }

        protected override bool ApplyLogicalOptionsEffects(ReadOnlyLogicalOptions logicalOptions)
        {
            Tries = logicalOptions?.NumberOfTries(Tech) ?? LogicalOptions.DefaultNumberOfTries;
            Tech.ApplyLogicalOptions(logicalOptions);
            // This becomes impossible if the tech itself becomes impossible
            return Tech.UselessByLogicalOptions;
        }

        public override bool IsNever()
        {
            return false;
        }
    }
}
