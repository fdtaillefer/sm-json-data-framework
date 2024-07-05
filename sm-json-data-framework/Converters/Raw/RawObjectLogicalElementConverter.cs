using sm_json_data_framework.Models.Raw.Requirements;
using sm_json_data_framework.Models.Raw.Requirements.ObjectRequirements;
using sm_json_data_framework.Models.Raw.Requirements.ObjectRequirements.Arrays;
using sm_json_data_framework.Models.Raw.Requirements.ObjectRequirements.SubObjects;
using sm_json_data_framework.Models.Requirements;
using sm_json_data_framework.Models.Requirements.ObjectRequirements;
using sm_json_data_framework.Models.Requirements.ObjectRequirements.Arrays;
using sm_json_data_framework.Models.Requirements.ObjectRequirements.Integers;
using sm_json_data_framework.Models.Requirements.ObjectRequirements.Strings;
using sm_json_data_framework.Models.Requirements.ObjectRequirements.SubRequirements;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.Json;
using System.Text.Json.Serialization;
using System.Threading.Tasks;

namespace sm_json_data_framework.Converters.Raw
{
    public class RawObjectLogicalElementConverter : JsonConverter<AbstractRawObjectLogicalElement>
    {
        public override AbstractRawObjectLogicalElement Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
        {
            if (reader.TokenType != JsonTokenType.StartObject)
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

            // Convert property name to logicalElementEnum
            ObjectLogicalElementTypeEnum elementTypeEnum
                = (ObjectLogicalElementTypeEnum)Enum.Parse(typeof(ObjectLogicalElementTypeEnum), propertyName, true);

            AbstractRawObjectLogicalElement logicalElement = null;
            // This should be placing us either at a StartObject or StartArray, depending on the property name.
            switch (elementTypeEnum.GetSubType())
            {
                case ObjectLogicalElementSubTypeEnum.SubRequirement:
                    logicalElement = CreateLogicalElementWithRequirements(ref reader, options, propertyName, elementTypeEnum);
                    break;
                case ObjectLogicalElementSubTypeEnum.Array:
                    logicalElement = CreateLogicalElementWithArray(ref reader, options, propertyName, elementTypeEnum);
                    break;
                case ObjectLogicalElementSubTypeEnum.Integer:
                    logicalElement = CreateLogicalElementWithInteger(ref reader, options, propertyName, elementTypeEnum);
                    break;
                case ObjectLogicalElementSubTypeEnum.String:
                    logicalElement = CreateLogicalElementWithString(ref reader, options, propertyName, elementTypeEnum);
                    break;
                case ObjectLogicalElementSubTypeEnum.SubObject:
                    logicalElement = CreateLogicalElementWithSubObject(ref reader, options, propertyName, elementTypeEnum);
                    break;
                default:
                    throw new Exception($"Logical element subtype enum {elementTypeEnum.GetSubType()} not recognized.");
            }

            // Read the end of the object that contained the name of the object
            reader.Read();
            if (reader.TokenType != JsonTokenType.EndObject)
            {
                throw new JsonException("An object logical element should end after one named property");
            }
            return logicalElement;
        }

        public override void Write(Utf8JsonWriter writer, AbstractRawObjectLogicalElement value, JsonSerializerOptions options)
        {
            // We're focusing on reading json files for now.
            throw new NotImplementedException();
        }

        private RawObjectLogicalElementWithSubRequirements CreateLogicalElementWithRequirements(ref Utf8JsonReader reader, JsonSerializerOptions options, 
            string propertyName, ObjectLogicalElementTypeEnum elementTypeEnum)
        {
            RawLogicalRequirements logicalRequirements = JsonSerializer.Deserialize<RawLogicalRequirements>(ref reader, options);
            RawObjectLogicalElementWithSubRequirements logicalElement = new RawObjectLogicalElementWithSubRequirements(propertyName, logicalRequirements);

            return logicalElement;
        }

