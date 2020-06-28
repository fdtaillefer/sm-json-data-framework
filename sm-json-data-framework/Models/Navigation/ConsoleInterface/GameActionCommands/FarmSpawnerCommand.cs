using System;
using System.Collections.Generic;
using System.Text;
using System.Text.RegularExpressions;

namespace sm_json_data_framework.Models.Navigation.ConsoleInterface.GameActionCommands
{
    public class FarmSpawnerCommand : ConsoleCommand, NavigationActionCommand
    {
        public bool OutputAnything { get; set; } = true;

        public bool OutputEffects { get; set; } = true;

        public bool OutputDetails { get; set; } = true;

        public FarmSpawnerCommand()
        {
            Regex farmSpawnerRegex = new Regex(@"^fs \S+");

            Name = "Farm Spawner";
            string commandIdentifier = "fs";
            SampleFormat = $"{commandIdentifier} SpawnerId";
            Description = "Farms the spawner of enemy with given in-room ID";
            ValidCommand = command => farmSpawnerRegex.IsMatch(command.ToLower());
            Execution = (navigator, command) => {
                string enemyId = farmSpawnerRegex.Match(command).Value.Substring(commandIdentifier.Length + 1);
                AbstractNavigationAction action = navigator.FarmSpawner(enemyId);
                if (OutputAnything)
                {
                    action.OutputToConsole(OutputEffects, OutputDetails);
                }
                return true;
            };
        }
    }
}
