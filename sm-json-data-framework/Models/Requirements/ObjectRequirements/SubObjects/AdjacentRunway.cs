using sm_json_data_framework.Models.InGameStates;
using sm_json_data_framework.Models.Requirements.ObjectRequirements.Integers;
using sm_json_data_framework.Models.Rooms;
using sm_json_data_framework.Models.Rooms.Nodes;
using sm_json_data_framework.Rules;
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
    public class AdjacentRunway : AbstractObjectLogicalElement
    {
        [JsonPropertyName("fromNode")]
        public int FromNodeId { get; set; }

        /// <summary>
        /// <para>Only available after a call to <see cref="InitializeReferencedLogicalElementProperties(SuperMetroidModel, Room)"/>.</para>
        /// <para>The node that this element's FromNodeId references. </para>
        /// </summary>
        [JsonIgnore]
        public RoomNode FromNode {get;set;}

        public IEnumerable<int> InRoomPath { get; set; } = Enumerable.Empty<int>();

        public decimal UsedTiles { get; set; }

        public ISet<PhysicsEnum> Physics { get; set; }

        public int UseFrames { get; set; } = 0;

        public bool OverrideRunwayRequirements { get; set; } = false;

        public override IEnumerable<string> InitializeReferencedLogicalElementProperties(SuperMetroidModel model, Room room)
        {
            if (room.Nodes.TryGetValue(FromNodeId, out RoomNode node))
            {
                FromNode = node;
                return Enumerable.Empty<string>();
            }
            else
            {
                return new[] { $"Node {FromNodeId} in room {room.Name}" };
            }
        }

        public override ExecutionResult Execute(SuperMetroidModel model, ReadOnlyInGameState inGameState, int times = 1, int previousRoomCount = 0)
        {
            // If no in-room path is specified, then player will be required to have entered at fromNode and not moved
            IEnumerable<int> requiredInRoomPath = (InRoomPath == null || !InRoomPath.Any()) ? new[] { FromNodeId } : InRoomPath;

            // Find all runways from the previous room that can be retroactively attempted and are long enough.
            // We're calculating runway length to account for open ends, but using 0 for tilesSavedWithStutter because no charging is involved.
            IEnumerable<Runway> retroactiveRunways = inGameState.GetRetroactiveRunways(requiredInRoomPath, Physics, previousRoomCount)
                .Where(r => model.Rules.CalculateEffectiveRunwayLength(r, tilesSavedWithStutter: 0) >= UsedTiles);

            // If we found no usable runways, give up
            if (!retroactiveRunways.Any())
            {
                return null;
            }

            // Make sure we're able to use one of the runways (unless we're overriding this step)
            ExecutionResult executionResult;
            if(OverrideRunwayRequirements)
            {
                executionResult = new ExecutionResult(inGameState.Clone());
            }
            else
            {
                (_, executionResult) = model.ExecuteBest(retroactiveRunways.Select(runway => runway.AsExecutable(comingIn: false)),
                    inGameState, times: times, previousRoomCount: previousRoomCount);
            }

            // If retroactive runway execution failed, give up
            if(executionResult == null)
            {
                return executionResult;
            }

            // If we have useFrames, apply them by spending frames at the previous room's exit node
            if(UseFrames > 0)
            {
                executionResult = executionResult.AndThen(UseFramesExecution, model, previousRoomCount: previousRoomCount + 1);
            }

            return executionResult;

            // Note that there are no concerns here about unlocking the previous door, because unlocking a door to use it cannot be done retroactively.
            // It has to have already been done in order to use the door in the first place.
        }

        IExecutable _useFramesExecution = null;
        /// <summary>
        /// <para>An IExecutable that corresponds to farming this group of enemies, by camping its spawner(s), killing it repeatedly, and grabbing the drops.
        /// This is repeated until all qualifying resources are filled.</para>
        /// <para>Qualifying resources are determined based on logical options.</para>
        /// <para>For simplicity, a farm execution will be considered a failure if it results in any kind of resource tradeoff.</para>
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
    }

    /// <summary>
    /// An IExecutable for spending frames at the last visited node in a given room. This only really makes sense if that node is a door, since otherwise there is no RoomEnvironment available.
    /// </summary>
    internal class UseFramesExecution : IExecutable
    {
        private int Frames{ get; set; }

        public UseFramesExecution(int frames)
        {
            Frames= frames;
        }

        public ExecutionResult Execute(SuperMetroidModel model, ReadOnlyInGameState inGameState, int times = 1, int previousRoomCount = 0)
        {
            List<IExecutable> frameExecutables = new List<IExecutable>();

            PhysicsEnum? physics = inGameState.GetCurrentDoorPhysics(previousRoomCount);
            if(physics != null)
            {
                frameExecutables.Add(physics.Value.FramesExecutable(Frames));
            }

            if (inGameState.IsHeatedRoom(previousRoomCount))
            {
                frameExecutables.Add(new HeatFrames(Frames));
            }

            return model.ExecuteAll(frameExecutables, inGameState, previousRoomCount: previousRoomCount);
        }
    }
}
