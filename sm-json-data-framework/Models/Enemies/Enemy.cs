using sm_json_data_framework.Utils;
using sm_json_data_framework.Models.Weapons;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.Json.Serialization;

namespace sm_json_data_framework.Models.Enemies
{
    public class Enemy : InitializablePostDeserializeOutOfRoom
    {
        public int Id { get; set; }

        public string Name { get; set; }

        [JsonPropertyName("attacks")]
        public IEnumerable<EnemyAttack> AttacksSequence { get; set; } = Enumerable.Empty<EnemyAttack>();

        private IDictionary<string, EnemyAttack> _attacksDictionary;
        /// <summary>
        /// The attacks of this enemy, mapped by name
        /// </summary>
        [JsonIgnore]
        public IDictionary<string, EnemyAttack> Attacks
        {
            get
            {
                if (_attacksDictionary == null)
                {
                    _attacksDictionary = AttacksSequence.ToDictionary(a => a.Name);
                }
                return _attacksDictionary;
            }
        }

        public int Hp { get; set; }

        public int AmountOfDrops { get; set; }

        public EnemyDrops Drops { get; set; }

        public EnemyDrops FarmableDrops { get; set; }

        [JsonPropertyName("dims")]
        public EnemyDimensions Dimensions { get; set; }

        public bool Freezable { get; set; }

        public bool Grapplable { get; set; }

        public int? RespawnFrames { get; set; }

        [JsonPropertyName("invul")]
        public IEnumerable<string> InvulnerabilityStrings { get; set; } = Enumerable.Empty<string>();

        /// <summary>
        /// <para>Not available before <see cref="Initialize(SuperMetroidModel)"/> has been called.</para>
        /// <para>The sequence of all weapons this enemy is invulnerable to.</para>
        /// </summary>
        [JsonIgnore]
        public IEnumerable<Weapon> InvulnerableWeapons { get; private set; }

        [JsonPropertyName("damageMultipliers")]
        public IEnumerable<RawEnemyDamageMultiplier> RawDamageMultipliers { get; set; } = Enumerable.Empty<RawEnemyDamageMultiplier>();

        /// <summary>
        /// <para>Not available before <see cref="Initialize(SuperMetroidModel)"/> has been called.</para>
        /// <para>Contains damage multipliers for all weapons this enemy takes damage from, mapped by weapon name.</para>
        /// </summary>
        [JsonIgnore]
        public Dictionary<string, WeaponMultiplier> WeaponMultipliers { get; private set; }

        public IEnumerable<string> Areas { get; set; } = Enumerable.Empty<string>();

        /// <summary>
        /// <para>Not available before <see cref="Initialize(SuperMetroidModel)"/> has been called.</para>
        /// <para>A dictionary containing all weapons this enemy takes damage from, alongside the number of shots needed to kill it</para>
        /// </summary>
        [JsonIgnore]
        public IDictionary<string, WeaponSusceptibility> WeaponSusceptibilities { get; set; }

        public void Initialize(SuperMetroidModel model)
        {
            // convert InvulnerabilityStrings to Weapons
            InvulnerableWeapons = InvulnerabilityStrings.NamesToWeapons(model);

            // Get a WeaponMultiplier for all non-immune weapons
            WeaponMultipliers = RawDamageMultipliers
                .SelectMany(rdm => rdm.Weapon.NameToWeapons(model).Select(w => new WeaponMultiplier(w, rdm.Value)))
                .ToDictionary(m => m.Weapon.Name);
            foreach(Weapon neutralWeapon in model.Weapons.Values
                .Except(WeaponMultipliers.Values.Select(wm => wm.Weapon), ObjectReferenceEqualityComparer<Weapon>.Default)
                .Except(InvulnerableWeapons, ObjectReferenceEqualityComparer<Weapon>.Default))
            {
                WeaponMultipliers.Add(neutralWeapon.Name, new WeaponMultiplier(neutralWeapon, 1m));
            }

            // Create a WeaponSusceptibility for each non-immune weapon
            WeaponSusceptibilities = WeaponMultipliers.Values
                .Select(wm => new WeaponSusceptibility(wm.Weapon, wm.NumberOfHits(Hp), wm.DamagePerShot))
                .ToDictionary(ws => ws.Weapon.Name);
        }

        public IEnumerable<string> InitializeReferencedLogicalElementProperties(SuperMetroidModel model)
        {
            // No logical elements in here
            return Enumerable.Empty<string>();
        }
    }
}
