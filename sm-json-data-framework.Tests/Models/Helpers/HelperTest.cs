using sm_json_data_framework.Models.GameFlags;
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
using sm_json_data_framework.Models.InGameStates;
using sm_json_data_framework.Models.Requirements.ObjectRequirements.SubRequirements;
using sm_json_data_framework.Models.Requirements.StringRequirements;

namespace sm_json_data_framework.Tests.Models.Helpers
{
    public class HelperTest
    {
        private static SuperMetroidModel Model = StaticTestObjects.UnmodifiableModel;
        private static SuperMetroidModel ModelWithOptions = StaticTestObjects.UnfinalizedModel.Finalize();

        #region Tests for construction from unfinalized model

        [Fact]
        public void CtorFromUnfinalized_SetsPropertiesCorrectly()
        {
            // Given/when standard model creation

            // Expect
            Helper helper = Model.Helpers["h_heatProof"];
            Assert.Equal("h_heatProof", helper.Name);
            Assert.NotNull(helper.Requires);
            Assert.Equal(1, helper.Requires.LogicalElements.Count());
            Assert.NotNull(helper.Requires.LogicalElement<Or>(0));
        }

        #endregion

        #region Tests for ApplyLogicalOptions() that check applied logical properties

        [Fact]
        public void ApplyLogicalOptions_SetsLogicalPropertiesOnHelpers()
        {
            // Given
            LogicalOptions logicalOptions = new LogicalOptions()
                .RegisterDisabledTech("canGateGlitch");
            logicalOptions.InternalStartConditions = StartConditions.CreateVanillaStartConditionsBuilder(ModelWithOptions).StartingInventory(
                ItemInventory.CreateVanillaStartingInventory(ModelWithOptions)
                    .ApplyAddItem(ModelWithOptions.Items["Morph"])
                    .ApplyAddItem(ModelWithOptions.Items["Bombs"])
                )
                .Build();

            // When
            ModelWithOptions.ApplyLogicalOptions(logicalOptions);

            // Expect
            Helper impossibleHelper = ModelWithOptions.Helpers["h_canBlueGateGlitch"];
            Assert.False(impossibleHelper.LogicallyRelevant);
            Assert.False(impossibleHelper.LogicallyAlways);
            Assert.False(impossibleHelper.LogicallyFree);
            Assert.True(impossibleHelper.LogicallyNever);

            Helper nonFreeHelper = ModelWithOptions.Helpers["h_hasBeamUpgrade"];
            Assert.True(nonFreeHelper.LogicallyRelevant);
            Assert.False(nonFreeHelper.LogicallyAlways);
            Assert.False(nonFreeHelper.LogicallyFree);
            Assert.False(nonFreeHelper.LogicallyNever);

            Helper freeHelper = ModelWithOptions.Helpers["h_canUseMorphBombs"];
            Assert.True(freeHelper.LogicallyRelevant);
            Assert.True(freeHelper.LogicallyAlways);
            Assert.True(freeHelper.LogicallyFree);
            Assert.False(freeHelper.LogicallyNever);
        }

        #endregion
    }
}
