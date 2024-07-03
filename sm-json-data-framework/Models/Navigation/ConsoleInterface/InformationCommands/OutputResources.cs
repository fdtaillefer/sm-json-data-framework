using sm_json_data_framework.Models.Items;
using System;
using System.Collections.Generic;
using System.Text;

namespace sm_json_data_framework.Models.Navigation.ConsoleInterface.InformationCommands
{
    /// <summary>
    /// A console interface command, to output all current and max resource counts.
    /// </summary>
    public class OutputResources: ConsoleCommand
    {
        public OutputResources()
        {
            Name = "Status - Resources";
            SampleFormat = "sr";
            Description = "Outputs current and maximum resource counts";
            ValidCommand = str => str.ToLower() == "sr";
            Execution = (navigator, command) =>
            {
                foreach (RechargeableResourceEnum currentResource in Enum.GetValues<RechargeableResourceEnum>())
                {
                    Console.WriteLine($"{currentResource}: {navigator.CurrentInGameState.Resources.GetAmount(currentResource)} " +
                        $"out of {navigator.CurrentInGameState.ResourceMaximums.GetAmount(currentResource)}");
                }

                return true;
            };
        }
    }
}
