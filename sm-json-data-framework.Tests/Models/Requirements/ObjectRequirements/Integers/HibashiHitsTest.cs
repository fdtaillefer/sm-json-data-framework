using sm_json_data_framework.Models.Requirements.ObjectRequirements.Integers;
using sm_json_data_framework.Models;
using sm_json_data_framework.Tests.TestTools;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using sm_json_data_framework.Models.InGameStates;
using sm_json_data_framework.Models.Items;
using sm_json_data_framework.Options;
using sm_json_data_framework.Rules.InitialState;
using sm_json_data_framework.Models.Requirements;

namespace sm_json_data_framework.Tests.Models.Requirements.ObjectRequirements.Integers
{
    public class HibashiHitsTest
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
            HibashiHits hibashiHits = model.Rooms["Wasteland"].Links[7].To[2].Strats["Morph Bombs"].Requires.LogicalElement<HibashiHits>(0);
            Assert.Equal(1, hibashiHits.Hits);
        }

        #endregion

        #region Tests for CalculateDamage()

        [Fact]
        public void CalculateDamage_Suitless_ReturnsCorrectValue()
        {
            // Given
            SuperMetroidModel model = ReusableModel();
            HibashiHits hibashiHits = model.Rooms["Wasteland"].Links[7].To[2].Strats["Morph Bombs"].Requires
                .LogicalElement<HibashiHits>(0, hibashiHits => hibashiHits.Hits == 1);
            InGameState inGameState = model.CreateInitialGameState();

            // When
            int result = hibashiHits.CalculateDamage(model, inGameState);

            // Expect
            Assert.Equal(30, result);
        }

        [Fact]
        public void CalculateDamage_WithVaria_ReturnsCorrectValue()
        {
            // Given
            SuperMetroidModel model = ReusableModel();
            HibashiHits hibashiHits = model.Rooms["Wasteland"].Links[7].To[2].Strats["Morph Bombs"].Requires
                .LogicalElement<HibashiHits>(0, hibashiHits => hibashiHits.Hits == 1);
            InGameState inGameState = model.CreateInitialGameState()
                .ApplyAddItem(SuperMetroidModel.VARIA_SUIT_NAME);

            // When
            int result = hibashiHits.CalculateDamage(model, inGameState);

            // Expect
            Assert.Equal(15, result);
        }

        [Fact]
        public void CalculateDamage_WithGravity_ReturnsCorrectValue()
        {
            // Given
            SuperMetroidModel model = ReusableModel();
            HibashiHits hibashiHits = model.Rooms["Wasteland"].Links[7].To[2].Strats["Morph Bombs"].Requires
                .LogicalElement<HibashiHits>(0, hibashiHits => hibashiHits.Hits == 1);
            InGameState inGameState = model.CreateInitialGameState()
                .ApplyAddItem(SuperMetroidModel.GRAVITY_SUIT_NAME);

            // When
            int result = hibashiHits.CalculateDamage(model, inGameState);

            // Expect
            Assert.Equal(7, result);
        }

        #endregion

        #region Tests for Execute()

        [Fact]
        public void Execute_NotEnoughEnergy_Fails()
        {
            // Given
            SuperMetroidModel model = ReusableModel();
            HibashiHits hibashiHits = model.Rooms["Wasteland"].Links[7].To[2].Strats["Morph Bombs"].Requires
                .LogicalElement<HibashiHits>(0, hibashiHits => hibashiHits.Hits == 1);
            InGameState inGameState = model.CreateInitialGameState()
                .ApplyConsumeResource(ConsumableResourceEnum.Energy, 69);

            // When
            ExecutionResult result = hibashiHits.Execute(model, inGameState);

            // Expect
            Assert.Null(result);
        }

        [Fact]
        public void Execute_EnoughEnergy_Succeeds()
        {
            // Given
            SuperMetroidModel model = ReusableModel();
            HibashiHits hibashiHits = model.Rooms["Wasteland"].Links[7].To[2].Strats["Morph Bombs"].Requires
                .LogicalElement<HibashiHits>(0, hibashiHits => hibashiHits.Hits == 1);
            InGameState inGameState = model.CreateInitialGameState()
                .ApplyConsumeResource(ConsumableResourceEnum.Energy, 68);

            // When
            ExecutionResult result = hibashiHits.Execute(model, inGameState);

            // Expect
            new ExecutionResultValidator(model, inGameState)
                .ExpectResourceVariation(RechargeableResourceEnum.RegularEnergy, -30)
                .AssertRespectedBy(result);
        }

        [Fact]
        public void Execute_EnoughEnergyWithVaria_Succeeds()
        {
            // Given
            SuperMetroidModel model = ReusableModel();
            HibashiHits hibashiHits = model.Rooms["Wasteland"].Links[7].To[2].Strats["Morph Bombs"].Requires
                .LogicalElement<HibashiHits>(0, hibashiHits => hibashiHits.Hits == 1);
            InGameState inGameState = model.CreateInitialGameState()
                .ApplyAddItem(SuperMetroidModel.VARIA_SUIT_NAME);

            // When
            ExecutionResult result = hibashiHits.Execute(model, inGameState);

            // Expect
            new ExecutionResultValidator(model, inGameState)
                .ExpectResourceVariation(RechargeableResourceEnum.RegularEnergy, -15)
                .ExpectDamageReducingItemInvolved(SuperMetroidModel.VARIA_SUIT_NAME)
                .AssertRespectedBy(result);
        }

        [Fact]
        public void Execute_EnoughEnergyWithGravity_Succeeds()
        {
            // Given
            SuperMetroidModel model = ReusableModel();
            HibashiHits hibashiHits = model.Rooms["Wasteland"].Links[7].To[2].Strats["Morph Bombs"].Requires
                .LogicalElement<HibashiHits>(0, hibashiHits => hibashiHits.Hits == 1);
            InGameState inGameState = model.CreateInitialGameState()
                .ApplyAddItem(SuperMetroidModel.GRAVITY_SUIT_NAME);

            // When
            ExecutionResult result = hibashiHits.Execute(model, inGameState);

            // Expect
            new ExecutionResultValidator(model, inGameState)
                .ExpectResourceVariation(RechargeableResourceEnum.RegularEnergy, -7)
                .ExpectDamageReducingItemInvolved(SuperMetroidModel.GRAVITY_SUIT_NAME)
                .AssertRespectedBy(result);
        }

        [Fact]
        public void Execute_MultipleTimes_ConsumesCorrectAmount()
        {
            // Given
            SuperMetroidModel model = ReusableModel();
            HibashiHits hibashiHits = model.Rooms["Wasteland"].Links[7].To[2].Strats["Morph Bombs"].Requires
                .LogicalElement<HibashiHits>(0, hibashiHits => hibashiHits.Hits == 1);
            InGameState inGameState = model.CreateInitialGameState();

            // When
            ExecutionResult result = hibashiHits.Execute(model, inGameState, times: 3);

            // Expect
            new ExecutionResultValidator(model, inGameState)
                .ExpectResourceVariation(RechargeableResourceEnum.RegularEnergy, -90)
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
            HibashiHits hibashiHits = model.Rooms["Wasteland"].Links[7].To[2].Strats["Morph Bombs"].Requires
                .LogicalElement<HibashiHits>(0, hibashiHits => hibashiHits.Hits == 1);
            Assert.True(hibashiHits.LogicallyRelevant);
            Assert.True(hibashiHits.LogicallyNever);
            Assert.False(hibashiHits.LogicallyAlways);
            Assert.False(hibashiHits.LogicallyFree);
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
            HibashiHits hibashiHits = model.Rooms["Wasteland"].Links[7].To[2].Strats["Morph Bombs"].Requires
                .LogicalElement<HibashiHits>(0, hibashiHits => hibashiHits.Hits == 1);
            Assert.True(hibashiHits.LogicallyRelevant);
            Assert.False(hibashiHits.LogicallyNever);
            Assert.False(hibashiHits.LogicallyAlways);
            Assert.False(hibashiHits.LogicallyFree);
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
                        .ApplyAddItem(model.Items[SuperMetroidModel.VARIA_SUIT_NAME])
                        .ApplyAddItem(model.Items[SuperMetroidModel.GRAVITY_SUIT_NAME])
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
            HibashiHits hibashiHits = model.Rooms["Wasteland"].Links[7].To[2].Strats["Morph Bombs"].Requires
                .LogicalElement<HibashiHits>(0, hibashiHits => hibashiHits.Hits == 1);
            Assert.True(hibashiHits.LogicallyRelevant);
            Assert.False(hibashiHits.LogicallyNever);
            Assert.False(hibashiHits.LogicallyAlways);
            Assert.False(hibashiHits.LogicallyFree);
        }

        #endregion
    }
}
