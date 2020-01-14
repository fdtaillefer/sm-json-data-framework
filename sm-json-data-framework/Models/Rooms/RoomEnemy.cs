using sm_json_data_framework.Models.Enemies;
using sm_json_data_framework.Models.Requirements;
using sm_json_data_framework.Models.Rooms.Nodes;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.Json.Serialization;

namespace sm_json_data_framework.Models.Rooms
{
    public class RoomEnemy : InitializablePostDeserializeInRoom
    {
        [JsonPropertyName("name")]
        public string EnemyName { get; set; }

        /// <summary>
        /// <para>Not available before <see cref="Initialize(SuperMetroidModel, Room)"/> has been called.</para>
        /// <para>The actual Enemy this RoomEnemy represents.</para>
        /// </summary>
        [JsonIgnore]
        public Enemy Enemy { get; set; }

        public int Quantity { get; set; }

        [JsonPropertyName("homeNodes")]
        public IEnumerable<int> HomeNodeIds { get; set; } = Enumerable.Empty<int>();

        /// <summary>
        /// <para>Not available before <see cref="Initialize(SuperMetroidModel, Room)"/> has been called.</para>
        /// <para>The nodes in which this enemy roams, mapped by their node ID. Mutually-exclusive with <see cref="BetweenNodes"/>.</para>
        /// </summary>
        [JsonIgnore]
        public Dictionary<int, RoomNode> HomeNodes { get; set; }

        [JsonPropertyName("betweenNodes")]
        public IEnumerable<int> BetweenNodeIds { get; set; } = Enumerable.Empty<int>();

        /// <summary>
        /// <para>Not available before <see cref="Initialize(SuperMetroidModel, Room)"/> has been called.</para>
        /// <para>Contains two nodes between which this enemy roams (without ever actually being in either), mapped by their node ID. Mutually-exclusive with <see cref="HomeNodes"/>.</para>
        /// </summary>
        [JsonIgnore]
        public Dictionary<int, RoomNode> BetweenNodes { get; set; }

        public LogicalRequirements Spawn { get; set; } = new LogicalRequirements();

        public LogicalRequirements StopSpawn { get; set; } = new LogicalRequirements();

        public LogicalRequirements DropRequires { get; set; } = new LogicalRequirements();

        public void Initialize(SuperMetroidModel model, Room room)
        {
            Enemy = model.Enemies[EnemyName];

            HomeNodes = HomeNodeIds.Select(id => room.Nodes[id]).ToDictionary(n => n.Id);

            BetweenNodes = BetweenNodeIds.Select(id => room.Nodes[id]).ToDictionary(n => n.Id);
        }

        public IEnumerable<string> InitializeReferencedLogicalElementProperties(SuperMetroidModel model, Room room)
        {
            List<string> unhandled = new List<string>();

            unhandled.AddRange(Spawn.InitializeReferencedLogicalElementProperties(model, room));

            unhandled.AddRange(StopSpawn.InitializeReferencedLogicalElementProperties(model, room));

            unhandled.AddRange(DropRequires.InitializeReferencedLogicalElementProperties(model, room));

            return unhandled.Distinct();
        }
    }
}
