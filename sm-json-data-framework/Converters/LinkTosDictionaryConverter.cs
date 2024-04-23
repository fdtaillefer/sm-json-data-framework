using sm_json_data_framework.Models.Rooms;
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
    /// A json converter that can interpert a json array of LinkTos into a Dictionary of LinkTos mapped by target node ID.
    /// </summary>
    public class LinkTosDictionaryConverter : JsonConverter<IDictionary<int, LinkTo>>
    {
        public override IDictionary<int, LinkTo> Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
        {
            List<LinkTo> linkTos = JsonSerializer.Deserialize<List<LinkTo>>(ref reader, options);
            return linkTos.ToDictionary(linkTo => linkTo.TargetNodeId, linkTo => linkTo);
        }

        public override void Write(Utf8JsonWriter writer, IDictionary<int, LinkTo> value, JsonSerializerOptions options)
        {
            // We're focusing on reading json files for now.
            throw new NotImplementedException();
        }
    }
}
