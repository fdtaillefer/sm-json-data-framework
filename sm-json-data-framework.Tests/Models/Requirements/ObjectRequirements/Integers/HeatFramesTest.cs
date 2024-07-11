﻿using sm_json_data_framework.Models.Items;
using sm_json_data_framework.Models.Requirements.ObjectRequirements.Integers;
using sm_json_data_framework.Models.Requirements;
using sm_json_data_framework.Models;
using sm_json_data_framework.Options;
using sm_json_data_framework.Rules.InitialState;
using sm_json_data_framework.Tests.TestTools;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using sm_json_data_framework.InGameStates;

namespace sm_json_data_framework.Tests.Models.Requirements.ObjectRequirements.Integers
{
    public class HeatFramesTest
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
            HeatFrames heatFrames = model.Rooms["Post Crocomire Power Bomb Room"].Links[1].To[2].Strats["Base"].Requires.LogicalElement<HeatFrames>(0);
            Assert.Equal(100, heatFrames.Frames);
        }

        #endregion

        #region Tests for CalculateDamage()

        [Fact]
        public void CalculateDamage_Suitless_ReturnsCorrectValue()
        {
            // Given
            SuperMetroidModel model = ReusableModel();
            HeatFrames heatFrames = model.Rooms["Post Crocomire Power Bomb Room"].Links[1].To[2].Strats["Base"].Requires
                .LogicalElement<HeatFrames>(0, heatFrames => heatFrames.Frames == 100);
            InGameState inGameState = model.CreateInitialGameState();

            // When
            int result = heatFrames.CalculateDamage(model, inGameState);

            // Expect
            Assert.Equal(25, result);
        }

        [Fact]
        public void CalculateDamage_WithVaria_ReturnsCorrectValue()
        {
            // Given
            SuperMetroidModel model = ReusableModel();
            HeatFrames heatFrames = model.Rooms["Post Crocomire Power Bomb Room"].Links[1].To[2].Strats["Base"].Requires
                .LogicalElement<HeatFrames>(0, heatFrames => heatFrames.Frames == 100);
            InGameState inGameState = model.CreateInitialGameState()
                .ApplyAddItem(SuperMetroidModel.VARIA_SUIT_NAME);

            // When
            int result = heatFrames.CalculateDamage(model, inGameState);

            // Expect
            Assert.Equal(0, result);
        }

        [Fact]
        public void CalculateDamage_WithGravity_ReturnsCorrectValue()
        {
            // Given
            SuperMetroidModel model = ReusableModel();
            HeatFrames heatFrames = model.Rooms["Post Crocomire Power Bomb Room"].Links[1].To[2].Strats["Base"].Requires
                .LogicalElement<HeatFrames>(0, heatFrames => heatFrames.Frames == 100);
            InGameState inGameState = model.CreateInitialGameState()
                .ApplyAddItem(SuperMetroidModel.GRAVITY_SUIT_NAME);

            // When
            int result = heatFrames.CalculateDamage(model, inGameState);

            // Expect
            Assert.Equal(0, result);
        }

        [Fact]
        public void CalculateDamage_WithLeniency_ReturnsCorrectValue()
        {
            // Given
            SuperMetroidModel model = NewModelForOptions();
            HeatFrames heatFrames = model.Rooms["Post Crocomire Power Bomb Room"].Links[1].To[2].Strats["Base"].Requires
                .LogicalElement<HeatFrames>(0, heatFrames => heatFrames.Frames == 100);
            LogicalOptions logicalOptions = new LogicalOptions();
            logicalOptions.HeatLeniencyMultiplier = 1.5M;
            model.ApplyLogicalOptions(logicalOptions);
            InGameState inGameState = model.CreateInitialGameState();

            // When
            int result = heatFrames.CalculateDamage(model, inGameState);

            // Expect
            Assert.Equal(37, result);
        }

        #endregion

        #region Tests for Execute()

        [Fact]
        public void Execute_NotEnoughEnergy_Fails()
        {
            // Given
            SuperMetroidModel model = ReusableModel();
            HeatFrames heatFrames = model.Rooms["Post Crocomire Power Bomb Room"].Links[1].To[2].Strats["Base"].Requires
                .LogicalElement<HeatFrames>(0, heatFrames => heatFrames.Frames == 100);
            InGameState inGameState = model.CreateInitialGameState()
                .ApplyConsumeResource(ConsumableResourceEnum.Energy, 74);

            // When
            ExecutionResult result = heatFrames.Execute(model, inGameState);

            // Expect
            Assert.Null(result);
        }

        [Fact]
        public void Execute_EnoughEnergy_Succeeds()
        {
            // Given
            SuperMetroidModel model = ReusableModel();
            HeatFrames heatFrames = model.Rooms["Post Crocomire Power Bomb Room"].Links[1].To[2].Strats["Base"].Requires
                .LogicalElement<HeatFrames>(0, heatFrames => heatFrames.Frames == 100);
            InGameState inGameState = model.CreateInitialGameState()
                .ApplyConsumeResource(ConsumableResourceEnum.Energy, 24);

            // When
            ExecutionResult result = heatFrames.Execute(model, inGameState);

            // Expect
            new ExecutionResultValidator(model, inGameState)
                .ExpectResourceVariation(RechargeableResourceEnum.RegularEnergy, -25)
                .AssertRespectedBy(result);
        }

        [Fact]
        public void Execute_EnoughEnergyWithVaria_Succeeds()
        {
            // Given
            SuperMetroidModel model = ReusableModel();
            HeatFrames heatFrames = model.Rooms["Post Crocomire Power Bomb Room"].Links[1].To[2].Strats["Base"].Requires
                .LogicalElement<HeatFrames>(0, heatFrames => heatFrames.Frames == 100);
            InGameState inGameState = model.CreateInitialGameState()
                .ApplyAddItem(SuperMetroidModel.VARIA_SUIT_NAME);

            // When
            ExecutionResult result = heatFrames.Execute(model, inGameState);

            // Expect
            new ExecutionResultValidator(model, inGameState)
                .ExpectDamageReducingItemInvolved(SuperMetroidModel.VARIA_SUIT_NAME)
                .AssertRespectedBy(result);
        }

        [Fact]
        public void Execute_EnoughEnergyWithGravity_Succeeds()
        {
            // Given
            SuperMetroidModel model = ReusableModel();
            HeatFrames heatFrames = model.Rooms["Post Crocomire Power Bomb Room"].Links[1].To[2].Strats["Base"].Requires
                .LogicalElement<HeatFrames>(0, heatFrames => heatFrames.Frames == 100);
            InGameState inGameState = model.CreateInitialGameState()
                .ApplyAddItem(SuperMetroidModel.GRAVITY_SUIT_NAME);

            // When
            ExecutionResult result = heatFrames.Execute(model, inGameState);

            // Expect
            new ExecutionResultValidator(model, inGameState)
                .ExpectDamageReducingItemInvolved(SuperMetroidModel.GRAVITY_SUIT_NAME)
                .AssertRespectedBy(result);
        }

        [Fact]
        public void Execute_WithLeniencyMultiplier_ConsumesCorrectAmount()
        {
            // Given
            SuperMetroidModel model = NewModelForOptions();
            HeatFrames heatFrames = model.Rooms["Post Crocomire Power Bomb Room"].Links[1].To[2].Strats["Base"].Requires
                .LogicalElement<HeatFrames>(0, heatFrames => heatFrames.Frames == 100);
            LogicalOptions logicalOptions = new LogicalOptions();
            logicalOptions.HeatLeniencyMultiplier = 1.5M;
            model.ApplyLogicalOptions(logicalOptions);
            InGameState inGameState = model.CreateInitialGameState();

            // When
            ExecutionResult result = heatFrames.Execute(model, inGameState);

            // Expect
            new ExecutionResultValidator(model, inGameState)
                .ExpectResourceVariation(RechargeableResourceEnum.RegularEnergy, -37)
                .AssertRespectedBy(result);
        }

        [Fact]
        public void Execute_MultipleTimes_ConsumesCorrectAmount()
        {
            // Given
            SuperMetroidModel model = ReusableModel();
            HeatFrames heatFrames = model.Rooms["Post Crocomire Power Bomb Room"].Links[1].To[2].Strats["Base"].Requires
                .LogicalElement<HeatFrames>(0, heatFrames => heatFrames.Frames == 100);
            InGameState inGameState = model.CreateInitialGameState();

            // When
            ExecutionResult result = heatFrames.Execute(model, inGameState, times: 2);

            // Expect
            new ExecutionResultValidator(model, inGameState)
                .ExpectResourceVariation(RechargeableResourceEnum.RegularEnergy, -50)
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
            ResourceCount baseResouces = ResourceCount.CreateVanillaBaseResourceMaximums()
                .ApplyAmount(RechargeableResourceEnum.RegularEnergy, 29);
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
            HeatFrames heatFrames = model.Rooms["Fast Pillars Setup Room"].Links[4].To[1].Strats["Base"].Requires.LogicalElement<HeatFrames>(0);
            Assert.True(heatFrames.LogicallyRelevant);
            Assert.True(heatFrames.LogicallyNever);
            Assert.False(heatFrames.LogicallyAlways);
            Assert.False(heatFrames.LogicallyFree);
            Assert.Equal(LogicalOptions.DefaultFrameLeniencyMultiplier, heatFrames.HeatLeniencyMultiplier);
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
            HeatFrames heatFrames = model.Rooms["Fast Pillars Setup Room"].Links[4].To[1].Strats["Base"].Requires.LogicalElement<HeatFrames>(0);
            Assert.True(heatFrames.LogicallyRelevant);
            Assert.False(heatFrames.LogicallyNever);
            Assert.False(heatFrames.LogicallyAlways);
            Assert.False(heatFrames.LogicallyFree);
            Assert.Equal(LogicalOptions.DefaultFrameLeniencyMultiplier, heatFrames.HeatLeniencyMultiplier);
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
            HeatFrames heatFrames = model.Rooms["Fast Pillars Setup Room"].Links[4].To[1].Strats["Base"].Requires.LogicalElement<HeatFrames>(0);
            Assert.True(heatFrames.LogicallyRelevant);
            Assert.False(heatFrames.LogicallyNever);
            Assert.True(heatFrames.LogicallyAlways);
            Assert.True(heatFrames.LogicallyFree);
            Assert.Equal(LogicalOptions.DefaultFrameLeniencyMultiplier, heatFrames.HeatLeniencyMultiplier);
        }

        [Fact]
        public void ApplyLogicalOptions_DifferentHeatLeniency_SetsLogicalProperties()
        {
            // Given
            SuperMetroidModel model = NewModelForOptions();
            LogicalOptions logicalOptions = new LogicalOptions();
            logicalOptions.HeatLeniencyMultiplier = 2;

            // When
            model.ApplyLogicalOptions(logicalOptions);

            // Expect
            HeatFrames heatFrames = model.Rooms["Fast Pillars Setup Room"].Links[4].To[1].Strats["Base"].Requires.LogicalElement<HeatFrames>(0);
            Assert.Equal(2, heatFrames.HeatLeniencyMultiplier);
        }

        #endregion
    }
}
