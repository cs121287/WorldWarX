using System.Collections.Generic;
using System.Windows.Media;

namespace WorldWarX.Models
{
    public class Player
    {
        public int PlayerId { get; set; }
        public string? Name { get; set; }
        public Country? Country { get; set; }
        public int Funds { get; set; }
        public Color PlayerColor { get; set; }
        public string? ColorCode { get; set; }
        public bool IsAI { get; set; }

        // List of units owned by this player
        public List<Unit> Units { get; set; }

        // List of properties (cities, factories, HQs) owned by this player
        public List<Tile> Properties { get; set; }

        // Power meter (0-100%)
        public int PowerMeter { get; set; }

        // Is power active
        public bool IsPowerActive { get; set; }
        public int PowerTurnsRemaining { get; set; }

        public Player(int playerId)
        {
            PlayerId = playerId;
            Units = new List<Unit>();
            Properties = new List<Tile>();
            Funds = 5000;
            PowerMeter = 0;
            IsPowerActive = false;

            // Default color and name
            switch (playerId)
            {
                case 0:
                    Name = "Player";
                    PlayerColor = Colors.Red;
                    ColorCode = "red";
                    IsAI = false;
                    break;
                case 1:
                    Name = "Enemy";
                    PlayerColor = Colors.Blue;
                    ColorCode = "blue";
                    IsAI = true;
                    break;
                case 2:
                    Name = "Ally";
                    PlayerColor = Colors.Green;
                    ColorCode = "green";
                    IsAI = true;
                    break;
                case 3:
                    Name = "Neutral";
                    PlayerColor = Colors.Yellow;
                    ColorCode = "yellow";
                    IsAI = true;
                    break;
            }
        }

        // Set player country
        public void SetCountry(Country country)
        {
            Country = country;
        }

        // Add a new unit to this player
        public void AddUnit(Unit unit)
        {
            unit.Owner = this;
            Units.Add(unit);
        }

        // Remove a destroyed unit
        public void RemoveUnit(Unit unit)
        {
            Units.Remove(unit);
        }

        // Claim a property (city, factory, HQ)
        public void ClaimProperty(Tile tile)
        {
            if (tile.Capturable)
            {
                tile.Owner = this;
                Properties.Add(tile);
            }
        }

        // Lose a property
        public void LoseProperty(Tile tile)
        {
            Properties.Remove(tile);
        }

        // Calculate income for this turn
        public int CalculateIncome()
        {
            int income = 0;

            foreach (Tile property in Properties)
            {
                income += property.IncomeValue;
            }

            // Apply country economy bonus if available
            if (Country != null && Country.EconomyBonus > 0)
            {
                income += (int)(income * Country.EconomyBonus);
            }

            return income;
        }

        // Add income to funds
        public void CollectIncome()
        {
            Funds += CalculateIncome();
        }

        // Try to buy a new unit
        public bool TryBuyUnit(UnitType unitType, out Unit? newUnit)
        {
            newUnit = null;

            Unit tempUnit = new Unit(unitType, this);
            int cost = tempUnit.Cost;

            if (Funds >= cost)
            {
                Funds -= cost;
                newUnit = tempUnit;
                Units.Add(newUnit);
                return true;
            }

            return false;
        }

        // Reset all units for a new turn
        public void StartNewTurn()
        {
            foreach (Unit unit in Units)
            {
                unit.ResetForNewTurn();
            }

            // Decrease power duration if active
            if (IsPowerActive && PowerTurnsRemaining > 0)
            {
                PowerTurnsRemaining--;
                if (PowerTurnsRemaining <= 0)
                {
                    IsPowerActive = false;
                }
            }
        }

        // Try to activate country power
        public bool TryActivatePower()
        {
            if (Country == null || PowerMeter < 100 || IsPowerActive)
                return false;

            IsPowerActive = true;
            PowerTurnsRemaining = 2; // Most powers last 2 turns
            PowerMeter = 0;
            return true;
        }

        // Increase power meter from action
        public void AddPowerCharge(int amount)
        {
            if (Country == null || IsPowerActive)
                return;

            // Apply country-specific charge rate
            amount = (int)(amount * Country.PowerChargeRate / 3.0f);

            PowerMeter += amount;
            if (PowerMeter > 100)
                PowerMeter = 100;
        }
    }
}