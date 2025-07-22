using System;
using System.Collections.Generic;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;

namespace WorldWarX.Views
{
    public class CountrySelectedEventArgs(Country country) : EventArgs
    {
        public required Country? SelectedCountry { get; set; } = country;
    }

    public partial class CountrySelectionControl : UserControl
    {
        private List<Country>? _availableCountries;
        private Country? _selectedCountry;

        // Events for navigation
        public event EventHandler? BackRequested;
        public event EventHandler<CountrySelectedEventArgs>? CountrySelected;

        public CountrySelectionControl()
        {
            InitializeComponent();
            LoadCountries();
            PopulateCountryList();
        }

        /// <summary>
        /// Allows an external parent (like QuickBattleControl) to set the available countries programmatically.
        /// This will override the default LoadCountries list.
        /// </summary>
        /// <param name="countries"></param>
        public void SetAvailableCountries(List<Country> countries)
        {
            _availableCountries = countries;
            PopulateCountryList();
        }

        private void LoadCountries()
        {
            // Create sample countries
            _availableCountries = new List<Country>
            {
                new Country
                {
                    Name = "Redonia",
                    Description = "A militaristic nation with superior ground forces and artillery.",
                    FlagImagePath = "/Assets/Flags/redonia_flag.png",
                    PrimaryColor = Colors.DarkRed,
                    SecondaryColor = Colors.Black,
                    PowerName = "Iron Fist",
                    PowerDescription = "Increases attack power of all ground units by 20% for 2 turns.",
                    PowerChargeRate = 3,
                    EconomyBonus = 0.0f,
                    UnitBonus = new Dictionary<UnitType, float> {
                        { UnitType.Tank, 0.15f },
                        { UnitType.Artillery, 0.2f }
                    }
                },
                new Country
                {
                    Name = "Azurea",
                    Description = "A naval superpower with advanced air technology.",
                    FlagImagePath = "/Assets/Flags/azurea_flag.png",
                    PrimaryColor = Colors.DarkBlue,
                    SecondaryColor = Colors.LightBlue,
                    PowerName = "Tidal Wave",
                    PowerDescription = "Naval units gain +2 movement and +10% attack for 2 turns.",
                    PowerChargeRate = 4,
                    EconomyBonus = 0.05f,
                    UnitBonus = new Dictionary<UnitType, float> {
                        { UnitType.Naval, 0.15f },
                        { UnitType.Fighter, 0.1f }
                    }
                },
                new Country
                {
                    Name = "Verdania",
                    Description = "An eco-friendly nation specializing in defensive tactics and economy.",
                    FlagImagePath = "/Assets/Flags/verdania_flag.png",
                    PrimaryColor = Colors.DarkGreen,
                    SecondaryColor = Colors.LightGreen,
                    PowerName = "Prosperity",
                    PowerDescription = "Gain 1000 extra funds and +10% defense to all units for 2 turns.",
                    PowerChargeRate = 3,
                    EconomyBonus = 0.2f,
                    TerrainBonus = new Dictionary<TerrainType, float> {
                        { TerrainType.Forest, 0.2f },
                        { TerrainType.Mountain, 0.15f }
                    }
                },
                new Country
                {
                    Name = "Solaris",
                    Description = "A technologically advanced desert nation with powerful air forces.",
                    FlagImagePath = "/Assets/Flags/solaris_flag.png",
                    PrimaryColor = Colors.Gold,
                    SecondaryColor = Colors.Orange,
                    PowerName = "Solar Strike",
                    PowerDescription = "Air units deal 30% more damage for 2 turns.",
                    PowerChargeRate = 4,
                    EconomyBonus = 0.05f,
                    UnitBonus = new Dictionary<UnitType, float> {
                        { UnitType.Helicopter, 0.15f },
                        { UnitType.Bomber, 0.2f }
                    }
                }
            };
        }

        private void PopulateCountryList()
        {
            CountryListView.ItemsSource = _availableCountries;
        }

        private void CountryListView_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (CountryListView.SelectedItem != null)
            {
                _selectedCountry = (Country)CountryListView.SelectedItem;
                UpdateCountryDetails(_selectedCountry);
                BtnSelectCountry.IsEnabled = true;
            }
        }

        private void UpdateCountryDetails(Country country)
        {
            CountryNameText.Text = country.Name;
            CountryDescriptionText.Text = country.Description;
            PowerNameText.Text = country.PowerName;
            PowerDescriptionText.Text = country.PowerDescription;

            // Update bonuses list
            BonusesList.Items.Clear();

            foreach (var bonus in country.UnitBonus)
            {
                BonusesList.Items.Add($"{bonus.Key}: +{bonus.Value * 100}% Effectiveness");
            }

            foreach (var bonus in country.TerrainBonus)
            {
                BonusesList.Items.Add($"{bonus.Key}: +{bonus.Value * 100}% Defense");
            }

            if (country.EconomyBonus > 0)
            {
                BonusesList.Items.Add($"Economy: +{country.EconomyBonus * 100}% Income");
            }
        }

        private void BtnSelectCountry_Click(object sender, RoutedEventArgs e)
        {
            if (_selectedCountry != null)
            {
                CountrySelected?.Invoke(this, new CountrySelectedEventArgs(_selectedCountry)
                {
                    SelectedCountry = _selectedCountry
                });
            }
        }

        private void BtnBack_Click(object sender, RoutedEventArgs e)
        {
            BackRequested?.Invoke(this, EventArgs.Empty);
        }
    }
}