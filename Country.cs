using System.Collections.Generic;
using System.Windows.Media;

namespace WorldWarX
{
    public class Country
    {
        public string Name { get; set; }
        public string Description { get; set; }
        public string FlagImagePath { get; set; }
        public Color PrimaryColor { get; set; }
        public Color SecondaryColor { get; set; }

        // Country Power attributes
        public string PowerName { get; set; }
        public string PowerDescription { get; set; }
        public int PowerChargeRate { get; set; } // How fast the power meter fills

        // Bonuses that this country provides
        public Dictionary<UnitType, float> UnitBonus { get; set; }
        public Dictionary<TerrainType, float> TerrainBonus { get; set; }
        public float EconomyBonus { get; set; }

        public Country()
        {
            Name = "";
            Description = "";
            FlagImagePath = "";
            PowerName = "";
            PowerDescription = "";
            UnitBonus = new Dictionary<UnitType, float>();
            TerrainBonus = new Dictionary<TerrainType, float>();
        }
    }

    // ...enums unchanged...
    public enum GameMode
    {
        Campaign,
        QuickBattle,
        Tutorial,
        Custom
    }

    public enum UnitType
    {
        // Land Units
        Infantry,
        Mechanized,
        Tank,
        HeavyTank,
        Artillery,
        RocketLauncher,
        AntiAir,
        TransportVehicle,
        SupplyTruck,

        // Air Units
        Helicopter,
        Fighter,
        Bomber,
        Stealth,
        TransportHelicopter,

        // Naval Units
        Battleship,
        Cruiser,
        Submarine,
        NavalTransport,
        Carrier,
        Naval // Legacy value for backward compatibility
    }

    public enum TerrainType
    {
        Plain,
        Forest,
        Mountain,
        Road,
        City,
        Factory,
        Airport,
        Seaport,
        HQ,
        Sea,
        Beach,
        River,
        Bridge
    }

    public enum MovementType
    {
        Infantry,   // Can traverse most land terrain
        Wheeled,    // Fast on roads but slow in rough terrain
        Treaded,    // Good all-round land movement
        Air,        // Can move over any terrain
        Ship,       // Limited to water
        Lander      // Can move on both land and water
    }
}