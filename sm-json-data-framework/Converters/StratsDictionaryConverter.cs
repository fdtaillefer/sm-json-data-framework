using sm_json_data_framework.Models.Requirements;
using sm_json_data_framework.Models.Requirements.ObjectRequirements;
using sm_json_data_framework.Models.Requirements.StringRequirements;
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
    /// A json convert that can interpert a json array of Strats into a Dictionary of Strats mapped by name
    /// </summary>
    public class StratsDictionaryConverter : JsonConverter<IDictionary<string, Strat>>
    {
        public override IDictionary<string, Strat> Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
        {
            List<Strat> strats = JsonSerializer.Deserialize<List<Strat>>(ref reader, options);
            return strats.ToDictionary(strat => strat.Name);
        }

        public override void Write(Utf8JsonWriter writer, IDictionary<string, Strat> value, JsonSerializerOptions options)
        {
            // We're focusing on reading json files for now.
            throw new NotImplementedException();
        }
    }
}
