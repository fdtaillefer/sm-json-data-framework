using sm_json_data_framework.Models;
using sm_json_data_framework.Models.Enemies;
using sm_json_data_framework.Options;
using sm_json_data_framework.Tests.TestTools;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

namespace sm_json_data_framework.Tests.Models.Enemies
{
    public class WeaponMultiplierTest
    {
        private static SuperMetroidModel ReusableModel() => StaticTestObjects.UnmodifiableModel;
        private static SuperMetroidModel NewModelForOptions() => StaticTestObjects.UnfinalizedModel.Finalize();

        #region Tests for construction from unfinalized model

        [Fact]
        public void CtorFromUnfinalized_SetsPropertiesCorrectly()
        {
            // Given/when standard model creation
            SuperMetroidModel model = ReusableModel();

            // Expect
            WeaponMultiplier halfMultiplier = model.Enemies["Kihunter (red)"].WeaponMultipliers["Spazer"];
            Assert.Same(model.Weapons["Spazer"], halfMultiplier.Weapon);
            Assert.Equal(0.5M, halfMultiplier.Multiplier);
            Assert.Equal(20, halfMultiplier.DamagePerShot);

            WeaponMultiplier doubleMultiplier = model.Enemies["Kihunter (red)"].WeaponMultipliers["ScrewAttack"];
            Assert.Same(model.Weapons["ScrewAttack"], doubleMultiplier.Weapon);
            Assert.Equal(2, doubleMultiplier.Multiplier);
            Assert.Equal(4000, doubleMultiplier.DamagePerShot);

            WeaponMultiplier multiplierFromCategory = model.Enemies["Mella"].WeaponMultipliers["Ice"];
            Assert.Same(model.Weapons["Ice"], multiplierFromCategory.Weapon);
            Assert.Equal(2, multiplierFromCategory.Multiplier);
            Assert.Equal(60, multiplierFromCategory.DamagePerShot);
        }

        #endregion

        #region Tests for NumberOfHits()

        [Fact]
        public void NumberOfHits_ReturnsCorrectValue()
        {
            SuperMetroidModel model = ReusableModel();
            WeaponMultiplier multiplier = model.Enemies["Multiviola"].WeaponMultipliers["Ice"];
            Assert.Equal(1, multiplier.NumberOfHits(60));
            Assert.Equal(2, multiplier.NumberOfHits(61));
            Assert.Equal(2, multiplier.NumberOfHits(120));
        }

        #endregion

        #region Tests for ApplyLogicalOptions() that check applied logical properties

        [Fact]
        public void ApplyLogicalOptions_SetsLogicalProperties()
        {
            // Given
            SuperMetroidModel model = NewModelForOptions();
            LogicalOptions logicalOptions = new LogicalOptions();
            logicalOptions.RegisterRemovedItem("Ice");

            // When
            model.ApplyLogicalOptions(logicalOptions);

            // Expect
            Enemy multiplierEnemy = model.Enemies["Kihunter (red)"];
            Assert.False(multiplierEnemy.WeaponMultipliers["Ice"].LogicallyRelevant);
            Assert.True(multiplierEnemy.WeaponMultipliers["Spazer"].LogicallyRelevant);
        }

        #endregion
    }
}
