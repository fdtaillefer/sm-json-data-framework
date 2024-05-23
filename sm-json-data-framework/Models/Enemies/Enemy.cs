﻿using sm_json_data_framework.Utils;
using sm_json_data_framework.Models.Weapons;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.Json.Serialization;
using sm_json_data_framework.Models.InGameStates;
using sm_json_data_framework.Models.Items;
using sm_json_data_framework.Models.Requirements;
using sm_json_data_framework.Models.Raw.Enemies;
using sm_json_data_framework.Options;
using sm_json_data_framework.Models.Rooms;
using sm_json_data_framework.Rules;

namespace sm_json_data_framework.Models.Enemies
{
    /// <summary>
    /// The definition of an enemy in Super Metroid.
    /// </summary>
    public class Enemy : AbstractModelElement<UnfinalizedEnemy, Enemy>
    {
        public Enemy(UnfinalizedEnemy sourceElement, Action<Enemy> mappingsInsertionCallback, ModelFinalizationMappings mappings)
            : base(sourceElement, mappingsInsertionCallback)
        {
            Id = sourceElement.Id;
            Name = sourceElement.Name;
            Hp = sourceElement.Hp;
            AmountOfDrops = sourceElement.AmountOfDrops;
            Drops = sourceElement.Drops;
            FarmableDrops = sourceElement.FarmableDrops;
            Freezable = sourceElement.Freezable;
            Grapplable = sourceElement.Grapplable;
            Attacks = sourceElement.Attacks.Values.Select(attack => attack.Finalize(mappings)).ToDictionary(attack => attack.Name);
            Dimensions = sourceElement.Dimensions.Finalize(mappings);
            InvulnerableWeapons = sourceElement.InvulnerableWeapons.Select(weapon => weapon.Finalize(mappings)).ToDictionary(weapon => weapon.Name).AsReadOnly();
            WeaponMultipliers = sourceElement.WeaponMultipliers.Select(kvp => new KeyValuePair<string, WeaponMultiplier>(kvp.Key, kvp.Value.Finalize(mappings))).ToDictionary(kvp => kvp.Key, kvp => kvp.Value).AsReadOnly();
            WeaponSusceptibilities = sourceElement.WeaponSusceptibilities.Select(kvp => new KeyValuePair<string, WeaponSusceptibility>(kvp.Key, kvp.Value.Finalize(mappings))).ToDictionary(kvp => kvp.Key, kvp => kvp.Value).AsReadOnly();
            Areas = sourceElement.Areas.AsReadOnly();
        }

        /// <summary>
        /// A unique, arbitrary numerical ID that can identify this enemy.
        /// </summary>
        public int Id { get; }

        /// <summary>
        /// A unique name that can identify this enemy in a comprehensible way.
        /// </summary>
        public string Name { get; }

        /// <summary>
        /// The attacks of this enemy, mapped by name.
        /// </summary>
        public IReadOnlyDictionary<string, EnemyAttack> Attacks { get; }

        /// <summary>
        /// The amount of HP this enemy has.
        /// </summary>
        public int Hp { get; }

        /// <summary>
        /// The number of individual drops this enemy generates upon death.
        /// </summary>
        public int AmountOfDrops { get; }

        /// <summary>
        /// The drop rates associated to drops generated on this enemy's death.
        /// </summary>
        public EnemyDrops Drops { get; }

        /// <summary>
        /// The drop rates associated to drops generated by killing farmable adds spawned by this enemy.
        /// </summary>
        public EnemyDrops FarmableDrops { get; }

        /// <summary>
        /// The graphical dimensions of this enemy.
        /// </summary>
        public EnemyDimensions Dimensions { get; }

        /// <summary>
        /// Whether it's possible to freeze this enemy.
        /// </summary>
        public bool Freezable { get; }

        /// <summary>
        /// Whether it's possible to grapple off this enemy.
        /// </summary>
        public bool Grapplable { get; }

        /// <summary>
        /// All weapons this enemy is invulnerable to, mapped by name.
        /// </summary>
        public IReadOnlyDictionary<string, Weapon> InvulnerableWeapons { get; }

        /// <summary>
        /// Contains damage multipliers for all weapons this enemy takes damage from, mapped by weapon name.
        /// </summary>
        public IReadOnlyDictionary<string, WeaponMultiplier> WeaponMultipliers { get; }

        /// <summary>
        /// The set of in-game areas where this enemy can be found.
        /// </summary>
        public IReadOnlySet<string> Areas { get; }

        /// <summary>
        /// A dictionary containing all weapons this enemy takes damage from, alongside the number of shots needed to kill it. Mapped by weapon name.
        /// </summary>
        public IReadOnlyDictionary<string, WeaponSusceptibility> WeaponSusceptibilities { get; }

