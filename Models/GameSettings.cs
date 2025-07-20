using System;

namespace WorldWarX.Models
{
    public class GameSettings
    {
        public GameMode GameMode { get; set; }
        public GameDifficulty Difficulty { get; set; }
        public MapSeason Season { get; set; }
        public bool WeatherEffectsEnabled { get; set; }
        public bool FogOfWarEnabled { get; set; } = true; // Added property to fix error
        public int StartingFunds { get; set; }
        public int MapWidth { get; set; }
        public int MapHeight { get; set; }
    }
}