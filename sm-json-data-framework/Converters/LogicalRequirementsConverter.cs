using sm_json_data_parser.Models.Requirements;
using sm_json_data_parser.Models.Requirements.ObjectRequirements;
using sm_json_data_parser.Models.Requirements.StringRequirements;
using sm_json_data_parser.Models.Rooms;
using System;
using System.Collections.Generic;
using System.Text;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace sm_json_data_parser.Converters
{
    public class LogicalRequirementsConverter : JsonConverter<LogicalRequirements>
    {
        public override LogicalRequirements Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
        {
            // Logical requirements take the form of a json array
            if (reader.TokenType != JsonTokenType.StartArray)
            {
                throw new JsonException("Logical requirements should be an array");
            }

            // Each element of the array will be a logical element.
            // Read each element one at a time until we've gone through the array
            List<AbstractLogicalElement> logicalElements = new List<AbstractLogicalElement>();
            JsonTokenType tokenType = reader.TokenType;
            while(tokenType != JsonTokenType.EndArray)
            {
                reader.Read();

                // Decide how to create current logical element depending on token type
                tokenType = reader.TokenType;
                if (tokenType != JsonTokenType.EndArray)
                {
                    AbstractLogicalElement logicalElement = tokenType switch
                    {
                        JsonTokenType.String => JsonSerializer.Deserialize<AbstractStringLogicalElement>(ref reader, options),
                        JsonTokenType.StartObject => JsonSerializer.Deserialize<AbstractObjectLogicalElement>(ref reader, options),
                        // Other token types are not supported and will fail fast
                    };

                    logicalElements.Add(logicalElement);
                }
            }

            return new LogicalRequirements(logicalElements);
        }

        public override void Write(Utf8JsonWriter writer, LogicalRequirements value, JsonSerializerOptions options)
        {
            // We're focusing on reading json files for now.
            throw new NotImplementedException();
        }
    }
}
