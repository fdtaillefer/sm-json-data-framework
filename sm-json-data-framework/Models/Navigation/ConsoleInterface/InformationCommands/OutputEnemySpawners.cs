﻿using sm_json_data_framework.Models.Rooms;
using sm_json_data_framework.Models.Rooms.Nodes;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace sm_json_data_framework.Models.Navigation.ConsoleInterface.InformationCommands
{
    /// <summary>
    /// A console interface command, to output all enemy spawners that are accessible from the current node.
    /// </summary>
    class OutputEnemySpawners : ConsoleCommand
    {
        public OutputEnemySpawners()
        {
            Name = "Status - Accessible Spawners";
            SampleFormat = "sas";
            Description = "Outputs enemy spawners that are accessible from current node";
            ValidCommand = str => str.ToLower() == "sas";
            Execution = (navigator, command) =>
            {
                RoomNode currentNode = navigator.CurrentInGameState.GetCurrentNode();
                IEnumerable<RoomEnemy> spawners = navigator.CurrentInGameState.GetCurrentRoom().EnemiesSequence
                    .Where(e => e.IsSpawner && e.Spawns(navigator.GameModel, navigator.CurrentInGameState) && e.HomeNodeIds.Contains(currentNode.Id));
                foreach (RoomEnemy spawner in spawners)
                {
                    Console.WriteLine($"Accessible spawner {spawner.Id}: {spawner.EnemyName}X{spawner.Quantity} ({spawner.GroupName})");
                }
                if (!spawners.Any())
                {
                    Console.WriteLine("No accessible enemy spawners");
                }

                return true;
            };
        }
    }
}
