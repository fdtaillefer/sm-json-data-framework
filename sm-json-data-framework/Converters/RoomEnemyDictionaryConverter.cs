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
    /// A json converter that can interpert a json array of RoomEnemy into a Dictionary of RoomEnemy mapped by ID
    /// </summary>
    public class RoomEnemyDictionaryConverter : JsonConverter<IDictionary<string, RoomEnemy>>
    {
        public override IDictionary<string, RoomEnemy> Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
        {
            List<RoomEnemy> roomEnemies = JsonSerializer.Deserialize<List<RoomEnemy>>(ref reader, options);
            return roomEnemies.ToDictionary(roomEnemy => roomEnemy.Id);
        }

        public override void Write(Utf8JsonWriter writer, IDictionary<string, RoomEnemy> value, JsonSerializerOptions options)
        {
            // We're focusing on reading json files for now.
            throw new NotImplementedException();
        }
    }
}
