using sm_json_data_framework.Models.InGameStates;
using sm_json_data_framework.Models.Items;
using sm_json_data_framework.Models.Requirements;
using sm_json_data_framework.Models;
using sm_json_data_framework.Rules.InitialState;
using sm_json_data_framework.Tests.TestTools;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace sm_json_data_framework.Tests.Rules.InitialState
{
    public class UnfinalizedItemInventoryTest
    {
        /// <summary>
        /// Returns a series of items of different kinds, compatible for use with <see cref="MemberDataAttribute"/>.
        /// </summary>
        /// <returns></returns>
        public static IEnumerable<object[]> DifferentObjectTypeSamples()
        {
            return new List<object[]> { 
                // Non-consumable item
                new object[] { Model.Items[SuperMetroidModel.VARIA_SUIT_NAME] },
                // Expansion item
                new object[] { Model.Items[SuperMetroidModel.MISSILE_NAME] },
                // Not even an in-game item
                new object[] { Model.Items[SuperMetroidModel.POWER_BEAM_NAME] }
            };
        }

        // Use a static model to build it only once.
        private static UnfinalizedSuperMetroidModel Model { get; set; } = new UnfinalizedSuperMetroidModel(StaticTestObjects.RawModel);

        #region Tests for ApplyAddItem()
        [Fact]
        public void ApplyAddItem_NonConsumableItem_AddsItem()
        {
            // Given
            UnfinalizedItem varia = Model.Items[SuperMetroidModel.VARIA_SUIT_NAME];
            UnfinalizedItemInventory inventory = new UnfinalizedItemInventory();

            // When
            inventory.ApplyAddItem(varia);

            // Expect
            Assert.Contains(varia.Name, inventory.NonConsumableItems.Keys);
            Assert.Same(varia, inventory.NonConsumableItems[varia.Name]);
            Assert.Empty(inventory.ExpansionItems);
        }

        [Fact]
        public void ApplyAddItem_ExpansionItem_AddsItem()
        {
            // Given
            UnfinalizedItem missilePack = Model.Items[SuperMetroidModel.MISSILE_NAME];
            UnfinalizedItemInventory inventory = new UnfinalizedItemInventory();

            // When
            inventory.ApplyAddItem(missilePack);

            // Expect
            Assert.Contains(missilePack.Name, inventory.ExpansionItems.Keys);
            Assert.Same(missilePack, inventory.ExpansionItems[missilePack.Name].item);
            Assert.Equal(1, inventory.ExpansionItems[missilePack.Name].count);
            Assert.Empty(inventory.NonConsumableItems);
        }

        [Fact]
        public void ApplyAddItem_ExpansionItem_ItemAlreadyPresent_AddsToCount()
        {
            // Given
            UnfinalizedItem missilePack = Model.Items[SuperMetroidModel.MISSILE_NAME];
            UnfinalizedItemInventory inventory = new UnfinalizedItemInventory();
            inventory.ApplyAddItem(missilePack);

            // When
            inventory.ApplyAddItem(missilePack);

            // Expect
            Assert.Contains(missilePack.Name, inventory.ExpansionItems.Keys);
            Assert.Same(missilePack, inventory.ExpansionItems[missilePack.Name].item);
            Assert.Equal(2, inventory.ExpansionItems[missilePack.Name].count);
            Assert.Empty(inventory.NonConsumableItems);
        }
        #endregion

        #region Tests for ApplyRemoveItem()

        [Fact]
        public void ApplyRemoveItem_NonConsumableItem_RemovesItem()
        {
            // Given
            UnfinalizedItem varia = Model.Items[SuperMetroidModel.VARIA_SUIT_NAME];
            UnfinalizedItem speedBooster = Model.Items[SuperMetroidModel.SPEED_BOOSTER_NAME];
            UnfinalizedItemInventory inventory = new UnfinalizedItemInventory()
                .ApplyAddItem(varia)
                .ApplyAddItem(speedBooster);

            // When
            inventory.ApplyRemoveItem(varia);

            // Expect
            Assert.DoesNotContain(varia.Name, inventory.NonConsumableItems.Keys);
            Assert.Contains(speedBooster.Name, inventory.NonConsumableItems.Keys);
        }

        [Fact]
        public void ApplyRemoveItem_NonConsumableItem_ItemNoPresent_DoesNothing()
        {
            // Given
            UnfinalizedItem varia = Model.Items[SuperMetroidModel.VARIA_SUIT_NAME];
            UnfinalizedItemInventory inventory = new UnfinalizedItemInventory();

            // When
            inventory.ApplyRemoveItem(varia);

            // Expect
            Assert.DoesNotContain(varia.Name, inventory.NonConsumableItems.Keys);
        }

        [Fact]
        public void ApplyRemoveItem_ExpansionItem_SubtractsItemCount()
        {
            // Given
            UnfinalizedItem missilePack = Model.Items[SuperMetroidModel.MISSILE_NAME];
            UnfinalizedItemInventory inventory = new UnfinalizedItemInventory()
                .ApplyAddItem(missilePack)
                .ApplyAddItem(missilePack)
                .ApplyAddItem(missilePack);

            // When
            inventory.ApplyRemoveItem(missilePack);

            // Expect
            Assert.Equal(2, inventory.ExpansionItems[missilePack.Name].count);
        }

        [Fact]
        public void ApplyRemoveItem_ExpansionItem_RemovingLast_RemovesItem()
        {
            // Given
            UnfinalizedItem missilePack = Model.Items[SuperMetroidModel.MISSILE_NAME];
            UnfinalizedItemInventory inventory = new UnfinalizedItemInventory()
                .ApplyAddItem(missilePack);

            // When
            inventory.ApplyRemoveItem(missilePack);

            // Expect
            Assert.DoesNotContain(missilePack.Name, inventory.ExpansionItems.Keys);
        }

        [Fact]
        public void ApplyRemoveItem_ExpansionItem_ItemNotPresent_DoesNothing()
        {
            // Given
            UnfinalizedItem missilePack = Model.Items[SuperMetroidModel.MISSILE_NAME];
            UnfinalizedItemInventory inventory = new UnfinalizedItemInventory();

            // When
            inventory.ApplyRemoveItem(missilePack);

            // Expect
            Assert.DoesNotContain(missilePack.Name, inventory.ExpansionItems.Keys);
        }

        #endregion

        #region Tests for cLone()
        [Fact]
        public void Clone_CopriesCorrectly()
        {
            // Given
            UnfinalizedItem varia = Model.Items[SuperMetroidModel.VARIA_SUIT_NAME];
            UnfinalizedItem speedBooster = Model.Items[SuperMetroidModel.SPEED_BOOSTER_NAME];
            UnfinalizedItem missilePack = Model.Items[SuperMetroidModel.MISSILE_NAME];

            UnfinalizedItemInventory inventory = new UnfinalizedItemInventory();
            inventory.ApplyAddItem(varia)
                .ApplyAddItem(speedBooster)
                .ApplyAddItem(missilePack).ApplyAddItem(missilePack);

            // When
            UnfinalizedItemInventory clone = inventory.Clone();

            // Expect
            Assert.Equal(2, clone.NonConsumableItems.Count);
            Assert.Contains(varia.Name, clone.NonConsumableItems.Keys);
            Assert.Contains(speedBooster.Name, clone.NonConsumableItems.Keys);
            Assert.Single(clone.ExpansionItems);
            Assert.Contains(missilePack.Name, clone.ExpansionItems.Keys);
            Assert.Equal(2, clone.ExpansionItems[missilePack.Name].count);
        }

        [Fact]
        public void Clone_SeparatesState()
        {
            // Given
            UnfinalizedItem varia = Model.Items[SuperMetroidModel.VARIA_SUIT_NAME];
            UnfinalizedItem speedBooster = Model.Items[SuperMetroidModel.SPEED_BOOSTER_NAME];
            UnfinalizedItem missilePack = Model.Items[SuperMetroidModel.MISSILE_NAME];
            UnfinalizedItemInventory inventory = new UnfinalizedItemInventory();

            // When
            UnfinalizedItemInventory clone = inventory.Clone();

            // Subsequently given
            inventory.ApplyAddItem(varia)
                .ApplyAddItem(speedBooster)
                .ApplyAddItem(missilePack)
                .ApplyAddItem(missilePack);

            // Expect
            Assert.Empty(clone.NonConsumableItems);
            Assert.Empty(clone.ExpansionItems);
        }
        #endregion

    }
}
