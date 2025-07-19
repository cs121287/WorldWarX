using System;
using System.Collections.Generic;
using System.Windows.Media;
using System.Windows.Media.Imaging;

namespace WorldWarX.Models
{
    public class Unit
    {
        // Core properties
        public UnitType UnitType { get; set; }
        public string Name { get; set; }
        public int Health { get; private set; } = 100; // 1-100
        public int MovementRange { get; set; }
        public int AttackRange { get; set; }
        public int AttackPower { get; set; }
        public int Defense { get; set; }
        public int Cost { get; set; }
        public MovementType MovementType { get; set; }

        // Fuel system
        public int Fuel { get; private set; }
        public int MaxFuel { get; set; }
        public int FuelConsumptionPerTurn { get; set; }

        // Position on map
        public int X { get; set; }
        public int Y { get; set; }

        // Owner
        public Player Owner { get; set; }

        // Status properties
        public bool HasMoved { get; set; }
        public bool HasAttacked { get; set; }
        public bool IsCapturing { get; set; }

        // Transport properties
        public bool CanTransport { get; set; }
        public List<Unit> TransportedUnits { get; private set; } = new List<Unit>();
        public int TransportCapacity { get; set; }
        public List<UnitType> TransportableUnitTypes { get; private set; } = new List<UnitType>();

        // Supply properties
        public bool CanResupply { get; set; }
        public int SupplyRange { get; set; } = 0; // 0 means adjacent only

        // Visual properties
        public ImageSource UnitImage { get; set; }

        public Unit(UnitType unitType, Player owner)
        {
            UnitType = unitType;
            Owner = owner;
            TransportedUnits = new List<Unit>();
            TransportableUnitTypes = new List<UnitType>();

            // Set default properties based on unit type
            SetUnitProperties();

            // Initialize fuel to max
            Fuel = MaxFuel;
        }

