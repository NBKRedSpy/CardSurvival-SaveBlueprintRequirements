using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SaveBlueprintRequirements
{
    public class BlueprintCard
    {
        public string Name { get; set; }
        public List<BlueprintResource> Resources { get; set; } = new List<BlueprintResource>();
    }
}
