using sm_json_data_framework.Models.Helpers;
using sm_json_data_framework.Models.InGameStates;
using sm_json_data_framework.Models.Requirements.ObjectRequirements.SubRequirements;
using sm_json_data_framework.Models;
using sm_json_data_framework.Options;
using sm_json_data_framework.Rules.InitialState;
using sm_json_data_framework.Tests.TestTools;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using sm_json_data_framework.Models.Techs;
using sm_json_data_framework.Models.Requirements.StringRequirements;

namespace sm_json_data_framework.Tests.Models.Techs
{
    public class TechTest
    {
        private static SuperMetroidModel Model = StaticTestObjects.UnmodifiableModel;
        private static SuperMetroidModel ModelWithOptions = StaticTestObjects.UnfinalizedModel.Finalize();

        #region Tests for construction from unfinalized model

        [Fact]
        public void CtorFromUnfinalized_SetsPropertiesCorrectly()
        {
            // Given/when standard model creation

            // Expect
            Tech tech = Model.Techs["canXRayClimb"];
            Assert.Equal("canXRayClimb", tech.Name);
            Assert.NotNull(tech.Requires);
            Assert.Equal(1, tech.Requires.LogicalElements.Count());
            Assert.NotNull(tech.Requires.LogicalElement<TechLogicalElement>(0));

            Assert.Equal(2, tech.ExtensionTechs.Count);
            Assert.True(tech.ExtensionTechs.ContainsKey("canRightFacingDoorXRayClimb"));
            Assert.True(tech.ExtensionTechs.ContainsKey("canLeftFacingDoorXRayClimb"));
        }

        #endregion

        #region Tests for SelectionWithExtensions()

        [Fact]
        public void SelectWithExtensions_ReturnsSelfAndAllDescendants()
        {
            Tech tech = Model.Techs["canTrickyUseFrozenEnemies"];
            IEnumerable<Tech> techs = tech.SelectWithExtensions();
            Assert.True(techs.Contains(tech, ReferenceEqualityComparer.Instance)); // Self
            Assert.True(techs.Contains(Model.Techs["canNonTrivialIceClip"], ReferenceEqualityComparer.Instance)); // Child
            Assert.True(techs.Contains(Model.Techs["canWallCrawlerIceClip"], ReferenceEqualityComparer.Instance)); // Grandchild
            Assert.False(techs.Contains(Model.Techs["canUseFrozenEnemies"], ReferenceEqualityComparer.Instance)); // Parent
        }

        #endregion

        #region Tests for ApplyLogicalOptions() that check applied logical properties

        [Fact]
        public void ApplyLogicalOptions_SetsLogicalPropertiesOnTechs()
        {
            // Given
            LogicalOptions logicalOptions = new LogicalOptions();
            logicalOptions.RegisterDisabledTech("canPreciseWalljump");
            logicalOptions.InternalStartConditions = StartConditions.CreateVanillaStartConditionsBuilder(ModelWithOptions).StartingInventory(
                ItemInventory.CreateVanillaStartingInventory(ModelWithOptions)
                    .ApplyAddItem(ModelWithOptions.Items["Morph"])
                    .ApplyAddItem(ModelWithOptions.Items["Bombs"])
                )
                .Build();

            // When
            ModelWithOptions.ApplyLogicalOptions(logicalOptions);

            // Expect
            Tech disabledTech = ModelWithOptions.Techs["canPreciseWalljump"];
            Assert.False(disabledTech.LogicallyRelevant);
            Assert.False(disabledTech.LogicallyAlways);
            Assert.False(disabledTech.LogicallyFree);
            Assert.True(disabledTech.LogicallyNever);

            Tech impossibleSubTech = ModelWithOptions.Techs["canDelayedWalljump"];
            Assert.False(impossibleSubTech.LogicallyRelevant);
            Assert.False(impossibleSubTech.LogicallyAlways);
            Assert.False(impossibleSubTech.LogicallyFree);
            Assert.True(impossibleSubTech.LogicallyNever);

            Tech nonFreeTech = ModelWithOptions.Techs["canGrappleClip"];
            Assert.True(nonFreeTech.LogicallyRelevant);
            Assert.False(nonFreeTech.LogicallyAlways);
            Assert.False(nonFreeTech.LogicallyFree);
            Assert.False(nonFreeTech.LogicallyNever);

            Tech freeTech = ModelWithOptions.Techs["canWalljump"];
            Assert.True(freeTech.LogicallyRelevant);
            Assert.True(freeTech.LogicallyAlways);
            Assert.True(freeTech.LogicallyFree);
            Assert.False(freeTech.LogicallyNever);

            Tech freeByStartItemTech = ModelWithOptions.Techs["canIBJ"];
            Assert.True(freeByStartItemTech.LogicallyRelevant);
            Assert.True(freeByStartItemTech.LogicallyAlways);
            Assert.True(freeByStartItemTech.LogicallyFree);
            Assert.False(freeByStartItemTech.LogicallyNever);
        }

        #endregion
    }
}
