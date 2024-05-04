using sm_json_data_framework.Models.InGameStates;
using sm_json_data_framework.Models.Navigation;
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
    public class StratObstacle : AbstractModelElement, InitializablePostDeserializeInRoom
    {
        [JsonPropertyName("id")]
        public string ObstacleId { get; set; }

        /// <summary>
        /// <para>Not available before <see cref="Initialize(SuperMetroidModel, Room)"/> has been called.</para>
        /// <para>The RoomObstacle that this StratObstacle indicates must be passed through</para>
        /// </summary>
        [JsonIgnore]
        public RoomObstacle Obstacle { get; set; }

        public LogicalRequirements Requires { get; set; } = new LogicalRequirements();

        /// <summary>
        /// LogicalRequirements to bypass this obstacle without destroying it when doing the associated strat. If this is null, the obstacle cannot be bypassed.
        /// </summary>
        public LogicalRequirements Bypass { get; set; }

        [JsonPropertyName("additionalObstacles")]
        public ISet<string> AdditionalObstacleIds { get; set; } = new HashSet<string>();

        /// <summary>
        /// <para>Not available before <see cref="Initialize(SuperMetroidModel, Room)"/> has been called.</para>
        /// <para>The additional RoomObstacles that are destroyed alongside this StratObstacle</para>
        /// </summary>
        [JsonIgnore]
        public IList<RoomObstacle> AdditionalObstacles { get; set; }

        public StratObstacle()
        {

        }

        public StratObstacle(RawStratObstacle rawStratObstacle, LogicalElementCreationKnowledgeBase knowledgeBase)
        {
            ObstacleId = rawStratObstacle.Id;
            Requires = rawStratObstacle.Requires.ToLogicalRequirements(knowledgeBase);
            if(rawStratObstacle.Bypass != null)
            {
                Bypass = rawStratObstacle.Bypass.ToLogicalRequirements(knowledgeBase);
            }
            AdditionalObstacleIds = new HashSet<string>(rawStratObstacle.AdditionalObstacles);
        }
        protected override bool ApplyLogicalOptionsEffects(ReadOnlyLogicalOptions logicalOptions)
        {
            Requires.ApplyLogicalOptions(logicalOptions);
            if (Bypass != null)
            {
                Bypass.ApplyLogicalOptions(logicalOptions);
            }

            // A StratObstacle never becomes truly impossible via logical options, as it's still possible to execute it
            // if the obstacle was destroyed previously from elsehwere
            return false;
        }

        public void InitializeProperties(SuperMetroidModel model, Room room)
        {
            // Initialize Obstacle
            Obstacle = room.Obstacles[ObstacleId];

            // Initialize AdditionalObstacles
            AdditionalObstacles = AdditionalObstacleIds.Select(id => room.Obstacles[id]).ToList();
        }

        public IEnumerable<string> InitializeReferencedLogicalElementProperties(SuperMetroidModel model, Room room)
        {
            List<string> unhandled = new List<string>();

            unhandled.AddRange(Requires.InitializeReferencedLogicalElementProperties(model, room));

            if (Bypass != null)
            {
                unhandled.AddRange(Bypass.InitializeReferencedLogicalElementProperties(model, room));
            }

            return unhandled.Distinct();
        }

        IExecutable _destroyExecution = null;
        /// <summary>
        /// An IExecutable that corresponds to destroying this obstacle.
        /// </summary>
        public IExecutable DestroyExecution
        {
            get
            {
                if(_destroyExecution == null)
                {
                    _destroyExecution = new DestroyExecution(this);
                }
                return _destroyExecution;
            }
        }

        IExecutable _bypassExecution = null;
        /// <summary>
        /// An IExecutable that corresponds to bypassing this obstacle.
        /// </summary>
        public IExecutable BypassExecution {
            get
            {
                if(_bypassExecution == null)
                {
                    _bypassExecution = new BypassExecution(this);
                }
                return _bypassExecution;
            }
        }
    }

    /// <summary>
    /// A class that encloses the destruction of a StratObstacle in an IExecutable interface.
    /// </summary>
    internal class DestroyExecution : IExecutable
    {
        private StratObstacle StratObstacle { get; set; }

        public DestroyExecution(StratObstacle stratObstacle)
        {
            StratObstacle = stratObstacle;
        }
        public ExecutionResult Execute(SuperMetroidModel model, ReadOnlyInGameState inGameState, int times = 1, int previousRoomCount = 0)
        {
            // There may be up to 2 requirements. This StratObstacle may have some, and the RoomObstacle may also have some general requirements that apply to any strat.

            // Start with the RoomObstacle's requirements
            ExecutionResult result = StratObstacle.Obstacle.Requires.Execute(model, inGameState, times: times, previousRoomCount: previousRoomCount);
            // If we couldn't execute the RoomObstacle's requirements, give up
            if (result == null)
            {
                return null;
            }

            // Add this specific StratObstacle's requirements
            result = result.AndThen(StratObstacle.Requires, model, times: times, previousRoomCount: previousRoomCount);
            // If that failed, give up
            if (result == null)
            {
                return null;
            }

            // We have succeeded, but we must update the ExecutionResult and its InGameState to reflect any destroyed obstacles
            result.ApplyDestroyObstacles(new[] { StratObstacle.Obstacle }.Concat(StratObstacle.AdditionalObstacles), previousRoomCount);

            return result;
        }
    }

    /// <summary>
    /// A class that encloses the bypassing of a StratObstacle in an IExecutable interface.
    /// </summary>
    internal class BypassExecution: IExecutable
    {
        private StratObstacle StratObstacle { get; set; }

        public BypassExecution(StratObstacle stratObstacle)
        {
            StratObstacle = stratObstacle;
        }
        public ExecutionResult Execute(SuperMetroidModel model, ReadOnlyInGameState inGameState, int times = 1, int previousRoomCount = 0)
        {
            // The bypass attempt fails if there's no way to bypass
            if (StratObstacle.Bypass == null)
            {
                return null;
            }
            else
            {
                return StratObstacle.Bypass.Execute(model, inGameState, times: times, previousRoomCount: previousRoomCount);
            }
        }
    }
}
