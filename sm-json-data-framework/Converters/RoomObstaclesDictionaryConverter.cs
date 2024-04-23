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
    /// A json convert that can interpert a json array of RoomObstacles into a Dictionary of Strats mapped by ID
    /// </summary>
    public class RoomObstaclesDictionaryConverter : JsonConverter<IDictionary<string, RoomObstacle>>
    {
        public override IDictionary<string, RoomObstacle> Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
        {
            List<RoomObstacle> roomObstacles = JsonSerializer.Deserialize<List<RoomObstacle>>(ref reader, options);
            return roomObstacles.ToDictionary(obstacle => obstacle.Id, obstacle => obstacle);
        }

        public override void Write(Utf8JsonWriter writer, IDictionary<string, RoomObstacle> value, JsonSerializerOptions options)
        {
            // We're focusing on reading json files for now.
            throw new NotImplementedException();
        }
    }
}
