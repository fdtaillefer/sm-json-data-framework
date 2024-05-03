using sm_json_data_framework.Utils;
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

namespace sm_json_data_framework.Models.Enemies
{
    public class Enemy : AbstractModelElement, InitializablePostDeserializeOutOfRoom
    {
        public int Id { get; set; }

        public string Name { get; set; }

        /// <summary>
        /// The attacks of this enemy, mapped by name
        /// </summary>
        public IDictionary<string, EnemyAttack> Attacks { get; set; } = new Dictionary<string, EnemyAttack>();

        public int Hp { get; set; }

        public int AmountOfDrops { get; set; }

        public EnemyDrops Drops { get; set; }

        public EnemyDrops FarmableDrops { get; set; }

        [JsonPropertyName("dims")]
        public EnemyDimensions Dimensions { get; set; }

        public bool Freezable { get; set; }

        public bool Grapplable { get; set; }

        [JsonPropertyName("invul")]
        public ISet<string> InvulnerabilityStrings { get; set; } = new HashSet<string>();

        /// <summary>
        /// <para>Not available before <see cref="Initialize(SuperMetroidModel)"/> has been called.</para>
        /// <para>The sequence of all weapons this enemy is invulnerable to.</para>
        /// </summary>
        [JsonIgnore]
        public IList<Weapon> InvulnerableWeapons { get; private set; }

        [JsonPropertyName("damageMultipliers")]
        public IList<RawEnemyDamageMultiplier> RawDamageMultipliers { get; set; } = new List<RawEnemyDamageMultiplier>();

        /// <summary>
        /// <para>Not available before <see cref="Initialize(SuperMetroidModel)"/> has been called.</para>
        /// <para>Contains damage multipliers for all weapons this enemy takes damage from, mapped by weapon name.</para>
        /// </summary>
        [JsonIgnore]
        public Dictionary<string, WeaponMultiplier> WeaponMultipliers { get; private set; }

        public ISet<string> Areas { get; set; } = new HashSet<string>();

        /// <summary>
        /// <para>Not available before <see cref="Initialize(SuperMetroidModel)"/> has been called.</para>
        /// <para>A dictionary containing all weapons this enemy takes damage from, alongside the number of shots needed to kill it</para>
        /// </summary>
        [JsonIgnore]
        public IDictionary<string, WeaponSusceptibility> WeaponSusceptibilities { get; set; }

        public Enemy()
        {

        }

        public Enemy(RawEnemy rawEnemy)
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
            Dimensions = new EnemyDimensions(rawEnemy.Dims);
            Freezable = rawEnemy.Freezable;
            Grapplable = rawEnemy.Grapplable;
            InvulnerabilityStrings = new HashSet<string>(rawEnemy.Invul);
            RawDamageMultipliers = rawEnemy.DamageMultipliers.Select(multiplier => multiplier.CLone()).ToList();
            Areas = new HashSet<string>(rawEnemy.Areas);
        }

        protected override bool ApplyLogicalOptionsEffects(ReadOnlyLogicalOptions logicalOptions)
        {
            // Logical options have no power here, but we can still delegate to the attacks.
            // The other sub-models are too abstract to be considered elements
            foreach (EnemyAttack attack in Attacks.Values)
            {
                attack.ApplyLogicalOptions(logicalOptions);
            }
            return false;
        }

        public void InitializeProperties(SuperMetroidModel model)
        {
            // Convert InvulnerabilityStrings to Weapons
            InvulnerableWeapons = InvulnerabilityStrings.NamesToWeapons(model).ToList();

            // Get a WeaponMultiplier for all non-immune weapons
            WeaponMultipliers = RawDamageMultipliers
                .SelectMany(rdm => rdm.Weapon.NameToWeapons(model).Select(w => new WeaponMultiplier(w, rdm.Value)))
                .ToDictionary(m => m.Weapon.Name);
            foreach (Weapon neutralWeapon in model.Weapons.Values
                .Except(WeaponMultipliers.Values.Select(wm => wm.Weapon), ObjectReferenceEqualityComparer<Weapon>.Default)
                .Except(InvulnerableWeapons, ObjectReferenceEqualityComparer<Weapon>.Default))
            {
                WeaponMultipliers.Add(neutralWeapon.Name, new WeaponMultiplier(neutralWeapon, 1m));
            }

            // Create a WeaponSusceptibility for each non-immune weapon
            WeaponSusceptibilities = WeaponMultipliers.Values
                .Select(wm => new WeaponSusceptibility(wm.NumberOfHits(Hp), wm))
                .ToDictionary(ws => ws.Weapon.Name);
        }

        public bool CleanUpUselessValues(SuperMetroidModel model)
        {
            // Nothing relevant to clean up
            return true;
        }

        public IEnumerable<string> InitializeReferencedLogicalElementProperties(SuperMetroidModel model)
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
