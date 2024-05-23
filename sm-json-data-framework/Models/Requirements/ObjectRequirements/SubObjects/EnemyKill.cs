using sm_json_data_framework.Models.Enemies;
using sm_json_data_framework.Models.InGameStates;
using sm_json_data_framework.Models.Rooms;
using sm_json_data_framework.Models.Weapons;
using sm_json_data_framework.Options;
using sm_json_data_framework.Rules;
using sm_json_data_framework.Utils;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.Json.Serialization;

namespace sm_json_data_framework.Models.Requirements.ObjectRequirements.SubObjects
{
    public class EnemyKill : AbstractObjectLogicalElement<UnfinalizedEnemyKill, EnemyKill>
    {
        public EnemyKill(UnfinalizedEnemyKill sourceElement, Action<EnemyKill> mappingsInsertionCallback, ModelFinalizationMappings mappings)
            : base(sourceElement, mappingsInsertionCallback)
        {
            GroupedEnemyNames = sourceElement.GroupedEnemyNames.Select(group => group.AsReadOnly()).ToList().AsReadOnly();
            GroupedEnemies = sourceElement.GroupedEnemies.Select(group => group.Select(enemy => enemy.Finalize(mappings)).ToList().AsReadOnly()).ToList().AsReadOnly();
            ExplicitWeapons = sourceElement.ExplicitWeapons.Select(weapon => weapon.Finalize(mappings)).ToDictionary(weapon => weapon.Name).AsReadOnly();
            ExcludedWeapons = sourceElement.ExcludedWeapons.Select(weapon => weapon.Finalize(mappings)).ToDictionary(weapon => weapon.Name).AsReadOnly();
            ValidWeapons = sourceElement.ValidWeapons.Select(weapon => weapon.Finalize(mappings)).ToDictionary(weapon => weapon.Name).AsReadOnly();
            FarmableAmmo = sourceElement.FarmableAmmo.ToHashSet().AsReadOnly();
        }

        public IReadOnlyList<IReadOnlyList<string>> GroupedEnemyNames { get; }

        /// <summary>
        /// The enemies that this element's GroupedEnemyNames reference. These enemies are the enemies that must be killed,
        /// separated into groups that can be entirely hit by a single shot from a weapon that hits groups.
        /// </summary>
        public IReadOnlyList<IReadOnlyList<Enemy>> GroupedEnemies { get; }

        /// <summary>
        /// the only weapons that are allowed for this enemy kill, overriding the default behavior of allowing all non-situational weapons.
        /// Mapped by name.
        /// </summary>
        public IReadOnlyDictionary<string, Weapon> ExplicitWeapons { get; }

        /// <summary>
        /// Weapons that are disallowed for this EnemyKill, mapped by name.
        /// </summary>
        public IReadOnlyDictionary<string, Weapon> ExcludedWeapons { get; }

        /// <summary>
        /// The weapons that Samus may attempt to use to resolve this EnemyKill, mapped by name. This is built based on ExplicitWeapons, ExcludedWeapons,
        /// and the list of all existing weapons.
        /// </summary>
        public IReadOnlyDictionary<string, Weapon> ValidWeapons { get; }

        /// <summary>
        /// The set of ammo types that are considered farmable in the context of this EnemyKill, meaning the ammo cost for those types can be waived.
        /// </summary>
        public IReadOnlySet<AmmoEnum> FarmableAmmo { get; }

