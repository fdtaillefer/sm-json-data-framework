using System;
using System.Collections.Generic;
using System.Text;

namespace sm_json_data_framework.Models.GameFlags
{
    public class GameFlag
    {
        public string Name { get; set; }

        public GameFlag() { }

        public GameFlag(string name)
        {
            Name = name;
        }
    }
}
