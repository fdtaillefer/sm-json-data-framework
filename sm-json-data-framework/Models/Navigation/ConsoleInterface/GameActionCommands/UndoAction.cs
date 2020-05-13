using sm_json_data_framework.Models.Navigation.ConsoleInterface.GameActionCommands;
using System;
using System.Collections.Generic;
using System.Text;

namespace sm_json_data_framework.Models.Navigation.ConsoleInterface.GameActionCommands
{
    /// <summary>
    /// A console interface command, to undo the last done action.
    /// </summary>
    public class UndoAction : ConsoleCommand, NavigationActionCommand
    {
        public bool OutputAnything { get; set; }

        public bool OutputEffects { get; set; }

        public bool OutputDetails { get; set; }

        public UndoAction()
        {
            Name = "Undo";
            SampleFormat = "z";
            Description = "Undoes last action";
            ValidCommand = str => str.ToLower() == "z";
            Execution = (navigator, command) => {
                AbstractNavigationAction action = navigator.Undo();
                if (OutputAnything)
                {
                    action.OutputToConsole(OutputEffects, OutputDetails);
                }
                return true;
            };
        }
    }
}
