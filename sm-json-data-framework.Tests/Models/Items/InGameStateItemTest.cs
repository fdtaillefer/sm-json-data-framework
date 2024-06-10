using sm_json_data_framework.Models.InGameStates;
using sm_json_data_framework.Models.Items;
using sm_json_data_framework.Models;
using sm_json_data_framework.Options;
using sm_json_data_framework.Rules.InitialState;
using sm_json_data_framework.Tests.TestTools;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace sm_json_data_framework.Tests.Models.Items
{
    public class InGameStateItemTest
    {
        private static SuperMetroidModel Model = StaticTestObjects.UnmodifiableModel;
        private static SuperMetroidModel ModelWithOptions = StaticTestObjects.UnfinalizedModel.Finalize();

        #region Tests for construction from unfinalized model

        [Fact]
        public void CtorFromUnfinalized_SetsPropertiesCorrectly()
        {
            // Given/when standard model creation

            // Expect
            InGameItem item = (InGameItem)Model.Items[SuperMetroidModel.SPEED_BOOSTER_NAME];
            Assert.Equal(SuperMetroidModel.SPEED_BOOSTER_NAME, item.Name);
            Assert.Equal("0xBA", item.Data);
        }

        #endregion

        #region Tests for ApplyLogicalOptions() that check applied logical properties

        [Fact]
        public void ApplyLogicalOptions_SetsLogicalProperties()
        {
            // Given
            LogicalOptions logicalOptions = new LogicalOptions();
            logicalOptions.RegisterRemovedItem("Bombs");
            logicalOptions.InternalStartConditions = StartConditions.CreateVanillaStartConditionsBuilder(ModelWithOptions).StartingInventory(
                ItemInventory.CreateVanillaStartingInventory(ModelWithOptions)
                    .ApplyAddItem(ModelWithOptions.Items["Morph"])
                )
                .Build();

            // When
            ModelWithOptions.ApplyLogicalOptions(logicalOptions);

            // Expect
            InGameItem freeItem = (InGameItem)ModelWithOptions.Items["Morph"];
            Assert.True(freeItem.LogicallyRelevant);
            Assert.False(freeItem.LogicallyNever);
            Assert.True(freeItem.LogicallyAlways);
            Assert.True(freeItem.LogicallyFree);

            InGameItem removedItem = (InGameItem)ModelWithOptions.Items["Bombs"];
            Assert.False(removedItem.LogicallyRelevant);
            Assert.True(removedItem.LogicallyNever);
            Assert.False(removedItem.LogicallyAlways);
            Assert.False(removedItem.LogicallyFree);

            InGameItem obtainableItem = (InGameItem)ModelWithOptions.Items["Charge"];
            Assert.True(obtainableItem.LogicallyRelevant);
            Assert.False(obtainableItem.LogicallyNever);
            Assert.False(obtainableItem.LogicallyAlways);
            Assert.False(obtainableItem.LogicallyFree);
        }

        #endregion
    }
}
