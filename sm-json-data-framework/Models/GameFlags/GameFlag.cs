using sm_json_data_framework.Models.Techs;
using sm_json_data_framework.Options;
using sm_json_data_framework.Rules;
using sm_json_data_framework.Utils;
using System;
using System.Collections.Generic;
using System.Text;

namespace sm_json_data_framework.Models.GameFlags
{
    /// <summary>
    /// A game flag, which gets toggled when the player does something and is read elsewhere in the game.
    /// </summary>
    public class GameFlag : AbstractModelElement<UnfinalizedGameFlag, GameFlag>, ILogicalExecutionPreProcessable
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

        protected override void PropagateLogicalOptions(ReadOnlyLogicalOptions logicalOptions, SuperMetroidRules rules)
        {
            // Nothing to do here
        }

        protected override void UpdateLogicalProperties(SuperMetroidRules rules)
        {
            base.UpdateLogicalProperties(rules);
            LogicallyNever = CalculateLogicallyNever(rules);
            LogicallyAlways = CalculateLogicallyAlways(rules);
        }

        public override bool CalculateLogicallyRelevant(SuperMetroidRules rules)
        {
            // A game flag that can't be enabled may as well not exist
            return !CalculateLogicallyNever(rules);
        }

        public bool LogicallyNever { get; private set; }

        /// <summary>
        /// Calculates what the value of <see cref="LogicallyNever"/> should currently be.
        /// </summary>
        /// <param name="rules">The active SuperMetroidRules, provided so they're available for consultation</param>
        /// <returns></returns>
        protected bool CalculateLogicallyNever(SuperMetroidRules rules)
        {
            // GameFlag is impossible if it's disabled
            return !AppliedLogicalOptions.IsGameFlagEnabled(this);
        }

        public bool LogicallyAlways { get; private set; }

        /// <summary>
        /// Calculates what the value of <see cref="LogicallyAlways"/> should currently be.
        /// </summary>
        /// <param name="rules">The active SuperMetroidRules, provided so they're available for consultation</param>
        /// <returns></returns>
        protected bool CalculateLogicallyAlways(SuperMetroidRules rules)
        {
            // Game flag is always enabled if the game always starts with it
            return AppliedLogicalOptions.StartConditions.StartingGameFlags.ContainsFlag(this);
        }

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
