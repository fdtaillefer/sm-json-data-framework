
using sm_json_data_framework.Models.Enemies;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace sm_json_data_framework.Tests.Models.Enemies
{
    public class EnemyDropsTest
    {

        #region Tests for GetDropRate()

        [Fact]
        public void getDropRate_ReturnsCorrectValue()
        {
            EnemyDrops enemyDrops = new EnemyDrops(
                noDrop: 2,
                smallEnergy: 4,
                bigEnergy: 8,
                missile: 12,
                super: 16,
                powerBomb: 60
            );

            Assert.Equal(2, enemyDrops.GetDropRate(EnemyDropEnum.NoDrop));
            Assert.Equal(4, enemyDrops.GetDropRate(EnemyDropEnum.SmallEnergy));
            Assert.Equal(8, enemyDrops.GetDropRate(EnemyDropEnum.BigEnergy));
            Assert.Equal(12, enemyDrops.GetDropRate(EnemyDropEnum.Missile));
            Assert.Equal(16, enemyDrops.GetDropRate(EnemyDropEnum.Super));
            Assert.Equal(60, enemyDrops.GetDropRate(EnemyDropEnum.PowerBomb));
        }

        #endregion
    }
}
