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

        public WeaponSusceptibility(Weapon weapon, int shots, int damagePerShot)
        {
            Weapon = weapon;
            Shots = shots;
            DamagePerShot = damagePerShot;
        }

        /// <summary>
        /// The weapon the implicit enemy is susceptible to
        /// </summary>
        public Weapon Weapon { get; set; }

        /// <summary>
        /// The number of perfectly-aimed shots of the weapon it would take to kill the implicit enemy
        /// </summary>
        public int Shots { get; set; }

        /// <summary>
        /// The amount of damage done to the implicit enemy per successful shot of the weapon
        /// </summary>
        public int DamagePerShot { get; set; }
    }
}
