using sm_json_data_framework.Models;
using sm_json_data_framework.Options;
using sm_json_data_framework.Rules.InitialState;
using sm_json_data_framework.Tests.TestTools;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using sm_json_data_framework.Models.Helpers;
using sm_json_data_framework.Models.Requirements.ObjectRequirements.SubRequirements;
using sm_json_data_framework.InGameStates;

namespace sm_json_data_framework.Tests.Models.Helpers
{
    public class HelperTest
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
            Helper helper = model.Helpers["h_heatProof"];
            Assert.Equal("h_heatProof", helper.Name);
            Assert.NotNull(helper.Requires);
            Assert.Equal(1, helper.Requires.LogicalElements.Count());
            Assert.NotNull(helper.Requires.LogicalElement<Or>(0));
        }

        #endregion

        #region Tests for ApplyLogicalOptions() that check applied logical properties

        [Fact]
        public void ApplyLogicalOptions_SetsLogicalProperties()
        {
            // Given
            SuperMetroidModel model = NewModelForOptions();
            LogicalOptions logicalOptions = new LogicalOptions()
                .RegisterDisabledTech("canGateGlitch");
            logicalOptions.InternalStartConditions = StartConditions.CreateVanillaStartConditionsBuilder(model).StartingInventory(
                ItemInventory.CreateVanillaStartingInventory(model)
                    .ApplyAddItem(model.Items["Morph"])
                    .ApplyAddItem(model.Items["Bombs"])
                )
                .Build();

            // When
            model.ApplyLogicalOptions(logicalOptions);

            // Expect
            Helper impossibleHelper = model.Helpers["h_canBlueGateGlitch"];
            Assert.False(impossibleHelper.LogicallyRelevant);
            Assert.False(impossibleHelper.LogicallyAlways);
            Assert.False(impossibleHelper.LogicallyFree);
            Assert.True(impossibleHelper.LogicallyNever);

            Helper nonFreeHelper = model.Helpers["h_hasBeamUpgrade"];
            Assert.True(nonFreeHelper.LogicallyRelevant);
            Assert.False(nonFreeHelper.LogicallyAlways);
            Assert.False(nonFreeHelper.LogicallyFree);
            Assert.False(nonFreeHelper.LogicallyNever);

            Helper freeHelper = model.Helpers["h_canUseMorphBombs"];
            Assert.True(freeHelper.LogicallyRelevant);
            Assert.True(freeHelper.LogicallyAlways);
            Assert.True(freeHelper.LogicallyFree);
            Assert.False(freeHelper.LogicallyNever);
        }

        #endregion
    }
}
