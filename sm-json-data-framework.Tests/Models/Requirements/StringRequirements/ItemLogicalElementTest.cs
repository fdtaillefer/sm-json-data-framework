using sm_json_data_framework.Models;
using sm_json_data_framework.Options;
using sm_json_data_framework.Tests.TestTools;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using sm_json_data_framework.Models.InGameStates;
using sm_json_data_framework.Models.Requirements.StringRequirements;
using sm_json_data_framework.Rules.InitialState;
using sm_json_data_framework.Models.Requirements;

namespace sm_json_data_framework.Tests.Models.Requirements.StringRequirements
{
    public class ItemLogicalElementTest
    {
        private static SuperMetroidModel Model = StaticTestObjects.UnmodifiableModel;
        private static SuperMetroidModel ModelWithOptions = StaticTestObjects.UnfinalizedModel.Finalize();

        #region Tests for construction from unfinalized model

        [Fact]
        public void CtorFromUnfinalized_SetsPropertiesCorrectly()
        {
            // Given/when standard model creation

            // Expect
            ItemLogicalElement itemLogicalElement = Model.Helpers["h_canBombThings"].Requires.LogicalElement<ItemLogicalElement>(0, element => element.Item.Name == "Morph");
            Assert.Same(Model.Items["Morph"], itemLogicalElement.Item);
        }

        #endregion

        #region Tests for Execute()

        [Fact]
        public void Execute_ItemNotPresent_Fails()
        {
            // Given
            ItemLogicalElement itemLogicalElement = Model.Helpers["h_canBombThings"].Requires.LogicalElement<ItemLogicalElement>(0, element => element.Item.Name == "Morph");
            InGameState inGameState = Model.CreateInitialGameState();

            // When
            ExecutionResult result = itemLogicalElement.Execute(Model, inGameState);

            // Expect
            Assert.Null(result);
        }

        [Fact]
        public void Execute_ItemPresent_Succeeds()
        {
            // Given
            ItemLogicalElement itemLogicalElement = Model.Helpers["h_canBombThings"].Requires.LogicalElement<ItemLogicalElement>(0, element => element.Item.Name == "Morph");
            InGameState inGameState = Model.CreateInitialGameState()
                .ApplyAddItem("Morph");

            // When
            ExecutionResult result = itemLogicalElement.Execute(Model, inGameState);

            // Expect
            new ExecutionResultValidator(Model, inGameState)
                .ExpectItemInvolved("Morph")
                .AssertRespectedBy(result);
        }

        [Fact]
        public void Execute_ItemPresentButLogicallyDisabled_Fails()
        {
            // Given
            ItemLogicalElement itemLogicalElement = ModelWithOptions.Helpers["h_canBombThings"].Requires.LogicalElement<ItemLogicalElement>(0, element => element.Item.Name == "Morph");
            LogicalOptions logicalOptions = new LogicalOptions()
                .RegisterRemovedItem("Morph");
            ModelWithOptions.ApplyLogicalOptions(logicalOptions);
            InGameState inGameState = ModelWithOptions.CreateInitialGameState()
                .ApplyAddItem("Morph");

            // When
            ExecutionResult result = itemLogicalElement.Execute(ModelWithOptions, inGameState);

            // Expect
            Assert.Null(result);
        }

        #endregion

        #region Tests for ApplyLogicalOptions() that check applied logical properties

        [Fact]
        public void ApplyLogicalOptions_SetsLogicalProperties()
        {
            // Given
            LogicalOptions logicalOptions = new LogicalOptions();
            logicalOptions.RegisterRemovedItem("Bombs");
            logicalOptions.InternalStartConditions = StartConditions.CreateVanillaStartConditionsBuilder(ModelWithOptions).StartingInventory(
                ItemInventory.CreateVanillaStartingInventory(ModelWithOptions)
                    .ApplyAddItem(ModelWithOptions.Items["Morph"])
                )
                .Build();

            // When
            ModelWithOptions.ApplyLogicalOptions(logicalOptions);

            // Expect
            ItemLogicalElement freeItemElement = ModelWithOptions.Helpers["h_canBombThings"].Requires.LogicalElement<ItemLogicalElement>(0, element => element.Item.Name == "Morph");
            Assert.True(freeItemElement.LogicallyRelevant);
            Assert.False(freeItemElement.LogicallyNever);
            Assert.True(freeItemElement.LogicallyAlways);
            Assert.True(freeItemElement.LogicallyFree);

            ItemLogicalElement removedItemElement = ModelWithOptions.Helpers["h_canUseMorphBombs"].Requires.LogicalElement<ItemLogicalElement>(0, element => element.Item.Name == "Bombs");
            Assert.True(removedItemElement.LogicallyRelevant);
            Assert.True(removedItemElement.LogicallyNever);
            Assert.False(removedItemElement.LogicallyAlways);
            Assert.False(removedItemElement.LogicallyFree);

            ItemLogicalElement obtainableItemElement = ModelWithOptions.Helpers["h_canUseSpringBall"].Requires.LogicalElement<ItemLogicalElement>(0, element => element.Item.Name == "SpringBall");
            Assert.True(obtainableItemElement.LogicallyRelevant);
            Assert.False(obtainableItemElement.LogicallyNever);
            Assert.False(obtainableItemElement.LogicallyAlways);
            Assert.False(obtainableItemElement.LogicallyFree);
        }

        #endregion
    }
}