        private void SetUnitProperties()
        {
            switch (UnitType)
            {
                // LAND UNITS

                case UnitType.Infantry:
                    Name = "Infantry";
                    MovementRange = 3;
                    AttackRange = 1;
                    AttackPower = 10;
                    Defense = 10;
                    Cost = 100;
                    MovementType = MovementType.Infantry;
                    MaxFuel = 99; // Infantry has practically unlimited fuel
                    FuelConsumptionPerTurn = 0; // No fuel consumption
                    CanTransport = false;
                    UnitImage = LoadUnitImage("infantry");
                    break;

                case UnitType.Mechanized:
                    Name = "Mech";
                    MovementRange = 2;
                    AttackRange = 1;
                    AttackPower = 15;
                    Defense = 15;
                    Cost = 300;
                    MovementType = MovementType.Infantry;
                    MaxFuel = 70; // Mech infantry has equipment that needs fuel
                    FuelConsumptionPerTurn = 1; // Low consumption
                    CanTransport = false;
                    UnitImage = LoadUnitImage("mech");
                    break;

                case UnitType.Tank:
                    Name = "Tank";
                    MovementRange = 5;
                    AttackRange = 1;
                    AttackPower = 25;
                    Defense = 20;
                    Cost = 700;
                    MovementType = MovementType.Treaded;
                    MaxFuel = 60;
                    FuelConsumptionPerTurn = 2;
                    CanTransport = false;
                    UnitImage = LoadUnitImage("tank");
                    break;

                case UnitType.HeavyTank:
                    Name = "Heavy Tank";
                    MovementRange = 4;
                    AttackRange = 1;
                    AttackPower = 40;
                    Defense = 30;
                    Cost = 1100;
                    MovementType = MovementType.Treaded;
                    MaxFuel = 50;
                    FuelConsumptionPerTurn = 3;
                    CanTransport = false;
                    UnitImage = LoadUnitImage("heavy_tank");
                    break;

                case UnitType.Artillery:
                    Name = "Artillery";
                    MovementRange = 3;
                    AttackRange = 3;
                    AttackPower = 30;
                    Defense = 5;
                    Cost = 600;
                    MovementType = MovementType.Wheeled;
                    MaxFuel = 50;
                    FuelConsumptionPerTurn = 1;
                    CanTransport = false;
                    UnitImage = LoadUnitImage("artillery");
                    break;

                case UnitType.RocketLauncher:
                    Name = "Rockets";
                    MovementRange = 2;
                    AttackRange = 5;
                    AttackPower = 40;
                    Defense = 5;
                    Cost = 900;
                    MovementType = MovementType.Wheeled;
                    MaxFuel = 40;
                    FuelConsumptionPerTurn = 1;
                    CanTransport = false;
                    UnitImage = LoadUnitImage("rocket");
                    break;

                case UnitType.AntiAir:
                    Name = "Anti-Air";
                    MovementRange = 4;
                    AttackRange = 2;
                    AttackPower = 20;
                    Defense = 15;
                    Cost = 800;
                    MovementType = MovementType.Treaded;
                    MaxFuel = 50;
                    FuelConsumptionPerTurn = 2;
                    CanTransport = false;
                    UnitImage = LoadUnitImage("antiair");
                    break;

                case UnitType.TransportVehicle:
                    Name = "APC";
                    MovementRange = 6;
                    AttackRange = 0; // Cannot attack
                    AttackPower = 0;
                    Defense = 10;
                    Cost = 500;
                    MovementType = MovementType.Treaded;
                    MaxFuel = 70;
                    FuelConsumptionPerTurn = 1;
                    CanTransport = true;
                    TransportCapacity = 1;
                    TransportableUnitTypes.Add(UnitType.Infantry);
                    TransportableUnitTypes.Add(UnitType.Mechanized);
                    UnitImage = LoadUnitImage("apc");
                    break;

                case UnitType.SupplyTruck:
                    Name = "Supply";
                    MovementRange = 5;
                    AttackRange = 0; // Cannot attack
                    AttackPower = 0;
                    Defense = 5;
                    Cost = 600;
                    MovementType = MovementType.Wheeled;
                    MaxFuel = 80;
                    FuelConsumptionPerTurn = 1;
                    CanTransport = false;
                    CanResupply = true;
                    SupplyRange = 1; // Can resupply adjacent units
                    UnitImage = LoadUnitImage("supply");
                    break;

                // AIR UNITS

                case UnitType.Helicopter:
                    Name = "Helicopter";
                    MovementRange = 6;
                    AttackRange = 1;
                    AttackPower = 25;
                    Defense = 10;
                    Cost = 900;
                    MovementType = MovementType.Air;
                    MaxFuel = 40;
                    FuelConsumptionPerTurn = 4;
                    CanTransport = false;
                    UnitImage = LoadUnitImage("helicopter");
                    break;

                case UnitType.Fighter:
                    Name = "Fighter";
                    MovementRange = 7;
                    AttackRange = 1;
                    AttackPower = 30;
                    Defense = 15;
                    Cost = 1000;
                    MovementType = MovementType.Air;
                    MaxFuel = 50;
                    FuelConsumptionPerTurn = 5;
                    CanTransport = false;
                    UnitImage = LoadUnitImage("fighter");
                    break;

                case UnitType.Bomber:
                    Name = "Bomber";
                    MovementRange = 6;
                    AttackRange = 1;
                    AttackPower = 40;
                    Defense = 10;
                    Cost = 1200;
                    MovementType = MovementType.Air;
                    MaxFuel = 40;
                    FuelConsumptionPerTurn = 5;
                    CanTransport = false;
                    UnitImage = LoadUnitImage("bomber");
                    break;

                case UnitType.Stealth:
                    Name = "Stealth";
                    MovementRange = 7;
                    AttackRange = 1;
                    AttackPower = 35;
                    Defense = 20;
                    Cost = 1800;
                    MovementType = MovementType.Air;
                    MaxFuel = 35;
                    FuelConsumptionPerTurn = 6;
                    CanTransport = false;
                    UnitImage = LoadUnitImage("stealth");
                    break;

                case UnitType.TransportHelicopter:
                    Name = "T-Copter";
                    MovementRange = 6;
                    AttackRange = 0; // Cannot attack
                    AttackPower = 0;
                    Defense = 5;
                    Cost = 800;
                    MovementType = MovementType.Air;
                    MaxFuel = 40;
                    FuelConsumptionPerTurn = 4;
                    CanTransport = true;
                    TransportCapacity = 1;
                    TransportableUnitTypes.Add(UnitType.Infantry);
                    TransportableUnitTypes.Add(UnitType.Mechanized);
                    UnitImage = LoadUnitImage("t_copter");
                    break;

                // NAVAL UNITS

                case UnitType.Battleship:
                    Name = "Battleship";
                    MovementRange = 5;
                    AttackRange = 4;
                    AttackPower = 35;
                    Defense = 25;
                    Cost = 1500;
                    MovementType = MovementType.Ship;
                    MaxFuel = 60;
                    FuelConsumptionPerTurn = 2;
                    CanTransport = false;
                    UnitImage = LoadUnitImage("battleship");
                    break;

                case UnitType.Cruiser:
                    Name = "Cruiser";
                    MovementRange = 6;
                    AttackRange = 2;
                    AttackPower = 25;
                    Defense = 15;
                    Cost = 900;
                    MovementType = MovementType.Ship;
                    MaxFuel = 70;
                    FuelConsumptionPerTurn = 2;
                    CanTransport = false;
                    UnitImage = LoadUnitImage("cruiser");
                    break;

                case UnitType.Submarine:
                    Name = "Submarine";
                    MovementRange = 5;
                    AttackRange = 2;
                    AttackPower = 30;
                    Defense = 10;
                    Cost = 1200;
                    MovementType = MovementType.Ship;
                    MaxFuel = 50;
                    FuelConsumptionPerTurn = 3;
                    CanTransport = false;
                    UnitImage = LoadUnitImage("submarine");
                    break;

                case UnitType.NavalTransport:
                    Name = "Lander";
                    MovementRange = 4;
                    AttackRange = 0; // Cannot attack
                    AttackPower = 0;
                    Defense = 10;
                    Cost = 700;
                    MovementType = MovementType.Lander;
                    MaxFuel = 60;
                    FuelConsumptionPerTurn = 1;
                    CanTransport = true;
                    TransportCapacity = 2;
                    TransportableUnitTypes.Add(UnitType.Infantry);
                    TransportableUnitTypes.Add(UnitType.Mechanized);
                    TransportableUnitTypes.Add(UnitType.Tank);
                    TransportableUnitTypes.Add(UnitType.Artillery);
                    TransportableUnitTypes.Add(UnitType.AntiAir);
                    TransportableUnitTypes.Add(UnitType.TransportVehicle);
                    TransportableUnitTypes.Add(UnitType.SupplyTruck);
                    UnitImage = LoadUnitImage("lander");
                    break;

                case UnitType.Carrier:
                    Name = "Carrier";
                    MovementRange = 4;
                    AttackRange = 0; // Cannot directly attack
                    AttackPower = 0;
                    Defense = 20;
                    Cost = 2000;
                    MovementType = MovementType.Ship;
                    MaxFuel = 70;
                    FuelConsumptionPerTurn = 2;
                    CanTransport = true;
                    TransportCapacity = 2;
                    TransportableUnitTypes.Add(UnitType.Fighter);
                    TransportableUnitTypes.Add(UnitType.Bomber);
                    TransportableUnitTypes.Add(UnitType.Helicopter);
                    TransportableUnitTypes.Add(UnitType.TransportHelicopter);
                    UnitImage = LoadUnitImage("carrier");
                    break;
            }
        }

