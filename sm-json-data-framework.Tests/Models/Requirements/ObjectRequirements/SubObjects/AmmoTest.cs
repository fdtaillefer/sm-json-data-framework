using sm_json_data_framework.Models;
using sm_json_data_framework.Options;
using sm_json_data_framework.Tests.TestTools;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using sm_json_data_framework.Models.Items;
using sm_json_data_framework.Models.Requirements.ObjectRequirements.SubObjects;
using sm_json_data_framework.Models.Requirements;
using sm_json_data_framework.InGameStates;

namespace sm_json_data_framework.Tests.Models.Requirements.ObjectRequirements.SubObjects
{
    public class AmmoTest
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
            Ammo ammo = model.Helpers["h_canOpenGreenDoors"].Requires.LogicalElement<Ammo>(0);
            Assert.Equal(1, ammo.Count);
            Assert.Equal(AmmoEnum.Super, ammo.AmmoType);
        }

        #endregion

        #region Tests for Execute()

        [Fact]
        public void Execute_NotEnoughAmmo_Fails()
        {
            // Given
            SuperMetroidModel model = ReusableModel();
            Ammo ammo = model.Techs["canCrystalFlash"].Requires.LogicalElement<Ammo>(0, ammo => ammo.AmmoType == AmmoEnum.Missile && ammo.Count == 10);
            InGameState inGameState = model.CreateInitialGameState()
                .ApplyAddItem(SuperMetroidModel.MISSILE_NAME)
                .ApplyAddItem(SuperMetroidModel.MISSILE_NAME)
                .ApplyAddItem(SuperMetroidModel.SUPER_NAME)
                .ApplyAddItem(SuperMetroidModel.SUPER_NAME)
                .ApplyAddItem(SuperMetroidModel.POWER_BOMB_NAME)
                .ApplyAddItem(SuperMetroidModel.POWER_BOMB_NAME)
                .ApplyRefillResources()
                .ApplyConsumeResource(ConsumableResourceEnum.Missile, 1);

            // When
            ExecutionResult result = ammo.Execute(model, inGameState);

            // Expect
            Assert.Null(result);
        }

        [Fact]
        public void Execute_EnoughAmmo_Succeeds()
        {
            // Given
            SuperMetroidModel model = ReusableModel();
            Ammo ammo = model.Techs["canCrystalFlash"].Requires.LogicalElement<Ammo>(0, ammo => ammo.AmmoType == AmmoEnum.Missile && ammo.Count == 10);
            InGameState inGameState = model.CreateInitialGameState()
                .ApplyAddItem(SuperMetroidModel.MISSILE_NAME)
                .ApplyAddItem(SuperMetroidModel.MISSILE_NAME)
                .ApplyRefillResources();

            // When
            ExecutionResult result = ammo.Execute(model, inGameState);

            // Expect
            new ExecutionResultValidator(model, inGameState)
                .ExpectResourceVariation(RechargeableResourceEnum.Missile, -10)
                .AssertRespectedBy(result);
        }

        #endregion

        #region Tests for ApplyLogicalOptions() that check applied logical properties

        [Fact]
        public void ApplyLogicalOptions_SetsLogicalProperties()
        {
            // Given
            SuperMetroidModel model = NewModelForOptions();
            LogicalOptions logicalOptions = new LogicalOptions();
            logicalOptions.InternalAvailableResourceInventory = new ResourceItemInventory(ResourceCount.CreateVanillaBaseResourceMaximums())
                .ApplyAddExpansionItem((ExpansionItem)model.Items["Missile"], 1)
                .ApplyAddExpansionItem((ExpansionItem)model.Items["Super"], 10)
                .ApplyAddExpansionItem((ExpansionItem)model.Items["PowerBomb"], 10)
                .ApplyAddExpansionItem((ExpansionItem)model.Items["ETank"], 14)
                .ApplyAddExpansionItem((ExpansionItem)model.Items["ReserveTank"], 4);

            // When
            model.ApplyLogicalOptions(logicalOptions);

            // Expect
            Ammo impossibleAmmo = model.Techs["canCrystalFlash"].Requires.LogicalElement<Ammo>(0);
            Assert.True(impossibleAmmo.LogicallyRelevant);
            Assert.True(impossibleAmmo.LogicallyNever);
            Assert.False(impossibleAmmo.LogicallyAlways);
            Assert.False(impossibleAmmo.LogicallyFree);

            Ammo possibleAmmo = model.Techs["canCrystalFlash"].Requires.LogicalElement<Ammo>(1);
            Assert.True(possibleAmmo.LogicallyRelevant);
            Assert.False(possibleAmmo.LogicallyNever);
            Assert.False(possibleAmmo.LogicallyAlways);
            Assert.False(possibleAmmo.LogicallyFree);
        }

        #endregion
    }
}
