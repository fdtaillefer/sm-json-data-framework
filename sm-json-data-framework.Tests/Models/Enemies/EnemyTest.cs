using sm_json_data_framework.Models;
using sm_json_data_framework.Models.Connections;
using sm_json_data_framework.Models.Enemies;
using sm_json_data_framework.Models.Weapons;
using sm_json_data_framework.Options;
using sm_json_data_framework.Tests.TestTools;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace sm_json_data_framework.Tests.Models.Enemies
{
    public class EnemyTest
    {
        private static SuperMetroidModel Model = StaticTestObjects.UnmodifiableModel;
        private static SuperMetroidModel ModelWithOptions = StaticTestObjects.UnfinalizedModel.Finalize();

        #region Tests for construction from unfinalized model

        [Fact]
        public void CtorFromUnfinalized_SetsPropertiesCorrectly()
        {
            // Given/when standard model creation

            // Expect
            Enemy enemy = Model.Enemies["Pink Space Pirate (standing)"];

            Assert.Equal(71, enemy.Id);
            Assert.Equal("Pink Space Pirate (standing)", enemy.Name);
            Assert.Equal(2, enemy.Attacks.Count);
            Assert.Equal(300, enemy.Hp);
            Assert.Equal(1, enemy.AmountOfDrops);
            Assert.Equal(0, enemy.Drops.NoDrop);
            Assert.Equal(20, enemy.Drops.SmallEnergy);
            Assert.Equal(48, enemy.Drops.BigEnergy);
            Assert.Equal(32, enemy.Drops.Missile);
            Assert.Equal(2, enemy.Drops.Super);
            Assert.Equal(0, enemy.Drops.PowerBomb);
            Assert.Null(enemy.FarmableDrops);
            Assert.NotNull(enemy.Dimensions);
            Assert.True(enemy.Freezable);
            Assert.False(enemy.Grapplable);
            int expectedInvulnerableCount = 4 + Model.WeaponsByCategory[WeaponCategoryEnum.BeamNoPlasma].Count + Model.WeaponsByCategory[WeaponCategoryEnum.PowerBombBlast].Count;
            int expectedSusceptibilityCount = Model.Weapons.Count - expectedInvulnerableCount;
            Assert.Equal(expectedInvulnerableCount, enemy.InvulnerableWeapons.Count);
            Assert.Empty(enemy.WeaponMultipliers.Where(multiplier => multiplier.Value.Multiplier != 1));
            Assert.Equal(expectedSusceptibilityCount, enemy.WeaponMultipliers.Where(multiplier => multiplier.Value.Multiplier == 1).Count());
            Assert.Equal(1, enemy.Areas.Count);
            Assert.Equal(expectedSusceptibilityCount, enemy.WeaponSusceptibilities.Count);

            Enemy otherEnemy = Model.Enemies["Bomb Torizo"];
            Assert.Equal(22, otherEnemy.FarmableDrops.NoDrop);
            Assert.Equal(46, otherEnemy.FarmableDrops.SmallEnergy);
            Assert.Equal(8, otherEnemy.FarmableDrops.BigEnergy);
            Assert.Equal(26, otherEnemy.FarmableDrops.Missile);
            Assert.Equal(0, otherEnemy.FarmableDrops.Super);
            Assert.Equal(0, otherEnemy.FarmableDrops.PowerBomb);
            Assert.Equal(1, otherEnemy.WeaponMultipliers.Where(multiplier => multiplier.Value.Multiplier != 1).Count());
        }

        #endregion

        #region Tests for ApplyLogicalOptions() that check applied logical properties

        [Fact]
        public void ApplyLogicalOptions_SetsLogicalProperties()
        {
            // Given
            LogicalOptions logicalOptions = new LogicalOptions();

            // When
            ModelWithOptions.ApplyLogicalOptions(logicalOptions);

            // Expect
            Enemy enemy = ModelWithOptions.Enemies["Evir"];
            Assert.True(enemy.LogicallyRelevant);
        }

        #endregion
    }
}
