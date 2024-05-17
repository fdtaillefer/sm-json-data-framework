using sm_json_data_framework.Models.GameFlags;
using sm_json_data_framework.Models.Rooms.Nodes;
using sm_json_data_framework.Rules.InitialState;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

namespace sm_json_data_framework.Tests.TestTools
{
    public class RandoBasicStartConditionsCustomizer : IBasicStartConditionsCustomizer
    {
        public void Customize(BasicStartConditions basicStartConditions)
        {
            // Enable game flags from Ceres and start with Zebes awake
            basicStartConditions.StartingFlagNames = new List<string> {
                "f_DefeatedCeresRidley",
                "f_ZebesAwake"
            };

            // Unlock Ceres locks
            basicStartConditions.StartingLockNames =  new List<string>
            {
                "Ceres Elevator Lock",
                "Ceres Ridley Room Grey Lock (to 58 Escape)",
                "Ceres Ridley Fight"
            };

            // Start at Ship
            basicStartConditions.StartingRoomName = "Landing Site";
            basicStartConditions.StartingNodeId = 5;
        }
    }
}
