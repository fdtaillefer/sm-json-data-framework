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
        public RoomObstacle(UnfinalizedRoomObstacle innerElement, Action<RoomObstacle> mappingsInsertionCallback, ModelFinalizationMappings mappings)
            : base(innerElement, mappingsInsertionCallback)
        {
            Id = innerElement.Id;
            Name = innerElement.Name;
            ObstacleType = innerElement.ObstacleType;
            Requires = innerElement.Requires.Finalize(mappings);
            Room = innerElement.Room.Finalize(mappings);
        }

        /// <summary>
        /// An arbitrary ID that identifies this obstacle. It is only unique within this obstacle's room.
        /// </summary>
        public string Id { get; }

        /// <summary>
        /// A human-legible name that identifies this obstacle.
        /// </summary>
        public string Name { get; }

        /// <summary>
        /// The type of obstacle this is.
        /// </summary>
        public ObstacleTypeEnum ObstacleType { get; }

        /// <summary>
        /// Logical requirements that must systematically be fulfilled in order to destroy this obstacle, regardless of any other context.
        /// Often, different strats that destroy obstacles also come with additional logical requirements based on the strat's context.
        /// </summary>
        public LogicalRequirements Requires { get; }

        /// <summary>
        /// The room in which this obstacle is.
        /// </summary>
        public Room Room { get; }

        protected override void PropagateLogicalOptions(ReadOnlyLogicalOptions logicalOptions)
        {
            Requires.ApplyLogicalOptions(logicalOptions);
        }

        protected override void UpdateLogicalProperties()
        {
            base.UpdateLogicalProperties();
            LogicallyIndestructible = CalculateLogicallyIndestructible();
        }

        public override bool CalculateLogicallyRelevant()
        {
            // An obstacle is always relevant.
            // Even if indestructible, and even if it has base requirements that are always free (because specific strats likely define additional requirements).
            return true;
        }

        /// <summary>
        /// If true, then it's always impossible to destroy obstacle given the current logical options, regardless of in-game state.
        /// This would mean this obstacle can never be destroyed by any strat, and the only way to possibly deal with it would be if a strat has a way to bypass it.
        /// </summary>
        public bool LogicallyIndestructible { get; private set; }

        /// <summary>
        /// Calculates what the value of <see cref="LogicallyIndestructible"/> should currently be.
        /// </summary>
        /// <returns></returns>
        protected bool CalculateLogicallyIndestructible()
        {
            // If the base destruction requirements are impossible to fulfill, this can never be destroyed
            // If they are possible to fulfill, the obstacle might still be indestructible but we can't tell based on the data we have here.
            return Requires.LogicallyNever;
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
