﻿using sm_json_data_framework.InGameStates;
using sm_json_data_framework.Models.Requirements.ObjectRequirements.Integers;
using sm_json_data_framework.Models.Rooms;
using sm_json_data_framework.Models.Rooms.Nodes;
using sm_json_data_framework.Options;
using sm_json_data_framework.Rules;
using sm_json_data_framework.Utils;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.Json.Serialization;

namespace sm_json_data_framework.Models.Requirements.ObjectRequirements.SubObjects
{
    /// <summary>
    /// A logical element which requires Samus to have been able to come into the room while gathering momentum
    /// </summary>
    public class AdjacentRunway : AbstractObjectLogicalElement<UnfinalizedAdjacentRunway, AdjacentRunway>
    {
        public AdjacentRunway(UnfinalizedAdjacentRunway sourceElement, Action<AdjacentRunway> mappingsInsertionCallback, ModelFinalizationMappings mappings)
            : base(sourceElement, mappingsInsertionCallback)
        {
            UsedTiles = sourceElement.UsedTiles;
            UseFrames = sourceElement.UseFrames;
            OverrideRunwayRequirements = sourceElement.OverrideRunwayRequirements;
            FromNode = sourceElement.FromNode.Finalize(mappings);
            InRoomPath = sourceElement.InRoomPath.AsReadOnly();
            Physics = sourceElement.Physics.AsReadOnly();
        }

        /// <summary>
        /// The node that this element's FromNodeId references.
        /// </summary>
        public RoomNode FromNode { get; }

        /// <summary>
        /// The precise list of nodes that must be traveled by Samus to execute this AdjacentRunway, from FromNode to the node where it's executed.
        /// </summary>
        public IList<int> InRoomPath { get; }

        /// <summary>
        /// The number of tiles Samus needs to use to gain enough momentum at the adjacent runway.
        /// </summary>
        public decimal UsedTiles { get; }

        /// <summary>
        /// The set of acceptable physics at the adjacent door. If the physics at the adjacent door is not in this set, this AdjacentRunway cannot be executed.
        /// </summary>
        public IReadOnlySet<PhysicsEnum> Physics { get; }

        /// <summary>
        /// The number of frames that Samus should expect to spend at the adjacent door, being subjected to the door environment.
        /// </summary>
        public int UseFrames { get; }

        /// <summary>
        /// Indicates whether the requirements on the Runway itself should be ignored.
        /// </summary>
        public bool OverrideRunwayRequirements { get; }

        protected override ExecutionResult ExecutePossible(SuperMetroidModel model, ReadOnlyInGameState inGameState, int times = 1, int previousRoomCount = 0)
        {
            // If no in-room path is specified, then player will be required to have entered at fromNode and not moved
            IEnumerable<int> requiredInRoomPath = (InRoomPath == null || !InRoomPath.Any()) ? new[] { FromNode.Id } : InRoomPath;

            // Find all runways from the previous room that can be retroactively attempted and are long enough.
            // We're calculating runway length to account for open ends, but using 0 for tilesSavedWithStutter because no charging is involved.
            IEnumerable<Runway> retroactiveRunways = inGameState.GetRetroactiveRunways(requiredInRoomPath, Physics, previousRoomCount)
                .Where(r => r.LogicalEffectiveRunwayLengthNoCharge >= UsedTiles);

            // If we found no usable runways, give up
            if (!retroactiveRunways.Any())
            {
                return null;
            }

            // Make sure we're able to use one of the runways (unless we're overriding this step)
            ExecutionResult executionResult;
            if (OverrideRunwayRequirements)
            {
                executionResult = new ExecutionResult(inGameState.Clone());
            }
            else
            {
                (_, executionResult) = retroactiveRunways.Select(runway => runway.AsExecutable(comingIn: false))
                    .ExecuteBest(model, inGameState, times: times, previousRoomCount: previousRoomCount);
            }

            // If retroactive runway execution failed, give up
            if (executionResult == null)
            {
                return executionResult;
            }

            // If we have useFrames, apply them by spending frames at the previous room's exit node
            if (UseFrames > 0)
            {
                executionResult = executionResult.AndThen(UseFramesExecution, model, previousRoomCount: previousRoomCount + 1);
            }

            return executionResult;

            // Note that there are no concerns here about unlocking the previous door, because unlocking a door to use it cannot be done retroactively.
            // It has to have already been done in order to use the door in the first place.
        }