        /// <summary>
        /// Creates and returns an instance of EnemyDrops reprenting this enemy's effective drop rates,
        /// given that the provided resources are full.
        /// </summary>
        /// <param name="model">A model that can be used to obtain data about the current game configuration.</param>
        /// <param name="fullResources">An enumeration of rechargeable resources that are considered full
        /// (and hence no longer cause their corresponding enemy drop to happen).</param>
        /// <returns></returns>
        public EnemyDrops GetEffectiveDropRates(SuperMetroidModel model, IEnumerable<RechargeableResourceEnum> fullResources)
        {
            return model.Rules.CalculateEffectiveDropRates(Drops, model.Rules.GetUnneededDrops(fullResources));
        }

        /// <summary>
        /// Creates and returns an instance of EnemyDrops reprenting this enemy's effective drop rates,
        /// given that the provided resources are full.
        /// </summary>
        /// <param name="model">A model that can be used to obtain data about the current game configuration.</param>
        /// <param name="fullResources">An enumeration of consumable resources that are considered full
        /// (and hence no longer cause their corresponding enemy drop to happen).</param>
        /// <returns></returns>
        public EnemyDrops GetEffectiveDropRates(SuperMetroidModel model, IEnumerable<ConsumableResourceEnum> fullResources)
        {
            if (fullResources.Any())
            {
                return model.Rules.CalculateEffectiveDropRates(Drops, model.Rules.GetUnneededDrops(fullResources));
            }
            else
            {
                return Drops.Clone();
            }
        }

        protected override void PropagateLogicalOptions(ReadOnlyLogicalOptions logicalOptions, SuperMetroidRules rules)
        {
            foreach (EnemyAttack attack in Attacks.Values)
            {
                attack.ApplyLogicalOptions(logicalOptions, rules);
            }

            foreach (WeaponMultiplier weaponMultiplier in WeaponMultipliers.Values)
            {
                weaponMultiplier.ApplyLogicalOptions(logicalOptions, rules);
            }

            foreach (WeaponSusceptibility weaponSusceptibility in WeaponSusceptibilities.Values)
            {
                weaponSusceptibility.ApplyLogicalOptions(logicalOptions, rules);
            }

            Dimensions.ApplyLogicalOptions(logicalOptions, rules);
        }

        public override bool CalculateLogicallyRelevant(SuperMetroidRules rules)
        {
            // An enemy always has some relevance
            return true;
        }
    }

    public class UnfinalizedEnemy : AbstractUnfinalizedModelElement<UnfinalizedEnemy, Enemy>, InitializablePostDeserializeOutOfRoom
    {
        public int Id { get; set; }

        public string Name { get; set; }

        /// <summary>
        /// The attacks of this enemy, mapped by name
        /// </summary>
        public IDictionary<string, UnfinalizedEnemyAttack> Attacks { get; set; } = new Dictionary<string, UnfinalizedEnemyAttack>();

        public int Hp { get; set; }

        public int AmountOfDrops { get; set; }

        public EnemyDrops Drops { get; set; }

        public EnemyDrops FarmableDrops { get; set; }

        public UnfinalizedEnemyDimensions Dimensions { get; set; }

        public bool Freezable { get; set; }

        public bool Grapplable { get; set; }

        public ISet<string> InvulnerabilityStrings { get; set; } = new HashSet<string>();

        /// <summary>
        /// <para>Not available before <see cref="Initialize(UnfinalizedSuperMetroidModel)"/> has been called.</para>
        /// <para>The sequence of all weapons this enemy is invulnerable to.</para>
        /// </summary>
        public IList<UnfinalizedWeapon> InvulnerableWeapons { get; private set; }

        public IList<RawEnemyDamageMultiplier> RawDamageMultipliers { get; set; } = new List<RawEnemyDamageMultiplier>();

        /// <summary>
        /// <para>Not available before <see cref="Initialize(UnfinalizedSuperMetroidModel)"/> has been called.</para>
        /// <para>Contains damage multipliers for all weapons this enemy takes damage from, mapped by weapon name.</para>
        /// </summary>
        public Dictionary<string, UnfinalizedWeaponMultiplier> WeaponMultipliers { get; private set; }

        public ISet<string> Areas { get; set; } = new HashSet<string>();

        /// <summary>
        /// <para>Not available before <see cref="Initialize(UnfinalizedSuperMetroidModel)"/> has been called.</para>
        /// <para>A dictionary containing all weapons this enemy takes damage from, alongside the number of shots needed to kill it. Mapped by weapon name.</para>
        /// </summary>
        public IDictionary<string, UnfinalizedWeaponSusceptibility> WeaponSusceptibilities { get; set; }

