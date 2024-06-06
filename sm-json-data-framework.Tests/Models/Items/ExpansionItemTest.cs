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
    public class ExpansionItemTest
    {
        private static SuperMetroidModel Model = StaticTestObjects.UnmodifiableModel;
        private static SuperMetroidModel ModelWithOptions = StaticTestObjects.UnfinalizedModel.Finalize();

        #region Tests for construction from unfinalized model

        [Fact]
        public void CtorFromUnfinalized_SetsPropertiesCorrectly()
        {
            // Given/when standard model creation

            // Expect
            ExpansionItem item = (ExpansionItem)Model.Items[SuperMetroidModel.MISSILE_NAME];
            Assert.Equal(SuperMetroidModel.MISSILE_NAME, item.Name);
            Assert.Equal("0xC2", item.Data);
            Assert.Equal(RechargeableResourceEnum.Missile, item.Resource);
            Assert.Equal(5, item.ResourceAmount);
        }

        #endregion

        #region Tests for ApplyLogicalOptions() that check applied logical properties

        [Fact]
        public void ApplyLogicalOptions_SetsLogicalPropertiesOnExpansionItems()
        {
            // Given
            LogicalOptions logicalOptions = new LogicalOptions();
            logicalOptions.RegisterRemovedItem(SuperMetroidModel.SUPER_NAME);
            logicalOptions.InternalStartConditions = StartConditions.CreateVanillaStartConditionsBuilder(ModelWithOptions).StartingInventory(
                ItemInventory.CreateVanillaStartingInventory(ModelWithOptions)
                    .ApplyAddItem(ModelWithOptions.Items[SuperMetroidModel.MISSILE_NAME])
                )
                .Build();

            // When
            ModelWithOptions.ApplyLogicalOptions(logicalOptions);

            // Expect
            ExpansionItem freeItem = (ExpansionItem)ModelWithOptions.Items[SuperMetroidModel.MISSILE_NAME];
            Assert.True(freeItem.LogicallyRelevant);
            Assert.False(freeItem.LogicallyNever);
            Assert.True(freeItem.LogicallyAlways);
            Assert.True(freeItem.LogicallyFree);

            ExpansionItem removedItem = (ExpansionItem)ModelWithOptions.Items[SuperMetroidModel.SUPER_NAME];
            Assert.False(removedItem.LogicallyRelevant);
            Assert.True(removedItem.LogicallyNever);
            Assert.False(removedItem.LogicallyAlways);
            Assert.False(removedItem.LogicallyFree);

            ExpansionItem obtainableItem = (ExpansionItem)ModelWithOptions.Items[SuperMetroidModel.ENERGY_TANK_NAME];
            Assert.True(obtainableItem.LogicallyRelevant);
            Assert.False(obtainableItem.LogicallyNever);
            Assert.False(obtainableItem.LogicallyAlways);
            Assert.False(obtainableItem.LogicallyFree);
        }

        #endregion
    }
}
