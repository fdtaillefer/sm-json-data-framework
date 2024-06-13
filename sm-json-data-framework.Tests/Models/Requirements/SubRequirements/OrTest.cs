using sm_json_data_framework.Models;
using sm_json_data_framework.Options;
using sm_json_data_framework.Rules;
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

namespace sm_json_data_framework.Tests.Models.Requirements.SubRequirements
{
    public class OrTest
    {
        private static SuperMetroidModel Model = StaticTestObjects.UnmodifiableModel;
        private static SuperMetroidModel ModelWithOptions = StaticTestObjects.UnfinalizedModel.Finalize();

        #region Tests for construction from unfinalized model

        [Fact]
        public void CtorFromUnfinalized_SetsPropertiesCorrectly()
        {
            // Given/when standard model creation

            // Expect
            Or or = Model.Rooms["Big Pink"].Links[13].To[10].Strats["Base"].Requires.LogicalElement<Or>(0);
            Assert.NotNull(or.LogicalRequirements);
            Assert.Equal(4, or.LogicalRequirements.LogicalElements.Count);
            Assert.NotNull(or.LogicalRequirements.LogicalElement<ItemLogicalElement>(0));
            Assert.NotNull(or.LogicalRequirements.LogicalElement<HelperLogicalElement>(0));
            Assert.NotNull(or.LogicalRequirements.LogicalElement<TechLogicalElement>(0));
            Assert.NotNull(or.LogicalRequirements.LogicalElement<And>(0));
        }

        #endregion

        #region Tests for Execute()

        [Fact] public void Execute_NoElementMet_Fails()
        {
            // Given
            Or or = Model.Helpers["h_heatProof"].Requires.LogicalElement<Or>(0);
            InGameState inGameState = Model.CreateInitialGameState();

            // When
            ExecutionResult result = or.Execute(Model, inGameState);

            // Expect
            Assert.Null(result);
        }

        [Fact]
        public void Execute_SomeElementsMetButNotAll_Succeeds()
        {
            // Given
            Or or = Model.Rooms["Big Pink"].Links[13].To[10].Strats["Base"].Requires.LogicalElement<Or>(0);
            InGameState inGameState = Model.CreateInitialGameState()
                .ApplyEnterRoom(Model.Rooms["Big Pink"].Nodes[13]);

            // When
            ExecutionResult result = or.Execute(Model, inGameState);

            // Expect
            new ExecutionResultValidator(Model, inGameState)
                .AssertRespectedBy(result);
        }

        [Fact]
        public void Execute_OneElementFreeButNotAll_SucceedsForFree()
        {
            // Given
            Or or = Model.Rooms["X-Ray Scope Room"].Links[3].To[1].Strats["Base"].Requires.LogicalElement<Or>(0);
            InGameState inGameState = Model.CreateInitialGameState()
                .ApplyAddItem(ModelWithOptions.Items["Morph"])
                .ApplyAddItem(ModelWithOptions.Items["SpringBall"])
                .ApplyAddItem(ModelWithOptions.Items[SuperMetroidModel.POWER_BOMB_NAME])
                .ApplyRefillResources()
                .ApplyEnterRoom(Model.Rooms["X-Ray Scope Room"].Nodes[3]);

            // When
            ExecutionResult result = or.Execute(Model, inGameState);

            // Expect
            new ExecutionResultValidator(Model, inGameState)
                .ExpectItemInvolved("Morph")
                .ExpectItemInvolved("SpringBall")
                .AssertRespectedBy(result);
        }

        [Fact]
        public void Execute_NoElementsFree_ExecutesCheapest()
        {
            // Given
            Or or = Model.Rooms["Pink Brinstar Power Bomb Room"].Links[1].To[4].Strats["Tank the Damage"].Requires.LogicalElement<Or>(0);
            InGameState inGameState = Model.CreateInitialGameState()
                .ApplyEnterRoom(Model.Rooms["Pink Brinstar Power Bomb Room"].Nodes[1]);

            // When
            ExecutionResult result = or.Execute(Model, inGameState);

            // Expect
            new ExecutionResultValidator(Model, inGameState)
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
            LogicalOptions logicalOptions = new LogicalOptions()
                .RegisterRemovedItem("Spazer")
                .RegisterRemovedItem("Wave");
            logicalOptions.InternalStartConditions = StartConditions.CreateVanillaStartConditionsBuilder(ModelWithOptions).StartingInventory(
                ItemInventory.CreateVanillaStartingInventory(ModelWithOptions)
                    .ApplyAddItem(ModelWithOptions.Items["Plasma"])
                    .ApplyAddItem(ModelWithOptions.Items[SuperMetroidModel.VARIA_SUIT_NAME])
                    .ApplyAddItem(ModelWithOptions.Items[SuperMetroidModel.GRAVITY_SUIT_NAME])
                )
                .Build();

            // When
            ModelWithOptions.ApplyLogicalOptions(logicalOptions);

            // Expect
            Or oneFreeOneNeverOnePossible = ModelWithOptions.Helpers["h_hasBeamUpgrade"].Requires.LogicalElement<Or>(0);
            Assert.True(oneFreeOneNeverOnePossible.LogicallyRelevant);
            Assert.False(oneFreeOneNeverOnePossible.LogicallyNever);
            Assert.True(oneFreeOneNeverOnePossible.LogicallyAlways);
            Assert.True(oneFreeOneNeverOnePossible.LogicallyFree);

            Or allFree = ModelWithOptions.Helpers["h_heatProof"].Requires.LogicalElement<Or>(0);
            Assert.True(allFree.LogicallyRelevant);
            Assert.False(allFree.LogicallyNever);
            Assert.True(allFree.LogicallyAlways);
            Assert.True(allFree.LogicallyFree);

            Or allNever = ModelWithOptions.Rooms["Morph Ball Room"].Links[1].To[6].Strats["Medium Sidehopper Kill"].Obstacles["C"].Requires.LogicalElement<Or>(0);
            Assert.True(allNever.LogicallyRelevant);
            Assert.True(allNever.LogicallyNever);
            Assert.False(allNever.LogicallyAlways);
            Assert.False(allNever.LogicallyFree);

            Or allPossible = ModelWithOptions.Rooms["Morph Ball Room"].Links[5].To[6].Strats["Bomb the Blocks"].Obstacles["A"].Requires.LogicalElement<Or>(0);
            Assert.True(allPossible.LogicallyRelevant);
            Assert.False(allPossible.LogicallyNever);
            Assert.False(allPossible.LogicallyAlways);
            Assert.False(allPossible.LogicallyFree);
        }

        #endregion
    }
}
