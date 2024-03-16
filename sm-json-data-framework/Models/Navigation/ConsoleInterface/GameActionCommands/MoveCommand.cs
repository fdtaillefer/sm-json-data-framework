using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;

namespace sm_json_data_framework.Models.Navigation.ConsoleInterface.GameActionCommands
{
    /// <summary>
    /// A console interface command, to move to the specified node. Supports some options to limit which strats may be used to move.
    /// </summary>
    public class MoveCommand: ConsoleCommand, NavigationActionCommand
    {
        public bool OutputAnything { get; set; } = true;

        public bool OutputEffects { get; set; } = true;

        public bool OutputDetails { get; set; } = true;

        private const string CommandIdentifier = "m";
        private const string StratNameOption = "-stratname";
        private const string StratNameStartsOption = "-stratnamestarts";
        private const string BreaksObstacleOption = "-obstacle";

        public MoveCommand()
        {
            Name = "Move";
            SampleFormat = $"{CommandIdentifier}##" + " {options}";
            Description = "Moves to the given node ID";
            Options = new string[] {
                "-stratname {name}: Specifies the strat to use",
                "-stratnamestarts {value}: Limits usable strats to those whose name starts with value",
                "-obstacle {id}: Limits usable strats to those whose that break a given obstacle"
            };
            ValidCommand = command =>
            {
                try
                {
                    InterpretString(command);
                    return true;
                }
                catch (InvalidCommandStringException e)
                {
                    return false;
                }
            };
            Execution = (navigator, command) => {
                MoveCommandDetails commandDetails = InterpretString(command);
                AbstractNavigationAction action = navigator.MoveToNode(commandDetails.NodeId, commandDetails.StratFilters);
                if (OutputAnything)
                {
                    action.OutputToConsole(OutputEffects, OutputDetails);
                }
                return true;
            };
        }

        /// <summary>
        /// Attempts to interpret the provided string into details of a Move command. Throws an exception if this fails.
        /// </summary>
        /// <param name="command">The string to interpret</param>
        /// <returns>A model containing the extracted details</returns>
        /// <exception cref="InvalidCommandStringException">Thrown if the string cannot be interpreted into this command</exception>
        private MoveCommandDetails InterpretString(string command)
        {
            Regex moveRegex = new Regex(@"^m\d+");
            int firstSpace = command.IndexOf(' ');
            string[] baseSplit = command.Split(' ', 2);

            // First part of the command must match base format
            if (!moveRegex.IsMatch(baseSplit[0]))
            {
                throw new InvalidCommandStringException(GetType());
            }

            // Base format is good... figure out node ID
            int nodeId = int.Parse(moveRegex.Match(baseSplit[0]).Value.Substring(CommandIdentifier.Length));

            // If there are no options, we're good to go
            if(baseSplit.Count() == 1)
            {
                return new MoveCommandDetails
                {
                    NodeId = nodeId
                };
            }

            // There are options, so we must interpret them
            IEnumerable<(string keyword, string value)> options = ExtractOptions(baseSplit[1].Trim()).ToArray();
            return new MoveCommandDetails
            {
                NodeId = nodeId,
                StratFilters = options.Select(option => OptionToStratFilter(option)).ToArray()
            };
        }

        /// <summary>
        /// Converts the provided option keyword-value pair into a StratFilter. Returns an exception if the keyword is unrecognized or the value is invalid.
        /// </summary>
        /// <param name="option"></param>
        /// <returns></returns>
        /// <exception cref="InvalidCommandStringException">Thrown if the keyword is unrecognized or the value is invalid</exception>
        private StratFilter OptionToStratFilter ((string keyword, string value) option)
        {
            if (option.value == null)
            {
                throw new InvalidCommandStringException(GetType());
            }

            return option.keyword switch {
                StratNameOption => StratFilter.NameIs(option.value),
                StratNameStartsOption => StratFilter.NameStartsWith(option.value),
                BreaksObstacleOption => StratFilter.BreaksObstacle(option.value),
                // Unrecognized option keyword
                _ => throw  new InvalidCommandStringException(GetType())
            };
        }

        /// <summary>
        /// Simple model containing details that parameterize a Move command.
        /// </summary>
        private class MoveCommandDetails
        {
            public int NodeId { get; set; }
            public StratFilter[] StratFilters { get; set; }
        }       
    }

}
