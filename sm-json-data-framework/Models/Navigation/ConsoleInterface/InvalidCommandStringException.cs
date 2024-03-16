using System;
using System.Collections.Generic;
using System.Text;

namespace sm_json_data_framework.Models.Navigation.ConsoleInterface
{
    /// <summary>
    /// An exception indicating that an attempt to interpret a string into a specific console command failed.
    /// </summary>
    public class InvalidCommandStringException : Exception
    {
        public InvalidCommandStringException(Type commandType) : base ($"String is not a valid representation of {commandType.Name}")
        {
            
        }
    }
}
