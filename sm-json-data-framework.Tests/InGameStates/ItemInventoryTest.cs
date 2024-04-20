using sm_json_data_framework.Models;
using sm_json_data_framework.Models.InGameStates;
using sm_json_data_framework.Models.Items;
using sm_json_data_framework.Models.Requirements;
using sm_json_data_framework.Reading;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace sm_json_data_framework.Tests.InGameStates
{
    public class ItemInventoryTest
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

        /// <summary>
        /// Returns all values of <see cref="RechargeableResourceEnum"/> in a format that can be used by <see cref="MemberDataAttribute"/>.
        /// </summary>
        /// <returns></returns>
        public static IEnumerable<object[]> RechargeableResourceValues()
        {

            foreach (RechargeableResourceEnum resource in Enum.GetValues(typeof(RechargeableResourceEnum)))
            {
                yield return new object[] { resource };
            }
        }

        /// <summary>
        /// Returns all values of <see cref="RechargeableResourceEnum"/> combined with the <see cref="ExpansionItem"/> that adds to the max amount of that resource,
        /// in a format that can be used by <see cref="MemberDataAttribute"/>.
        /// </summary>
        /// <returns></returns>
        public static IEnumerable<object[]> RechargeableResourceValuesWithExpansionItem()
        {

            foreach (RechargeableResourceEnum resource in Enum.GetValues(typeof(RechargeableResourceEnum)))
            {
                yield return new object[] { resource,
                    resource switch
                    {
                        RechargeableResourceEnum.RegularEnergy => Model.Items[SuperMetroidModel.ENERGY_TANK_NAME],
                        RechargeableResourceEnum.ReserveEnergy => Model.Items[SuperMetroidModel.RESERVE_TANK_NAME],
                        RechargeableResourceEnum.Missile=> Model.Items[SuperMetroidModel.MISSILE_NAME],
                        RechargeableResourceEnum.Super=> Model.Items[SuperMetroidModel.SUPER_NAME],
                        RechargeableResourceEnum.PowerBomb=> Model.Items[SuperMetroidModel.POWER_BOMB_NAME],
                        _ => throw new NotImplementedException() }
                };
            }
        }

        // Use a static model to read it only once.
        private static SuperMetroidModel Model { get; set; } = ModelReader.ReadModel();

        #region Tests for ApplyAddItem()
        [Fact]
        public void ApplyAddItem_NonConsumableItem_AddsItem()
        {
            // Given
            Item varia = Model.Items[SuperMetroidModel.VARIA_SUIT_NAME];
            ItemInventory inventory = new ItemInventory(ResourceCount.CreateVanillaBaseResourceMaximums());

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
            Item missilePack = Model.Items[SuperMetroidModel.MISSILE_NAME];
            ItemInventory inventory = new ItemInventory(ResourceCount.CreateVanillaBaseResourceMaximums());

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
            Item missilePack = Model.Items[SuperMetroidModel.MISSILE_NAME];
            ItemInventory inventory = new ItemInventory(ResourceCount.CreateVanillaBaseResourceMaximums());
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

        #region Tests for ApplyDisableItem()
        [Fact]
        public void ApplyDisableItem_NonConsumableItem_DisablesItem()
        {
            // Given
            Item varia = Model.Items[SuperMetroidModel.VARIA_SUIT_NAME];
            ItemInventory inventory = new ItemInventory(ResourceCount.CreateVanillaBaseResourceMaximums());
            inventory.ApplyAddItem(varia);

            // When
            inventory.ApplyDisableItem(varia);

            // Expect
            Assert.Contains(varia.Name, inventory.DisabledItemNames);
        }

        [Fact]
        public void ApplyDisableItem_NonConsumableItem_ItemNotInInventory_DoesNothing()
        {
            // Given
            Item varia = Model.Items[SuperMetroidModel.VARIA_SUIT_NAME];
            ItemInventory inventory = new ItemInventory(ResourceCount.CreateVanillaBaseResourceMaximums());

            // When
            inventory.ApplyDisableItem(varia);
            
            // Expect
            Assert.Empty(inventory.DisabledItemNames);
        }

        [Fact]
        public void ApplyDisableItem_ExpansionItem_DoesNothing()
        {
            // Given
            Item missilePack = Model.Items[SuperMetroidModel.MISSILE_NAME];
            ItemInventory inventory = new ItemInventory(ResourceCount.CreateVanillaBaseResourceMaximums());
            inventory.ApplyAddItem(missilePack);

            // When
            inventory.ApplyDisableItem(missilePack);

            // Expect
            Assert.Empty(inventory.DisabledItemNames);
        }
        #endregion

        #region Tests for ApplyEnableItem()
        [Fact]
        public void ApplyEnableItem_NonConsumableItem_EnablesItem()
        {
            // Given
            Item varia = Model.Items[SuperMetroidModel.VARIA_SUIT_NAME];
            ItemInventory inventory = new ItemInventory(ResourceCount.CreateVanillaBaseResourceMaximums());
            inventory.ApplyAddItem(varia);
            inventory.ApplyDisableItem(varia);

            // When
            inventory.ApplyEnableItem(varia);

            // Expect
            Assert.Empty(inventory.DisabledItemNames);
        }

        [Fact]
        public void ApplyEnableItem_NonConsumableItem_ItemNotDisabled_DoesNothing()
        {
            // Given
            Item varia = Model.Items[SuperMetroidModel.VARIA_SUIT_NAME];
            ItemInventory inventory = new ItemInventory(ResourceCount.CreateVanillaBaseResourceMaximums());
            inventory.ApplyAddItem(varia);

            // When
            inventory.ApplyEnableItem(varia);

            // Expect
            Assert.Empty(inventory.DisabledItemNames);
        }

        [Fact]
        public void ApplyEnableItem_NonConsumableItem_ItemNotInInventory_DoesNothing()
        {
            // Given
            Item varia = Model.Items[SuperMetroidModel.VARIA_SUIT_NAME];
            ItemInventory inventory = new ItemInventory(ResourceCount.CreateVanillaBaseResourceMaximums());

            // When
            inventory.ApplyEnableItem(varia);

            // Expect
            Assert.Empty(inventory.DisabledItemNames);
        }
        #endregion

        #region Tests for ExceptIn()
        [Fact]
        public void ExceptIn_BuildsProperDifference()
        {
            // Given
            Item varia = Model.Items[SuperMetroidModel.VARIA_SUIT_NAME];
            Item gravity = Model.Items[SuperMetroidModel.GRAVITY_SUIT_NAME];
            Item speedBooster = Model.Items[SuperMetroidModel.SPEED_BOOSTER_NAME];
            Item missilePack = Model.Items[SuperMetroidModel.MISSILE_NAME];
            Item superPack = Model.Items[SuperMetroidModel.SUPER_NAME];
            Item powerBombPack = Model.Items[SuperMetroidModel.POWER_BOMB_NAME];
            ItemInventory inventory = new ItemInventory(ResourceCount.CreateVanillaBaseResourceMaximums())
                .ApplyAddItem(varia)
                .ApplyAddItem(gravity)
                .ApplyAddItem(missilePack)
                .ApplyAddItem(missilePack)
                .ApplyAddItem(missilePack)
                .ApplyAddItem(superPack);

            ItemInventory otherInventory = new ItemInventory(ResourceCount.CreateVanillaBaseResourceMaximums())
                .ApplyAddItem(speedBooster)
                .ApplyAddItem(gravity)
                .ApplyAddItem(missilePack)
                .ApplyAddItem(superPack)
                .ApplyAddItem(superPack)
                .ApplyAddItem(superPack)
                .ApplyAddItem(powerBombPack);

            // When
            ItemInventory result = inventory.ExceptIn(otherInventory);

            // Expect
            Assert.Contains(varia.Name, result.NonConsumableItems.Keys);
            Assert.DoesNotContain(gravity.Name, result.NonConsumableItems.Keys);
            Assert.DoesNotContain(speedBooster.Name, result.NonConsumableItems.Keys);
            Assert.Contains(missilePack.Name, result.ExpansionItems.Keys);
            Assert.Equal(2, result.ExpansionItems[missilePack.Name].count);
            Assert.DoesNotContain(superPack.Name, result.ExpansionItems.Keys);
            Assert.DoesNotContain(powerBombPack.Name, result.ExpansionItems.Keys);
        }

        [Fact]
        public void ExceptIn_ReturnsSeparateState()
        {
            // Given
            Item varia = Model.Items[SuperMetroidModel.VARIA_SUIT_NAME];
            Item missilePack = Model.Items[SuperMetroidModel.MISSILE_NAME];
            ItemInventory inventory = new ItemInventory(ResourceCount.CreateVanillaBaseResourceMaximums());
            ItemInventory otherInventory = new ItemInventory(ResourceCount.CreateVanillaBaseResourceMaximums());

            // When
            ItemInventory result = inventory.ExceptIn(otherInventory);

            // Subsequently given
            inventory.ApplyAddItem(varia).ApplyAddItem(missilePack).ApplyDisableItem(varia);
            otherInventory.ApplyAddItem(varia).ApplyAddItem(missilePack).ApplyDisableItem(varia);

            // Expect
            Assert.Empty(result.NonConsumableItems);
            Assert.Empty(result.ExpansionItems);
            Assert.Empty(result.DisabledItemNames);
        }
        #endregion

        #region Tests for HasItem()
        [Theory]
        [MemberData(nameof(DifferentObjectTypeSamples))]
        public void HasItem_ItemIsPresent_ReturnsTrue(Item item)
        {
            // Given
            ItemInventory inventory = new ItemInventory(ResourceCount.CreateVanillaBaseResourceMaximums())
                .ApplyAddItem(item);

            // When
            bool result = inventory.HasItem(item);

            // Expect
            Assert.True(result);
        }

        [Theory]
        [MemberData(nameof(DifferentObjectTypeSamples))]
        public void HasItem_ItemNotPresent_ReturnsFalse(Item item)
        {
            // Given
            ItemInventory inventory = new ItemInventory(ResourceCount.CreateVanillaBaseResourceMaximums());

            // When
            bool result = inventory.HasItem(item);

            // Expect
            Assert.False(result);
        }

        [Fact]
        public void HasItem_ItemPresentButDisabled_ReturnsFalse()
        {
            // Given
            Item varia = Model.Items[SuperMetroidModel.VARIA_SUIT_NAME];
            ItemInventory inventory = new ItemInventory(ResourceCount.CreateVanillaBaseResourceMaximums())
                .ApplyAddItem(varia)
                .ApplyDisableItem(varia);

            // When
            bool result = inventory.HasItem(varia);

            // Expect
            Assert.False(result);
        }
        #endregion

        #region Tests for ResourceMaximums property
        [Theory]
        [MemberData(nameof(RechargeableResourceValues))]
        public void ResourceMaximums_RechargeableResource_NoExpansions_ReturnsBaseValue(RechargeableResourceEnum resource)
        {
            // Given
            int amount = 5;
            ItemInventory inventory = new ItemInventory(new ResourceCount().ApplyAmount(resource, amount));

            // When
            int result = inventory.ResourceMaximums.GetAmount(resource);

            // Expect
            Assert.Equal(amount, result);
        }

        [Theory]
        [InlineData(RechargeableResourceEnum.Missile)]
        [InlineData(RechargeableResourceEnum.Super)]
        [InlineData(RechargeableResourceEnum.PowerBomb)]
        public void ResourceMaximums_ConsumableAmmo_NoExpansions_ReturnsBaseValue(RechargeableResourceEnum resource)
        {
            // Given
            int amount = 5;
            ItemInventory inventory = new ItemInventory(new ResourceCount().ApplyAmount(resource, amount));

            // When
            int result = inventory.ResourceMaximums.GetAmount(resource.ToConsumableResource());

            // Expect
            Assert.Equal(amount, result);
        }

        [Fact]
        public void ResourceMaximums_ConsumableEnergy_NoExpansions_ReturnsBaseValuesSum()
        {
            // Given
            ItemInventory inventory = new ItemInventory(
                new ResourceCount().ApplyAmount(RechargeableResourceEnum.RegularEnergy, 2).ApplyAmount(RechargeableResourceEnum.ReserveEnergy, 4)
            );

            // When
            int result = inventory.ResourceMaximums.GetAmount(ConsumableResourceEnum.Energy);

            // Expect
            Assert.Equal(6, result);
        }

        [Theory]
        [MemberData(nameof(RechargeableResourceValuesWithExpansionItem))]
        public void ResourceMaximums_RechargeableResource_WithExpansions_ReturnsSumOfBaseAndExpansion(RechargeableResourceEnum resource, ExpansionItem item)
        {
            // Given
            int baseAmount = 5;
            ItemInventory inventory = new ItemInventory(new ResourceCount().ApplyAmount(resource, baseAmount))
                .ApplyAddItem(item).ApplyAddItem(item);

            // When
            int result = inventory.ResourceMaximums.GetAmount(resource);

            // Expect
            int expected = baseAmount + 2 * item.ResourceAmount;
            Assert.Equal(expected, result);

        }

        [Theory]
        [InlineData(RechargeableResourceEnum.Missile, SuperMetroidModel.MISSILE_NAME)]
        [InlineData(RechargeableResourceEnum.Super, SuperMetroidModel.SUPER_NAME)]
        [InlineData(RechargeableResourceEnum.PowerBomb, SuperMetroidModel.POWER_BOMB_NAME)]
        public void ResourceMaximums_ConsumableAmmo_WithExpansions_ReturnsSumOfBaseAndExpansion(RechargeableResourceEnum resource, string itemName)
        {
            // Given
            int baseAmount = 5;
            ExpansionItem item = (ExpansionItem) Model.Items[itemName];
            ItemInventory inventory = new ItemInventory(new ResourceCount().ApplyAmount(resource, baseAmount))
                .ApplyAddItem(item).ApplyAddItem(item);

            // When
            int result = inventory.ResourceMaximums.GetAmount(resource.ToConsumableResource());

            // Expect
            int expected = baseAmount + 2 * item.ResourceAmount;
            Assert.Equal(expected, result);
        }

        [Fact]
        public void ResourceMaximums_ConsumableEnergy_WithExpansions_ReturnsSumOfBaseAndExpansionForBothEnergyTypes()
        {
            // Given
            ExpansionItem etank = (ExpansionItem)Model.Items[SuperMetroidModel.ENERGY_TANK_NAME];
            ExpansionItem reserve = (ExpansionItem)Model.Items[SuperMetroidModel.RESERVE_TANK_NAME];
            ItemInventory inventory = new ItemInventory(
                new ResourceCount().ApplyAmount(RechargeableResourceEnum.RegularEnergy, 2).ApplyAmount(RechargeableResourceEnum.ReserveEnergy, 4))
                .ApplyAddItem(etank)
                .ApplyAddItem(etank)
                .ApplyAddItem(reserve);

            // When
            int result = inventory.ResourceMaximums.GetAmount(ConsumableResourceEnum.Energy);

            // Expect
            int expected = 306; // 2 + 4 + 100 * 3
            Assert.Equal(expected, result);
        }
        #endregion

        #region Tests for ContainsAnyInGameItem()
        [Fact]
        public void ContainsAnyInGameItem_HasNonConsumableItem_ReturnsTrue()
        {
            // Given
            Item varia = Model.Items[SuperMetroidModel.VARIA_SUIT_NAME];
            ItemInventory inventory = new ItemInventory(ResourceCount.CreateVanillaBaseResourceMaximums()).ApplyAddItem(varia);

            // When
            bool result = inventory.ContainsAnyInGameItem();

            // Expect
            Assert.True(result);
        }

        [Fact]
        public void ContainsAnyInGameItem_HasExpansionItem_ReturnsTrue()
        {
            // Given
            Item missilePack = Model.Items[SuperMetroidModel.MISSILE_NAME];
            ItemInventory inventory = new ItemInventory(ResourceCount.CreateVanillaBaseResourceMaximums()).ApplyAddItem(missilePack);

            // When
            bool result = inventory.ContainsAnyInGameItem();

            // Expect
            Assert.True(result);
        }

        [Fact]
        public void ContainsAnyInGameItem_HasOnlyNonGameItem_ReturnsFalse()
        {
            // Given
            Item powerBeam = Model.Items[SuperMetroidModel.POWER_BEAM_NAME];
            ItemInventory inventory = new ItemInventory(ResourceCount.CreateVanillaBaseResourceMaximums()).ApplyAddItem(powerBeam);

            // When
            bool result = inventory.ContainsAnyInGameItem();

            // Expect
            Assert.False(result);
        }
        #endregion

        #region Tests for IsItemDisabled()
        [Fact]
        public void IsItemDisabled_ItemNotPresent_ReturnsFalse()
        {
            // Given
            Item varia = Model.Items[SuperMetroidModel.VARIA_SUIT_NAME];
            ItemInventory inventory = new ItemInventory(ResourceCount.CreateVanillaBaseResourceMaximums());

            // When
            bool result = inventory.IsItemDisabled(varia);

            // Expect
            Assert.False(result);
        }

        [Fact]
        public void IsItemDisabled_ItemPresentButEnabled_ReturnsFalse()
        {
            // Given
            Item varia = Model.Items[SuperMetroidModel.VARIA_SUIT_NAME];
            ItemInventory inventory = new ItemInventory(ResourceCount.CreateVanillaBaseResourceMaximums()).ApplyAddItem(varia);

            // When
            bool result = inventory.IsItemDisabled(varia);

            // Expect
            Assert.False(result);
        }

        [Fact]
        public void IsItemDisabled_ItemPresentAndDisabled_ReturnsTrue()
        {
            // Given
            Item varia = Model.Items[SuperMetroidModel.VARIA_SUIT_NAME];
            ItemInventory inventory = new ItemInventory(ResourceCount.CreateVanillaBaseResourceMaximums());
            inventory.ApplyAddItem(varia).ApplyDisableItem(varia);
            // When
            bool result = inventory.IsItemDisabled(varia);

            // Expect
            Assert.True(result);
        }
        #endregion

        #region Tests for cLone()
        [Fact]
        public void Clone_CopiesCorrectly()
        {
            // Given
            Item varia = Model.Items[SuperMetroidModel.VARIA_SUIT_NAME];
            Item speedBooster = Model.Items[SuperMetroidModel.SPEED_BOOSTER_NAME];
            Item missilePack = Model.Items[SuperMetroidModel.MISSILE_NAME];

            ResourceCount resourceCount = new ResourceCount();
            int value = 1;
            foreach (RechargeableResourceEnum resource in Enum.GetValues(typeof(RechargeableResourceEnum)))
            {
                resourceCount.ApplyAmount(resource, value++);
            }
            ItemInventory inventory = new ItemInventory(resourceCount);
            inventory.ApplyAddItem(varia)
                .ApplyAddItem(speedBooster).ApplyDisableItem(speedBooster)
                .ApplyAddItem(missilePack).ApplyAddItem(missilePack);

            // When
            ItemInventory clone = inventory.Clone();

            // Expect
            Assert.Equal(2, clone.NonConsumableItems.Count);
            Assert.Contains(varia.Name, clone.NonConsumableItems.Keys);
            Assert.Contains(speedBooster.Name, clone.NonConsumableItems.Keys);
            Assert.Contains(speedBooster.Name, clone.DisabledItemNames);
            Assert.Single(clone.ExpansionItems);
            Assert.Contains(missilePack.Name, clone.ExpansionItems.Keys);
            Assert.Equal(2, clone.ExpansionItems[missilePack.Name].count);
            foreach (RechargeableResourceEnum resource in Enum.GetValues(typeof(RechargeableResourceEnum)))
            {
                Assert.Equal(resourceCount.GetAmount(resource), clone.BaseResourceMaximums.GetAmount(resource));
            }
        }

        [Fact]
        public void Clone_SeparatesState()
        {
            // Given
            Item varia = Model.Items[SuperMetroidModel.VARIA_SUIT_NAME];
            Item speedBooster = Model.Items[SuperMetroidModel.SPEED_BOOSTER_NAME];
            Item missilePack = Model.Items[SuperMetroidModel.MISSILE_NAME];
            ItemInventory inventory = new ItemInventory(ResourceCount.CreateVanillaBaseResourceMaximums());

            // When
            ItemInventory clone = inventory.Clone();

            // Subsequently given
            inventory.ApplyAddItem(varia)
                .ApplyAddItem(speedBooster).ApplyDisableItem(speedBooster)
                .ApplyAddItem(missilePack).ApplyAddItem(missilePack);

            // Expect
            Assert.Empty(clone.NonConsumableItems);
            Assert.Empty(clone.ExpansionItems);
            Assert.Empty(clone.DisabledItemNames);
        }
        #endregion

        #region Tests for WithResourceMaximums()
        [Fact]
        public void WithResourceMaximums_CopiesCorrectly()
        {
            // Given
            Item varia = Model.Items[SuperMetroidModel.VARIA_SUIT_NAME];
            Item speedBooster = Model.Items[SuperMetroidModel.SPEED_BOOSTER_NAME];
            Item missilePack = Model.Items[SuperMetroidModel.MISSILE_NAME];

            ItemInventory inventory = new ItemInventory(ResourceCount.CreateVanillaBaseResourceMaximums());
            inventory.ApplyAddItem(varia)
                .ApplyAddItem(speedBooster).ApplyDisableItem(speedBooster)
                .ApplyAddItem(missilePack).ApplyAddItem(missilePack);
            ResourceCount resourceCount = new ResourceCount();
            int value = 1;
            foreach (RechargeableResourceEnum resource in Enum.GetValues(typeof(RechargeableResourceEnum)))
            {
                resourceCount.ApplyAmount(resource, value++);
            }

            // When
            ItemInventory clone = inventory.WithBaseResourceMaximums(resourceCount);

            // Expect
            Assert.Equal(2, clone.NonConsumableItems.Count);
            Assert.Contains(varia.Name, clone.NonConsumableItems.Keys);
            Assert.Contains(speedBooster.Name, clone.NonConsumableItems.Keys);
            Assert.Contains(speedBooster.Name, clone.DisabledItemNames);
            Assert.Single(clone.ExpansionItems);
            Assert.Contains(missilePack.Name, clone.ExpansionItems.Keys);
            Assert.Equal(2, clone.ExpansionItems[missilePack.Name].count);
            foreach (RechargeableResourceEnum resource in Enum.GetValues(typeof(RechargeableResourceEnum)))
            {
                Assert.Equal(resourceCount.GetAmount(resource), clone.BaseResourceMaximums.GetAmount(resource));
            }
        }

        [Fact]
        public void WithResourceMaximums_SeparatesState()
        {
            // Given
            ResourceCount resourceCount = new ResourceCount();
            int value = 1;
            foreach (RechargeableResourceEnum resource in Enum.GetValues(typeof(RechargeableResourceEnum)))
            {
                resourceCount.ApplyAmount(resource, value++);
            }
            Item varia = Model.Items[SuperMetroidModel.VARIA_SUIT_NAME];
            Item speedBooster = Model.Items[SuperMetroidModel.SPEED_BOOSTER_NAME];
            Item missilePack = Model.Items[SuperMetroidModel.MISSILE_NAME];
            ItemInventory inventory = new ItemInventory(ResourceCount.CreateVanillaBaseResourceMaximums());

            // When
            ItemInventory clone = inventory.WithBaseResourceMaximums(resourceCount);

            // Subsequently given
            inventory.ApplyAddItem(varia)
                .ApplyAddItem(speedBooster).ApplyDisableItem(speedBooster)
                .ApplyAddItem(missilePack).ApplyAddItem(missilePack);

            // Expect
            Assert.Empty(clone.NonConsumableItems);
            Assert.Empty(clone.ExpansionItems);
            Assert.Empty(clone.DisabledItemNames);
        }
        #endregion
    }
}
