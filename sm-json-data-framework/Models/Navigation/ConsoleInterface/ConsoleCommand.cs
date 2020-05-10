using System;
using System.Collections.Generic;
using System.Text;

namespace sm_json_data_framework.Models.Navigation.ConsoleInterface
{
    /// <summary>
    /// And abstract class for a command in the console interface.
    /// </summary>
    public abstract class ConsoleCommand
    {
        public string Name { get; protected set; }

        public string SampleFormat { get; protected set; }

        public string Description { get; protected set; }

        public Predicate<string> ValidCommand { get; protected set; }

        public Func<GameNavigator, string, bool> Execution { get; protected set; }
    }
}
