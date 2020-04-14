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

        /// <summary>
        /// <para>Only available after a call to <see cref="InitializeReferencedLogicalElementProperties(SuperMetroidModel, Room)"/>.</para>
        /// <para>The weapons that Samus may attempt to use to resolve this EnemyKill. This is built based on ExplicitWeapons, ExcludedWeapons,
        /// and the list of all existing weapons.</para>
        /// </summary>
        [JsonIgnore]
        public IEnumerable<Weapon> ValidWeapons { get; set; }

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

            List<Weapon> validWeapons = new List<Weapon>();
            // If some explicit weapons were provided, only they are allowed. Then take away any excluded weapons.
            if (ExplicitWeapons.Any())
            {
                validWeapons.AddRange(ExplicitWeapons.Except(ExcludedWeapons, ObjectReferenceEqualityComparer<Weapon>.Default));
            }
            // If no explicit weapons were provided, the base list of weapons is all non-situational weapons. All of those are valid except excluded ones.
            else
            {
                validWeapons.AddRange(model.Weapons.Values.Where(w => !w.Situational).Except(ExcludedWeapons, ObjectReferenceEqualityComparer<Weapon>.Default));
            }
            ValidWeapons = validWeapons;

            return unhandled.Distinct();
        }

        public override bool IsFulfilled(SuperMetroidModel model, InGameState inGameState, bool usePreviousRoom = false)
        {
            // Filter the list of valid weapons, to keep only those we can actually use right now
            IEnumerable<Weapon> usableWeapons = ValidWeapons.Where(w => w.UseRequires.IsFulfilled(model, inGameState, usePreviousRoom: usePreviousRoom));

            // Find all usable weapons that are free to use. That's all weapons without an ammo cost, plus all weapon whose ammo is farmable in this EnemyKill
            IEnumerable<Weapon> freeWeapons = usableWeapons.Where(w => !w.ShotRequires.LogicalElements.Where(le => le is Ammo ammo && !FarmableAmmo.Contains(ammo.AmmoType)).Any());

            // Build a list of enemy groups with enemies that can't be killed by free weapons
            IEnumerable<IEnumerable<Enemy>> nonFreeGroups = GroupedEnemies
                .Select(g => g
                    .Where(e => !e.WeaponSusceptibilities.Values
                        .Where(ws => freeWeapons.Contains(ws.Weapon, ObjectReferenceEqualityComparer<Weapon>.Default))
                        .Any()
                    )
                )
                // Eliminate groups with no enemy left
                .Where(g => g.Any());

            // Eliminate from remaining enemies all enemies that can be killed with available ammo
            IEnumerable<Weapon> nonFreeWeapons = usableWeapons.Except(freeWeapons, ObjectReferenceEqualityComparer<Weapon>.Default);
            IEnumerable<IEnumerable<Enemy>> remainingGroups = nonFreeGroups
                .Select(g => g
                // STITCHME Checking this one enemy at a time prevents us from calculating the ammo cost of splash weapons correctly.
                // Will probably leave this here until we are able (and required) to decide which ammo we'd rather use, in situations where many types are effective.
                    .Where(e => !e.WeaponSusceptibilities.Values
                        .Where(ws => nonFreeWeapons.Contains(ws.Weapon, ObjectReferenceEqualityComparer<Weapon>.Default)
                            // STITCHME This ammo check only checks for one shot. We need a feature to evaluate a LogicalRequirements N times (by multiplying all resource costs).
                            && ws.Weapon.ShotRequires.IsFulfilled(model, inGameState, usePreviousRoom :usePreviousRoom))
                        .Any()
                    )
                )
                // Eliminate groups with no enemy left
                .Where(g => g.Any());

            // EnemyKill is fulfilled if we were able to kill all enemies
            return !remainingGroups.Any();
        }
    }
}
