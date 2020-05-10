using System;
using System.Collections.Generic;
using System.Text;

namespace sm_json_data_framework.Models.Navigation.ConsoleInterface.GameActionCommands
{
    /// <summary>
    /// A console interface command, to interact with the current node.
    /// </summary>
    public class InteractCommand: ConsoleCommand, NavigationActionCommand
    {
        public bool OutputAnything { get; set; } = true;

        public bool OutputEffects { get; set; } = true;

        public bool OutputDetails { get; set; } = true;

        public InteractCommand()
        {
            Name = "Interact";
            SampleFormat = "i";
            Description = "Interacts with current node";
            ValidCommand = str => str == "i";
            Execution = (navigator, command) => {
                AbstractNavigationAction action = navigator.InteractWithNode();
                if (OutputAnything)
                {
                    action.OutputToConsole(OutputEffects, OutputDetails);
                }
                return true;
            };
        }
    }
}
