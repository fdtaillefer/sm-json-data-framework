using sm_json_data_framework.Models.Techs;
using sm_json_data_framework.Options;
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
        public string Name { get { return InnerElement.Name; } }
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

        protected override bool ApplyLogicalOptionsEffects(ReadOnlyLogicalOptions logicalOptions)
        {
            bool explicitlyDisabled = !logicalOptions.IsGameFlagEnabled(this);
            return explicitlyDisabled;
        }
    }
}
