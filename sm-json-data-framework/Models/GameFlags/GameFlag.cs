using sm_json_data_framework.Models.Techs;
using sm_json_data_framework.Options;
using System;
using System.Collections.Generic;
using System.Text;

namespace sm_json_data_framework.Models.GameFlags
{
    public class GameFlag : AbstractModelElement
    {
        public string Name { get; set; }

        public GameFlag() { }

        public GameFlag(string name)
        {
            Name = name;
        }

        protected override bool ApplyLogicalOptionsEffects(ReadOnlyLogicalOptions logicalOptions)
        {
            bool explicitlyDisabled = !logicalOptions.IsGameFlagEnabled(this);
            return explicitlyDisabled;
        }
    }
}
