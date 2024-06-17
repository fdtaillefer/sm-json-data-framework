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
        private static SuperMetroidModel ReusableModel() => StaticTestObjects.UnmodifiableModel;
        private static SuperMetroidModel NewModelForOptions() => StaticTestObjects.UnfinalizedModel.Finalize();

        #region Tests for construction from unfinalized model

        [Fact]
        public void CtorFromUnfinalized_SetsPropertiesCorrectly()
        {
            // Given/when standard model creation
            SuperMetroidModel model = ReusableModel();

            // Expect
            InGameItem item = (InGameItem)model.Items[SuperMetroidModel.SPEED_BOOSTER_NAME];
            Assert.Equal(SuperMetroidModel.SPEED_BOOSTER_NAME, item.Name);
            Assert.Equal("0xBA", item.Data);
        }

        #endregion

        #region Tests for ApplyLogicalOptions() that check applied logical properties

        [Fact]
        public void ApplyLogicalOptions_SetsLogicalProperties()
        {
            // Given
            SuperMetroidModel model = NewModelForOptions();
            LogicalOptions logicalOptions = new LogicalOptions();
            logicalOptions.RegisterRemovedItem("Bombs");
            logicalOptions.InternalStartConditions = StartConditions.CreateVanillaStartConditionsBuilder(model).StartingInventory(
                ItemInventory.CreateVanillaStartingInventory(model)
                    .ApplyAddItem(model.Items["Morph"])
                )
                .Build();

            // When
            model.ApplyLogicalOptions(logicalOptions);

            // Expect
            InGameItem freeItem = (InGameItem)model.Items["Morph"];
            Assert.True(freeItem.LogicallyRelevant);
            Assert.False(freeItem.LogicallyNever);
            Assert.True(freeItem.LogicallyAlways);
            Assert.True(freeItem.LogicallyFree);

            InGameItem removedItem = (InGameItem)model.Items["Bombs"];
            Assert.False(removedItem.LogicallyRelevant);
            Assert.True(removedItem.LogicallyNever);
            Assert.False(removedItem.LogicallyAlways);
            Assert.False(removedItem.LogicallyFree);

            InGameItem obtainableItem = (InGameItem)model.Items["Charge"];
            Assert.True(obtainableItem.LogicallyRelevant);
            Assert.False(obtainableItem.LogicallyNever);
            Assert.False(obtainableItem.LogicallyAlways);
            Assert.False(obtainableItem.LogicallyFree);
        }

        #endregion
    }
}
