using sm_json_data_parser.Models.Requirements;
using sm_json_data_parser.Models.Requirements.ObjectRequirements;
using sm_json_data_parser.Models.Requirements.ObjectRequirements.Integers;
using sm_json_data_parser.Models.Requirements.ObjectRequirements.Strings;
using sm_json_data_parser.Models.Requirements.ObjectRequirements.SubObjects;
using sm_json_data_parser.Models.Requirements.ObjectRequirements.SubRequirements;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace sm_json_data_parser.Converters
{
    public class ObjectLogicalElementConverter : JsonConverter<AbstractObjectLogicalElement>
    {
        public static readonly IEnumerable<string> LOGICAL_REQUIREMENT_OBJECTS = new[] { "and", "or" };
        public static readonly IEnumerable<string> INTEGER_OBJECTS = new[] { "acidFrames", "draygonElectricityFrames",
            "energyAtMost", "heatFrames", "hibashiHits", "lavaFrames", "previousNode", "spikeHits", "thornHits"};
        public static readonly IEnumerable<string> STRING_OBJECTS = new[] { "previousStratProperty" };
        public static readonly IEnumerable<string> SUBOBJECT_OBJECTS = new[] { "adjacentRunway", "ammo", "ammoDrain",
            "canComeInCharged", "canShineCharge", "enemyDamage", "enemyKill", "resetRoom"};

        public override AbstractObjectLogicalElement Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
        {
            if(reader.TokenType != JsonTokenType.StartObject)
            {
                throw new JsonException("An object logical element should be an object");
            }

            // Use the property name to decide what to do
            reader.Read();
            if (reader.TokenType != JsonTokenType.PropertyName)
            {
                throw new JsonException("An object logical element should start with a property name");
            }
            string propertyName = reader.GetString();

            // Read the next element, to put the reader in position to deserialize the logical element
            reader.Read();

            // 
            AbstractObjectLogicalElement logicalElement = null;
            // This should be placing us either at a StartObject or StartArray, depending on the property name.
            if (LOGICAL_REQUIREMENT_OBJECTS.Contains(propertyName))
            {
                logicalElement = CreateLogicalElementWithRequirements(ref reader, options, propertyName);
            }
            else if(INTEGER_OBJECTS.Contains(propertyName))
            {
                logicalElement = CreateLogicalElementWithInteger(ref reader, options, propertyName);
            }
            else if(STRING_OBJECTS.Contains(propertyName))
            {
                logicalElement = CreateLogicalElementWithString(ref reader, options, propertyName);
            }
            else if (SUBOBJECT_OBJECTS.Contains(propertyName))
            {
                logicalElement = CreateLogicalElementWithSubObject(ref reader, options, propertyName);
            }
            else
            {
                throw new JsonException($"Object logical element '{propertyName}' is not recognized");
            }

            // Read the end of the object that contained the name of the object
            reader.Read();
            if (reader.TokenType != JsonTokenType.EndObject)
            {
                throw new JsonException("An object logical element should end after one named property");
            }
            return logicalElement;
        }

        public override void Write(Utf8JsonWriter writer, AbstractObjectLogicalElement value, JsonSerializerOptions options)
        {
            throw new NotImplementedException();
        }

        private AbstractObjectLogicalElement CreateLogicalElementWithRequirements(ref Utf8JsonReader reader, JsonSerializerOptions options, string propertyName)
        {
            if (reader.TokenType != JsonTokenType.StartArray)
            {
                throw new JsonException($"Logical element object '{propertyName}' should be an array");
            }
            LogicalRequirements logicalRequirements = JsonSerializer.Deserialize<LogicalRequirements>(ref reader, options);

            AbstractObjectLogicalElementWithSubRequirements logicalElement = propertyName switch
            {
                "and" => new And(),
                "or" => new Or()
            };
            logicalElement.LogicalRequirements = logicalRequirements;

            return logicalElement;
        }

        // STITCHME How can the behavior be overridden? One way would be to delegate to something that can be configured.
        // Possibly some metadata in the options? Possibly another set of options?
        private AbstractObjectLogicalElement CreateLogicalElementWithInteger(ref Utf8JsonReader reader, JsonSerializerOptions options, string propertyName)
        {
            if (reader.TokenType != JsonTokenType.Number)
            {
                throw new JsonException($"Logical element object '{propertyName}' should be a number");
            }
            int value = reader.GetInt32();

            AbstractObjectLogicalElementWithInteger logicalElement = propertyName switch
            {
                "acidFrames" => new AcidFrames(),
                "draygonElectricityFrames" => new DraygonElectricityFrames(),
                "heatFrames" => new HeatFrames(),
                "hibashiHits" => new HibashiHits(),
                "lavaFrames" => new LavaFrames(),
                "energyAtMost" => new EnergyAtMost(),
                "spikeHits" => new SpikeHits(),
                "thornHits" => new ThornHits(),
                "previousNode" => new PreviousNode()
            };
            logicalElement.Value = value;

            return logicalElement;
        }

        private AbstractObjectLogicalElement CreateLogicalElementWithString(ref Utf8JsonReader reader, JsonSerializerOptions options, string propertyName)
        {
            if (reader.TokenType != JsonTokenType.String)
            {
                throw new JsonException($"Logical element object '{propertyName}' should be a string");
            }
            string value = reader.GetString();

            AbstractObjectLogicalElementWithString logicalElement = propertyName switch
            {
                "previousStratProperty" => new PreviousStratProperty()
            };
            logicalElement.Value = value;

            return logicalElement;
        }

        private AbstractObjectLogicalElement CreateLogicalElementWithSubObject(ref Utf8JsonReader reader, JsonSerializerOptions options, string propertyName)
        {
            if (reader.TokenType != JsonTokenType.StartObject)
            {
                throw new JsonException($"Logical element object '{propertyName}' should be an object");
            }

            AbstractObjectLogicalElement logicalElement = propertyName switch
            {
                "ammo" => JsonSerializer.Deserialize<Ammo>(ref reader, options),
                "ammoDrain" => JsonSerializer.Deserialize<AmmoDrain>(ref reader, options),
                "enemyKill" => JsonSerializer.Deserialize<EnemyKill>(ref reader, options),
                "enemyDamage" => JsonSerializer.Deserialize<EnemyDamage>(ref reader, options),
                "adjacentRunway" => JsonSerializer.Deserialize<AdjacentRunway>(ref reader, options),
                "canComeInCharged" => JsonSerializer.Deserialize<CanComeInCharged>(ref reader, options),
                "canShineCharge" => JsonSerializer.Deserialize<CanShineCharge>(ref reader, options),
                "resetRoom" => JsonSerializer.Deserialize<ResetRoom>(ref reader, options)
            };

            return logicalElement;
        }
    }
}
