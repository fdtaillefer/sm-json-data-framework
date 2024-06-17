using sm_json_data_framework.Models;
using sm_json_data_framework.Options;
using sm_json_data_framework.Tests.TestTools;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using sm_json_data_framework.Models.InGameStates;
using sm_json_data_framework.Models.Requirements.ObjectRequirements.SubRequirements;
using sm_json_data_framework.Rules.InitialState;
using sm_json_data_framework.Models.Requirements.StringRequirements;
using sm_json_data_framework.Models.Requirements;
using sm_json_data_framework.Models.Items;

namespace sm_json_data_framework.Tests.Models.Requirements.ObjectRequirements.SubRequirements
{
    public class OrTest
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
            Or or = model.Rooms["Big Pink"].Links[13].To[10].Strats["Base"].Requires.LogicalElement<Or>(0);
            Assert.NotNull(or.LogicalRequirements);
            Assert.Equal(4, or.LogicalRequirements.LogicalElements.Count);
            Assert.NotNull(or.LogicalRequirements.LogicalElement<ItemLogicalElement>(0));
            Assert.NotNull(or.LogicalRequirements.LogicalElement<HelperLogicalElement>(0));
            Assert.NotNull(or.LogicalRequirements.LogicalElement<TechLogicalElement>(0));
            Assert.NotNull(or.LogicalRequirements.LogicalElement<And>(0));
        }

        #endregion

        #region Tests for Execute()

        [Fact]
        public void Execute_NoElementMet_Fails()
        {
            // Given
            SuperMetroidModel model = ReusableModel();
            Or or = model.Helpers["h_heatProof"].Requires.LogicalElement<Or>(0);
            InGameState inGameState = model.CreateInitialGameState();

            // When
            ExecutionResult result = or.Execute(model, inGameState);

            // Expect
            Assert.Null(result);
        }

        [Fact]
        public void Execute_SomeElementsMetButNotAll_Succeeds()
        {
            // Given
            SuperMetroidModel model = ReusableModel();
            Or or = model.Rooms["Big Pink"].Links[13].To[10].Strats["Base"].Requires.LogicalElement<Or>(0);
            InGameState inGameState = model.CreateInitialGameState()
                .ApplyEnterRoom("Big Pink", 13);

            // When
            ExecutionResult result = or.Execute(model, inGameState);

            // Expect
            new ExecutionResultValidator(model, inGameState)
                .AssertRespectedBy(result);
        }

        [Fact]
        public void Execute_OneElementFreeButNotAll_SucceedsForFree()
        {
            // Given
            SuperMetroidModel model = ReusableModel();
            Or or = model.Rooms["X-Ray Scope Room"].Links[3].To[1].Strats["Base"].Requires.LogicalElement<Or>(0);
            InGameState inGameState = model.CreateInitialGameState()
                .ApplyAddItem("Morph")
                .ApplyAddItem("SpringBall")
                .ApplyAddItem(SuperMetroidModel.POWER_BOMB_NAME)
                .ApplyRefillResources()
                .ApplyEnterRoom("X-Ray Scope Room", 3);

            // When
            ExecutionResult result = or.Execute(model, inGameState);

            // Expect
            new ExecutionResultValidator(model, inGameState)
                .ExpectItemInvolved("Morph")
                .ExpectItemInvolved("SpringBall")
                .AssertRespectedBy(result);
        }

        [Fact]
        public void Execute_NoElementsFree_ExecutesCheapest()
        {
            // Given
            SuperMetroidModel model = ReusableModel();
            Or or = model.Rooms["Pink Brinstar Power Bomb Room"].Links[1].To[4].Strats["Tank the Damage"].Requires.LogicalElement<Or>(0);
            InGameState inGameState = model.CreateInitialGameState()
                .ApplyEnterRoom("Pink Brinstar Power Bomb Room", 1);

            // When
            ExecutionResult result = or.Execute(model, inGameState);

            // Expect
            new ExecutionResultValidator(model, inGameState)
                // 60 from a spike hit is less than 80 from a Sidehopper hit
                .ExpectResourceVariation(RechargeableResourceEnum.RegularEnergy, -60)
                .AssertRespectedBy(result);
        }

        #endregion

        #region Tests for ApplyLogicalOptions() that check applied logical properties

        [Fact]
        public void ApplyLogicalOptions_SetsLogicalProperties()
        {
            // Given
            SuperMetroidModel model = NewModelForOptions();
            LogicalOptions logicalOptions = new LogicalOptions()
                .RegisterRemovedItem("Spazer")
                .RegisterRemovedItem("Wave");
            logicalOptions.InternalStartConditions = StartConditions.CreateVanillaStartConditionsBuilder(model).StartingInventory(
                ItemInventory.CreateVanillaStartingInventory(model)
                    .ApplyAddItem(model.Items["Plasma"])
                    .ApplyAddItem(model.Items[SuperMetroidModel.VARIA_SUIT_NAME])
                    .ApplyAddItem(model.Items[SuperMetroidModel.GRAVITY_SUIT_NAME])
                )
                .Build();

            // When
            model.ApplyLogicalOptions(logicalOptions);

            // Expect
            Or oneFreeOneNeverOnePossible = model.Helpers["h_hasBeamUpgrade"].Requires.LogicalElement<Or>(0);
            Assert.True(oneFreeOneNeverOnePossible.LogicallyRelevant);
            Assert.False(oneFreeOneNeverOnePossible.LogicallyNever);
            Assert.True(oneFreeOneNeverOnePossible.LogicallyAlways);
            Assert.True(oneFreeOneNeverOnePossible.LogicallyFree);

            Or allFree = model.Helpers["h_heatProof"].Requires.LogicalElement<Or>(0);
            Assert.True(allFree.LogicallyRelevant);
            Assert.False(allFree.LogicallyNever);
            Assert.True(allFree.LogicallyAlways);
            Assert.True(allFree.LogicallyFree);

            Or allNever = model.Rooms["Morph Ball Room"].Links[1].To[6].Strats["Medium Sidehopper Kill"].Obstacles["C"].Requires.LogicalElement<Or>(0);
            Assert.True(allNever.LogicallyRelevant);
            Assert.True(allNever.LogicallyNever);
            Assert.False(allNever.LogicallyAlways);
            Assert.False(allNever.LogicallyFree);

            Or allPossible = model.Rooms["Morph Ball Room"].Links[5].To[6].Strats["Bomb the Blocks"].Obstacles["A"].Requires.LogicalElement<Or>(0);
            Assert.True(allPossible.LogicallyRelevant);
            Assert.False(allPossible.LogicallyNever);
            Assert.False(allPossible.LogicallyAlways);
            Assert.False(allPossible.LogicallyFree);
        }

        #endregion
    }
}