        IExecutable _useFramesExecution = null;
        /// <summary>
        /// An IExecutable for spending frames at the last visited node in a given room. 
        /// This only really makes sense if that node is a door, since otherwise there is no DoorEnvironment available.
        /// </summary>
        public IExecutable UseFramesExecution
        {
            get
            {
                if (_useFramesExecution == null)
                {
                    _useFramesExecution = new UseFramesExecution(UseFrames);
                }
                return _useFramesExecution;
            }
        }

        protected override void PropagateLogicalOptions(ReadOnlyLogicalOptions logicalOptions, SuperMetroidModel model)
        {
            // Nothing to do here
        }

        protected override bool CalculateLogicallyNever(SuperMetroidModel model)
        {
            // This could be impossible, but that depends on layout and not logic, and is beyond the scope of this method.
            return false;
        }

        protected override bool CalculateLogicallyAlways(SuperMetroidModel model)
        {
            // This could be always possible, but that depends on layout and not logic, and is beyond the scope of this method.
            return false;
        }

        protected override bool CalculateLogicallyFree(SuperMetroidModel model)
        {
            // This could be always free (using an adjacent runway never costs resources), but that depends on layout and not logic, and is beyond the scope of this method.
            return false;
        }
    }

    /// <summary>
    /// An IExecutable for spending frames at the last visited node in a given room. This only really makes sense if that node is a door, since otherwise there is no DoorEnvironment available.
    /// </summary>
    internal class UseFramesExecution : IExecutable
    {
        private int Frames { get; set; }

        public UseFramesExecution(int frames)
        {
            Frames = frames;
        }

        public ExecutionResult Execute(SuperMetroidModel model, ReadOnlyInGameState inGameState, int times = 1, int previousRoomCount = 0)
        {
            List<IExecutable> frameExecutables = new List<IExecutable>();

            PhysicsEnum? physics = inGameState.GetCurrentDoorPhysics(previousRoomCount);
            if (physics != null)
            {
                frameExecutables.Add(physics.Value.FramesExecutable(Frames, model.AppliedLogicalOptions, model));
            }

            if (inGameState.IsHeatedRoom(previousRoomCount))
            {
                HeatFrames heatFrames = new HeatFrames(Frames);
                // You normally shouldn't apply logical options out of the blue, but this is a temporary element with no ties to any elements in the model,
                // and it needs the logical options to have access to leniency
                heatFrames.ApplyLogicalOptions(model.AppliedLogicalOptions, model);
                frameExecutables.Add(heatFrames);
            }

            return frameExecutables.ExecuteAll(model, inGameState, previousRoomCount: previousRoomCount);
        }
    }

    public class UnfinalizedAdjacentRunway : AbstractUnfinalizedObjectLogicalElement<UnfinalizedAdjacentRunway, AdjacentRunway>
    {
        public int FromNodeId { get; set; }

        /// <summary>
        /// <para>Only available after a call to <see cref="InitializeReferencedLogicalElementProperties(UnfinalizedSuperMetroidModel, UnfinalizedRoom)"/>.</para>
        /// <para>The node that this element's FromNodeId references. </para>
        /// </summary>
        public UnfinalizedRoomNode FromNode {get;set;}

        public IList<int> InRoomPath { get; set; } = new List<int>();

        public decimal UsedTiles { get; set; }

        public ISet<PhysicsEnum> Physics { get; set; }

        public int UseFrames { get; set; } = 0;

        public bool OverrideRunwayRequirements { get; set; } = false;

        protected override AdjacentRunway CreateFinalizedElement(UnfinalizedAdjacentRunway sourceElement, Action<AdjacentRunway> mappingsInsertionCallback, ModelFinalizationMappings mappings)
        {
            return new AdjacentRunway(sourceElement, mappingsInsertionCallback, mappings);
        }

        public override IEnumerable<string> InitializeReferencedLogicalElementProperties(UnfinalizedSuperMetroidModel model, UnfinalizedRoom room)
        {
            if (room.Nodes.TryGetValue(FromNodeId, out UnfinalizedRoomNode node))
            {
                FromNode = node;
                return Enumerable.Empty<string>();
            }
            else
            {
                return new[] { $"Node {FromNodeId} in room {room.Name}" };
            }
        }
    }
}