        private ImageSource LoadUnitImage(string unitName)
        {
            string colorCode = Owner?.ColorCode ?? "red";
            string path = $"pack://application:,,,/Assets/Units/{colorCode}_{unitName}.png";
            return new BitmapImage(new System.Uri(path));
        }

        // Take damage
        public bool TakeDamage(int amount)
        {
            Health -= amount;
            if (Health <= 0)
            {
                Health = 0;
                return true; // Unit is destroyed
            }
            return false;
        }

        // Heal unit
        public void Heal(int amount)
        {
            Health += amount;
            if (Health > 100)
                Health = 100;
        }

        // Resupply fuel
        public void ResupplyFuel(int amount)
        {
            Fuel += amount;
            if (Fuel > MaxFuel)
                Fuel = MaxFuel;
        }

        // Fully resupply fuel
        public void FullResupply()
        {
            Fuel = MaxFuel;
        }

        // Consume fuel per turn
        public bool ConsumeFuel()
        {
            Fuel -= FuelConsumptionPerTurn;
            if (Fuel <= 0)
            {
                Fuel = 0;
                // If fuel reaches 0, unit takes damage
                TakeDamage(10); // 10% damage per turn with no fuel
                return false;
            }
            return true;
        }

        // Check if unit has enough fuel to move
        public bool HasFuelToMove()
        {
            return Fuel > 0;
        }

        // Load unit into transport
        public bool LoadUnit(Unit unit)
        {
            if (!CanTransport || TransportedUnits.Count >= TransportCapacity)
                return false;

            if (!TransportableUnitTypes.Contains(unit.UnitType))
                return false;

            TransportedUnits.Add(unit);
            return true;
        }

