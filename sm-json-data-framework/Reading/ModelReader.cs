using sm_json_data_framework.Converters;
using sm_json_data_framework.Models;
using sm_json_data_framework.Models.Enemies;
using sm_json_data_framework.Models.GameFlags;
using sm_json_data_framework.Models.Helpers;
using sm_json_data_framework.Models.Items;
using sm_json_data_framework.Models.Requirements.StringRequirements;
using sm_json_data_framework.Models.Techs;
using sm_json_data_framework.Models.Weapons;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace sm_json_data_framework.Reading
{
    public static class ModelReader
    {
        public static SuperMetroidModel ReadModel(string baseDirectory, bool initialize = true)
        {
            SuperMetroidModel model = new SuperMetroidModel();

            JsonSerializerOptions options = CreateJsonSerializerOptions(model);
            StringLogicalElementConverter stringLogicalElementConverter = options.GetConverter(typeof(AbstractStringLogicalElement)) as StringLogicalElementConverter;

            string itemsPath = baseDirectory + "\\items.json";
            string helpersPath = baseDirectory + "\\helpers.json";
            string techPath = baseDirectory + "\\tech.json";
            string weaponPath = baseDirectory + "\\weapons\\main.json";
            string enemyPath = baseDirectory + "\\enemies\\main.json";
            string bossPath = baseDirectory + "\\enemies\\bosses\\main.json";

            // Read items and put them in the model
            ItemContainer itemContainer = JsonSerializer.Deserialize<ItemContainer>(File.ReadAllText(itemsPath), options);
            model.Items = itemContainer.BaseItemNames
                .Select(n => new Item(n))
                .Concat(itemContainer.UpgradeItems)
                .Concat(itemContainer.ExpansionItems)
                .ToDictionary(i => i.Name);

            // Read game flags and put them in the model
            model.GameFlags = itemContainer.GameFlagNames
                .Select(n => new GameFlag(n))
                .ToDictionary(f => f.Name);

            // STITCHME Something missing about starting game state
            // Can't do this until I have a concept of game state

            // Read helpers and techs
            HelperContainer helperContainer = JsonSerializer.Deserialize<HelperContainer>(File.ReadAllText(helpersPath), options);
            model.Helpers = helperContainer.Helpers.ToDictionary(h => h.Name);

            TechContainer techContainer = JsonSerializer.Deserialize<TechContainer>(File.ReadAllText(techPath), options);
            model.Techs = techContainer.Techs.ToDictionary(t => t.Name);

            // At this point, Techs and Helpers contain some raw string requirements (referencing other techs and helpers)
            // Resolve those now
            List<string> unresolvedStrings = new List<string>();
            foreach(Helper helper in model.Helpers.Values)
            {
                unresolvedStrings.AddRange(helper.Requires.ReplaceRawStringRequirements(stringLogicalElementConverter));
                
            }

            foreach (Tech tech in model.Techs.Values)
            {
                unresolvedStrings.AddRange(tech.Requires.ReplaceRawStringRequirements(stringLogicalElementConverter));
            }

            // If there was any raw string we failed to resolve, consider that an error
            if(unresolvedStrings.Any())
            {
                throw new JsonException($"The following string requirements, found in helpers and techs, could not be resolved " +
                    $"to 'never', an item, a helper, a tech, or a game flag: {string.Join(", ", unresolvedStrings.Distinct())}");
            }

            // STITCHME Starting now, I'd like to fail fast if any string requirement fails to resolve.

            // Read weapons
            WeaponContainer weaponContainer = JsonSerializer.Deserialize<WeaponContainer>(File.ReadAllText(weaponPath), options);
            model.Weapons = weaponContainer.Weapons.ToDictionary(w => w.Name);

            // Read regular enemies
            EnemyContainer enemyContainer = JsonSerializer.Deserialize<EnemyContainer>(File.ReadAllText(enemyPath), options);
            model.Enemies = enemyContainer.Enemies.ToDictionary(e => e.Name);
            // Initialize calculated data in enemies if requested
            if(initialize)
            {
                foreach(Enemy enemy in model.Enemies.Values)
                {
                    enemy.Initialize(model);
                }
            }

            // Read bosses
            EnemyContainer bossContainer = JsonSerializer.Deserialize<EnemyContainer>(File.ReadAllText(bossPath), options);
            foreach(Enemy boss in bossContainer.Enemies)
            {
                // Initialize calculated data in boss if requested
                if (initialize)
                {
                    boss.Initialize(model);
                }
                model.Enemies.Add(boss.Name, boss);
            }

            // STITCHME rooms, connections

            return model;
        }

        public static JsonSerializerOptions CreateJsonSerializerOptions(SuperMetroidModel superMetroidModel)
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
            options.Converters.Add(new ObjectLogicalElementConverter());

            return options;
        }
    }
}
