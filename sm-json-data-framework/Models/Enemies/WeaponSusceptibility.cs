using sm_json_data_parser.Models.Weapons;
using System;
using System.Collections.Generic;
using System.Text;

namespace sm_json_data_parser.Models.Enemies
{
    public class WeaponSusceptibility
    {
        public Weapon Weapon { get; set; }

        public int Shots { get; set; }
    }
}
