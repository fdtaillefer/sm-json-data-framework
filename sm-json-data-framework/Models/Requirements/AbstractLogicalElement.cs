using sm_json_data_framework.Models.InGameStates;
using sm_json_data_framework.Models.Requirements.StringRequirements;
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
        public abstract ExecutionResult Execute(SuperMetroidModel model, ReadOnlyInGameState inGameState, int times = 1, int previousRoomCount = 0);

        /// <summary>
        /// Returns whether this logical element in its current state is one that can never get fulfilled because it is a (or depends on a mandatory)
        /// <see cref="NeverLogicalElement"/>.
        /// This does not tell whether the logical element should be replaced by a never, because that depends on map layout and logical options, 
        /// which are not available here.
        /// </summary>
        /// <returns></returns>
        public abstract bool IsNever();

        // STITCHME could be nice to ask for always? As in isAlwaysFree()
    }
}
