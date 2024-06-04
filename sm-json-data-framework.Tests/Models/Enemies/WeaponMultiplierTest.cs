using sm_json_data_framework.Models;
using sm_json_data_framework.Models.Enemies;
using sm_json_data_framework.Models.Weapons;
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
        private static SuperMetroidModel Model = StaticTestObjects.UnmodifiableModel;
        private static SuperMetroidModel ModelWithOptions = StaticTestObjects.UnfinalizedModel.Finalize();

        #region Tests for construction from unfinalized model

        [Fact]
        public void CtorFromUnfinalized_SetsPropertiesCorrectly()
        {
            // Given/when standard model creation

            // Expect
            WeaponMultiplier halfMultiplier = Model.Enemies["Kihunter (red)"].WeaponMultipliers["Spazer"];
            Assert.Same(Model.Weapons["Spazer"], halfMultiplier.Weapon);
            Assert.Equal(0.5M, halfMultiplier.Multiplier);
            Assert.Equal(20, halfMultiplier.DamagePerShot);

            WeaponMultiplier doubleMultiplier = Model.Enemies["Kihunter (red)"].WeaponMultipliers["ScrewAttack"];
            Assert.Same(Model.Weapons["ScrewAttack"], doubleMultiplier.Weapon);
            Assert.Equal(2, doubleMultiplier.Multiplier);
            Assert.Equal(4000, doubleMultiplier.DamagePerShot);

            WeaponMultiplier multiplierFromCategory = Model.Enemies["Mella"].WeaponMultipliers["Ice"];
            Assert.Same(Model.Weapons["Ice"], multiplierFromCategory.Weapon);
            Assert.Equal(2, multiplierFromCategory.Multiplier);
            Assert.Equal(60, multiplierFromCategory.DamagePerShot);
        }

        #endregion

        #region Tests for NumberOfHits()

        [Fact]
        public void NumberOfHits_ReturnsCorrectValue()
        {
            WeaponMultiplier multiplier = Model.Enemies["Multiviola"].WeaponMultipliers["Ice"];
            Assert.Equal(1, multiplier.NumberOfHits(60));
            Assert.Equal(2, multiplier.NumberOfHits(61));
            Assert.Equal(2, multiplier.NumberOfHits(120));
        }

        #endregion

        #region Tests for ApplyLogicalOptions() that check applied logical properties

        [Fact]
        public void ApplyLogicalOptions_SetsLogicalPropertiesOnWeaponMultipliers()
        {
            // Given
            LogicalOptions logicalOptions = new LogicalOptions();
            logicalOptions.RegisterRemovedItem("Ice");

            // When
            ModelWithOptions.ApplyLogicalOptions(logicalOptions);

            // Expect
            Enemy multiplierEnemy = ModelWithOptions.Enemies["Kihunter (red)"];
            Assert.False(multiplierEnemy.WeaponMultipliers["Ice"].LogicallyRelevant);
            Assert.True(multiplierEnemy.WeaponMultipliers["Spazer"].LogicallyRelevant);
        }

        #endregion
    }
}