        public UnfinalizedEnemy()
        {

        }

        public UnfinalizedEnemy(RawEnemy rawEnemy)
        {
            Id = rawEnemy.Id;
            Name = rawEnemy.Name;
            Attacks = rawEnemy.Attacks.ToDictionary(attack => attack.Name);
            Hp = rawEnemy.Hp;
            AmountOfDrops = rawEnemy.AmountOfDrops;
            Drops = new EnemyDrops(rawEnemy.Drops);
            if (rawEnemy.FarmableDrops != null)
            {
                FarmableDrops = new EnemyDrops(rawEnemy.FarmableDrops);
            }
            Dimensions = new UnfinalizedEnemyDimensions(rawEnemy.Dims);
            Freezable = rawEnemy.Freezable;
            Grapplable = rawEnemy.Grapplable;
            InvulnerabilityStrings = new HashSet<string>(rawEnemy.Invul);
            RawDamageMultipliers = rawEnemy.DamageMultipliers.Select(multiplier => multiplier.CLone()).ToList();
            Areas = new HashSet<string>(rawEnemy.Areas);
        }

        protected override Enemy CreateFinalizedElement(UnfinalizedEnemy sourceElement, Action<Enemy> mappingsInsertionCallback, ModelFinalizationMappings mappings)
        {
            return new Enemy(sourceElement, mappingsInsertionCallback, mappings);
        }

        public void InitializeProperties(UnfinalizedSuperMetroidModel model)
        {
            // Convert InvulnerabilityStrings to Weapons
            InvulnerableWeapons = InvulnerabilityStrings.NamesToWeapons(model).ToList();

            // Get a WeaponMultiplier for all non-immune weapons
            WeaponMultipliers = RawDamageMultipliers
                .SelectMany(rdm => rdm.Weapon.NameToWeapons(model).Select(w => new UnfinalizedWeaponMultiplier(w, rdm.Value)))
                .ToDictionary(m => m.Weapon.Name);
            foreach (UnfinalizedWeapon neutralWeapon in model.Weapons.Values
                .Except(WeaponMultipliers.Values.Select(wm => wm.Weapon), ReferenceEqualityComparer.Instance)
                .Except(InvulnerableWeapons, ReferenceEqualityComparer.Instance))
            {
                WeaponMultipliers.Add(neutralWeapon.Name, new UnfinalizedWeaponMultiplier(neutralWeapon, 1m));
            }

            // Create a WeaponSusceptibility for each non-immune weapon
            WeaponSusceptibilities = WeaponMultipliers.Values
                .Select(wm => new UnfinalizedWeaponSusceptibility(wm.NumberOfHits(Hp), wm))
                .ToDictionary(ws => ws.Weapon.Name);
        }

        public IEnumerable<string> InitializeReferencedLogicalElementProperties(UnfinalizedSuperMetroidModel model)
        {
            // No logical elements in here
            return Enumerable.Empty<string>();
        }

        /// <summary>
        /// Creates and returns an instance of EnemyDrops reprenting this enemy's effective drop rates,
        /// given that the provided resources are full.
        /// </summary>
        /// <param name="model">A model that can be used to obtain data about the current game configuration.</param>
        /// <param name="fullResources">An enumeration of rechargeable resources that are considered full
        /// (and hence no longer cause their corresponding enemy drop to happen).</param>
        /// <returns></returns>
        public EnemyDrops GetEffectiveDropRates(UnfinalizedSuperMetroidModel model, IEnumerable<RechargeableResourceEnum> fullResources)
        {
            return model.Rules.CalculateEffectiveDropRates(Drops, model.Rules.GetUnneededDrops(fullResources));
        }

        /// <summary>
        /// Creates and returns an instance of EnemyDrops reprenting this enemy's effective drop rates,
        /// given that the provided resources are full.
        /// </summary>
        /// <param name="model">A model that can be used to obtain data about the current game configuration.</param>
        /// <param name="fullResources">An enumeration of consumable resources that are considered full
        /// (and hence no longer cause their corresponding enemy drop to happen).</param>
        /// <returns></returns>
        public EnemyDrops GetEffectiveDropRates(UnfinalizedSuperMetroidModel model, IEnumerable<ConsumableResourceEnum> fullResources)
        {
            if(fullResources.Any())
            {
                return model.Rules.CalculateEffectiveDropRates(Drops, model.Rules.GetUnneededDrops(fullResources));
            }
            else
            {
                return Drops.Clone();
            }
        }
    }
}
