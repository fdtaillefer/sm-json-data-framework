using sm_json_data_framework.Models.GameFlags;
using sm_json_data_framework.Models.InGameStates;
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
        private UnfinalizedGameFlagLogicalElement InnerElement { get; set; }

        public GameFlagLogicalElement(UnfinalizedGameFlagLogicalElement innerElement, Action<GameFlagLogicalElement> mappingsInsertionCallback, ModelFinalizationMappings mappings)
            : base(innerElement, mappingsInsertionCallback)
        {
            InnerElement = innerElement;
            GameFlag = innerElement.GameFlag.Finalize(mappings);
        }

        /// <summary>
        /// The game flag that must be activated to fulfill this logical element.
        /// </summary>
        public GameFlag GameFlag { get; }

        public override bool IsNever()
        {
            return false;
        }

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

        protected override bool ApplyLogicalOptionsEffects(ReadOnlyLogicalOptions logicalOptions)
        {
            GameFlag.ApplyLogicalOptions(logicalOptions);

            // This becomes impossible if the game flag itself becomes useless
            return GameFlag.UselessByLogicalOptions;
        }

        public override bool IsNever()
        {
            return false;
        }

        protected override UnfinalizedExecutionResult ExecuteUseful(UnfinalizedSuperMetroidModel model, ReadOnlyUnfinalizedInGameState inGameState, int times = 1, int previousRoomCount = 0)
        {
            if (inGameState.ActiveGameFlags.ContainsFlag(GameFlag))
            {
                // Clone the In-game state to fulfill method contract
                return new UnfinalizedExecutionResult(inGameState.Clone());
            }
            else
            {
                return null;
            }
        }
    }
}
