using sm_json_data_framework.Models.Raw.Enemies;
using sm_json_data_framework.Options;
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
        /// <summary>
        /// The rate for a drop producing nothing.
        /// </summary>
        public decimal NoDrop { get; }

        /// <summary>
        /// The rate for a drop producing small energy.
        /// </summary>
        public decimal SmallEnergy { get; }

        /// <summary>
        /// The rate for a drop producing big energy.
        /// </summary>
        public decimal BigEnergy { get; }

        /// <summary>
        /// The rate for a drop producing missiles.
        /// </summary>
        public decimal Missile { get; }

        /// <summary>
        /// The rate for a drop producing a super missile.
        /// </summary>
        public decimal Super { get; }

        /// <summary>
        /// The rate for a drop producing a power bomb.
        /// </summary>
        public decimal PowerBomb { get; }

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

        public EnemyDrops(RawEnemyDrops drops)
        {
            NoDrop = drops.NoDrop;
            SmallEnergy = drops.SmallEnergy;
            BigEnergy = drops.BigEnergy;
            Missile = drops.Missile;
            Super = drops.Super;
            PowerBomb = drops.PowerBomb;
        }

        public EnemyDrops(decimal noDrop = 0, decimal smallEnergy = 0, decimal bigEnergy = 0, decimal missile = 0, decimal super = 0, decimal powerBomb = 0)
        {
            NoDrop = noDrop;
            SmallEnergy = smallEnergy;
            BigEnergy = bigEnergy;
            Missile = missile;
            Super = super;
            PowerBomb = powerBomb;
        }

        public EnemyDrops Clone()
        {
            return new EnemyDrops(this);
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
