using sm_json_data_framework.Models.Raw.Rooms;
using sm_json_data_framework.Models.Requirements;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.Json.Serialization;

namespace sm_json_data_framework.Models.Rooms
{
    public class RoomObstacle : InitializablePostDeserializeInRoom
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

        public RoomObstacle()
        {

        }

        public RoomObstacle(RawRoomObstacle rawRoomObstacle, LogicalElementCreationKnowledgeBase knowledgeBase)
        {
            Id = rawRoomObstacle.Id;
            Name = rawRoomObstacle.Name;
            ObstacleType = rawRoomObstacle.ObstacleType;
            Requires = rawRoomObstacle.Requires.ToLogicalRequirements(knowledgeBase);
        }

        public void InitializeProperties(SuperMetroidModel model, Room room)
        {
            Room = room;
        }

        public bool CleanUpUselessValues(SuperMetroidModel model, Room room)
        {
            // Nothing relevant to cleanup

            // An obstacle that is impossible to destroy is still useful, to know that the obstacle exists and can't be destroyed.
            return true;
        }

        public IEnumerable<string> InitializeReferencedLogicalElementProperties(SuperMetroidModel model, Room room)
        {
            List<string> unhandled = new List<string>();

            unhandled.AddRange(Requires.InitializeReferencedLogicalElementProperties(model, room));

            return unhandled.Distinct();
        }
    }
}
