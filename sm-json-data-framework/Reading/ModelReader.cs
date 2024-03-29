﻿using sm_json_data_framework.Converters;
using sm_json_data_framework.Models;
using sm_json_data_framework.Models.Connections;
using sm_json_data_framework.Models.Enemies;
using sm_json_data_framework.Models.GameFlags;
using sm_json_data_framework.Models.Helpers;
using sm_json_data_framework.Models.InGameStates;
using sm_json_data_framework.Models.Items;
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
using System.Runtime.CompilerServices;
using System.Text;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace sm_json_data_framework.Reading
{
    public static class ModelReader
    {

        /// <summary>
        /// Reads all sm-json-data json files, from the provided base directory, and builds a SuperMetroidModel.
        /// </summary>
        /// <param name="rules">A repository of game rules to operate by.
        /// If null, will use the default constructor of SuperMetroidRules, giving vanilla rules.</param>
        /// <param name="logicalOptions">A container of logical options to go with the representation of the world.
        /// If null, will use the default constructor of LogicalOptions (giving an arbitrary option set).</param>
        /// <param name="startConditionsFactory">An object that can create the player's starting conditions for this representation of the world.
        /// If null, will use a <see cref="DefaultStartConditionsFactory"/>.</param>
        /// <param name="baseDirectory">An override of the path to the base directory of the data model to read.
        /// If left null, this method will use the path of the model included with this project.</param>
        /// <param name="initialize">If true, pre-processes a lot of data to initialize additional properties in many objects within the returned model.
        /// If false, the objects in the returned model will contain mostly just raw data.</param>
        /// <param name="overrideTypes">A sequence of tuples, pairing together an ObjectLogicalElementTypeEnum and the C# type that should be used to 
        /// to represent that ObjectLogicalElementTypeEnum when deserializing logical requirements from a json file.
        /// The provided C# types must extend the default type that is normally used for any given ObjectLogicalElementTypeEnum.</param>
        /// <returns>The generated SuperMetroidModel</returns>
        public static SuperMetroidModel ReadModel(SuperMetroidRules rules = null, LogicalOptions logicalOptions = null, IStartConditionsFactory startConditionsFactory = null,
            string baseDirectory = null, bool initialize = true, IEnumerable<(ObjectLogicalElementTypeEnum typeEnum, Type type)> overrideTypes = null)
        {
            rules ??= new SuperMetroidRules();
            logicalOptions ??= new LogicalOptions();
            startConditionsFactory ??= new DefaultStartConditionsFactory();
            baseDirectory ??= Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "sm-json-data");

            SuperMetroidModel model = new SuperMetroidModel();

            model.Rules = rules;
            model.LogicalOptions = logicalOptions;

            JsonSerializerOptions options = CreateJsonSerializerOptions(model, overrideTypes);
            StringLogicalElementConverter stringLogicalElementConverter = options.GetConverter(typeof(AbstractStringLogicalElement)) as StringLogicalElementConverter;

            string itemsPath = baseDirectory + "\\items.json";
            string helpersPath = baseDirectory + "\\helpers.json";
            string techPath = baseDirectory + "\\tech.json";
            string weaponPath = baseDirectory + "\\weapons\\main.json";
            string enemyPath = baseDirectory + "\\enemies\\main.json";
            string bossPath = baseDirectory + "\\enemies\\bosses\\main.json";
            string connectionBaseDirectory = baseDirectory + "\\connection";
            string roomBaseDirectory = baseDirectory + "\\region";

            // Read items and put them in the model
            ItemContainer itemContainer = JsonSerializer.Deserialize<ItemContainer>(File.ReadAllText(itemsPath), options);
            model.Items = itemContainer.ImplicitItemNames
                .Select(n => new Item(n))
                .Concat(itemContainer.UpgradeItems)
                .Concat(itemContainer.ExpansionItems)
                .ToDictionary(i => i.Name);

            // Read game flags and put them in the model
            model.GameFlags = itemContainer.GameFlagNames
                .Select(n => new GameFlag(n))
                .ToDictionary(f => f.Name);

            // Read helpers and techs
            HelperContainer helperContainer = JsonSerializer.Deserialize<HelperContainer>(File.ReadAllText(helpersPath), options);
            model.Helpers = helperContainer.Helpers.ToDictionary(h => h.Name);

            TechContainer techContainer = JsonSerializer.Deserialize<TechContainer>(File.ReadAllText(techPath), options);
            model.Techs = techContainer.SelectAllTechs().ToDictionary(t => t.Name);

            // At this point, Techs and Helpers contain some raw string requirements (referencing other techs and helpers)
            // Resolve those now
            List<string> unresolvedStrings = new List<string>();
            foreach(Helper helper in model.Helpers.Values)
            {
                unresolvedStrings.AddRange(helper.Requires.ReplaceRawStringElements(stringLogicalElementConverter));
            }

            foreach (Tech tech in model.Techs.Values)
            {
                unresolvedStrings.AddRange(tech.Requires.ReplaceRawStringElements(stringLogicalElementConverter));
            }

            // If there was any raw string we failed to resolve, consider that an error
            if(unresolvedStrings.Any())
            {
                throw new JsonException($"The following string requirements, found in helpers and techs, could not be resolved " +
                    $"to 'never', an item, a helper, a tech, or a game flag: {string.Join(", ", unresolvedStrings.Distinct().Select(s => $"'{s}'"))}");
            }

            // Starting now, fail-fast if any string requirement that fails to resolve
            stringLogicalElementConverter.AllowRawStringElements = false;

            // Read weapons
            WeaponContainer weaponContainer = JsonSerializer.Deserialize<WeaponContainer>(File.ReadAllText(weaponPath), options);
            model.Weapons = weaponContainer.Weapons.ToDictionary(w => w.Name);

            // Read regular enemies and bosses
            EnemyContainer enemyContainer = JsonSerializer.Deserialize<EnemyContainer>(File.ReadAllText(enemyPath), options);
            EnemyContainer bossContainer = JsonSerializer.Deserialize<EnemyContainer>(File.ReadAllText(bossPath), options);
            model.Enemies = enemyContainer.Enemies.Concat(bossContainer.Enemies).ToDictionary(e => e.Name);
            // Initialize calculated data in enemies if requested
            if(initialize)
            {
                foreach(Enemy enemy in model.Enemies.Values)
                {
                    enemy.Initialize(model);
                }
            }

            // Find and read all connection files
            string[] allConnectionFiles = Directory.GetFiles(connectionBaseDirectory, "*.json", SearchOption.AllDirectories);
            foreach(string connectionFile in allConnectionFiles)
            {
                ConnectionContainer connectionContainer = JsonSerializer.Deserialize<ConnectionContainer>(File.ReadAllText(connectionFile), options);
                foreach(JsonConnection connection in connectionContainer.Connections)
                {
                    ConnectionNode node1 = connection.Nodes.ElementAt(0);
                    ConnectionNode node2 = connection.Nodes.ElementAt(1);

                    // If the forward direction is applicable for this json connection, create and add a corresponding forward one-way connection
                    if(connection.Direction == ConnectionDirectionEnum.Forward 
                        || connection.Direction == ConnectionDirectionEnum.Bidirectional)
                    {
                        Connection forwardConnection = new Connection
                        {
                            ConnectionType = connection.ConnectionType,
                            FromNode = connection.Nodes.First(),
                            ToNode = connection.Nodes.ElementAt(1)
                        };

                        model.Connections.Add(forwardConnection.FromNode.IdentifyingString, forwardConnection);
                    }

                    // If the backward direction is applicable for this json connection, create and add a corresponding backward one-way connection
                    if (connection.Direction == ConnectionDirectionEnum.Backward
                        || connection.Direction == ConnectionDirectionEnum.Bidirectional)
                    {
                        Connection backwardConnection = new Connection
                        {
                            ConnectionType = connection.ConnectionType,
                            FromNode = connection.Nodes.ElementAt(1),
                            ToNode = connection.Nodes.First()
                        };

                        model.Connections.Add(backwardConnection.FromNode.IdentifyingString, backwardConnection);
                    }
                }
            }

            // Find and read all room files
            string[] allRoomFiles = Directory.GetFiles(roomBaseDirectory, "*.json", SearchOption.AllDirectories);
            foreach (string roomFile in allRoomFiles)
            {
                RoomContainer roomContainer = JsonSerializer.Deserialize<RoomContainer>(File.ReadAllText(roomFile), options);
                foreach(Room room in roomContainer.Rooms)
                {
                    model.Rooms.Add(room.Name, room);

                    foreach(RoomNode node in room.Nodes.Values)
                    {
                        model.Nodes.Add(node.Name, node);
                        foreach(Runway runway in node.Runways)
                        {
                            model.Runways.Add(runway.Name, runway);
                        }
                    }
                }
            }
            // The initialization of rooms initializes the nodes' connection destination node.
            // That requires this node (and, logically, its room beforehand) to have been created already.
            // This means we can't initialize a room safely until we've created all rooms, so iterate a second time on rooms to initialize.
            if (initialize)
            {
                foreach (Room room in model.Rooms.Values)
                {
                    room.Initialize(model);
                }
            }

            // Now that rooms, flags, and items are in the model, create and assign start conditions
            startConditionsFactory ??= new DefaultStartConditionsFactory();
            model.StartConditions = startConditionsFactory.CreateStartConditions(model, itemContainer);

            if (initialize)
            {
                // Create and assign initial game state
                model.InitialGameState = new InGameState(model, itemContainer);

                // Initialize all references within logical elements
                List<string> unhandledLogicalElementProperties = new List<string>();

                foreach (Helper helper in model.Helpers.Values)
                {
                    unhandledLogicalElementProperties.AddRange(helper.InitializeReferencedLogicalElementProperties(model));
                }

                foreach(Tech tech in model.Techs.Values)
                {
                    unhandledLogicalElementProperties.AddRange(tech.InitializeReferencedLogicalElementProperties(model));
                }

                foreach(Weapon weapon in model.Weapons.Values)
                {
                    unhandledLogicalElementProperties.AddRange(weapon.InitializeReferencedLogicalElementProperties(model));
                }

                foreach(Room room in model.Rooms.Values)
                {
                    unhandledLogicalElementProperties.AddRange(room.InitializeReferencedLogicalElementProperties(model));
                }

                // If there was any logical element property we failed to resolve, consider that an error
                if (unhandledLogicalElementProperties.Any())
                {
                    throw new JsonException($"The following logical element property values could not be resolved " +
                        $"to to an object of their expected type: {string.Join(", ", unhandledLogicalElementProperties.Distinct().Select(s => $"'{s}'"))}");
                }
            }

            model.Initialized = initialize;
            return model;
        }

        public static JsonSerializerOptions CreateJsonSerializerOptions(SuperMetroidModel superMetroidModel,
            IEnumerable<(ObjectLogicalElementTypeEnum typeEnum, Type type)> overrideTypes = null)
        {
            JsonSerializerOptions options = new JsonSerializerOptions
            {
                // Need this to be able to  use PascalCase properties in our C# objects
                PropertyNamingPolicy = JsonNamingPolicy.CamelCase
            };

            // We want to interpret enums by their string value, not by their int.
            options.Converters.Add(new JsonStringEnumConverter());

            // Add custom converters for logical requirements and logical elements
            options.Converters.Add(new LogicalRequirementsConverter());
            options.Converters.Add(new StringLogicalElementConverter(superMetroidModel));
            options.Converters.Add(new ObjectLogicalElementConverter(overrideTypes));
            options.Converters.Add(new StratsDictionaryConverter());

            return options;
        }
    }
}
