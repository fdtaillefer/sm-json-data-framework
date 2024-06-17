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
        private static SuperMetroidModel ReusableModel() => StaticTestObjects.UnmodifiableModel;
        private static SuperMetroidModel NewModelForOptions() => StaticTestObjects.UnfinalizedModel.Finalize();

        #region Tests for Execute()

        [Fact]
        public void Execute_Fails()
        {
            // Given
            SuperMetroidModel model = ReusableModel();
            NeverLogicalElement neverLogicalElement = model.Locks["Etecoon Exit Grey Lock"].UnlockStrats["Base"].Requires.LogicalElement<NeverLogicalElement>(0);
            InGameState inGameState = model.CreateInitialGameState();

            // When
            ExecutionResult result = neverLogicalElement.Execute(model, inGameState);

            // Expect
            Assert.Null(result);
        }

        #endregion

        #region Tests for ApplyLogicalOptions() that check applied logical properties

        [Fact]
        public void ApplyLogicalOptions_SetsLogicalProperties()
        {
            // Given
            SuperMetroidModel model = NewModelForOptions();
            LogicalOptions logicalOptions = new LogicalOptions();

            // When
            model.ApplyLogicalOptions(logicalOptions);

            // Expect
            NeverLogicalElement never = model.Locks["Etecoon Exit Grey Lock"].UnlockStrats["Base"].Requires.LogicalElement<NeverLogicalElement>(0);
            Assert.True(never.LogicallyRelevant);
            Assert.False(never.LogicallyAlways);
            Assert.False(never.LogicallyFree);
            Assert.True(never.LogicallyNever);
        }

        #endregion
    }
}
