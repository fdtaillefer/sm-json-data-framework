﻿using sm_json_data_framework.Models.Enemies;
using sm_json_data_framework.Models.Weapons;
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
    public class EnemyDimensionsTest
    {
        private static SuperMetroidModel Model = StaticTestObjects.UnmodifiableModel;
        private static SuperMetroidModel ModelWithOptions = StaticTestObjects.UnfinalizedModel.Finalize();

        #region Tests for construction from unfinalized model

        [Fact]
        public void CtorFromUnfinalized_SetsPropertiesCorrectly()
        {
            // Given/when standard model creation

            // Expect
            EnemyDimensions enemyDimensions = Model.Enemies["Evir"].Dimensions;
            Assert.Equal(16, enemyDimensions.Width);
            Assert.Equal(20, enemyDimensions.Height);
        }

        #endregion

        #region Tests for ApplyLogicalOptions() that check applied logical properties

        [Fact]
        public void ApplyLogicalOptions_SetsLogicalPropertiesOnEnemyDimensions()
        {
            // Given
            LogicalOptions logicalOptions = new LogicalOptions();

            // When
            ModelWithOptions.ApplyLogicalOptions(logicalOptions);

            // Expect
            EnemyDimensions enemyDimensions = ModelWithOptions.Enemies["Evir"].Dimensions;
            Assert.False(enemyDimensions.LogicallyRelevant);
        }

        #endregion
    }
}