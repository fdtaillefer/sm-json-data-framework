using sm_json_data_framework.Models.InGameStates;
using sm_json_data_framework.Models.Rooms;
using System;
using System.Collections.Generic;
using System.Text;

namespace sm_json_data_framework.Models.Requirements
{
    public abstract class AbstractLogicalElement : IExecutable
    {
        /// <summary>
        /// If this logical element contains any properties that are an object referenced by another property(which is its identifier), initializes them.
        /// Also delegates to any sub-logical elements.
        /// </summary>
        /// <param name="model">A SuperMetroidModel that contains global data</param>
        /// <param name="room">The room in which this logical element is, or null if it's not in a room</param>
        /// <returns>A sequence of strings describing references that could not be initialized properly.</returns>
        public abstract IEnumerable<string> InitializeReferencedLogicalElementProperties(SuperMetroidModel model, Room room);

        // Inherited from IExecutable.
        public abstract ExecutionResult Execute(SuperMetroidModel model, InGameState inGameState, int times = 1, int previousRoomCount = 0);

        // STITCHME could be nice to ask for always and never? As in isAlwaysFree() and isNever()
    }
}
