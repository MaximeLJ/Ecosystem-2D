using System.Collections.Generic;

namespace Diseases
{    
    public static class Diseases
    {
        public static Dictionary<string, List<(string, float)>> GeneticDiseases = new Dictionary<string, List<(string, float)>>
        {
            {"Blind", new List<(string, float)> {("raycastDistance", 0f), ("speed", 1.3f)}},
            {"Albinos", new List<(string, float)> {("color", 0f)}},
            {"Dwarfism", new List<(string, float)> {("lifetime", 0.5f), ("speed", 0.5f), ("rotationSpeed", 0.5f), ("endSize", 0.6f), ("matingDuration", 1.3f), ("maxHP", 0.5f)}},
            {"Gigantism", new List<(string, float)> {("lifetime", 0.5f), ("speed", 0.5f), ("rotationSpeed", 0.5f), ("endSize", 1.5f), ("matingDuration", 1.3f), ("maxHP", 2f), ("digestingTime", 2f)}},
            {"SlowMetabolism", new List<(string, float)> {("maxHP", 0.5f), ("digestingTime", 1.5f), ("speed", 0.5f), ("lifetime", 0.5f)}},
            {"FastMetabolism", new List<(string, float)> {("maxHP", 1.5f), ("digestingTime", 0.5f), ("speed", 1.5f), ("lifetime", 0.5f)}},
            {"HighFertility", new List<(string, float)> {("matingDuration", 0.8f), ("pregnancyTime", 0.8f), ("maturingTime", 0.9f)}},
            {"SCID", new List<(string, float)> {("speed", 0.5f), ("digestingTime", 1.5f), ("maxHP", 1.5f), ("rotationSpeed", 0.5f), ("raycastDistance", 0.7f), ("angleRange", 0.7f), ("raycastPrecision", 0.7f), ("matingDuration", 0.8f), ("pregnancyTime", 0.8f), ("maturingTime", 0.8f), ("lifetime", 0.4f), ("color", 0.8f)}}
        };

        public static Dictionary<string, List<(string, float)>> InfectiousDiseases = new Dictionary<string, List<(string, float)>>
        {
            {"WhiteSpot", new List<(string, float)> {
                ("speed", 0.9f),  // Increased speed
                ("rotationSpeed", 0.9f),  // Increased rotation speed
                ("raycastDistance", 0.85f),  // Altered interaction with environment
                ("angleRange", 0.85f),  // Altered interaction with environment
                ("raycastPrecision", 0.85f),  // Altered interaction with environment
                ("digestingTime", 0.8f),  // Reduced appetite and weakened digestion
                ("maxHP", 0.8f),  // Reduced appetite
                ("matingDuration", 0.85f),  // Indirect impact on reproductive processes
                ("pregnancyTime", 0.85f),  // Indirect impact on reproductive processes
                ("maturingTime", 0.85f),  // Indirect impact on maturation processes
            }},
            {"Salmonella", new List<(string, float)> {
                ("speed", 0.6f),
                ("digestingTime", 0.75f),
                ("maxHP", 0.8f),
                ("rotationSpeed", 0.7f),
                ("raycastDistance", 0.8f),
                ("angleRange", 0.85f),
                ("raycastPrecision", 0.9f),
                ("matingDuration", 0.85f),
                ("pregnancyTime", 0.85f),
                ("maturingTime", 0.85f),
                ("size", 0.9f)
            }},
            {"FinRot", new List<(string, float)> {
                ("rotationSpeed", 0.7f),  // Decreased rotation speed
                ("matingDuration", 1.2f),  // Prolonged mating duration
                ("maxHP", 1.1f)  // Increased appetite due to stress
            }},
            {"Ichthyobesity", new List<(string, float)> {
                ("speed", 0.75f),  // Reduced swimming speed
                ("digestingTime", 1.5f),  // Prolonged digestion time
                ("size", 1.2f)  // Increased size due to obesity
            }},
            {"ScaleInfection", new List<(string, float)> {
                ("speed", 0.85f),  // Reduced swimming speed
                ("rotationSpeed", 0.9f),  // Slightly decreased rotation speed
            }},
            {"ToxicFood", new List<(string, float)> {
                ("speed", 0.7f),  // Reduced swimming speed
                ("digestingTime", 1.4f),  // Prolonged digestion time due to toxic exposure
                ("size", 0.8f)  // Reduced size due to toxic effects
            }},
            {"ParasiticInfestation", new List<(string, float)> {
                ("speed", 0.8f),  // Reduced swimming speed due to parasites
                ("maxHP", 1.3f),  // Increased appetite from the energy drain caused by parasites
                ("digestingTime", 1.2f),  // Prolonged digestion time due to weakened digestive system
            }}    
        };
    }
}