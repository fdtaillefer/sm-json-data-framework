using sm_json_data_framework.Models.Raw.Rooms;
using sm_json_data_framework.Models.Requirements;
using sm_json_data_framework.Options;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.Json.Serialization;

namespace sm_json_data_framework.Models.Rooms
{
    /// <summary>
    /// Represents an obstacle within a room. An obstacle is something that impedes Samus' movement and can be destroyed, 
    /// and doesn't respawn until the room is reloaded.
    /// </summary>
    public class RoomObstacle : AbstractModelElement<UnfinalizedRoomObstacle, RoomObstacle>
    {
        private UnfinalizedRoomObstacle InnerElement { get; set; }

        public RoomObstacle(UnfinalizedRoomObstacle innerElement, Action<RoomObstacle> mappingsInsertionCallback, ModelFinalizationMappings mappings)
            : base(innerElement, mappingsInsertionCallback)
        {
            InnerElement = innerElement;
            Requires = InnerElement.Requires.Finalize(mappings);
            Room = InnerElement.Room.Finalize(mappings);
        }

        /// <summary>
        /// An arbitrary ID that identifies this obstacle. It is only unique within this obstacle's room.
        /// </summary>
        public string Id { get { return InnerElement.Id; } }

        /// <summary>
        /// A human-legible name that identifies this obstacle.
        /// </summary>
        public string Name { get { return InnerElement.Name; } }

        /// <summary>
        /// The type of obstacle this is.
        /// </summary>
        public ObstacleTypeEnum ObstacleType { get { return InnerElement.ObstacleType; } }

        /// <summary>
        /// Logical requirements that must systematically be fulfilled in order to destroy this obstacle, regardless of any other context.
        /// Often, different strats that destroy obstacles also come with additional logical requirements based on the strat's context.
        /// </summary>
        public LogicalRequirements Requires { get; }

        /// <summary>
        /// The room in which this obstacle is.
        /// </summary>
        public Room Room { get; }
    }

    public class UnfinalizedRoomObstacle : AbstractUnfinalizedModelElement<UnfinalizedRoomObstacle, RoomObstacle>, InitializablePostDeserializeInRoom
    {
        public string Id { get; set; }

        public string Name { get; set; }

        public ObstacleTypeEnum ObstacleType { get; set; }

        public UnfinalizedLogicalRequirements Requires { get; set; } = new UnfinalizedLogicalRequirements();

        /// <summary>
        /// <para>Not available before <see cref="Initialize(SuperMetroidModel, UnfinalizedRoom)"/> has been called.</para>
        /// <para>The room in which this obstacle is.</para>
        /// </summary>
        [JsonIgnore]
        public UnfinalizedRoom Room { get; set; }

        public UnfinalizedRoomObstacle()
        {

        }

        public UnfinalizedRoomObstacle(RawRoomObstacle rawRoomObstacle, LogicalElementCreationKnowledgeBase knowledgeBase)
        {
            Id = rawRoomObstacle.Id;
            Name = rawRoomObstacle.Name;
            ObstacleType = rawRoomObstacle.ObstacleType;
            Requires = rawRoomObstacle.Requires.ToLogicalRequirements(knowledgeBase);
        }

        protected override RoomObstacle CreateFinalizedElement(UnfinalizedRoomObstacle sourceElement, Action<RoomObstacle> mappingsInsertionCallback, ModelFinalizationMappings mappings)
        {
            return new RoomObstacle(sourceElement, mappingsInsertionCallback, mappings);
        }

        protected override bool ApplyLogicalOptionsEffects(ReadOnlyLogicalOptions logicalOptions)
        {
            Requires.ApplyLogicalOptions(logicalOptions);

            // This obstacle's flat requirements must be fulfilled regardless of where and how the obstacle is destroyed.
            // So if it becomes impossible, the obstacle becomes impossible to destroy.
            return Requires.UselessByLogicalOptions;
        }

        public void InitializeProperties(SuperMetroidModel model, UnfinalizedRoom room)
        {
            Room = room;
        }

        public IEnumerable<string> InitializeReferencedLogicalElementProperties(SuperMetroidModel model, UnfinalizedRoom room)
        {
            List<string> unhandled = new List<string>();

            unhandled.AddRange(Requires.InitializeReferencedLogicalElementProperties(model, room));

            return unhandled.Distinct();
        }
    }
}
