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
        private UnfinalizedGameFlag InnerElement { get; set; }

        public GameFlag(UnfinalizedGameFlag innerElement, Action<GameFlag> mappingsInsertionCallback)
            : base(innerElement, mappingsInsertionCallback)
        {
            InnerElement = innerElement;
        }

        /// <summary>
        /// The unique name of the game flag. Game flag names are defined by the model and are not official names.
        /// </summary>
        public string Name => InnerElement.Name;

        protected override bool PropagateLogicalOptions(ReadOnlyLogicalOptions logicalOptions)
        {
            bool explicitlyDisabled = !logicalOptions.IsGameFlagEnabled(this);
            return explicitlyDisabled;
        }

        protected override void UpdateLogicalProperties()
        {
            base.UpdateLogicalProperties();
            LogicallyNever = CalculateLogicallyNever();
            LogicallyAlways = CalculateLogicallyAlways();
        }

        /// <summary>
        /// If true, then this GameFlag is impossible to enable with its currently applied logical options, regardless of in-game state.
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
        /// If true, then this Gameflag is always enabled with its currently applied logical options, regardless of in-game state.
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
