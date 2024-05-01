using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace sm_json_data_framework.Models.Enemies
{
    public class EnemyContainer
    {
        public IList<Enemy> Enemies { get; set; } = new List<Enemy>();
    }
}
