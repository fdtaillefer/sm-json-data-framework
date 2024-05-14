using sm_json_data_framework.Converters;
using sm_json_data_framework.Converters.Raw;
using sm_json_data_framework.Models;
using sm_json_data_framework.Models.Connections;
using sm_json_data_framework.Models.Enemies;
using sm_json_data_framework.Models.GameFlags;
using sm_json_data_framework.Models.Helpers;
using sm_json_data_framework.Models.InGameStates;
using sm_json_data_framework.Models.Items;
using sm_json_data_framework.Models.Raw;
using sm_json_data_framework.Models.Raw.Connections;
using sm_json_data_framework.Models.Raw.Enemies;
using sm_json_data_framework.Models.Raw.Helpers;
using sm_json_data_framework.Models.Raw.Items;
using sm_json_data_framework.Models.Raw.Rooms;
using sm_json_data_framework.Models.Raw.Techs;
using sm_json_data_framework.Models.Raw.Weapons;
using sm_json_data_framework.Models.Requirements.ObjectRequirements;
using sm_json_data_framework.Models.Requirements.StringRequirements;
using sm_json_data_framework.Models.Rooms;
using sm_json_data_framework.Models.Rooms.Nodes;
using sm_json_data_framework.Models.Techs;
using sm_json_data_framework.Models.Weapons;
using sm_json_data_framework.Options;
using sm_json_data_framework.Rules;
using System;
using System.Collections.Generic;
using System.Data;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Runtime.CompilerServices;
using System.Text;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace sm_json_data_framework.Reading
{
    public static class ModelReader
    {

        /// <summary>
        /// Delegates to <see cref="ReadRawModel(string)"/>, then uses that raw model to delegate to
        /// <see cref="UnfinalizedSuperMetroidModel.SuperMetroidModel(RawSuperMetroidModel, SuperMetroidRules, LogicalOptions, IBasicStartConditionsCustomizer, IEnumerable{ValueTuple{ObjectLogicalElementTypeEnum, Type}}, IEnumerable{ValueTuple{StringLogicalElementTypeEnum, Type}})"/>
        /// </summary>
        /// <param name="rules">A repository of game rules to operate by.
        /// If null, will use the default constructor of SuperMetroidRules, giving vanilla rules.</param>
        /// <param name="basicStartConditionsCustomizer">An optional object that can apply modifications to the <see cref="BasicStartConditions"/> that will
        /// be created and assigned to the model.</param>
        /// <param name="baseDirectory">An override of the path to the base directory of the data model to read.
        /// If left null, this method will use the path of the model included with this project.</param>
        /// <param name="overrideObjectTypes">A sequence of tuples, pairing together an ObjectLogicalElementTypeEnum and the C# type that should be used to 
        /// to represent that ObjectLogicalElementTypeEnum when deserializing logical requirements from a json file.
        /// The provided C# types must extend the default type that is normally used for any given ObjectLogicalElementTypeEnum.</param>
        /// <returns>The generated SuperMetroidModel</returns>
        public static UnfinalizedSuperMetroidModel ReadModel(SuperMetroidRules rules = null,
            IBasicStartConditionsCustomizer basicStartConditionsCustomizer = null,
            string baseDirectory = null, 
            IEnumerable<(ObjectLogicalElementTypeEnum typeEnum, Type type)> overrideObjectTypes = null,
            IEnumerable<(StringLogicalElementTypeEnum typeEnum, Type type)> overrideStringTypes = null)
        {
            RawSuperMetroidModel rawModel = ReadRawModel(baseDirectory);
            return new UnfinalizedSuperMetroidModel(rawModel, rules, basicStartConditionsCustomizer, overrideObjectTypes, overrideStringTypes);
        }

        private static JsonSerializerOptions CreateJsonSerializerOptionsForRawModel()
        {
            JsonSerializerOptions options = CreateBaseJsonSerializerOptions();

            // Add custom converters for logical requirements and logical elements
            options.Converters.Add(new RawLogicalRequirementsConverter());
            options.Converters.Add(new RawObjectLogicalElementConverter());
            options.Converters.Add(new RawStringLogicalElementConverter());

            return options;
        }

        private static JsonSerializerOptions CreateBaseJsonSerializerOptions()
        {
            JsonSerializerOptions options = new JsonSerializerOptions
            {
                // Need this to be able to  use PascalCase properties in our C# objects
                PropertyNamingPolicy = JsonNamingPolicy.CamelCase
            };

            // We want to interpret enums by their string value, not by their int.
            options.Converters.Add(new JsonStringEnumConverter());
            return options;
        }

        /// <summary>
        /// Reads all sm-json-data json files, from the provided (or default) base directory, and builds a RawSuperMetroidModel.
        /// </summary>
        /// <param name="baseDirectory">An override of the path to the base directory of the data model to read.
        /// If left null, this method will use the path of the model included with this project.</param>
        /// <returns>The generated RawSuperMetroidModel</returns>
        public static RawSuperMetroidModel ReadRawModel(string baseDirectory = null)
        {
            baseDirectory ??= Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "sm-json-data");
            JsonSerializerOptions options = CreateJsonSerializerOptionsForRawModel();

            RawSuperMetroidModel model = new RawSuperMetroidModel();

            string itemsPath = baseDirectory + "\\items.json";
            string helpersPath = baseDirectory + "\\helpers.json";
            string techPath = baseDirectory + "\\tech.json";
            string weaponPath = baseDirectory + "\\weapons\\main.json";
            string enemyPath = baseDirectory + "\\enemies\\main.json";
            string bossPath = baseDirectory + "\\enemies\\bosses\\main.json";
            string connectionBaseDirectory = baseDirectory + "\\connection";
            string roomBaseDirectory = baseDirectory + "\\region";

            // Read items file and put it in the model
            model.ItemContainer = JsonSerializer.Deserialize<RawItemContainer>(File.ReadAllText(itemsPath), options);

            // Read helper files
            model.HelperContainer = JsonSerializer.Deserialize<RawHelperContainer>(File.ReadAllText(helpersPath), options);

            // Read techs file
            model.TechContainer = JsonSerializer.Deserialize<RawTechContainer>(File.ReadAllText(techPath), options);

            // Read weapons file
            model.WeaponContainer = JsonSerializer.Deserialize<RawWeaponContainer>(File.ReadAllText(weaponPath), options);

            // Read regular enemies and bosses files
            model.EnemyContainer = JsonSerializer.Deserialize<RawEnemyContainer>(File.ReadAllText(enemyPath), options);
            model.BossContainer = JsonSerializer.Deserialize<RawEnemyContainer>(File.ReadAllText(bossPath), options);

            // Find and read all connection files
            List<RawConnection> allConnections = new List<RawConnection>();
            string[] allConnectionFiles = Directory.GetFiles(connectionBaseDirectory, "*.json", SearchOption.AllDirectories);
            foreach (string connectionFile in allConnectionFiles)
            {
                RawConnectionContainer connectionContainer = JsonSerializer.Deserialize<RawConnectionContainer>(File.ReadAllText(connectionFile), options);
                allConnections.AddRange(connectionContainer.Connections);
            }
            model.ConnectionContainer = new RawConnectionContainer { Connections = allConnections };

            // Find and read all room files
            List<RawRoom> allRooms = new List<RawRoom>();
            string[] allRoomFiles = Directory.GetFiles(roomBaseDirectory, "*.json", SearchOption.AllDirectories);
            foreach (string roomFile in allRoomFiles)
            {
                RawRoomContainer roomContainer = JsonSerializer.Deserialize<RawRoomContainer>(File.ReadAllText(roomFile), options);
                allRooms.AddRange(roomContainer.Rooms);
            }
            model.RoomContainer = new RawRoomContainer { Rooms = allRooms };

            return model;
        }
    }
}
