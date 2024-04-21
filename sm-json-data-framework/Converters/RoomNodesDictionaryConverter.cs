using sm_json_data_framework.Models.Rooms.Nodes;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.Json;
using System.Text.Json.Serialization;
using System.Threading.Tasks;

namespace sm_json_data_framework.Converters
{
    /// <summary>
    /// A json converter that can interpert a json array of NodeLocks into a Dictionary of RoomNodes mapped by in-room numerical ID.
    /// Naturally, this kind of mapping does not work if the scope of the array of nodes is larger than a room as there will be duplicate IDs.
    /// </summary>
    public class RoomNodesDictionaryConverter : JsonConverter<IDictionary<int, RoomNode>>
    {
        public override IDictionary<int, RoomNode> Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
        {
            List<RoomNode> nodes = JsonSerializer.Deserialize<List<RoomNode>>(ref reader, options);
            return nodes.ToDictionary(node => node.Id, node => node);
        }

        public override void Write(Utf8JsonWriter writer, IDictionary<int, RoomNode> value, JsonSerializerOptions options)
        {
            // We're focusing on reading json files for now.
            throw new NotImplementedException();
        }
    }
}
