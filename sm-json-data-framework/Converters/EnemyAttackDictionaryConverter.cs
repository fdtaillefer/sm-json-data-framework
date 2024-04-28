using sm_json_data_framework.Models.Enemies;
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
    /// A json converter that can interpert a json array of EnemyAttacks into a Dictionary of EnemyAttacks mapped by name
    /// </summary>
    public class EnemyAttackDictionaryConverter : JsonConverter<IDictionary<string, EnemyAttack>>
    {
        public override IDictionary<string, EnemyAttack> Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
        {
            List<EnemyAttack> attacks = JsonSerializer.Deserialize<List<EnemyAttack>>(ref reader, options);
            return attacks.ToDictionary(attack => attack.Name);
        }

        public override void Write(Utf8JsonWriter writer, IDictionary<string, EnemyAttack> value, JsonSerializerOptions options)
        {
            // We're focusing on reading json files for now.
            throw new NotImplementedException();
        }
    }
}