        // Unload unit at specific coordinates
        public Unit UnloadUnit(int index, int x, int y)
        {
            if (!CanTransport || index < 0 || index >= TransportedUnits.Count)
                return null;

            Unit unloadedUnit = TransportedUnits[index];
            unloadedUnit.X = x;
            unloadedUnit.Y = y;
            unloadedUnit.HasMoved = true; // Unit can't move after being unloaded

            TransportedUnits.RemoveAt(index);
            return unloadedUnit;
        }

        // Check if transport can unload at specific coordinates
        public bool CanUnloadAt(int x, int y, GameMap gameMap)
        {
            // Check if coordinates are within map bounds
            if (x < 0 || x >= gameMap.Width || y < 0 || y >= gameMap.Height)
                return false;

            // Check if destination is adjacent to transport
            int distance = Math.Abs(X - x) + Math.Abs(Y - y);
            if (distance != 1)
                return false;

            // Check if tile is valid for unloaded units
            Tile tile = gameMap.GetTile(x, y);
            if (tile == null || tile.OccupyingUnit != null)
                return false;

            // Check if terrain is suitable based on first transported unit's movement type
            if (TransportedUnits.Count > 0)
            {
                Unit firstUnit = TransportedUnits[0];
                switch (firstUnit.MovementType)
                {
                    case MovementType.Infantry:
                    case MovementType.Wheeled:
                    case MovementType.Treaded:
                        // Land units can't be unloaded on water
                        if (tile.TerrainType == TerrainType.Sea)
                            return false;
                        break;

                    case MovementType.Ship:
                        // Naval units can only be unloaded on water
                        if (tile.TerrainType != TerrainType.Sea && tile.TerrainType != TerrainType.Beach)
                            return false;
                        break;
                }
            }

            return true;
        }

        // Reset unit for new turn
        public void ResetForNewTurn()
        {
            HasMoved = false;
            HasAttacked = false;
            IsCapturing = false;

            // Consume fuel for this turn
            ConsumeFuel();
        }

        // Calculate damage against a specific unit type
        public int CalculateDamageAgainst(Unit target, Tile targetTile)
        {
            if (target == null)
                return 0;

            // Base damage calculation
            float baseDamage = AttackPower;

            // Apply terrain defense bonus
            float terrainDefenseMultiplier = 1.0f - (targetTile.DefenseBonus / 100.0f);

            // Apply target's defense
            float defenseMultiplier = 100.0f / (100.0f + target.Defense);

            // Apply health percentage (damaged units deal less damage)
            float healthMultiplier = Health / 100.0f;

            // Apply weakness/strength modifiers based on unit types
            float typeMatchupMultiplier = GetTypeMatchupMultiplier(target.UnitType);

            // Apply country bonuses
            float countryBonusMultiplier = 1.0f;
            if (Owner != null && Owner.Country != null)
            {
                if (Owner.Country.UnitBonus.TryGetValue(UnitType, out float bonus))
                    countryBonusMultiplier += bonus;
            }

            // Apply fuel penalty (if low on fuel)
            float fuelMultiplier = (Fuel < MaxFuel * 0.2f) ? 0.8f : 1.0f;

            // Calculate final damage
            int finalDamage = (int)(baseDamage * terrainDefenseMultiplier * defenseMultiplier *
                                    healthMultiplier * typeMatchupMultiplier * countryBonusMultiplier *
                                    fuelMultiplier);

            return finalDamage;
        }