        private AbstractRawObjectLogicalElement CreateLogicalElementWithArray(ref Utf8JsonReader reader, JsonSerializerOptions options,
            string propertyName, ObjectLogicalElementTypeEnum elementTypeEnum)
        {
            if (reader.TokenType != JsonTokenType.StartArray)
            {
                throw new JsonException($"Logical element object '{elementTypeEnum}' should be an array");
            }
            Type typeToInstanciate = GetArrayLogicalElementType(elementTypeEnum);
            Type listTypeToInstanciate = GetArrayLogicalElementListType(elementTypeEnum);
            object itemList = JsonSerializer.Deserialize(ref reader, listTypeToInstanciate, options);

            AbstractRawObjectLogicalElement logicalElement = (AbstractRawObjectLogicalElement)Activator.CreateInstance(typeToInstanciate, itemList);

            return logicalElement;
        }

        private AbstractRawObjectLogicalElement CreateLogicalElementWithInteger(ref Utf8JsonReader reader, JsonSerializerOptions options,
            string propertyName, ObjectLogicalElementTypeEnum elementTypeEnum)
        {
            if (reader.TokenType != JsonTokenType.Number)
            {
                throw new JsonException($"Logical element object '{elementTypeEnum}' should be a number");
            }
            int value = reader.GetInt32();
            RawObjectLogicalElementWithInteger logicalElement = new RawObjectLogicalElementWithInteger(propertyName, value);
            
            return logicalElement;
        }

        private AbstractRawObjectLogicalElement CreateLogicalElementWithString(ref Utf8JsonReader reader, JsonSerializerOptions options,
            string propertyName, ObjectLogicalElementTypeEnum elementTypeEnum)
        {
            if (reader.TokenType != JsonTokenType.String)
            {
                throw new JsonException($"Logical element object '{elementTypeEnum}' should be a string");
            }
            string value = reader.GetString();
            RawObjectLogicalElementWithString logicalElement = new RawObjectLogicalElementWithString(propertyName, value);

            return logicalElement;
        }

        private AbstractRawObjectLogicalElement CreateLogicalElementWithSubObject(ref Utf8JsonReader reader, JsonSerializerOptions options,
            string propertyName, ObjectLogicalElementTypeEnum elementTypeEnum)
        {
            if (reader.TokenType != JsonTokenType.StartObject)
            {
                throw new JsonException($"Logical element object '{elementTypeEnum}' should be an object");
            }
            Type typeToInstanciate = GetSubObjectLogicalElementType(elementTypeEnum);
            AbstractRawObjectLogicalElement logicalElement = (AbstractRawObjectLogicalElement)JsonSerializer.Deserialize(ref reader, typeToInstanciate, options);

            return logicalElement;
        }

        private Type GetSubObjectLogicalElementType(ObjectLogicalElementTypeEnum elementTypeEnum)
        {
            return elementTypeEnum switch
            {
                ObjectLogicalElementTypeEnum.AdjacentRunway => typeof(RawAdjacentRunway),
                ObjectLogicalElementTypeEnum.Ammo => typeof(RawAmmo),
                ObjectLogicalElementTypeEnum.AmmoDrain => typeof(RawAmmoDrain),
                ObjectLogicalElementTypeEnum.CanComeInCharged => typeof(RawCanComeInCharged),
                ObjectLogicalElementTypeEnum.CanShineCharge => typeof(RawCanShineCharge),
                ObjectLogicalElementTypeEnum.EnemyDamage => typeof(RawEnemyDamage),
                ObjectLogicalElementTypeEnum.EnemyKill => typeof(RawEnemyKill),
                ObjectLogicalElementTypeEnum.ResetRoom => typeof(RawResetRoom),
                _ => throw new NotSupportedException($"Element type {elementTypeEnum} is not supported here")
            };
        }

        private Type GetArrayLogicalElementType(ObjectLogicalElementTypeEnum elementTypeEnum)
        {
            return elementTypeEnum switch
            {
                ObjectLogicalElementTypeEnum.ResourceCapacity => typeof(RawResourceCapacityLogicalElement),
                _ => throw new NotSupportedException($"Element type {elementTypeEnum} is not supported here")
            };
        }

        private Type GetArrayLogicalElementListType(ObjectLogicalElementTypeEnum elementTypeEnum)
        {
            return elementTypeEnum switch
            {
                ObjectLogicalElementTypeEnum.ResourceCapacity => typeof(List<RawResourceCapacityLogicalElementItem>),
                _ => throw new NotSupportedException($"Element type {elementTypeEnum} is not supported here")
            };
        }
    }
}
