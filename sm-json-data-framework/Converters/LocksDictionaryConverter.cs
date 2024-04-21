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
    /// A json converter that can interpert a json array of NodeLocks into a Dictionary of NodeLocks mapped by name
    /// </summary>
    public class LocksDictionaryConverter : JsonConverter<IDictionary<string, NodeLock>>
    {
        public override IDictionary<string, NodeLock> Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
        {
            List<NodeLock> locks = JsonSerializer.Deserialize<List<NodeLock>>(ref reader, options);
            return locks.ToDictionary(locks => locks.Name, locks => locks);
        }

        public override void Write(Utf8JsonWriter writer, IDictionary<string, NodeLock> value, JsonSerializerOptions options)
        {
            // We're focusing on reading json files for now.
            throw new NotImplementedException();
        }
    }
}
