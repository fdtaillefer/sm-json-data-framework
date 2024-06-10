using sm_json_data_framework.Models.Enemies;
using sm_json_data_framework.Models;
using sm_json_data_framework.Options;
using sm_json_data_framework.Tests.TestTools;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace sm_json_data_framework.Tests.Models.Enemies
{
    public class EnemyAttackTest
    {
        private static SuperMetroidModel Model = StaticTestObjects.UnmodifiableModel;
        private static SuperMetroidModel ModelWithOptions = StaticTestObjects.UnfinalizedModel.Finalize();

        #region Tests for construction from unfinalized model

        [Fact]
        public void CtorFromUnfinalized_SetsPropertiesCorrectly()
        {
            // Given/when standard model creation

            // Expect
            EnemyAttack enemyAttack = Model.Enemies["Mother Brain 2"].Attacks["rainbow"];
            Assert.Equal("rainbow", enemyAttack.Name);
            Assert.Equal(600, enemyAttack.BaseDamage);
            Assert.True(enemyAttack.AffectedByVaria);
            Assert.False(enemyAttack.AffectedByGravity);
        }

        #endregion

        #region Tests for ApplyLogicalOptions() that check applied logical properties

        [Fact]
        public void ApplyLogicalOptions_SetsLogicalProperties()
        {
            // Given
            LogicalOptions logicalOptions = new LogicalOptions();
            logicalOptions.RegisterRemovedItem("Ice");

            // When
            ModelWithOptions.ApplyLogicalOptions(logicalOptions);

            // Expect
            EnemyAttack enemyAttack = ModelWithOptions.Enemies["Evir"].Attacks["contact"];
            Assert.True(enemyAttack.LogicallyRelevant);
        }

        #endregion
    }
}
