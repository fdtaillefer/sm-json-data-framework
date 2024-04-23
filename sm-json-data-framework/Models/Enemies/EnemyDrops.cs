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
                case EnemyDropEnum.NoDrop:
                    NoDrop = dropRate;
                    break;
                case EnemyDropEnum.SmallEnergy:
                    SmallEnergy = dropRate;
                    break;
                case EnemyDropEnum.BigEnergy:
                    BigEnergy = dropRate;
                    break;
                case EnemyDropEnum.Missile:
                    Missile = dropRate;
                    break;
                case EnemyDropEnum.Super:
                    Super = dropRate;
                    break;
                case EnemyDropEnum.PowerBomb:
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
                EnemyDropEnum.NoDrop => NoDrop,
                EnemyDropEnum.SmallEnergy => SmallEnergy,
                EnemyDropEnum.BigEnergy => BigEnergy,
                EnemyDropEnum.Missile => Missile,
                EnemyDropEnum.Super => Super,
                EnemyDropEnum.PowerBomb => PowerBomb,
                _ => throw new Exception($"Unrecognized enemy drop {enemyDrop}")
            };
        }

        public override bool Equals(object obj)
        {
            return obj is EnemyDrops drops &&
                   NoDrop == drops.NoDrop &&
                   SmallEnergy == drops.SmallEnergy &&
                   BigEnergy == drops.BigEnergy &&
                   Missile == drops.Missile &&
                   Super == drops.Super &&
                   PowerBomb == drops.PowerBomb;
        }

        public override int GetHashCode()
        {
            return HashCode.Combine(NoDrop, SmallEnergy, BigEnergy, Missile, Super, PowerBomb);
        }
    }
}
