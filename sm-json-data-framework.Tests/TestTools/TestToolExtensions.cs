using sm_json_data_framework.Models.Navigation;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace sm_json_data_framework.Tests.TestTools
{
    public static class TestToolExtensions
    {
        /// <summary>
        /// Asserts that this AbstractNavigationAction's execution was a success.
        /// </summary>
        /// <param name="action">This action</param>
        public static void AssertSucceeded(this AbstractNavigationAction action)
        {
            Assert.True(action.Succeeded);
        }

        /// <summary>
        /// Asserts that this AbstractNavigationAction's execution was a failure.
        /// </summary>
        /// <param name="action">This action</param>
        public static void AssertFailed(this AbstractNavigationAction action)
        {
            Assert.False(action.Succeeded);
        }
    }
}
