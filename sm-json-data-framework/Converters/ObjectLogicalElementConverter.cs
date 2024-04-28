using sm_json_data_framework.Models.Requirements;
using sm_json_data_framework.Models.Requirements.ObjectRequirements;
using sm_json_data_framework.Models.Requirements.ObjectRequirements.Integers;
using sm_json_data_framework.Models.Requirements.ObjectRequirements.Strings;
using sm_json_data_framework.Models.Requirements.ObjectRequirements.SubObjects;
using sm_json_data_framework.Models.Requirements.ObjectRequirements.SubRequirements;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace sm_json_data_framework.Converters
{
    public class ObjectLogicalElementConverter : JsonConverter<AbstractObjectLogicalElement>
    {
        private IDictionary<ObjectLogicalElementTypeEnum, Type> overrideLogicalElementTypes = new Dictionary<ObjectLogicalElementTypeEnum, Type>();

        public ObjectLogicalElementConverter(IEnumerable<(ObjectLogicalElementTypeEnum typeEnum, Type type)> overrideTypes = null)
        {
            // Validate and initialize override types
            overrideLogicalElementTypes = LogicalElementCreationUtils.CreateObjectLogicalElementTypeEnumMapping(overrideTypes);
        }

        /// <summary>
        /// Returns the logical element type to instantiate for the provided elementTypeEnum.
        /// </summary>
        /// <param name="elementTypeEnum"></param>
        /// <returns></returns>
        private Type GetLogicalElementType(ObjectLogicalElementTypeEnum elementTypeEnum)
        {
            if (overrideLogicalElementTypes.TryGetValue(elementTypeEnum, out Type overriddenType))
            {
                return overriddenType;
            }
            else
            {
                return LogicalElementCreationUtils.DefaultObjectLogicalElementTypes[elementTypeEnum];
            }

        }

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

            // Convert property name to logicalElementEnum
            ObjectLogicalElementTypeEnum elementTypeEnum 
                = (ObjectLogicalElementTypeEnum)Enum.Parse(typeof(ObjectLogicalElementTypeEnum), propertyName, true);

            AbstractObjectLogicalElement logicalElement = null;
            // This should be placing us either at a StartObject or StartArray, depending on the property name.
            switch (elementTypeEnum.GetSubType())
            {
                case ObjectLogicalElementSubTypeEnum.SubRequirement:
                    logicalElement = CreateLogicalElementWithRequirements(ref reader, options, elementTypeEnum);
                    break;
                case ObjectLogicalElementSubTypeEnum.Integer:
                    logicalElement = CreateLogicalElementWithInteger(ref reader, options, elementTypeEnum);
                    break;
                case ObjectLogicalElementSubTypeEnum.String:
                    logicalElement = CreateLogicalElementWithString(ref reader, options, elementTypeEnum);
                    break;
                case ObjectLogicalElementSubTypeEnum.SubObject:
                    logicalElement = CreateLogicalElementWithSubObject(ref reader, options, elementTypeEnum);
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

        public override void Write(Utf8JsonWriter writer, AbstractObjectLogicalElement value, JsonSerializerOptions options)
        {
            // We're focusing on reading json files for now.
            throw new NotImplementedException();
        }

        private AbstractObjectLogicalElement CreateLogicalElementWithRequirements(ref Utf8JsonReader reader, JsonSerializerOptions options,
            ObjectLogicalElementTypeEnum elementTypeEnum)
        {
            if (reader.TokenType != JsonTokenType.StartArray)
            {
                throw new JsonException($"Logical element object '{elementTypeEnum}' should be an array");
            }
            LogicalRequirements logicalRequirements = JsonSerializer.Deserialize<LogicalRequirements>(ref reader, options);

            Type typeToInstanciate = GetLogicalElementType(elementTypeEnum);
            AbstractObjectLogicalElementWithSubRequirements logicalElement 
                = (AbstractObjectLogicalElementWithSubRequirements)Activator.CreateInstance(typeToInstanciate);
            logicalElement.LogicalRequirements = logicalRequirements;

            return logicalElement;
        }

        private AbstractObjectLogicalElement CreateLogicalElementWithInteger(ref Utf8JsonReader reader, JsonSerializerOptions options,
            ObjectLogicalElementTypeEnum elementTypeEnum)
        {
            if (reader.TokenType != JsonTokenType.Number)
            {
                throw new JsonException($"Logical element object '{elementTypeEnum}' should be a number");
            }
            int value = reader.GetInt32();

            Type typeToInstanciate = GetLogicalElementType(elementTypeEnum);
            AbstractObjectLogicalElementWithInteger logicalElement
                = (AbstractObjectLogicalElementWithInteger)Activator.CreateInstance(typeToInstanciate);
            logicalElement.Value = value;

            return logicalElement;
        }

        private AbstractObjectLogicalElement CreateLogicalElementWithString(ref Utf8JsonReader reader, JsonSerializerOptions options,
            ObjectLogicalElementTypeEnum elementTypeEnum)
        {
            if (reader.TokenType != JsonTokenType.String)
            {
                throw new JsonException($"Logical element object '{elementTypeEnum}' should be a string");
            }
            string value = reader.GetString();

            Type typeToInstanciate = GetLogicalElementType(elementTypeEnum);
            AbstractObjectLogicalElementWithString logicalElement
                = (AbstractObjectLogicalElementWithString)Activator.CreateInstance(typeToInstanciate);
            logicalElement.Value = value;

            return logicalElement;
        }

        private AbstractObjectLogicalElement CreateLogicalElementWithSubObject(ref Utf8JsonReader reader, JsonSerializerOptions options,
            ObjectLogicalElementTypeEnum elementTypeEnum)
        {
            if (reader.TokenType != JsonTokenType.StartObject)
            {
                throw new JsonException($"Logical element object '{elementTypeEnum}' should be an object");
            }

            Type typeToInstanciate = GetLogicalElementType(elementTypeEnum);
            AbstractObjectLogicalElement logicalElement = (AbstractObjectLogicalElement)JsonSerializer.Deserialize(ref reader, typeToInstanciate, options);

            return logicalElement;
        }
    }
}
