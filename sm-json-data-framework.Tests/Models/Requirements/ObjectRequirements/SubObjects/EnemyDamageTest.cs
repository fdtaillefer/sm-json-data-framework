using sm_json_data_framework.Models;
using sm_json_data_framework.Options;
using sm_json_data_framework.Tests.TestTools;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using sm_json_data_framework.Models.InGameStates;
using sm_json_data_framework.Models.Items;
using sm_json_data_framework.Models.Requirements.ObjectRequirements.SubObjects;
using sm_json_data_framework.Rules.InitialState;
using sm_json_data_framework.Models.Requirements;

namespace sm_json_data_framework.Tests.Models.Requirements.ObjectRequirements.SubObjects
{
    public class EnemyDamageTest
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
            EnemyDamage enemyDamage = model.Rooms["Morph Ball Room"].Links[1].To[6].Strats["Run Through"].Requires.LogicalElement<EnemyDamage>(0);
            Assert.Equal(1, enemyDamage.Hits);
            Assert.Same(model.Enemies["Sidehopper"], enemyDamage.Enemy);
            Assert.Same(model.Enemies["Sidehopper"].Attacks["contact"], enemyDamage.Attack);
        }

        #endregion

        #region Tests for Execute()

        [Fact]
        public void Execute_RemovesEnergy()
        {
            // Given
            SuperMetroidModel model = ReusableModel();
            EnemyDamage enemyDamage = model.Rooms["Green Brinstar Beetom Room"].Links[1].To[2].Strats["Bomb the Beetoms"].Requires
                .LogicalElement<EnemyDamage>(0, enemyDamage => enemyDamage.Enemy.Name == "Beetom");
            InGameState inGameState = model.CreateInitialGameState();

            // When
            ExecutionResult result =  enemyDamage.Execute(model, inGameState);

            // Expect
            new ExecutionResultValidator(model, inGameState)
                .ExpectResourceVariation(RechargeableResourceEnum.RegularEnergy, -20)
                .AssertRespectedBy(result);
        }

        [Fact]
        public void Execute_NotEnoughEnergy_Fails()
        {
            // Given
            SuperMetroidModel model = ReusableModel();
            EnemyDamage enemyDamage = model.Rooms["Green Brinstar Beetom Room"].Links[1].To[2].Strats["Bomb the Beetoms"].Requires
                .LogicalElement<EnemyDamage>(0, enemyDamage => enemyDamage.Enemy.Name == "Beetom");
            InGameState inGameState = model.CreateInitialGameState()
                .ApplyConsumeResource(ConsumableResourceEnum.Energy, 79);

            // When
            ExecutionResult result = enemyDamage.Execute(model, inGameState);

            // Expect
            Assert.Null(result);
        }

        [Fact]
        public void Execute_WithVaria_ConsumesCorrectAmount()
        {
            // Given
            SuperMetroidModel model = ReusableModel();
            EnemyDamage enemyDamage = model.Rooms["Green Brinstar Beetom Room"].Links[1].To[2].Strats["Bomb the Beetoms"].Requires
                .LogicalElement<EnemyDamage>(0, enemyDamage => enemyDamage.Enemy.Name == "Beetom");
            InGameState inGameState = model.CreateInitialGameState()
                .ApplyAddItem(SuperMetroidModel.VARIA_SUIT_NAME);

            // When
            ExecutionResult result = enemyDamage.Execute(model, inGameState);

            // Expect
            new ExecutionResultValidator(model, inGameState)
                .ExpectResourceVariation(RechargeableResourceEnum.RegularEnergy, -10)
                .ExpectDamageReducingItemInvolved(SuperMetroidModel.VARIA_SUIT_NAME)
                .AssertRespectedBy(result);
        }

        [Fact]
        public void Execute_WithGravity_ConsumesCorrectAmount()
        {
            // Given
            SuperMetroidModel model = ReusableModel();
            EnemyDamage enemyDamage = model.Rooms["Green Brinstar Beetom Room"].Links[1].To[2].Strats["Bomb the Beetoms"].Requires
                .LogicalElement<EnemyDamage>(0, enemyDamage => enemyDamage.Enemy.Name == "Beetom");
            InGameState inGameState = model.CreateInitialGameState()
                .ApplyAddItem(SuperMetroidModel.GRAVITY_SUIT_NAME);

            // When
            ExecutionResult result = enemyDamage.Execute(model, inGameState);

            // Expect
            new ExecutionResultValidator(model, inGameState)
                .ExpectResourceVariation(RechargeableResourceEnum.RegularEnergy, -4)
                .ExpectDamageReducingItemInvolved(SuperMetroidModel.GRAVITY_SUIT_NAME)
                .AssertRespectedBy(result);
        }

        // With gravity but not reduced by it
        [Fact]
        public void Execute_WithGravity_AttackNotAffectedByGravity_ConsumesCorrectAmount()
        {
            // Given
            SuperMetroidModel model = ReusableModel();
            EnemyDamage enemyDamage = model.Locks["Mother Brain 2 and 3 Fight"].UnlockStrats["Base"].Requires
                .LogicalElement<EnemyDamage>(0, enemyDamage => enemyDamage.Attack.Name == "rainbow");
            InGameState inGameState = model.CreateInitialGameState()
                .ApplyAddItem(SuperMetroidModel.GRAVITY_SUIT_NAME)
                .ApplyAddItem(SuperMetroidModel.ENERGY_TANK_NAME)
                .ApplyAddItem(SuperMetroidModel.ENERGY_TANK_NAME)
                .ApplyAddItem(SuperMetroidModel.ENERGY_TANK_NAME)
                .ApplyAddItem(SuperMetroidModel.ENERGY_TANK_NAME)
                .ApplyAddItem(SuperMetroidModel.ENERGY_TANK_NAME)
                .ApplyAddItem(SuperMetroidModel.ENERGY_TANK_NAME)
                .ApplyRefillResources();

            // When
            ExecutionResult result = enemyDamage.Execute(model, inGameState);

            // Expect
            new ExecutionResultValidator(model, inGameState)
                .ExpectResourceVariation(RechargeableResourceEnum.RegularEnergy, -600)
                .AssertRespectedBy(result);
        }

        #endregion

        #region Tests for ApplyLogicalOptions() that check applied logical properties

        [Fact]
        public void ApplyLogicalOptions_LessPossibleEnergyThanBestCaseDamage_SetsLogicalProperties()
        {
            // Given
            SuperMetroidModel model = NewModelForOptions();
            LogicalOptions logicalOptions = new LogicalOptions()
                .RegisterRemovedItem(SuperMetroidModel.VARIA_SUIT_NAME)
                .RegisterRemovedItem(SuperMetroidModel.GRAVITY_SUIT_NAME);
            ResourceCount baseResouces = ResourceCount.CreateVanillaBaseResourceMaximums();
            baseResouces.ApplyAmount(RechargeableResourceEnum.RegularEnergy, 29);
            logicalOptions.InternalStartConditions = StartConditions.CreateVanillaStartConditionsBuilder(model)
                .StartingInventory(
                    ItemInventory.CreateVanillaStartingInventory(model)
                        .ApplyAddItem(model.Items[SuperMetroidModel.SPEED_BOOSTER_NAME])
                )
                .BaseResourceMaximums(baseResouces)
                .StartingResources(baseResouces)
                .Build();
            logicalOptions.InternalAvailableResourceInventory = new ResourceItemInventory(baseResouces)
                .ApplyAddExpansionItem((ExpansionItem)model.Items["Missile"], 46)
                .ApplyAddExpansionItem((ExpansionItem)model.Items["Super"], 10)
                .ApplyAddExpansionItem((ExpansionItem)model.Items["PowerBomb"], 10);

            // When
            model.ApplyLogicalOptions(logicalOptions);

            // Expect
            EnemyDamage enemyDamage = model.Rooms["Green Brinstar Beetom Room"].Links[1].To[2].Strats["Tank the Damage"].Requires.LogicalElement<EnemyDamage>(0);
            Assert.True(enemyDamage.LogicallyRelevant);
            Assert.True(enemyDamage.LogicallyNever);
            Assert.False(enemyDamage.LogicallyAlways);
            Assert.False(enemyDamage.LogicallyFree);
        }

        [Fact]
        public void ApplyLogicalOptions_NormalPossibleEnergy_SetsLogicalProperties()
        {
            // Given
            SuperMetroidModel model = NewModelForOptions();
            LogicalOptions logicalOptions = new LogicalOptions()
                .RegisterRemovedItem(SuperMetroidModel.VARIA_SUIT_NAME)
                .RegisterRemovedItem(SuperMetroidModel.GRAVITY_SUIT_NAME);
            ResourceCount baseResouces = ResourceCount.CreateVanillaBaseResourceMaximums();
            baseResouces.ApplyAmount(RechargeableResourceEnum.RegularEnergy, 29);
            logicalOptions.InternalStartConditions = StartConditions.CreateVanillaStartConditionsBuilder(model)
                .StartingInventory(
                    ItemInventory.CreateVanillaStartingInventory(model)
                        .ApplyAddItem(model.Items[SuperMetroidModel.SPEED_BOOSTER_NAME])
                )
                .BaseResourceMaximums(baseResouces)
                .StartingResources(baseResouces)
                .Build();
            logicalOptions.InternalAvailableResourceInventory = new ResourceItemInventory(baseResouces)
                .ApplyAddExpansionItem((ExpansionItem)model.Items["Super"], 10)
                .ApplyAddExpansionItem((ExpansionItem)model.Items["Missile"], 46)
                .ApplyAddExpansionItem((ExpansionItem)model.Items["Super"], 10)
                .ApplyAddExpansionItem((ExpansionItem)model.Items["PowerBomb"], 10)
                .ApplyAddExpansionItem((ExpansionItem)model.Items["ETank"], 14)
                .ApplyAddExpansionItem((ExpansionItem)model.Items["ReserveTank"], 4);

            // When
            model.ApplyLogicalOptions(logicalOptions);

            // Expect
            EnemyDamage enemyDamage = model.Rooms["Green Brinstar Beetom Room"].Links[1].To[2].Strats["Tank the Damage"].Requires.LogicalElement<EnemyDamage>(0);
            Assert.True(enemyDamage.LogicallyRelevant);
            Assert.False(enemyDamage.LogicallyNever);
            Assert.False(enemyDamage.LogicallyAlways);
            Assert.False(enemyDamage.LogicallyFree);
        }

        [Fact]
        public void ApplyLogicalOptions_BothSuitsFree_SetsLogicalProperties()
        {
            // Given
            SuperMetroidModel model = NewModelForOptions();
            LogicalOptions logicalOptions = new LogicalOptions();
            ResourceCount baseResouces = ResourceCount.CreateVanillaBaseResourceMaximums();
            baseResouces.ApplyAmount(RechargeableResourceEnum.RegularEnergy, 29);
            logicalOptions.InternalStartConditions = StartConditions.CreateVanillaStartConditionsBuilder(model)
                .StartingInventory(
                    ItemInventory.CreateVanillaStartingInventory(model)
                        .ApplyAddItem(model.Items[SuperMetroidModel.SPEED_BOOSTER_NAME])
                        .ApplyAddItem(model.Items[SuperMetroidModel.VARIA_SUIT_NAME])
                        .ApplyAddItem(model.Items[SuperMetroidModel.GRAVITY_SUIT_NAME])
                )
                .BaseResourceMaximums(baseResouces)
                .StartingResources(baseResouces)
                .Build();

            // When
            model.ApplyLogicalOptions(logicalOptions);

            // Expect
            EnemyDamage enemyDamage = model.Rooms["Green Brinstar Beetom Room"].Links[1].To[2].Strats["Tank the Damage"].Requires.LogicalElement<EnemyDamage>(0);
            Assert.True(enemyDamage.LogicallyRelevant);
            Assert.False(enemyDamage.LogicallyNever);
            Assert.False(enemyDamage.LogicallyAlways);
            Assert.False(enemyDamage.LogicallyFree);
        }

        #endregion
    }
}
