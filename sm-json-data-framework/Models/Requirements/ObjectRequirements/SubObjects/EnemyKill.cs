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
    /// <summary>
    /// A logical element which requires Samus to kill enemies, possibly spending ammo if no other weapons work.
    /// </summary>
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

        public override InGameState AttemptFulfill(SuperMetroidModel model, InGameState inGameState, int times = 1, bool usePreviousRoom = false)
        {
            // Filter the list of valid weapons, to keep only those we can actually use right now
            IEnumerable<Weapon> usableWeapons = ValidWeapons.Where(w => w.UseRequires.AttemptFulfill(model, inGameState, times: times, usePreviousRoom: usePreviousRoom) != null);

            // Find all usable weapons that are free to use. That's all weapons without an ammo cost, plus all weapons whose ammo is farmable in this EnemyKill
            // Technically if a weapon were to exist with a shot cost that requires something other than ammo (something like energy or ammo drain?),
            // this wouldn't work. Should that be a worry?
            IEnumerable<Weapon> freeWeapons = usableWeapons.Where(w => !w.ShotRequires.LogicalElements.Where(le => le is Ammo ammo && !FarmableAmmo.Contains(ammo.AmmoType)).Any());

            // Remove all enemies that can be killed by free weapons
            IEnumerable<IEnumerable<Enemy>> nonFreeGroups = GroupedEnemies
                .RemoveEnemies(e => e.WeaponSusceptibilities.Values
                    .Where(ws => freeWeapons.Contains(ws.Weapon, ObjectReferenceEqualityComparer<Weapon>.Default))
                    .Any());

            // If there are no enemies left, we are done!
            if(!nonFreeGroups.Any())
            {
                // Even though the state hasn't changed, clone it to fulfill the method's contract
                return inGameState.Clone();
            }

            // The remaining enemies require ammo
            IEnumerable<Weapon> nonFreeWeapons = usableWeapons.Except(freeWeapons, ObjectReferenceEqualityComparer<Weapon>.Default);
            IEnumerable<Weapon> nonFreeSplashWeapons = nonFreeWeapons.Where(w => w.HitsGroup);
            IEnumerable<Weapon> nonFreeIndividualWeapons = nonFreeWeapons.Where(w => !w.HitsGroup);

            // Iterate over each group, killing it and updating the resulting state.
            // We'll test many scenarios, each with 0 to 1 splash weapon and a fixed number of splash weapon shots (after which enemies are killed with single-target weapons).
            // We will not test multiple combinations of splash weapons.
            InGameState resultingState = inGameState;
            foreach(IEnumerable<Enemy> currentEnemyGroup in nonFreeGroups)
            {
                // Build a list of combinations of splash weapons and splash shots (including one entry for no splash weapon at all)
                IEnumerable<(Weapon splashWeapon, int splashShots)> combinations = nonFreeSplashWeapons.SelectMany(w =>
                    // Figure out what different shot counts for this weapon will lead to different numbers of casualties
                    currentEnemyGroup
                        .Select(e => e.WeaponSusceptibilities.TryGetValue(w.Name, out WeaponSusceptibility susceptibility) ? susceptibility.Shots : 0)
                        .Where(shots => shots > 0)
                        // Convert each different number of shot into a combination of this weapon and the number of shots
                        .Select(shots => (splashWeapon: w, splashShots: shots))
                )
                // Add the one entry for not using a splash weapon at all
                .Append((splashWeapon: null, splashShots: 0));

                // Evaluate all combinations and apply the cheapest to our current resulting state
                resultingState = model.ApplyOr(resultingState, combinations,
                    (c, igs) => KillEnemyGroup(model, igs, times, usePreviousRoom, currentEnemyGroup, c.splashWeapon, c.splashShots, nonFreeIndividualWeapons)
                );

                // If we failed to kill an enemy group, we can't kill all enemies
                if (resultingState == null)
                {
                    return null;
                }
            }

            return resultingState;
        }

        /// <summary>
        /// <para>Kills the provided enemy group, first by applying the provided number of splash shots with the provided splash weapon, then
        /// finishing off survivors with the cheapest of the provided individual weapons.</para>
        /// <para>If no splash weapon is provided, skips immediately to killing enemies with individual weapons.</para>
        /// <para>Returns a new InGameState describing the resulting state after the cheapest identified solution, or null if no solution works.</para>
        /// </summary>
        /// <param name="model">A model that can be used to obtain data about the current game configuration.</param>
        /// <param name="inGameState">The in-game state to evaluate</param>
        /// <param name="times">The number of consecutive times that the enemies should be killed. Impacts resource cost.</param>
        /// <param name="usePreviousRoom">If true, uses the last known room state at the previous room instead of the current room to answer
        /// (whenever in-room state is relevant).</param>
        /// <param name="enemyGroup">The enemy group to kill</param>
        /// <param name="splashWeapon">An optional splash weapon to initially use against the group. If null, the splash attack is skipped.</param>
        /// <param name="splashShots">The number of shots to use for the splash weapon</param>
        /// <param name="individualWeapons">An enumeration of weapons to try to use to finish off the enemies after the splash attack.</param>
        /// <returns></returns>
        private InGameState KillEnemyGroup(SuperMetroidModel model, InGameState inGameState, int times, bool usePreviousRoom, IEnumerable<Enemy> enemyGroup,
            Weapon splashWeapon, int splashShots, IEnumerable<Weapon> individualWeapons)
        {
            InGameState resultingState = null;
            // We'll need to track the health of individual enemies, so create an EnemyWithHealth for each
            IEnumerable<EnemyWithHealth> enemiesWithHealth = enemyGroup.Select(e => new EnemyWithHealth(e));

            // If no splashWeapon is provided (or it's somehow not a splash weapon), skip that step and only do the individual kill
            if(splashWeapon == null || !splashWeapon.HitsGroup)
            {
                resultingState = inGameState;
            }
            // If using a splash weapon, spend the ammo then apply the damage
            else
            {
                resultingState = splashWeapon.ShotRequires.AttemptFulfill(model, inGameState, times: times * splashShots, usePreviousRoom: usePreviousRoom);

                // If we can't spend the ammo, fail immediately
                if (resultingState == null)
                {
                    return null;
                }

                // Apply the splash attack to each enemy
                enemiesWithHealth = enemiesWithHealth
                    .Select(e => {
                        e.Enemy.WeaponSusceptibilities.TryGetValue(splashWeapon.Name, out WeaponSusceptibility susceptibility);
                        // If the splash weapon hurts the enemy, apply damage
                        if(susceptibility != null)
                        {
                            int damage = susceptibility.DamagePerShot * splashShots;
                            e.Health -= damage;
                        }

                        // Return null if enemy is dead
                        return e.IsAlive() ? e : null;
                    })
                    // Remove dead enemies
                    .Where(e => e != null);
            }

            // Iterate over each remaining enemy, killing it with the cheapest individual weapon
            foreach(EnemyWithHealth currentEnemy in enemiesWithHealth)
            {
                resultingState = model.ApplyOr(resultingState, individualWeapons, (w, igs) => KillEnemy(model, igs, times, usePreviousRoom, currentEnemy, w));

                // If we can't kill one of the enemies, give up
                if (resultingState == null)
                {
                    return null;
                }
            }

            return resultingState;
        }

        /// <summary>
        /// Kills the provided enemy using the provided weapon, returning the resulting InGameState.
        /// </summary>
        /// <param name="model">A model that can be used to obtain data about the current game configuration.</param>
        /// <param name="inGameState">The in-game state to evaluate</param>
        /// <param name="times">The number of consecutive times that the enemy should be killed. Impacts resource cost.</param>
        /// <param name="usePreviousRoom">If true, uses the last known room state at the previous room instead of the current room to answer
        /// (whenever in-room state is relevant).</param>
        /// <param name="enemyWithHealth">The enemy to kill, with is current remaining health</param>
        /// <param name="weapon">The weapon to use to kill the enemy</param>
        /// <returns>The resulting InGameState after killing the enemy, or null if killing the enemy failed.</returns>
        private InGameState KillEnemy(SuperMetroidModel model, InGameState inGameState, int times, bool usePreviousRoom, EnemyWithHealth enemyWithHealth, Weapon weapon)
        {
            enemyWithHealth.Enemy.WeaponSusceptibilities.TryGetValue(weapon.Name, out WeaponSusceptibility susceptibility);
            if (susceptibility == null)
            {
                return null;
            }
            int numberOfShots = enemyWithHealth.Health / susceptibility.DamagePerShot;
            return weapon.ShotRequires.AttemptFulfill(model, inGameState, times: times * numberOfShots, usePreviousRoom: usePreviousRoom);
        }
    }

    /// <summary>
    /// A simple combination of an Enemy and a current health count.
    /// </summary>
    public class EnemyWithHealth
    {

        public EnemyWithHealth(Enemy enemy)
        {
            Enemy = enemy;
            Health = enemy.Hp;
        }

        public EnemyWithHealth(EnemyWithHealth other)
        {
            Enemy = other.Enemy;
            Health = other.Health;
        }

        public Enemy Enemy { get; set; }

        public int Health { get; set; }

        public bool IsAlive()
        {
            return Health > 0;
        }
    }

    /// <summary>
    /// Contains extension methods used in the context of EnemyKill.
    /// </summary>
    public static class EnemyKillExtensions
    {
        /// <summary>
        /// Returns a sequence based on this sequence of EnemyGroups, but without the enemies that meet the provided enemyRemovalCondition.
        /// Also removes any groups that end up with no enemies left.
        /// </summary>
        /// <param name="enemyGroups">The Enemy groups to remove enemies from</param>
        /// <param name="enemyRemovalCondition">A Predicate that will cause all enemies that meet it to be removed</param>
        /// <returns>The enemy groups with proper enemies removed, excluding groups with no enemy left.</returns>
        public static IEnumerable<IEnumerable<Enemy>> RemoveEnemies(this IEnumerable<IEnumerable<Enemy>> enemyGroups, Predicate<Enemy> enemyRemovalCondition)
        {
            return enemyGroups
                // Transform each group of enemies into a new equivalent group, but with only a subset of enemies
                .Select(g => g
                    // Remove all enemies that meet the removal condition - so retain those that do not
                    .Where(e => !enemyRemovalCondition(e))
                )
                // After we removed enemies in the groups, eliminate groups with no enemy left
                .Where(g => g.Any());
        }
    }
}
