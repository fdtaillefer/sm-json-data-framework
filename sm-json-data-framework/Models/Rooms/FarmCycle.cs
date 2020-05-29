using sm_json_data_framework.Models.InGameStates;
using sm_json_data_framework.Models.Requirements;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace sm_json_data_framework.Models.Rooms
{
    /// <summary>
    /// Represents a method to farm one cycle of a respawning group of enemies, with the approximate duration.
    /// </summary>
    public class FarmCycle: InitializablePostDeserializeInRoom, IExecutable
    {
        public string name { get; set; }

        public int CycleFrames { get; set; }

        public LogicalRequirements Requires { get; set; }

        public ExecutionResult Execute(SuperMetroidModel model, InGameState inGameState, int times = 1, bool usePreviousRoom = false)
        {
            return Requires.Execute(model, inGameState, times: times, usePreviousRoom: usePreviousRoom);
        }

        public IEnumerable<Action> Initialize(SuperMetroidModel model, Room room)
        {
            // Nothing to do here
            return Enumerable.Empty<Action>();
        }

        public IEnumerable<string> InitializeReferencedLogicalElementProperties(SuperMetroidModel model, Room room)
        {
            return Requires.InitializeReferencedLogicalElementProperties(model, room);
        }
    }
}
