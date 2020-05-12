using System;
using System.Collections.Generic;
using System.Text;

namespace sm_json_data_framework.Models.Navigation.ConsoleInterface.GameActionCommands
{
    /// <summary>
    /// A console unlock command, to unlock the current node without interacting with it.
    /// </summary>
    public class UnlockCommand : ConsoleCommand, NavigationActionCommand
    {
        public bool OutputAnything { get; set; } = true;

        public bool OutputEffects { get; set; } = true;

        public bool OutputDetails { get; set; } = true;

        public UnlockCommand()
        {
            Name = "Unlock";
            SampleFormat = "u";
            Description = "Unlocks current node";
            ValidCommand = str => str == "u";
            Execution = (navigator, command) => {
                AbstractNavigationAction action = navigator.UnlockNode();
                if (OutputAnything)
                {
                    action.OutputToConsole(OutputEffects, OutputDetails);
                }
                return true;
            };
        }
    }
}
