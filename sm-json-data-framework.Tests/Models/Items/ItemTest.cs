using sm_json_data_framework.Models;
using sm_json_data_framework.Options;
using sm_json_data_framework.Tests.TestTools;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using sm_json_data_framework.Models.Items;

namespace sm_json_data_framework.Tests.Models.Items
{
    public class ItemTest
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
            Item item = model.Items[SuperMetroidModel.POWER_SUIT_NAME];
            Assert.Equal(SuperMetroidModel.POWER_SUIT_NAME, item.Name);
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

            // When
            model.ApplyLogicalOptions(logicalOptions);
            
            // Expect
            Item freeItem = model.Items[SuperMetroidModel.POWER_SUIT_NAME];
            Assert.True(freeItem.LogicallyRelevant);
            Assert.False(freeItem.LogicallyNever);
            Assert.True(freeItem.LogicallyAlways);
            Assert.True(freeItem.LogicallyFree);

            Item removedItem = model.Items["Bombs"];
            Assert.False(removedItem.LogicallyRelevant);
            Assert.True(removedItem.LogicallyNever);
            Assert.False(removedItem.LogicallyAlways);
            Assert.False(removedItem.LogicallyFree);

            Item obtainableItem = model.Items["Charge"];
            Assert.True(obtainableItem.LogicallyRelevant);
            Assert.False(obtainableItem.LogicallyNever);
            Assert.False(obtainableItem.LogicallyAlways);
            Assert.False(obtainableItem.LogicallyFree);
        }

        #endregion
    }
}
