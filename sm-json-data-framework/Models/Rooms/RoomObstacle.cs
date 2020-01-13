using sm_json_data_framework.Models.Requirements;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.Json.Serialization;

namespace sm_json_data_framework.Models.Rooms
{
    public class RoomObstacle
    {
        public string Id { get; set; }

        public string Name { get; set; }

        public ObstacleTypeEnum ObstacleType { get; set; }

        public LogicalRequirements Requires { get; set; } = new LogicalRequirements();

        /// <summary>
        /// <para>Not available before <see cref="Initialize(SuperMetroidModel, Room)"/> has been called.</para>
        /// <para>The room in which this obstacle is.</para>
        /// </summary>
        [JsonIgnore]
        public Room Room { get; set; }

        /// <summary>
        /// Initializes additional properties in this RoomObstacle, which wouldn't be initialized by simply parsing a rooms json file.
        /// All such properties are identified in their own documentation and should not be read if this method isn't called.
        /// </summary>
        /// <param name="model">The model to use to initialize the additional properties</param>
        /// <param name="room">The room in which this ndoe is</param>
        public void Initialize(SuperMetroidModel model, Room room)
        {
            Room = room;
        }

        /// <summary>
        /// Goes through all logical elements within this RoomObstacle (and all LogicalRequirements within any of them),
        /// attempting to initialize any property that is an object referenced by another property(which is its identifier).
        /// </summary>
        /// <param name="model">A SuperMetroidModel that contains global data</param>
        /// <param name="room">The room in which this RoomObstacle is</param>
        /// <returns>A sequence of strings describing references that could not be initialized properly.</returns>
        public IEnumerable<string> InitializeReferencedLogicalElementProperties(SuperMetroidModel model, Room room)
        {
            List<string> unhandled = new List<string>();

            unhandled.AddRange(Requires.InitializeReferencedLogicalElementProperties(model, room));

            return unhandled.Distinct();
        }
    }
}
