using sm_json_data_framework.Models.Items;
using sm_json_data_framework.Models.Navigation.ConsoleInterface.GameActionCommands;
using sm_json_data_framework.Models.Navigation.ConsoleInterface.InformationCommands;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;

namespace sm_json_data_framework.Models.Navigation.ConsoleInterface
{
    /// <summary>
    /// A class for interfacing with a <see cref="GameNavigator"/> using the console.
    /// </summary>
    public class ConsoleInterface
    {
        private GameNavigator Navigator {get;set;}

        private List<ConsoleCommand> Commands { get; set; }

        public ConsoleInterface(GameNavigator navigator)
        {
            Navigator = navigator;
            Commands = new List<ConsoleCommand>();

            Commands.Add(new HelpCommand(Commands));
            Commands.Add(new ExitCommand());
            Commands.Add(new InteractCommand());
            Commands.Add(new UnlockCommand());
            Commands.Add(new MoveCommand());
            Commands.Add(new FarmSpawnerCommand());
            Commands.Add(new DisableItemCommand());
            Commands.Add(new EnableItemCommand());
            Commands.Add(new OutputResources());
            Commands.Add(new OutputInventory());
            Commands.Add(new OutputMissingItemLocations());
            Commands.Add(new OutputAdjacentNodes());
            Commands.Add(new OutputEnvironment());
            Commands.Add(new OutputEnemySpawners());

            Commands.Add(new UndoAction());
            Commands.Add(new RedoAction());
        }

        public void Run(bool outputAnything, bool outputEffects, bool outputDetails)
        {
            Console.WriteLine("Beginning console navigation");

            foreach(NavigationActionCommand actionCommand in Commands.OfType<NavigationActionCommand>())
            {
                actionCommand.OutputAnything = outputAnything;
                actionCommand.OutputEffects = outputEffects;
                actionCommand.OutputDetails = outputDetails;
            }

            bool keepGoing = true;
            Regex moveRegex = new Regex(@"m\d+");



            while (keepGoing)
            {
                Console.WriteLine($"Now in {Navigator.CurrentInGameState.CurrentRoom.Name} at node {Navigator.CurrentInGameState.GetCurrentNode().Id}");

                string stringCommand = Console.ReadLine().Trim();
                ConsoleCommand matchingCommand = Commands.FirstOrDefault(c => c.ValidCommand(stringCommand));
                if (matchingCommand != null)
                {
                    keepGoing = matchingCommand.Execution(Navigator, stringCommand);
                }
                else
                {
                    Console.WriteLine("Command not recognized. run command \"h\" to list existing commands.");
                }
                Console.WriteLine("");
            }
        }

        private void OutputResources()
        {
            foreach (RechargeableResourceEnum currentResource in Enum.GetValues(typeof(RechargeableResourceEnum)))
            {
                Console.WriteLine($"{currentResource}: {Navigator.CurrentInGameState.Resources.GetAmount(currentResource)} " +
                    $"out of {Navigator.CurrentInGameState.ResourceMaximums.GetAmount(currentResource)}");
            }
        }

        private void OutputItems()
        {
        }

    }

    static class ConsoleOutputExtensions
    {
        /// <summary>
        /// Outputs this action to console, maybe.
        /// </summary>
        /// <param name="action">The action to maybe output</param>
        /// <param name="outputAnything">If true, at least outputs basic info. If false, this will do nothing regardless of other parameters.</param>
        /// <param name="outputEffects">If true, outputs the effects of this action (provided outputAnything is true)</param>
        /// <param name="outputDetails">If true, outputs details about how this action happened (provided outputAnything is true)</param>
        public static void OutputActionOptional(this AbstractNavigationAction action, bool outputAnything, bool outputEffects, bool outputDetails)
        {
            if (outputAnything)
            {
                action.OutputToConsole(outputEffects, outputDetails);
            }
        }
    }
}