        // Get damage multiplier based on unit type matchups
        private float GetTypeMatchupMultiplier(UnitType targetType)
        {
            // Define type advantages (e.g., infantry is weak against tanks)
            switch (UnitType)
            {
                case UnitType.Infantry:
                    if (targetType == UnitType.Infantry) return 1.0f;
                    if (targetType == UnitType.Mechanized) return 0.8f;
                    if (targetType == UnitType.Tank) return 0.2f;
                    if (targetType == UnitType.HeavyTank) return 0.1f;
                    break;

                case UnitType.Mechanized:
                    if (targetType == UnitType.Infantry) return 1.2f;
                    if (targetType == UnitType.Tank) return 0.5f;
                    if (targetType == UnitType.HeavyTank) return 0.3f;
                    break;

                case UnitType.Tank:
                    if (targetType == UnitType.Infantry) return 1.5f;
                    if (targetType == UnitType.Mechanized) return 1.3f;
                    if (targetType == UnitType.Tank) return 1.0f;
                    if (targetType == UnitType.HeavyTank) return 0.7f;
                    if (targetType == UnitType.Artillery) return 1.2f;
                    if (targetType == UnitType.RocketLauncher) return 1.2f;
                    if (targetType == UnitType.TransportVehicle) return 1.3f;
                    if (targetType == UnitType.SupplyTruck) return 1.3f;
                    break;

                case UnitType.HeavyTank:
                    if (targetType == UnitType.Infantry) return 1.8f;
                    if (targetType == UnitType.Mechanized) return 1.5f;
                    if (targetType == UnitType.Tank) return 1.3f;
                    if (targetType == UnitType.HeavyTank) return 1.0f;
                    if (targetType == UnitType.Artillery) return 1.4f;
                    if (targetType == UnitType.RocketLauncher) return 1.4f;
                    if (targetType == UnitType.TransportVehicle) return 1.5f;
                    if (targetType == UnitType.SupplyTruck) return 1.5f;
                    break;

                case UnitType.Artillery:
                    if (targetType == UnitType.Infantry) return 1.2f;
                    if (targetType == UnitType.Mechanized) return 1.2f;
                    if (targetType == UnitType.Tank) return 1.0f;
                    if (targetType == UnitType.HeavyTank) return 0.8f;
                    break;

                case UnitType.RocketLauncher:
                    if (targetType == UnitType.Infantry) return 1.3f;
                    if (targetType == UnitType.Mechanized) return 1.3f;
                    if (targetType == UnitType.Tank) return 1.1f;
                    if (targetType == UnitType.HeavyTank) return 0.9f;
                    break;

                case UnitType.AntiAir:
                    if (targetType == UnitType.Helicopter) return 2.0f;
                    if (targetType == UnitType.TransportHelicopter) return 2.0f;
                    if (targetType == UnitType.Fighter) return 0.8f;
                    if (targetType == UnitType.Bomber) return 1.2f;
                    if (targetType == UnitType.Stealth) return 0.6f;
                    break;

                case UnitType.Helicopter:
                    if (targetType == UnitType.Infantry) return 1.2f;
                    if (targetType == UnitType.Mechanized) return 1.2f;
                    if (targetType == UnitType.Tank) return 1.0f;
                    if (targetType == UnitType.HeavyTank) return 0.8f;
                    if (targetType == UnitType.AntiAir) return 0.2f;
                    break;

                case UnitType.Fighter:
                    if (targetType == UnitType.Helicopter) return 2.0f;
                    if (targetType == UnitType.TransportHelicopter) return 2.0f;
                    if (targetType == UnitType.Bomber) return 1.5f;
                    if (targetType == UnitType.Fighter) return 1.0f;
                    if (targetType == UnitType.Stealth) return 0.8f;
                    break;

                case UnitType.Bomber:
                    if (targetType == UnitType.Tank) return 1.5f;
                    if (targetType == UnitType.HeavyTank) return 1.3f;
                    if (targetType == UnitType.Artillery) return 1.5f;
                    if (targetType == UnitType.RocketLauncher) return 1.5f;
                    if (targetType == UnitType.NavalTransport) return 1.5f;
                    if (targetType == UnitType.Battleship) return 0.8f;
                    if (targetType == UnitType.Cruiser) return 1.2f;
                    break;

                case UnitType.Stealth:
                    if (targetType == UnitType.Fighter) return 1.2f;
                    if (targetType == UnitType.Bomber) return 1.5f;
                    if (targetType == UnitType.Stealth) return 1.0f;
                    if (targetType == UnitType.AntiAir) return 1.3f;
                    break;

                case UnitType.Battleship:
                    if (targetType == UnitType.Battleship) return 1.0f;
                    if (targetType == UnitType.Cruiser) return 1.2f;
                    if (targetType == UnitType.Submarine) return 0.5f; // Weak against subs
                    break;

                case UnitType.Cruiser:
                    if (targetType == UnitType.Submarine) return 1.8f; // Strong against subs
                    if (targetType == UnitType.Battleship) return 0.8f;
                    if (targetType == UnitType.NavalTransport) return 1.3f;
                    break;

                case UnitType.Submarine:
                    if (targetType == UnitType.Battleship) return 1.5f; // Strong against battleships
                    if (targetType == UnitType.Cruiser) return 0.7f; // Weak against cruisers
                    if (targetType == UnitType.NavalTransport) return 1.5f;
                    if (targetType == UnitType.Carrier) return 1.4f;
                    if (targetType == UnitType.Submarine) return 1.0f;
                    break;
            }

            // Default multiplier for no specific advantage/disadvantage
            return 1.0f;
        }

        public string GetFuelStatusText()
        {
            return $"{Fuel}/{MaxFuel}";
        }

        public float GetFuelPercentage()
        {
            return (float)Fuel / MaxFuel;
        }
    }
}