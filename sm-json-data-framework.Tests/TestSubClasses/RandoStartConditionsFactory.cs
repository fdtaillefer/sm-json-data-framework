using sm_json_data_framework.Models.GameFlags;
using sm_json_data_framework.Models.InGameStates;
using sm_json_data_framework.Models.Items;
using sm_json_data_framework.Models.Rooms.Nodes;
using sm_json_data_framework.Models.Rooms;
using sm_json_data_framework.Models;
using sm_json_data_framework.Rules;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using static System.Formats.Asn1.AsnWriter;
using System.Runtime.ConstrainedExecution;

namespace sm_json_data_framework.Tests.TestSubClasses
{
    /// <summary>
    /// An override of DefaultStartConditionsFactory, which also overrides some of the contents of items.json to give 
    /// starting conditions that match a typical randomizer.
    /// </summary>
    public class RandoStartConditionsFactory: DefaultStartConditionsFactory
    {
        public override StartConditions CreateStartConditions(SuperMetroidModel model, ItemContainer itemContainer)
        {
            // Create standard start conditions, we'll adjust them after
            StartConditions startConditions = base.CreateStartConditions(model, itemContainer);

            // Enable game flags from Ceres and start with Zebes awake
            startConditions.StartingGameFlags = new List<GameFlag> { 
                model.GameFlags["f_DefeatedCeresRidley"],
                model.GameFlags["f_ZebesAwake"]
            };

            // Unlock Ceres locks
            List<string> startingLockNames = new List<string>
            {
                "Ceres Elevator Lock",
                "Ceres Ridley Room Grey Lock (to 58 Escape)",
                "Ceres Ridley Fight"
            };
            List<NodeLock> startingLocks = new List<NodeLock>();
            foreach (string lockName in startingLockNames)
            {
                if (!model.Locks.TryGetValue(lockName, out NodeLock nodeLock))
                {
                    throw new Exception($"Starting node lock {lockName} not found.");
                }
                startingLocks.Add(nodeLock);
            }
            startConditions.StartingOpenLocks = startingLocks;

            // Start at Ship
            startConditions.StartingNode = model.GetNodeInRoom("Landing Site", 5);

            return startConditions;
        }
    }
}