        protected override ExecutionResult ExecuteUseful(SuperMetroidModel model, ReadOnlyInGameState inGameState, int times = 1, int previousRoomCount = 0)
        {
            // Create an ExecutionResult immediately so we can record free kills in it
            ExecutionResult result = new ExecutionResult(inGameState.Clone());

            // Filter the list of valid weapons, to keep only those we can actually use right now
            IEnumerable<Weapon> usableWeapons = ValidWeapons.Values.WhereLogicallyRelevant().Where(w => w.UseRequires.Execute(model, inGameState, times: times, previousRoomCount: previousRoomCount) != null);

            // Find all usable weapons that are free to use. That's all weapons without an ammo cost, plus all weapons whose ammo is farmable in this EnemyKill
            // Technically if a weapon were to exist with a shot cost that requires something other than ammo (something like energy or ammo drain?),
            // this wouldn't work. Should that be a worry?
            IEnumerable<Weapon> freeWeapons = usableWeapons.Where(w => !w.ShotRequires.LogicalElements.Where(le => le is Ammo ammo && !FarmableAmmo.Contains(ammo.AmmoType)).Any());

            // Remove all enemies that can be killed by free weapons
            IEnumerable<IList<Enemy>> nonFreeGroups = GroupedEnemies
                .RemoveEnemies(e =>
                {
                    // Look for a free usable weapon this enemy is susceptible to.
                    var firstWeaponSusceptibility = e.WeaponSusceptibilities.Values
                        .Where(ws => freeWeapons.Contains(ws.Weapon, ReferenceEqualityComparer.Instance))
                        .FirstOrDefault();

                    // If we found a weapon, record a kill and return true (to remove the enemy)
                    if (firstWeaponSusceptibility != null)
                    {
                        result.AddKilledEnemy(e, firstWeaponSusceptibility.Weapon, firstWeaponSusceptibility.Shots);
                        return true;
                    }
                    // If we didn't find a weapon, return false (to retain the enemy)
                    else
                    {
                        return false;
                    }
                });

            // If there are no enemies left, we are done!
            if (!nonFreeGroups.Any())
            {
                return result;
            }

            // The remaining enemies require ammo
            IEnumerable<Weapon> nonFreeWeapons = usableWeapons.Except(freeWeapons, ObjectReferenceEqualityComparer<Weapon>.Default);
            IEnumerable<Weapon> nonFreeSplashWeapons = nonFreeWeapons.Where(w => w.HitsGroup);
            IList<Weapon> nonFreeIndividualWeapons = nonFreeWeapons.Where(w => !w.HitsGroup).ToList();

            // Iterate over each group, killing it and updating the resulting state.
            // We'll test many scenarios, each with 0 to 1 splash weapon and a fixed number of splash weapon shots (after which enemies are killed with single-target weapons).
            // We will not test multiple combinations of splash weapons.
            foreach (IList<Enemy> currentEnemyGroup in nonFreeGroups)
            {
                // Build a list of combinations of splash weapons and splash shots (including one entry for no splash weapon at all)
                IEnumerable<(Weapon splashWeapon, int splashShots)> splashCombinations = nonFreeSplashWeapons.SelectMany(w =>
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
                (_, ExecutionResult killResult) = model.ExecuteBest(splashCombinations.Select(combination => new EnemyGroupAmmoExecutable(currentEnemyGroup, nonFreeIndividualWeapons, combination.splashWeapon, combination.splashShots)),
                    result.ResultingState, times: times, previousRoomCount: previousRoomCount);

                // If we failed to kill an enemy group, we can't kill all enemies
                if (killResult == null)
                {
                    return null;
                }

                // Update the sequential ExecutionResult
                result = result.ApplySubsequentResult(killResult);
            }

            return result;
        }

        protected override void PropagateLogicalOptions(ReadOnlyLogicalOptions logicalOptions, SuperMetroidRules rules)
        {
            // Propagate to all valid weapons, so we can tell if any of them is still usable
            foreach (Weapon weapon in ValidWeapons.Values)
            {
                weapon.ApplyLogicalOptions(logicalOptions, rules);
            }
        }

        protected override bool CalculateLogicallyNever(SuperMetroidRules rules)
        {
            // Can't fulfill this if none of the valid weapons are usable
            bool unkillable = !ValidWeapons.Values.Any(weapon => !weapon.LogicallyNever);

            // This could also become impossible by requiring more ammo than the max ammo we can ever get,
            // but max ammo is not available in logical options.

            return unkillable;
        }

        protected override bool CalculateLogicallyAlways(SuperMetroidRules rules)
        {
            // This is always possible if any of the valid weapons also is
            bool alwaysKillable = ValidWeapons.Values.Any(weapon => weapon.LogicallyAlways);

            return alwaysKillable;
        }

        protected override bool CalculateLogicallyFree(SuperMetroidRules rules)
        {
            // This is always free if any of the valid weapons also is
            bool free = ValidWeapons.Values.Any(weapon => weapon.LogicallyFree);

            return free;
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

        /// <summary>
        /// Creates and returns an executable that kills this enemy (with its remaining health) with the provided weapon.
        /// </summary>
        /// <param name="weapon">The weapon to use to finish off the enemy</param>
        /// <param name="priorSplashWeapon">An optional splash weapon, that was previously used to attack this enemy</param>
        /// <param name="priorSplashShots">The number of shots of priorSplashWeapons that this enemy was previously hit with</param>
        /// <returns></returns>
        public EnemyWithHealthExecutable ToExecutable(Weapon weapon, Weapon priorSplashWeapon, int priorSplashShots)
        {
            return new EnemyWithHealthExecutable(this, weapon, priorSplashWeapon, priorSplashShots);
        }
    }

    /// <summary>
    /// <para>An executable that corresponds to an ammo kill of an enemy group, with a specific splash weapon scenario
    /// (involving an optional splash weapon and an accompanying number of shots for that weapon).</para>
    /// <para>The execution involves firing the prescribed number of splash weapon shots, then finishing off the remaining enemies
    /// with the cheapest non-splash weapon. </para>
    /// </summary>
    public class EnemyGroupAmmoExecutable : IExecutable
    {
        public EnemyGroupAmmoExecutable(ICollection<Enemy> enemyGroup, ICollection<Weapon> nonSplashWeapons,
            Weapon splashWeapon, int splashShots)
        {
            EnemyGroup = new List<Enemy>(enemyGroup);
            SplashWeapon = splashWeapon;
            NonSplashWeapons = new List<Weapon>(nonSplashWeapons);
            SplashShots = splashShots;
        }

        private IList<Enemy> EnemyGroup { get; set; }

        private IList<Weapon> NonSplashWeapons { get; set; }

        private Weapon SplashWeapon { get; set; }

        private int SplashShots { get; set; }

        public ExecutionResult Execute(SuperMetroidModel model, ReadOnlyInGameState inGameState, int times = 1, int previousRoomCount = 0)
        {
            ExecutionResult result = null;
            // We'll need to track the health of individual enemies, so create an EnemyWithHealth for each
            IEnumerable<EnemyWithHealth> enemiesWithHealth = EnemyGroup.Select(e => new EnemyWithHealth(e));

            // If using a splash weapon, spend the ammo then apply the damage
            if (SplashWeapon != null && SplashWeapon.HitsGroup)
            {
                result = SplashWeapon.ShotRequires.Execute(model, inGameState, times: times * SplashShots, previousRoomCount: previousRoomCount);

                // If we can't spend the ammo, fail immediately
                if (result == null)
                {
                    return null;
                }

                // Apply the splash attack to each enemy
                enemiesWithHealth = enemiesWithHealth
                    .Select(e =>
                    {
                        e.Enemy.WeaponSusceptibilities.TryGetValue(SplashWeapon.Name, out WeaponSusceptibility susceptibility);
                        // If the splash weapon hurts the enemy, apply damage
                        if (susceptibility != null)
                        {
                            int damage = susceptibility.DamagePerShot * SplashShots;
                            e.Health -= damage;
                        }

                        // Return the new state of the enemy.
                        if (e.IsAlive())
                        {
                            return e;
                        }
                        // If the enemy is dead, record the kill and return null
                        else
                        {
                            // No matter how many shots we dealt to the group, record how many shots it took to actually kill this enemy.
                            result.AddKilledEnemy(e.Enemy, SplashWeapon, susceptibility.Shots);
                            return null;
                        }
                    })
                    // Remove dead enemies
                    .Where(e => e != null);
            }
            // No else: If no splashWeapon is provided (or it's somehow not a splash weapon), skip that step and only do the individual kill


            // Iterate over each remaining enemy, killing it with the cheapest non-splash weapon
            foreach (EnemyWithHealth currentEnemy in enemiesWithHealth)
            {
                var (_, killResult) = model.ExecuteBest(NonSplashWeapons.Select(weapon => currentEnemy.ToExecutable(weapon, SplashWeapon, SplashShots)),
                    result?.ResultingState ?? inGameState, times: times, previousRoomCount: previousRoomCount);

                // If we can't kill one of the enemies, give up
                if (killResult == null)
                {
                    return null;
                }

                // Update the sequential ExecutionResult
                result = result == null ? killResult : result.ApplySubsequentResult(killResult);
            }

            return result;
        }
    }

    /// <summary>
    /// Represents the killing of an enemy with a current health count, possibly after it was attacked with a splash weapon first.
    /// </summary>
    public class EnemyWithHealthExecutable : IExecutable
    {
        public EnemyWithHealthExecutable(EnemyWithHealth enemyWithHealth, Weapon weapon, Weapon priorSplashWeapon, int priorSplashShots)
        {
            EnemyWithHealth = enemyWithHealth;
            Weapon = weapon;
            PriorSplashWeapon = priorSplashWeapon;
            PriorSplashShots = priorSplashShots;
        }

        private EnemyWithHealth EnemyWithHealth { get; set; }

        private Weapon Weapon { get; set; }

        private Weapon PriorSplashWeapon { get; set; }

        private int PriorSplashShots { get; set; }

        public ExecutionResult Execute(SuperMetroidModel model, ReadOnlyInGameState inGameState, int times = 1, int previousRoomCount = 0)
        {
            bool enemyInitiallyFull = EnemyWithHealth.Health == EnemyWithHealth.Enemy.Hp;

            EnemyWithHealth.Enemy.WeaponSusceptibilities.TryGetValue(Weapon.Name, out WeaponSusceptibility susceptibility);
            if (susceptibility == null)
            {
                return null;
            }
            int numberOfShots = susceptibility.NumberOfHits(EnemyWithHealth.Health);

            ExecutionResult result = Weapon.ShotRequires.Execute(model, inGameState, times: times * numberOfShots, previousRoomCount: previousRoomCount);

            if (result != null)
            {
                // Record the kill

                // If the enemy was full, then the prior splash attack (if any) didn't affect it. Ignore it.
                if (enemyInitiallyFull)
                {
                    result.AddKilledEnemy(EnemyWithHealth.Enemy, Weapon, numberOfShots);
                }
                // If the enemy was not full, the splash attack contributed to its death
                else
                {
                    result.AddKilledEnemy(EnemyWithHealth.Enemy, new[] { (PriorSplashWeapon, PriorSplashShots), (Weapon, numberOfShots) });
                }
            }
            return result;
        }
    }

    /// <summary>
    /// A logical element which requires Samus to kill enemies, possibly spending ammo if no other weapons work.
    /// </summary>
    public class UnfinalizedEnemyKill : AbstractUnfinalizedObjectLogicalElement<UnfinalizedEnemyKill, EnemyKill>
    {
        public IList<IList<string>> GroupedEnemyNames { get; set; } = new List<IList<string>>();

        /// <summary>
        /// <para>Only available after a call to <see cref="InitializeReferencedLogicalElementProperties(UnfinalizedSuperMetroidModel, UnfinalizedRoom)"/>.</para>
        /// <para>The enemies that this element's GroupedEnemyNames reference. These enemies are the enemies that must be killed,
        /// separated into groups that can be entirely hit by a single shot from a weapon that hits groups.</para>
        /// </summary>
        public IList<IList<UnfinalizedEnemy>> GroupedEnemies { get; set; }

        public ISet<string> ExplicitWeaponNames { get; set; } = new HashSet<string>();

        /// <summary>
        /// <para>Only available after a call to <see cref="InitializeReferencedLogicalElementProperties(UnfinalizedSuperMetroidModel, UnfinalizedRoom)"/>.</para>
        /// <para>The weapons that this element's ExplicitWeapons reference. These weapons are the only ones that are allowed for this enemy kill,
        /// overriding the default behavior of allowing all non-situational weapons.</para>
        /// </summary>
        public IList<UnfinalizedWeapon> ExplicitWeapons { get; set; }

        public ISet<string> ExcludedWeaponNames { get; set; } = new HashSet<string>();

        /// <summary>
        /// <para>Only available after a call to <see cref="InitializeReferencedLogicalElementProperties(UnfinalizedSuperMetroidModel, UnfinalizedRoom)"/>.</para>
        /// <para>The weapons that this element's ExcludedWeapons reference. These weapons are not allowed for this enemy kill.</para>
        /// </summary>
        public IList<UnfinalizedWeapon> ExcludedWeapons { get; set; }

        /// <summary>
        /// <para>Only available after a call to <see cref="InitializeReferencedLogicalElementProperties(UnfinalizedSuperMetroidModel, UnfinalizedRoom)"/>.</para>
        /// <para>The weapons that Samus may attempt to use to resolve this EnemyKill. This is built based on ExplicitWeapons, ExcludedWeapons,
        /// and the list of all existing weapons.</para>
        /// </summary>
        public IList<UnfinalizedWeapon> ValidWeapons { get; set; }

        public ISet<AmmoEnum> FarmableAmmo { get; set; } = new HashSet<AmmoEnum>();

        protected override EnemyKill CreateFinalizedElement(UnfinalizedEnemyKill sourceElement, Action<EnemyKill> mappingsInsertionCallback, ModelFinalizationMappings mappings)
        {
            return new EnemyKill(sourceElement, mappingsInsertionCallback, mappings);
        }

        public override IEnumerable<string> InitializeReferencedLogicalElementProperties(UnfinalizedSuperMetroidModel model, UnfinalizedRoom room)
        {
            List<string> unhandled = new List<string>();

            List<IList<UnfinalizedEnemy>> groupedEnemies = new List<IList<UnfinalizedEnemy>>();
            foreach(IList<string> enemyNameGroup in GroupedEnemyNames)
            {
                List<UnfinalizedEnemy> enemyGroup = new List<UnfinalizedEnemy>();
                foreach(string enemyName in enemyNameGroup)
                {
                    if (model.Enemies.TryGetValue(enemyName, out UnfinalizedEnemy enemy))
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

            List<UnfinalizedWeapon> explicitWeapons = new List<UnfinalizedWeapon>();
            foreach(string explicitWeaponName in ExplicitWeaponNames)
            {
                IEnumerable<UnfinalizedWeapon> weapons = explicitWeaponName.NameToWeapons(model);
                if (weapons == null)
                {
                    unhandled.Add($"Weapon {explicitWeaponName}");
                }
                else
                {
                    explicitWeapons.AddRange(weapons);
                }
            }
            ExplicitWeapons = explicitWeapons.Distinct().ToList();


            List<UnfinalizedWeapon> excludedWeapons = new List<UnfinalizedWeapon>();
            foreach (string excludedWeaponName in ExcludedWeaponNames)
            {
                IEnumerable<UnfinalizedWeapon> weapons = excludedWeaponName.NameToWeapons(model);
                if (weapons == null)
                {
                    unhandled.Add($"Weapon {excludedWeaponName}");
                }
                else
                {
                    excludedWeapons.AddRange(weapons);
                }
            }
            ExcludedWeapons = excludedWeapons.Distinct().ToList();

            List<UnfinalizedWeapon> validWeapons = new List<UnfinalizedWeapon>();
            // If some explicit weapons were provided, only they are allowed. Then take away any excluded weapons.
            if (ExplicitWeapons.Any())
            {
                validWeapons.AddRange(ExplicitWeapons.Except(ExcludedWeapons, ObjectReferenceEqualityComparer<UnfinalizedWeapon>.Default));
            }
            // If no explicit weapons were provided, the base list of weapons is all non-situational weapons. All of those are valid except excluded ones.
            else
            {
                validWeapons.AddRange(model.Weapons.Values.Where(w => !w.Situational).Except(ExcludedWeapons, ObjectReferenceEqualityComparer<UnfinalizedWeapon>.Default));
            }
            ValidWeapons = validWeapons;

            return unhandled.Distinct();
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
        public static IEnumerable<IList<Enemy>> RemoveEnemies(this IReadOnlyList<IReadOnlyList<Enemy>> enemyGroups, Predicate<Enemy> enemyRemovalCondition)
        {
            return enemyGroups
                // Transform each group of enemies into a new equivalent group, but with only a subset of enemies
                .Select(g => g
                    // Remove all enemies that meet the removal condition - so retain those that do not
                    .Where(e => !enemyRemovalCondition(e))
                    .ToList()
                )
                // After we removed enemies in the groups, eliminate groups with no enemy left
                .Where(g => g.Any());
        }
    }
}
