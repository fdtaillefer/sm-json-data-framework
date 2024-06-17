using sm_json_data_framework.Models;
using sm_json_data_framework.Options;
using sm_json_data_framework.Tests.TestTools;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using sm_json_data_framework.Models.Requirements.StringRequirements;
using sm_json_data_framework.Models.InGameStates;
using sm_json_data_framework.Models.Requirements;

namespace sm_json_data_framework.Tests.Models.Requirements.StringRequirements
{
    public class NeverLogicalElementTest
    {
        private static SuperMetroidModel Model = StaticTestObjects.UnmodifiableModel;
        private static SuperMetroidModel ModelWithOptions = StaticTestObjects.UnfinalizedModel.Finalize();

        #region Tests for Execute()

        [Fact]
        public void Execute_Fails()
        {
            // Given
            NeverLogicalElement neverLogicalElement = Model.Locks["Etecoon Exit Grey Lock"].UnlockStrats["Base"].Requires.LogicalElement<NeverLogicalElement>(0);
            InGameState inGameState = Model.CreateInitialGameState();

            // When
            ExecutionResult result = neverLogicalElement.Execute(Model, inGameState);

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

            // When
            ModelWithOptions.ApplyLogicalOptions(logicalOptions);

            // Expect
            NeverLogicalElement never = ModelWithOptions.Locks["Etecoon Exit Grey Lock"].UnlockStrats["Base"].Requires.LogicalElement<NeverLogicalElement>(0);
            Assert.True(never.LogicallyRelevant);
            Assert.False(never.LogicallyAlways);
            Assert.False(never.LogicallyFree);
            Assert.True(never.LogicallyNever);
        }

        #endregion
    }
}
