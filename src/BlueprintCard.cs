using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;

namespace SaveBlueprintRequirements
{
    public class BlueprintCard
    {
        public string Name { get; set; }

        public TimeSpan StageTimeRemaining { get; set; }

        public TimeSpan TotalTimeRemaining { get; set; }

        public List<BlueprintResource> Resources { get; set; } = new List<BlueprintResource>();
        public List<BlueprintResource> AllNeededResources { get; set; } = new List<BlueprintResource>();

        public bool IsLastStage { get; set; }
    }
}
