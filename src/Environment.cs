using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SaveBlueprintRequirements
{
    public class Environment
    {
        public string Name { get; set; }

        public List<BlueprintCard> Blueprints { get; set; } = new List<BlueprintCard>();
    }
}
