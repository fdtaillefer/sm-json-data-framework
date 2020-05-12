using sm_json_data_framework.Models.Weapons;
using System;
using System.Collections.Generic;
using System.Text;

namespace sm_json_data_framework.Models.Enemies
{
    /// <summary>
    /// Describes an implicit enemy's susceptibility to a weapon, and the number of perfectly-aimed shots it would take
    /// from this weapon to kill the enemy.
    /// </summary>
    public class WeaponSusceptibility
    {
        public WeaponSusceptibility() { }

        public WeaponSusceptibility(int shots, WeaponMultiplier weaponMultiplier)
        {
            Shots = shots;
            WeaponMultiplier = weaponMultiplier;
        }

        /// <summary>
        /// The weapon the implicit enemy is susceptible to
        /// </summary>
        public Weapon Weapon { get { return WeaponMultiplier.Weapon; } }

        /// <summary>
        /// The number of perfectly-aimed shots of the weapon it would take to kill the implicit enemy
        /// </summary>
        public int Shots { get; }

        /// <summary>
        /// The amount of damage done to the implicit enemy per successful shot of the weapon
        /// </summary>
        public int DamagePerShot { get {return WeaponMultiplier.DamagePerShot; } }

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
        public WeaponMultiplier WeaponMultiplier { get; }
    }
}
