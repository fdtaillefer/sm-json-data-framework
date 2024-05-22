using sm_json_data_framework.Models.Techs;
using sm_json_data_framework.Options;
using sm_json_data_framework.Utils;
using System;
using System.Collections.Generic;
using System.Text;

namespace sm_json_data_framework.Models.GameFlags
{
    /// <summary>
    /// A game flag, which gets toggled when the player does something and is read elsewhere in the game.
    /// </summary>
    public class GameFlag : AbstractModelElement<UnfinalizedGameFlag, GameFlag>
    {
        public GameFlag(UnfinalizedGameFlag sourceElement, Action<GameFlag> mappingsInsertionCallback)
            : base(sourceElement, mappingsInsertionCallback)
        {
            Name = sourceElement.Name;
        }

        /// <summary>
        /// The unique name of the game flag. Game flag names are defined by the model and are not official names.
        /// </summary>
        public string Name { get; }

        protected override void PropagateLogicalOptions(ReadOnlyLogicalOptions logicalOptions)
        {
            // Nothing to do here
        }

        protected override void UpdateLogicalProperties()
        {
            base.UpdateLogicalProperties();
            LogicallyNever = CalculateLogicallyNever();
            LogicallyAlways = CalculateLogicallyAlways();
        }

        public override bool CalculateLogicallyRelevant()
        {
            // A game flag that can't be enabled may as well not exist
            return !CalculateLogicallyNever();
        }

        /// <summary>
        /// If true, then this GameFlag is impossible to enable given the current logical options, regardless of in-game state.
        /// </summary>
        public bool LogicallyNever { get; private set; }

        /// <summary>
        /// Calculates what the value of <see cref="LogicallyNever"/> should currently be.
        /// </summary>
        /// <returns></returns>
        protected bool CalculateLogicallyNever()
        {
            // GameFlag is impossible if it's disabled
            return !AppliedLogicalOptions.IsGameFlagEnabled(this);
        }

        /// <summary>
        /// If true, then this Gameflag is always enabled given the current logical options, regardless of in-game state.
        /// </summary>
        public bool LogicallyAlways { get; private set; }

        /// <summary>
        /// Calculates what the value of <see cref="LogicallyAlways"/> should currently be.
        /// </summary>
        /// <returns></returns>
        protected bool CalculateLogicallyAlways()
        {
            // Game flag is always enabled if the game always starts with it
            return AppliedLogicalOptions.StartConditions.StartingGameFlags.ContainsKey(Name);
        }

        /// <summary>
        /// If true, not only is this GameFlag always enabled given the current logical options, regardless of in-game state,
        /// but that enabled state is also guaranteed to cost no resources.
        /// </summary>
        public bool LogicallyFree => LogicallyAlways; // Just having a gameFlag enabled can never cost resources
    }

    public class UnfinalizedGameFlag : AbstractUnfinalizedModelElement<UnfinalizedGameFlag, GameFlag>
    {
        public string Name { get; set; }

        public UnfinalizedGameFlag() { }

        public UnfinalizedGameFlag(string name)
        {
            Name = name;
        }

        protected override GameFlag CreateFinalizedElement(UnfinalizedGameFlag sourceElement, Action<GameFlag> mappingsInsertionCallback, ModelFinalizationMappings mappings)
        {
            return new GameFlag(sourceElement, mappingsInsertionCallback);
        }
    }
}
