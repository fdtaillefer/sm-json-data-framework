using System;
using System.Collections.Generic;
using System.Text;

namespace sm_json_data_framework.Models.Raw.Enemies
{
    public class RawEnemyDamageMultiplier
    {
        public RawEnemyDamageMultiplier()
        {

        }

        public RawEnemyDamageMultiplier(RawEnemyDamageMultiplier other)
        {
            this.Weapon = other.Weapon;
            this.Value = other.Value;
        }

        public RawEnemyDamageMultiplier CLone()
        {
            return new RawEnemyDamageMultiplier(this);
        }

        public string Weapon { get; set; }

        public decimal Value { get; set; }
    }
}
