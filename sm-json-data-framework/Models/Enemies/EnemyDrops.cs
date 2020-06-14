using System;
using System.Collections.Generic;
using System.Text;

namespace sm_json_data_framework.Models.Enemies
{
    /// <summary>
    /// Contains drop rates for all enemy drop types.
    /// This class makes no assumption about whether those rates are out of 100 or 102 or 255 or any other value.
    /// </summary>
    public class EnemyDrops
    {
        public decimal NoDrop { get; set; }

        public decimal SmallEnergy { get; set; }

        public decimal BigEnergy { get; set; }

        public decimal Missile { get; set; }

        public decimal Super { get; set; }

        public decimal PowerBomb { get; set; }

        public EnemyDrops()
        {

        }

        public EnemyDrops(EnemyDrops other)
        {
            NoDrop = other.NoDrop;
            SmallEnergy = other.SmallEnergy;
            BigEnergy = other.BigEnergy;
            Missile = other.Missile;
            Super = other.Super;
            PowerBomb = other.PowerBomb;
        }

        public EnemyDrops Clone()
        {
            return new EnemyDrops(this);
        }

        /// <summary>
        /// Sets the provided drop rate for the provided EnemyDrop in this EnemyDrops.
        /// </summary>
        /// <param name="enemyDrop">The enemy drop for which to set a drop rate</param>
        /// <param name="dropRate">The drop rate to set</param>
        public void SetDropRate(EnemyDropEnum enemyDrop, decimal dropRate)
        {
            switch(enemyDrop)
            {
                case EnemyDropEnum.NO_DROP:
                    NoDrop = dropRate;
                    break;
                case EnemyDropEnum.SMALL_ENERGY:
                    SmallEnergy = dropRate;
                    break;
                case EnemyDropEnum.BIG_ENERGY:
                    BigEnergy = dropRate;
                    break;
                case EnemyDropEnum.MISSILE:
                    Missile = dropRate;
                    break;
                case EnemyDropEnum.SUPER:
                    Super = dropRate;
                    break;
                case EnemyDropEnum.POWER_BOMB:
                    PowerBomb = dropRate;
                    break;
            }
        }

        /// <summary>
        /// Returns the drop rate in this EnemyDrops for the provided enemy drop.
        /// </summary>
        /// <param name="enemyDrop"></param>
        /// <returns></returns>
        public decimal GetDropRate(EnemyDropEnum enemyDrop)
        {
            return enemyDrop switch
            {
                EnemyDropEnum.NO_DROP => NoDrop,
                EnemyDropEnum.SMALL_ENERGY => SmallEnergy,
                EnemyDropEnum.BIG_ENERGY => BigEnergy,
                EnemyDropEnum.MISSILE => Missile,
                EnemyDropEnum.SUPER => Super,
                EnemyDropEnum.POWER_BOMB => PowerBomb,
                _ => throw new Exception($"Unrecognized enemy drop {enemyDrop}")
            };
        }
    }
}
