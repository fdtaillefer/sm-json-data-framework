using sm_json_data_framework.Models.Raw.Rooms;
using sm_json_data_framework.Models.Requirements;
using sm_json_data_framework.Options;
using sm_json_data_framework.Rules;
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
        public RoomObstacle(UnfinalizedRoomObstacle sourceElement, Action<RoomObstacle> mappingsInsertionCallback, ModelFinalizationMappings mappings)
            : base(sourceElement, mappingsInsertionCallback)
        {
            Id = sourceElement.Id;
            Name = sourceElement.Name;
            ObstacleType = sourceElement.ObstacleType;
            Requires = sourceElement.Requires.Finalize(mappings);
            Room = sourceElement.Room.Finalize(mappings);
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

        protected override void PropagateLogicalOptions(ReadOnlyLogicalOptions logicalOptions, SuperMetroidModel model)
        {
            Requires.ApplyLogicalOptions(logicalOptions, model);
        }

        protected override void UpdateLogicalProperties(SuperMetroidModel model)
        {
            base.UpdateLogicalProperties(model);
            LogicallyIndestructible = CalculateLogicallyIndestructible(model);
            LogicallyAlwaysDestructible = CalculateLogicallyAlwaysDestructible(model);
            LogicallyDestructibleForFree = CalculateLogicallyDestructibleForFree(model);
        }

        public override bool CalculateLogicallyRelevant(SuperMetroidModel model)
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
        /// <param name="model">The model this element belongs to</param>
        /// <returns></returns>
        protected bool CalculateLogicallyIndestructible(SuperMetroidModel model)
        {
            // If the base destruction requirements are impossible to fulfill, this can never be destroyed
            // If they are possible to fulfill, the obstacle might still be indestructible but we can't tell based on the data we have here.
            return Requires.LogicallyNever;
        }

        /// <summary>
        /// If true, the "common" requirements for destroying this obstacle are always possible (though not necessarily for free) given the current logical options,
        /// regardless of in-game state. This makes no statement on any strat's additional destruction requirements
        /// </summary>
        public bool LogicallyAlwaysDestructible { get; private set; }

        /// <summary>
        /// Calculates what the value of <see cref="LogicallyAlwaysDestructible"/> should currently be.
        /// </summary>
        /// <param name="model">The model this element belongs to</param>
        /// <returns></returns>
        protected bool CalculateLogicallyAlwaysDestructible(SuperMetroidModel model)
        {
            return Requires.LogicallyAlways;
        }

        /// <summary>
        /// If true, the "common" requirements for destroying this obstacle are always possible for free given the current logical options,
        /// regardless of in-game state. This makes no statement on any strat's additional destruction requirements
        /// </summary>
        public bool LogicallyDestructibleForFree { get; private set; }

        /// <summary>
        /// Calculates what the value of <see cref="LogicallyDestructibleForFree"/> should currently be.
        /// </summary>
        /// <param name="model">The model this element belongs to</param>
        /// <returns></returns>
        protected bool CalculateLogicallyDestructibleForFree(SuperMetroidModel model)
        {
            return Requires.LogicallyFree;
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
