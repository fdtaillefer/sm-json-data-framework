using System;
using System.Collections.Generic;
using System.Text;
using System.Text.RegularExpressions;

namespace sm_json_data_framework.Models.Navigation.ConsoleInterface.GameActionCommands
{
    /// <summary>
    /// A console interface command, to move to the specified node.
    /// </summary>
    public class MoveCommand: ConsoleCommand, NavigationActionCommand
    {
        public bool OutputAnything { get; set; } = true;

        public bool OutputEffects { get; set; } = true;

        public bool OutputDetails { get; set; } = true;

        public MoveCommand()
        {
            Regex moveRegex = new Regex(@"m\d+");

            Name = "Move";
            SampleFormat = "m##";
            Description = "Moves to the given node ID";
            ValidCommand = command => moveRegex.IsMatch(command);
            Execution = (navigator, command) => {
                int nodeId = int.Parse(moveRegex.Match(command).Value.Substring(1));
                AbstractNavigationAction action = navigator.MoveToNode(nodeId);
                if (OutputAnything)
                {
                    action.OutputToConsole(OutputEffects, OutputDetails);
                }
                return true;
            };
        }
    }
}
