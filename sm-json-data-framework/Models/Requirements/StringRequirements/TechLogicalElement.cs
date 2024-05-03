using sm_json_data_framework.Models.InGameStates;
using sm_json_data_framework.Models.Techs;
using sm_json_data_framework.Options;
using System;
using System.Collections.Generic;
using System.Text;

namespace sm_json_data_framework.Models.Requirements.StringRequirements
{
    /// <summary>
    /// A logical element that is fulfilled by fulfilling the requirements of an inner Tech.
    /// </summary>
    public class TechLogicalElement : AbstractStringLogicalElement
    {
        private Tech Tech { get; set; }

        private int Tries { get; set; } = LogicalOptions.DefaultNumberOfTries;

        public TechLogicalElement(Tech tech)
        {
            Tech = tech;
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

        protected override ExecutionResult ExecuteUseful(SuperMetroidModel model, ReadOnlyInGameState inGameState, int times = 1, int previousRoomCount = 0)
        {
            return Tech.Requires.Execute(model, inGameState, times: times * Tries, previousRoomCount: previousRoomCount);
        }
    }
}
