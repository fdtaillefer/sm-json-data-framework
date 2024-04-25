using sm_json_data_framework.Models.Rooms;
using sm_json_data_framework.Models.Rooms.Nodes;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.Json.Serialization;

namespace sm_json_data_framework.Models.Requirements.ObjectRequirements.Integers
{
    public abstract class AbstractObjectLogicalElementWithNodeId : AbstractObjectLogicalElementWithInteger
    {
        public AbstractObjectLogicalElementWithNodeId()
        {

        }

        public AbstractObjectLogicalElementWithNodeId(int id) : base(id)
        {

        }

        /// <summary>
        /// <para>Only available after a call to <see cref="InitializeReferencedLogicalElementProperties(SuperMetroidModel, Room)"/>.</para>
        /// <para>The node that this element's value references. </para>
        /// </summary>
        [JsonIgnore]
        public RoomNode Node {get;set;}

        public override IEnumerable<string> InitializeReferencedLogicalElementProperties(SuperMetroidModel model, Room room)
        {
            if (room.Nodes.TryGetValue(Value, out RoomNode node))
            {
                Node = node;
                return Enumerable.Empty<string>();
            }
            else
            {
                return new[] { $"Node {Value} in room {room.Name}" };
            }
        }
    }
}
