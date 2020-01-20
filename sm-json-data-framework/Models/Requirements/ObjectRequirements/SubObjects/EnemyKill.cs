using sm_json_data_framework.Models.Enemies;
using sm_json_data_framework.Models.InGameStates;
using sm_json_data_framework.Models.Rooms;
using sm_json_data_framework.Models.Weapons;
using sm_json_data_framework.Utils;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.Json.Serialization;

namespace sm_json_data_framework.Models.Requirements.ObjectRequirements.SubObjects
{
    public class EnemyKill : AbstractObjectLogicalElement
    {
        [JsonPropertyName("enemies")]
        public IEnumerable<IEnumerable<string>> GroupedEnemyNames { get; set; } = Enumerable.Empty<IEnumerable<string>>();

        /// <summary>
        /// <para>Only available after a call to <see cref="InitializeReferencedLogicalElementProperties(SuperMetroidModel, Room)"/>.</para>
        /// <para>The enemies that this element's GroupedEnemyNames reference. These enemies are the enemies that must be killed,
        /// separated into groups that can be entirely hit by a single shot from a weapon that hits groups.</para>
        /// </summary>
        [JsonIgnore]
        public IEnumerable<IEnumerable<Enemy>> GroupedEnemies { get; set; }

        [JsonPropertyName("explicitWeapons")]
        public IEnumerable<string> ExplicitWeaponNames { get; set; } = Enumerable.Empty<string>();

        /// <summary>
        /// <para>Only available after a call to <see cref="InitializeReferencedLogicalElementProperties(SuperMetroidModel, Room)"/>.</para>
        /// <para>The weapons that this element's ExplicitWeapons reference. These weapons are the only ones that are allowed for this enemy kill,
        /// overriding the default behavior of allowing all non-situational weapons.</para>
        /// </summary>
        [JsonIgnore]
        public IEnumerable<Weapon> ExplicitWeapons { get; set; }

        [JsonPropertyName("excludedWeapons")]
        public IEnumerable<string> ExcludedWeaponNames { get; set; } = Enumerable.Empty<string>();

        /// <summary>
        /// <para>Only available after a call to <see cref="InitializeReferencedLogicalElementProperties(SuperMetroidModel, Room)"/>.</para>
        /// <para>The weapons that this element's ExcludedWeapons reference. These weapons are not allowed for this enemy kill.</para>
        /// </summary>
        [JsonIgnore]
        public IEnumerable<Weapon> ExcludedWeapons { get; set; }

        public IEnumerable<AmmoEnum> FarmableAmmo { get; set; } = Enumerable.Empty<AmmoEnum>();

        public override IEnumerable<string> InitializeReferencedLogicalElementProperties(SuperMetroidModel model, Room room)
        {
            List<string> unhandled = new List<string>();

            List<IEnumerable<Enemy>> groupedEnemies = new List<IEnumerable<Enemy>>();
            foreach(IEnumerable<string> enemyNameGroup in GroupedEnemyNames)
            {
                List<Enemy> enemyGroup = new List<Enemy>();
                foreach(string enemyName in enemyNameGroup)
                {
                    if (model.Enemies.TryGetValue(enemyName, out Enemy enemy))
                    {
                        enemyGroup.Add(enemy);
                    }
                    else
                    {
                        unhandled.Add($"Enemy {enemyName}");
                    }
                }
                groupedEnemies.Add(enemyGroup);
            }
            GroupedEnemies = groupedEnemies;

            List<Weapon> explicitWeapons = new List<Weapon>();
            foreach(string explicitWeaponName in ExplicitWeaponNames)
            {
                IEnumerable<Weapon> weapons = explicitWeaponName.NameToWeapons(model);
                if (weapons == null)
                {
                    unhandled.Add($"Weapon {explicitWeaponName}");
                }
                else
                {
                    explicitWeapons.AddRange(weapons);
                }
            }
            ExplicitWeapons = explicitWeapons.Distinct();


            List<Weapon> excludedWeapons = new List<Weapon>();
            foreach (string excludedWeaponName in ExcludedWeaponNames)
            {
                IEnumerable<Weapon> weapons = excludedWeaponName.NameToWeapons(model);
                if (weapons == null)
                {
                    unhandled.Add($"Weapon {excludedWeaponName}");
                }
                else
                {
                    excludedWeapons.AddRange(weapons);
                }
            }
            ExcludedWeapons = excludedWeapons.Distinct();

            return unhandled.Distinct();
        }

        public override bool IsFulfilled(InGameState inGameState, bool usePreviousRoom = false)
        {
            // STITCHME Do something
            throw new NotImplementedException();
        }
    }
}
