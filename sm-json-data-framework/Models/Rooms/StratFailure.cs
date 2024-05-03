using sm_json_data_framework.Models.Raw.Rooms;
using sm_json_data_framework.Models.Requirements;
using sm_json_data_framework.Models.Rooms.Nodes;
using sm_json_data_framework.Options;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.Json.Serialization;

namespace sm_json_data_framework.Models.Rooms
{
    public class StratFailure : AbstractModelElement, InitializablePostDeserializeInRoom
    {
        public string Name { get; set; }

        [JsonPropertyName("leadsToNode")]
        public int? LeadsToNodeId { get; set; }

        /// <summary>
        /// <para>Not available before <see cref="Initialize(SuperMetroidModel, Room)"/> has been called.</para>
        /// <para>The node that this strat failure leads to, if it leads to a node</para>
        /// </summary>
        [JsonIgnore]
        public RoomNode LeadsToNode { get; set; }

        public LogicalRequirements Cost { get; set; } = new LogicalRequirements();

        public bool Softlock { get; set; } = false;

        public bool ClearsPreviousNode { get; set; } = false;

        public StratFailure()
        {

        }

        public StratFailure(RawStratFailure rawFailure, LogicalElementCreationKnowledgeBase knowledgeBase)
        {
            Name = rawFailure.Name;
            LeadsToNodeId = rawFailure.LeadsToNode;
            Cost = rawFailure.Cost.ToLogicalRequirements(knowledgeBase);
            Softlock = rawFailure.Softlock;
            ClearsPreviousNode = rawFailure.ClearsPreviousNode;
        }

        protected override bool ApplyLogicalOptionsEffects(ReadOnlyLogicalOptions logicalOptions)
        {
            Cost.ApplyLogicalOptions(logicalOptions);

            // A failure object is not useless even if it describes an effective softlock
            return false;
        }

        public void InitializeProperties(SuperMetroidModel model, Room room)
        {
            // Initialize LeadsToNode
            if (LeadsToNodeId != null)
            {
                LeadsToNode = room.Nodes[(int)LeadsToNodeId];
            }
        }

        public bool CleanUpUselessValues(SuperMetroidModel model, Room room)
        {
            // Nothing relevant to clean up
            return true;
        }

        public IEnumerable<string> InitializeReferencedLogicalElementProperties(SuperMetroidModel model, Room room)
        {
            List<string> unhandled = new List<string>();

            unhandled.AddRange(Cost.InitializeReferencedLogicalElementProperties(model, room));

            return unhandled.Distinct();
        }
    }
}
