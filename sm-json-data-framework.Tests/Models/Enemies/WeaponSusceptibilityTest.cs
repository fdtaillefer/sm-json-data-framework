﻿using sm_json_data_framework.Models.Enemies;
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
    public class WeaponSusceptibilityTest
    {
        private static SuperMetroidModel Model = StaticTestObjects.UnmodifiableModel;
        private static SuperMetroidModel ModelWithOptions = StaticTestObjects.UnfinalizedModel.Finalize();

        #region Tests for construction from unfinalized model

        [Fact]
        public void CtorFromUnfinalized_SetsPropertiesCorrectly()
        {
            // Given/when standard model creation

            // Expect
            WeaponSusceptibility halfSusceptibility = Model.Enemies["Kihunter (red)"].WeaponSusceptibilities["Spazer"];
            Assert.Same(Model.Weapons["Spazer"], halfSusceptibility.Weapon);
            Assert.Equal(0.5M, halfSusceptibility.WeaponMultiplier.Multiplier);
            Assert.Equal(20, halfSusceptibility.DamagePerShot);
            Assert.Equal(90, halfSusceptibility.Shots);

            WeaponSusceptibility doubleSusceptibility = Model.Enemies["Kihunter (red)"].WeaponSusceptibilities["ScrewAttack"];
            Assert.Same(Model.Weapons["ScrewAttack"], doubleSusceptibility.Weapon);
            Assert.Equal(2, doubleSusceptibility.WeaponMultiplier.Multiplier);
            Assert.Equal(4000, doubleSusceptibility.DamagePerShot);
            Assert.Equal(1, doubleSusceptibility.Shots);

            WeaponSusceptibility baseSusceptibility = Model.Enemies["Kihunter (red)"].WeaponSusceptibilities["Plasma"];
            Assert.Same(Model.Weapons["Plasma"], baseSusceptibility.Weapon);
            Assert.Equal(1, baseSusceptibility.WeaponMultiplier.Multiplier);
            Assert.Equal(150, baseSusceptibility.DamagePerShot);
            Assert.Equal(12, baseSusceptibility.Shots);

            WeaponSusceptibility susceptibilityFromCategory = Model.Enemies["Mella"].WeaponSusceptibilities["Ice"];
            Assert.Same(Model.Weapons["Ice"], susceptibilityFromCategory.Weapon);
            Assert.Equal(2, susceptibilityFromCategory.WeaponMultiplier.Multiplier);
            Assert.Equal(60, susceptibilityFromCategory.DamagePerShot);
            Assert.Equal(1, susceptibilityFromCategory.Shots);
        }

        #endregion

        #region Tests for NumberOfHits()

        [Fact]
        public void NumberOfHits_ReturnsCorrectValue()
        {
            WeaponSusceptibility susceptibility = Model.Enemies["Multiviola"].WeaponSusceptibilities["Ice"];
            Assert.Equal(1, susceptibility.NumberOfHits(60));
            Assert.Equal(2, susceptibility.NumberOfHits(61));
            Assert.Equal(2, susceptibility.NumberOfHits(120));
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
            Enemy susceptibilityEnemy = ModelWithOptions.Enemies["Kihunter (red)"];
            Assert.False(susceptibilityEnemy.WeaponSusceptibilities["Ice"].LogicallyRelevant);
            Assert.True(susceptibilityEnemy.WeaponSusceptibilities["Spazer"].LogicallyRelevant);
        }

        #endregion
    }
}
