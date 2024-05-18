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
        public bool IndestructibleByLogicalOptions { get; protected set; } = false;

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
        public string Id => InnerElement.Id;

        /// <summary>
        /// A human-legible name that identifies this obstacle.
        /// </summary>
        public string Name => InnerElement.Name;

        /// <summary>
        /// The type of obstacle this is.
        /// </summary>
        public ObstacleTypeEnum ObstacleType => InnerElement.ObstacleType;

        /// <summary>
        /// Logical requirements that must systematically be fulfilled in order to destroy this obstacle, regardless of any other context.
        /// Often, different strats that destroy obstacles also come with additional logical requirements based on the strat's context.
        /// </summary>
        public LogicalRequirements Requires { get; }

        /// <summary>
        /// The room in which this obstacle is.
        /// </summary>
        public Room Room { get; }

        protected override bool PropagateLogicalOptions(ReadOnlyLogicalOptions logicalOptions)
        {
            Requires.ApplyLogicalOptions(logicalOptions);

            // While it's possible for an obstacle to become logically indestructible, we can't say it's useless
            // because it still blocks the player, and also because it could still potentially be bypassed in some strats.
            // It's still useful to know if this is indestructible though
            IndestructibleByLogicalOptions = Requires.UselessByLogicalOptions;
            return false;
        }
    }

    public class UnfinalizedRoomObstacle : AbstractUnfinalizedModelElement<UnfinalizedRoomObstacle, RoomObstacle>, InitializablePostDeserializeInRoom
    {
        public string Id { get; set; }

        public string Name { get; set; }

        public ObstacleTypeEnum ObstacleType { get; set; }

        public UnfinalizedLogicalRequirements Requires { get; set; } = new UnfinalizedLogicalRequirements();

        /// <summary>
        /// <para>Not available before <see cref="Initialize(UnfinalizedSuperMetroidModel, UnfinalizedRoom)"/> has been called.</para>
        /// <para>The room in which this obstacle is.</para>
        /// </summary>
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

        public void InitializeProperties(UnfinalizedSuperMetroidModel model, UnfinalizedRoom room)
        {
            Room = room;
        }

        public IEnumerable<string> InitializeReferencedLogicalElementProperties(UnfinalizedSuperMetroidModel model, UnfinalizedRoom room)
        {
            List<string> unhandled = new List<string>();

            unhandled.AddRange(Requires.InitializeReferencedLogicalElementProperties(model, room));

            return unhandled.Distinct();
        }
    }
}
