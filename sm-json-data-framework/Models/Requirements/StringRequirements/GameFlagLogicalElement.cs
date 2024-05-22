using sm_json_data_framework.Models.GameFlags;
using sm_json_data_framework.Models.Helpers;
using sm_json_data_framework.Models.InGameStates;
using sm_json_data_framework.Models.Items;
using sm_json_data_framework.Models.Techs;
using sm_json_data_framework.Options;
using sm_json_data_framework.Utils;
using System;
using System.Collections.Generic;
using System.Text;

namespace sm_json_data_framework.Models.Requirements.StringRequirements
{
    /// <summary>
    /// A logical element that is fulfilled by previously activating an in-game flag.
    /// </summary>
    public class GameFlagLogicalElement : AbstractStringLogicalElement<UnfinalizedGameFlagLogicalElement, GameFlagLogicalElement>
    {
        public GameFlagLogicalElement(UnfinalizedGameFlagLogicalElement sourceElement, Action<GameFlagLogicalElement> mappingsInsertionCallback, ModelFinalizationMappings mappings)
            : base(sourceElement, mappingsInsertionCallback)
        {
            GameFlag = sourceElement.GameFlag.Finalize(mappings);
        }

        /// <summary>
        /// The game flag that must be activated to fulfill this logical element.
        /// </summary>
        public GameFlag GameFlag { get; }

        protected override ExecutionResult ExecuteUseful(SuperMetroidModel model, ReadOnlyInGameState inGameState, int times = 1, int previousRoomCount = 0)
        {
            if (inGameState.ActiveGameFlags.ContainsFlag(GameFlag))
            {
                // Clone the In-game state to fulfill method contract
                return new ExecutionResult(inGameState.Clone());
            }
            else
            {
                return null;
            }
        }

        protected override void PropagateLogicalOptions(ReadOnlyLogicalOptions logicalOptions)
        {
            GameFlag.ApplyLogicalOptions(logicalOptions);
        }

        protected override bool CalculateLogicallyNever()
        {
            // This is impossible if the game flag itself is impossible
            return GameFlag.LogicallyNever;
        }

        protected override bool CalculateLogicallyAlways()
        {
            // This is always possible if the game flag itself also is
            return GameFlag.LogicallyAlways;
        }

        protected override bool CalculateLogicallyFree()
        {
            return GameFlag.LogicallyFree;
        }
    }

    public class UnfinalizedGameFlagLogicalElement : AbstractUnfinalizedStringLogicalElement<UnfinalizedGameFlagLogicalElement, GameFlagLogicalElement>
    {
        public UnfinalizedGameFlag GameFlag { get; set; }

        public UnfinalizedGameFlagLogicalElement(UnfinalizedGameFlag gameFlag)
        {
            GameFlag = gameFlag;
        }

        protected override GameFlagLogicalElement CreateFinalizedElement(UnfinalizedGameFlagLogicalElement sourceElement, Action<GameFlagLogicalElement> mappingsInsertionCallback,
            ModelFinalizationMappings mappings)
        {
            return new GameFlagLogicalElement(sourceElement, mappingsInsertionCallback, mappings);
        }
    }
}
