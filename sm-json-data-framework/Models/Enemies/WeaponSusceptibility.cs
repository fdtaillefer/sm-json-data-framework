using sm_json_data_framework.Models.Weapons;
using sm_json_data_framework.Options;
using System;
using System.Collections.Generic;
using System.Text;

namespace sm_json_data_framework.Models.Enemies
{
    public class WeaponSusceptibility : AbstractModelElement<UnfinalizedWeaponSusceptibility, WeaponSusceptibility>
    {
        private UnfinalizedWeaponSusceptibility InnerElement { get; set; }

        public WeaponSusceptibility(UnfinalizedWeaponSusceptibility innerElement, Action<WeaponSusceptibility> mappingsInsertionCallback, ModelFinalizationMappings mappings)
            : base(innerElement, mappingsInsertionCallback)
        {
            InnerElement = innerElement;
            WeaponMultiplier = InnerElement.WeaponMultiplier.Finalize(mappings);
        }

        /// <summary>
        /// The WeaponMultiplier that is the basis behind this WeaponSusceptibility.
        /// It knows the damage per shot, and how many shots it takes to kill an enemy with a given amount of hp.
        /// </summary>
        public WeaponMultiplier WeaponMultiplier { get; }

        /// <summary>
        /// The weapon the implicit enemy is susceptible to
        /// </summary>
        public Weapon Weapon => WeaponMultiplier.Weapon;

        /// <summary>
        /// The number of perfectly-aimed shots of the weapon it would take to kill the implicit enemy
        /// </summary>
        public int Shots => InnerElement.Shots;

        /// <summary>
        /// The amount of damage done to the implicit enemy per successful shot of the weapon
        /// </summary>
        public int DamagePerShot => WeaponMultiplier.DamagePerShot;

        /// <summary>
        /// Calculates the number of hits an enemy with the provided hp will take to die if this WeaponMultiplier is applicable to the attack.
        /// </summary>
        /// <param name="enemyHp">Hp of the enemy</param>
        /// <returns>The number of hits to kill the enemy</returns>
        public int NumberOfHits(int enemyHp)
        {
            return WeaponMultiplier.NumberOfHits(enemyHp);
        }

        protected override bool PropagateLogicalOptions(ReadOnlyLogicalOptions logicalOptions)
        {
            // The state of the weapon itself is useful to calculate logical relevance, so propagate to it to be sure it's up-to-date when we calculate.
            Weapon.ApplyLogicalOptions(logicalOptions);

            return false;
        }

        public override bool CalculateLogicallyRelevant()
        {
            // If our weapon isn't relevant, neither is a susceptibility involving it.
            return Weapon.LogicallyRelevant;
        }
    }

    /// <summary>
    /// Describes an implicit enemy's susceptibility to a weapon, and the number of perfectly-aimed shots it would take
    /// from this weapon to kill the enemy.
    /// </summary>
    public class UnfinalizedWeaponSusceptibility : AbstractUnfinalizedModelElement<UnfinalizedWeaponSusceptibility, WeaponSusceptibility>
    {
        public UnfinalizedWeaponSusceptibility() { }

        public UnfinalizedWeaponSusceptibility(int shots, UnfinalizedWeaponMultiplier weaponMultiplier)
        {
            Shots = shots;
            WeaponMultiplier = weaponMultiplier;
        }

        protected override WeaponSusceptibility CreateFinalizedElement(UnfinalizedWeaponSusceptibility sourceElement, Action<WeaponSusceptibility> mappingsInsertionCallback, ModelFinalizationMappings mappings)
        {
            return new WeaponSusceptibility(sourceElement, mappingsInsertionCallback, mappings);
        }

        /// <summary>
        /// The weapon the implicit enemy is susceptible to
        /// </summary>
        public UnfinalizedWeapon Weapon => WeaponMultiplier.Weapon;

        /// <summary>
        /// The number of perfectly-aimed shots of the weapon it would take to kill the implicit enemy
        /// </summary>
        public int Shots { get; }

        /// <summary>
        /// The amount of damage done to the implicit enemy per successful shot of the weapon
        /// </summary>
        public int DamagePerShot => WeaponMultiplier.DamagePerShot;

        /// <summary>
        /// Calculates the number of hits an enemy with the provided hp will take to die if this WeaponMultiplier is applicable to the attack.
        /// </summary>
        /// <param name="enemyHp">Hp of the enemy</param>
        /// <returns>The number of hits to kill the enemy</returns>
        public int NumberOfHits(int enemyHp)
        {
            return WeaponMultiplier.NumberOfHits(enemyHp);
        }

        /// <summary>
        /// The WeaponMultiplier that is the basis behind this WeaponSusceptibility.
        /// It knows the damage per shot, and how many shots it takes to kill an enemy with a given amount of hp.
        /// </summary>
        public UnfinalizedWeaponMultiplier WeaponMultiplier { get; }
    }
}
