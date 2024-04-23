using sm_json_data_framework.Models.Rooms;
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
    /// A json converter that can interpert a json array of Links into a Dictionary of Links mapped by origin node ID.
    /// </summary>
    public class LinksDictionaryConverter : JsonConverter<IDictionary<int, Link>>
    {
        public override IDictionary<int, Link> Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
        {
            List<Link> links = JsonSerializer.Deserialize<List<Link>>(ref reader, options);
            return links.ToDictionary(locks => locks.FromNodeId, locks => locks);
        }

        public override void Write(Utf8JsonWriter writer, IDictionary<int, Link> value, JsonSerializerOptions options)
        {
            // We're focusing on reading json files for now.
            throw new NotImplementedException();
        }
    }
}
