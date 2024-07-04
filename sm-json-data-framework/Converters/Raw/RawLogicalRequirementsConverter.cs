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
            List<AbstractRawLogicalElement> logicalElements = new List<AbstractRawLogicalElement>();
            // Logical requirements take the form of a json array or a single logical element
            if (reader.TokenType == JsonTokenType.StartArray)
            {
                // Each element of the array will be a logical element.
                // Read each element one at a time until we've gone through the array
                JsonTokenType tokenType = reader.TokenType;
                while (tokenType != JsonTokenType.EndArray)
                {
                    reader.Read();

                    // Decide how to create current logical element depending on token type
                    tokenType = reader.TokenType;
                    if (tokenType != JsonTokenType.EndArray)
                    {
                        logicalElements.Add(InterpretLogicalElementToken(ref reader, options));
                    }
                }
            }
            else
            {
                logicalElements.Add(InterpretLogicalElementToken(ref reader, options));
            }

            return new RawLogicalRequirements(logicalElements);
        }

        private AbstractRawLogicalElement InterpretLogicalElementToken(ref Utf8JsonReader reader, JsonSerializerOptions options)
        {
            JsonTokenType tokenType = reader.TokenType;
            return tokenType switch
            {
                JsonTokenType.String => JsonSerializer.Deserialize<RawStringLogicalElement>(ref reader, options),
                JsonTokenType.StartObject => JsonSerializer.Deserialize<AbstractRawObjectLogicalElement>(ref reader, options),
                _ => throw new NotImplementedException($"Token type {tokenType} is not supported in logical requirements."),
            };
        }

        public override void Write(Utf8JsonWriter writer, RawLogicalRequirements value, JsonSerializerOptions options)
        {
            // We're focusing on reading json files for now.
            throw new NotImplementedException();
        }
    }
}
