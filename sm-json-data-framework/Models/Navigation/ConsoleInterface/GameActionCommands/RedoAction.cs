using System;
using System.Collections.Generic;
using System.Text;

namespace sm_json_data_framework.Models.Navigation.ConsoleInterface.GameActionCommands
{
    /// <summary>
    /// A console interface command, to redo the last undone action.
    /// </summary>
    public class RedoAction : ConsoleCommand, NavigationActionCommand
    {
        public bool OutputAnything { get; set; }

        public bool OutputEffects { get; set; }

        public bool OutputDetails { get; set; }

        public RedoAction()
        {
            Name = "Redo";
            SampleFormat = "y";
            Description = "Redoes last undone action";
            ValidCommand = str => str.ToLower() == "y";
            Execution = (navigator, command) => {
                AbstractNavigationAction action = navigator.Redo();
                if (OutputAnything)
                {
                    action.OutputToConsole(OutputEffects, OutputDetails);
                }
                return true;
            };
        }
    }
}
