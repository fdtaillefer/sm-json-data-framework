using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace sm_json_data_parser.Models.Enemies
{
    public class EnemyContainer
    {
        public IEnumerable<Enemy> Enemies { get; set; } = Enumerable.Empty<Enemy>();
    }
}
