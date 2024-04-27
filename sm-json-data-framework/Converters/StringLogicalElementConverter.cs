using sm_json_data_framework.Models;
using sm_json_data_framework.Models.GameFlags;
using sm_json_data_framework.Models.Helpers;
using sm_json_data_framework.Models.Items;
using sm_json_data_framework.Models.Requirements.StringRequirements;
using sm_json_data_framework.Models.Techs;
using System;
using System.Collections.Generic;
using System.Text;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace sm_json_data_framework.Converters
{
    public class StringLogicalElementConverter : JsonConverter<AbstractStringLogicalElement>
    {
        private SuperMetroidModel SuperMetroidModel { get; set; }

        /// <summary>
        /// Indicates whether raw strings that don't match something in the superMetroidModel should be allowed. Defaults to true.
        /// </summary>
        public bool AllowRawStringElements { get; set; } = true;

        public StringLogicalElementConverter(SuperMetroidModel superMetroidModel)
        {
            SuperMetroidModel = superMetroidModel;
        }

        public override AbstractStringLogicalElement Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
        {
            if (reader.TokenType != JsonTokenType.String)
            {
                throw new JsonException("A string logical element should be a string");
            }

            string stringValue = reader.GetString();
            return CreateLogicalElement(stringValue);
        }

        public override void Write(Utf8JsonWriter writer, AbstractStringLogicalElement value, JsonSerializerOptions options)
        {
            // We're focusing on reading json files for now.
            throw new NotImplementedException();
        }

        public AbstractStringLogicalElement CreateLogicalElement(string stringValue)
        {
            // STITCHME This creates logical elements without allowing the use of an override type. Fix this?
            if (Equals(stringValue, "never"))
            {
                return new NeverLogicalElement();
            }
            // If the string is the name of a helper that's already in the model, return an appropriate logical element
            else if (SuperMetroidModel.Helpers.TryGetValue(stringValue, out Helper helper))
            {
                return new HelperLogicalElement(helper);
            }
            // If the string is the name of a tech that's already in the model...
            else if (SuperMetroidModel.Techs.TryGetValue(stringValue, out Tech tech))
            {
                // Return an appropriate logical element if the tech is enabled
                if (SuperMetroidModel.LogicalOptions.IsTechEnabled(tech))
                {
                    return new TechLogicalElement(tech);
                }
                // Return a never logical element if the tech is not enabled
                else
                {
                    return new NeverLogicalElement();
                }
            }
            // If the string is the name of an item that's already in the model, return an appropriate logical element
            else if (SuperMetroidModel.Items.TryGetValue(stringValue, out Item item))
            {
                return new ItemLogicalElement(item);
            }
            // If the string is the name of a game flag that's already in the model...
            else if (SuperMetroidModel.GameFlags.TryGetValue(stringValue, out GameFlag gameFlag))
            {
                // Return an appropriate logical element if the game flag is enabled
                if (SuperMetroidModel.LogicalOptions.IsGameFlagEnabled(gameFlag))
                {
                    return new GameFlagLogicalElement(gameFlag);
                }
                // Return a never logical element if the game flag is not enabled
                else
                {
                    return new NeverLogicalElement();
                }
            }
            // If the string matched nothing that's in the model maybe it's referencing something that's
            // not been read and put in the model yet.
            // Return an uninterpreted string logical element. This will need to be replaced before initialization is done.
            else if (AllowRawStringElements)
            {
                return new UninterpretedStringLogicalElement(stringValue);
            }
            else
            {
                throw new JsonException($"Logical element string {stringValue} could not be matched to anything.");
            }
        }
    }
}
