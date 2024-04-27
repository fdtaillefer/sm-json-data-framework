using sm_json_data_framework.Models.Raw.Requirements;
using sm_json_data_framework.Models.Requirements.ObjectRequirements;
using sm_json_data_framework.Models.Requirements.StringRequirements;
using sm_json_data_framework.Models.Requirements;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.Json;
using System.Text.Json.Serialization;
using System.Threading.Tasks;
using sm_json_data_framework.Models.Raw.Requirements.ObjectRequirements;

namespace sm_json_data_framework.Converters.Raw
{
    public class RawLogicalRequirementsConverter : JsonConverter<RawLogicalRequirements>
    {
        public override RawLogicalRequirements Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
        {
            // Logical requirements take the form of a json array
            if (reader.TokenType != JsonTokenType.StartArray)
            {
                throw new JsonException("Logical requirements should be an array");
            }

            // Each element of the array will be a logical element.
            // Read each element one at a time until we've gone through the array
            List<AbstractRawLogicalElement> logicalElements = new List<AbstractRawLogicalElement>();
            JsonTokenType tokenType = reader.TokenType;
            while (tokenType != JsonTokenType.EndArray)
            {
                reader.Read();

                // Decide how to create current logical element depending on token type
                tokenType = reader.TokenType;
                if (tokenType != JsonTokenType.EndArray)
                {
                    AbstractRawLogicalElement logicalElement = tokenType switch
                    {
                        JsonTokenType.String => JsonSerializer.Deserialize<RawStringLogicalElement>(ref reader, options),
                        JsonTokenType.StartObject => JsonSerializer.Deserialize<AbstractRawObjectLogicalElement>(ref reader, options),
                        // Other token types are not supported and will fail fast
                    };

                    logicalElements.Add(logicalElement);
                }
            }

            return new RawLogicalRequirements(logicalElements);
        }

        public override void Write(Utf8JsonWriter writer, RawLogicalRequirements value, JsonSerializerOptions options)
        {
            // We're focusing on reading json files for now.
            throw new NotImplementedException();
        }
    }
}
