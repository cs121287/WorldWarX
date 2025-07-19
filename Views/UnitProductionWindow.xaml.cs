using System;
using System.Collections.Generic;
using System.Windows;
using System.Windows.Controls;
using WorldWarX.Models;

namespace WorldWarX.Views
{
    public partial class UnitProductionWindow : Window
    {
        private Player _player;
        private Tile _factory;
        private Dictionary<UnitType, int> _unitCosts = new Dictionary<UnitType, int>();

        public Unit ProducedUnit { get; private set; }

        public UnitProductionWindow(Player player, Tile factory)
        {
            InitializeComponent();

            _player = player;
            _factory = factory;

            InitializeUnitCosts();
            LoadUnitList();
            UpdateFundsDisplay();
        }

        private void InitializeUnitCosts()
        {
            // Set up unit costs
            _unitCosts[UnitType.Infantry] = 100;
            _unitCosts[UnitType.Mechanized] = 300;
            _unitCosts[UnitType.Tank] = 700;
            _unitCosts[UnitType.HeavyTank] = 1100;
            _unitCosts[UnitType.Artillery] = 600;
            _unitCosts[UnitType.RocketLauncher] = 900;
            _unitCosts[UnitType.AntiAir] = 800;
            _unitCosts[UnitType.TransportVehicle] = 500;
            _unitCosts[UnitType.SupplyTruck] = 600;
            _unitCosts[UnitType.Helicopter] = 900;
            _unitCosts[UnitType.Fighter] = 1000;
            _unitCosts[UnitType.Bomber] = 1200;
            _unitCosts[UnitType.Stealth] = 1800;
            _unitCosts[UnitType.TransportHelicopter] = 800;
            _unitCosts[UnitType.Battleship] = 1500;
            _unitCosts[UnitType.Cruiser] = 900;
            _unitCosts[UnitType.Submarine] = 1200;
            _unitCosts[UnitType.NavalTransport] = 700;
            _unitCosts[UnitType.Carrier] = 2000;
        }

        private void LoadUnitList()
        {
            List<UnitListItem> units = new List<UnitListItem>();

            // Determine which units can be built based on terrain
            bool isAirport = _factory.TerrainType == TerrainType.Airport;
            bool isSeaport = _factory.TerrainType == TerrainType.Seaport;
            bool isLandFactory = !isAirport && !isSeaport;

            // Add land units (from factory)
            if (isLandFactory)
            {
                AddUnitToList(units, UnitType.Infantry);
                AddUnitToList(units, UnitType.Mechanized);
                AddUnitToList(units, UnitType.Tank);
                AddUnitToList(units, UnitType.HeavyTank);
                AddUnitToList(units, UnitType.Artillery);
                AddUnitToList(units, UnitType.RocketLauncher);
                AddUnitToList(units, UnitType.AntiAir);
                AddUnitToList(units, UnitType.TransportVehicle);
                AddUnitToList(units, UnitType.SupplyTruck);
            }

            // Add air units (from airport)
            if (isAirport)
            {
                AddUnitToList(units, UnitType.Helicopter);
                AddUnitToList(units, UnitType.Fighter);
                AddUnitToList(units, UnitType.Bomber);
                AddUnitToList(units, UnitType.Stealth);
                AddUnitToList(units, UnitType.TransportHelicopter);
            }

            // Add naval units (from seaport)
            if (isSeaport)
            {
                AddUnitToList(units, UnitType.Battleship);
                AddUnitToList(units, UnitType.Cruiser);
                AddUnitToList(units, UnitType.Submarine);
                AddUnitToList(units, UnitType.NavalTransport);
                AddUnitToList(units, UnitType.Carrier);
            }

            // Set as the ItemsSource for our ListView
            UnitsListView.ItemsSource = units;
        }

        private void AddUnitToList(List<UnitListItem> units, UnitType unitType)
        {
            // Create temp unit to get info
            Unit tempUnit = new Unit(unitType, _player);

            UnitListItem item = new UnitListItem
            {
                UnitType = unitType,
                Name = tempUnit.Name,
                Cost = _unitCosts[unitType],
                Image = tempUnit.UnitImage,
                CanAfford = _player.Funds >= _unitCosts[unitType],
                FuelInfo = $"Fuel: {tempUnit.MaxFuel}"
            };

            units.Add(item);
        }

        private void UpdateFundsDisplay()
        {
            FundsText.Text = $"Available Funds: {_player.Funds}G";
        }

        private void UnitsListView_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (UnitsListView.SelectedItem != null)
            {
                UnitListItem selectedItem = (UnitListItem)UnitsListView.SelectedItem;

                // Update unit details
                UnitNameText.Text = selectedItem.Name;
                UnitCostText.Text = $"{selectedItem.Cost}G";
                UnitImage.Source = selectedItem.Image;

                // Create temp unit to get stats
                Unit tempUnit = new Unit(selectedItem.UnitType, _player);
                UnitMovementText.Text = tempUnit.MovementRange.ToString();
                UnitAttackText.Text = tempUnit.AttackPower.ToString();
                UnitDefenseText.Text = tempUnit.Defense.ToString();
                UnitRangeText.Text = tempUnit.AttackRange.ToString();

                // Show fuel info
                string fuelInfo = $"Max Fuel: {tempUnit.MaxFuel}\nFuel Usage: {tempUnit.FuelConsumptionPerTurn}/turn";
                UnitDescriptionText.Text = fuelInfo;

                // Show transport capacity if applicable
                if (tempUnit.CanTransport)
                {
                    string transportInfo = $"\nTransport Capacity: {tempUnit.TransportCapacity}";
                    string transportTypes = "Can transport: " + string.Join(", ", tempUnit.TransportableUnitTypes);
                    UnitDescriptionText.Text += transportInfo + "\n" + transportTypes;
                }

                // Enable/disable build button based on funds
                BtnBuild.IsEnabled = selectedItem.CanAfford;
            }
            else
            {
                BtnBuild.IsEnabled = false;
            }
        }

        private void BtnBuild_Click(object sender, RoutedEventArgs e)
        {
            if (UnitsListView.SelectedItem == null)
                return;

            UnitListItem selectedItem = (UnitListItem)UnitsListView.SelectedItem;

            // Check if player has enough funds
            if (_player.Funds < selectedItem.Cost)
            {
                MessageBox.Show("Not enough funds to build this unit!", "Insufficient Funds", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            // Deduct cost
            _player.Funds -= selectedItem.Cost;

            // Create the unit
            Unit newUnit = new Unit(selectedItem.UnitType, _player);
            newUnit.X = _factory.X;
            newUnit.Y = _factory.Y;
            newUnit.HasMoved = true; // Unit can't move on the turn it's built

            // Return the created unit
            ProducedUnit = newUnit;

            // Close the window
            DialogResult = true;
            Close();
        }

        private void BtnCancel_Click(object sender, RoutedEventArgs e)
        {
            DialogResult = false;
            Close();
        }
    }

    public class UnitListItem
    {
        public UnitType UnitType { get; set; }
        public string Name { get; set; }
        public int Cost { get; set; }
        public System.Windows.Media.ImageSource Image { get; set; }
        public bool CanAfford { get; set; }
        public string FuelInfo { get; set; }
    }
}