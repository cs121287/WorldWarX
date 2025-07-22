using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using WorldWarX.Models;

namespace WorldWarX.Views
{
    public class BattleStartEventArgs : EventArgs
    {
        public Map? Map { get; set; }
        public Country? PlayerCountry { get; set; }
        public Country? OpponentCountry { get; set; }
        public GameSettings? GameSettings { get; set; }

        public BattleStartEventArgs(Map map, Country playerCountry, Country opponentCountry, GameSettings gameSettings)
        {
            Map = map;
            PlayerCountry = playerCountry;
            OpponentCountry = opponentCountry;
            GameSettings = gameSettings;
        }
    }

    public partial class QuickBattleControl : UserControl
    {
        public event EventHandler? BackRequested;
        public event EventHandler<BattleStartEventArgs>? BattleStartRequested;

        private List<Map> _availableMaps = new List<Map>();
        private Map? _selectedMap;

        private List<Country> _availableCountries = new List<Country>();
        private Country? _selectedPlayerCountry;
        private Country? _selectedOpponentCountry;

        // Track which step is currently active
        private enum Step
        {
            CountrySelect,
            MapSelect,
            Settings
        }
        private Step _currentStep = Step.CountrySelect;

        public QuickBattleControl()
        {
            InitializeComponent();
            LoadCountries();
            LoadMaps();
            SetupControls();
            UpdateUI();

            CountrySelectionPanel.CountrySelected += CountrySelectionPanel_CountrySelected;

            UpdateButtonStates();
        }

        private void LoadCountries()
        {
            _availableCountries = new List<Country>
            {
                new Country
                {
                    Name = "Redonia",
                    Description = "A militaristic nation with superior ground forces and artillery.",
                    FlagImagePath = "/Assets/Flags/redonia_flag.png",
                    PrimaryColor = System.Windows.Media.Colors.DarkRed,
                    SecondaryColor = System.Windows.Media.Colors.Black,
                    PowerName = "Iron Fist",
                    PowerDescription = "Increases attack power of all ground units by 20% for 2 turns.",
                },
                new Country
                {
                    Name = "Azurea",
                    Description = "A naval superpower with advanced air technology.",
                    FlagImagePath = "/Assets/Flags/azurea_flag.png",
                    PrimaryColor = System.Windows.Media.Colors.DarkBlue,
                    SecondaryColor = System.Windows.Media.Colors.LightBlue,
                    PowerName = "Tidal Wave",
                    PowerDescription = "Naval units gain +2 movement and +10% attack for 2 turns.",
                },
                new Country
                {
                    Name = "Verdania",
                    Description = "An eco-friendly nation specializing in defensive tactics and economy.",
                    FlagImagePath = "/Assets/Flags/verdania_flag.png",
                    PrimaryColor = System.Windows.Media.Colors.DarkGreen,
                    SecondaryColor = System.Windows.Media.Colors.LightGreen,
                    PowerName = "Prosperity",
                    PowerDescription = "Gain 1000 extra funds and +10% defense to all units for 2 turns.",
                },
                new Country
                {
                    Name = "Solaris",
                    Description = "A technologically advanced desert nation with powerful air forces.",
                    FlagImagePath = "/Assets/Flags/solaris_flag.png",
                    PrimaryColor = System.Windows.Media.Colors.Gold,
                    SecondaryColor = System.Windows.Media.Colors.Orange,
                    PowerName = "Solar Strike",
                    PowerDescription = "Air units deal 30% more damage for 2 turns.",
                }
            };
            CountrySelectionPanel.SetAvailableCountries(_availableCountries);
        }

        private void LoadMaps()
        {
            _availableMaps = new List<Map>
            {
                new Map
                {
                    Name = "Test Map",
                    Description = "A small balanced map for testing",
                    Width = 12,
                    Height = 10,
                    PreviewImagePath = "/Assets/Maps/testmap_preview.png"
                },
                new Map
                {
                    Name = "Island Assault",
                    Description = "A strategic map with islands and bridges.",
                    Width = 16,
                    Height = 12,
                    PreviewImagePath = "/Assets/Maps/islandassault_preview.png"
                },
                new Map
                {
                    Name = "Urban Clash",
                    Description = "Fight for control over a city with many factories.",
                    Width = 14,
                    Height = 10,
                    PreviewImagePath = "/Assets/Maps/urbanclash_preview.png"
                }
            };
        }

        private void SetupControls()
        {
            MapListView.ItemsSource = _availableMaps;
            MapListView.SelectionChanged += MapListView_SelectionChanged;
            MapListView.SelectedIndex = 0;
            _selectedMap = _availableMaps.Count > 0 ? _availableMaps[0] : null;
            _selectedPlayerCountry = null;
            _selectedOpponentCountry = null;

            CountryStepPanel.Visibility = Visibility.Visible;
            MapStepPanel.Visibility = Visibility.Collapsed;
            SettingsStepPanel.Visibility = Visibility.Collapsed;
            _currentStep = Step.CountrySelect;
            UpdateButtonStates();
            UpdateSummaryPanel();
        }

        private void UpdateUI()
        {
            // Map info
            if (_selectedMap != null)
            {
                MapNameText.Text = _selectedMap.Name;
                MapDescriptionText.Text = _selectedMap.Description;
                MapPreviewImage.Source = !string.IsNullOrEmpty(_selectedMap.PreviewImagePath)
                    ? new System.Windows.Media.Imaging.BitmapImage(new Uri(_selectedMap.PreviewImagePath, UriKind.RelativeOrAbsolute))
                    : null;
            }
            else
            {
                MapNameText.Text = "";
                MapDescriptionText.Text = "";
                MapPreviewImage.Source = null;
            }

            // Country details panel update
            if (_selectedPlayerCountry != null)
            {
                CountryDetailsText.Text =
                    $"{_selectedPlayerCountry.Name}\n\n{_selectedPlayerCountry.Description}\n\nPower: {_selectedPlayerCountry.PowerName}\n{_selectedPlayerCountry.PowerDescription}";
            }
            else
            {
                CountryDetailsText.Text = "Select your country to see its details, strengths, and bonuses.";
            }

            UpdateButtonStates();
            UpdateSummaryPanel();
        }

        private void UpdateSummaryPanel()
        {
            // Only update if summary fields exist (SettingsStepPanel only)
            if (SummaryPlayerCountryText == null) return;

            SummaryPlayerCountryText.Text = _selectedPlayerCountry != null ? _selectedPlayerCountry.Name : "[Not Selected]";
            SummaryOpponentCountryText.Text = _selectedOpponentCountry != null ? _selectedOpponentCountry.Name : "[Auto/Random]";
            SummaryMapText.Text = _selectedMap != null ? _selectedMap.Name : "[Not Selected]";
            SummaryMapSizeText.Text = _selectedMap != null ? $"{_selectedMap.Width} x {_selectedMap.Height}" : "";

            // Difficulty
            string difficulty = "Medium";
            switch (DifficultyComboBox.SelectedIndex)
            {
                case 0: difficulty = "Easy"; break;
                case 1: difficulty = "Medium"; break;
                case 2: difficulty = "Hard"; break;
                case 3: difficulty = "Expert"; break;
            }
            SummaryDifficultyText.Text = difficulty;

            // Season
            SummarySeasonText.Text = SummerRadioButton.IsChecked == true ? "Summer" : "Winter";

            // Weather Effects
            SummaryWeatherEffectsText.Text = WeatherEffectsCheckBox.IsChecked == true ? "Enabled" : "Disabled";

            // Fog of War
            SummaryFogOfWarText.Text = FogOfWarCheckBox.IsChecked == true ? "Enabled" : "Disabled";

            // Starting Funds
            string funds = "5000";
            switch (FundsComboBox.SelectedIndex)
            {
                case 1: funds = "3000"; break;
                case 2: funds = "10000"; break;
                default: funds = "5000"; break;
            }
            SummaryFundsText.Text = funds;
        }

        private void MapListView_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            _selectedMap = MapListView.SelectedItem as Map;
            UpdateUI();
        }

        private void CountrySelectionPanel_CountrySelected(object? sender, CountrySelectedEventArgs e)
        {
            _selectedPlayerCountry = e.SelectedCountry;
            // Pick a random country for the AI, excluding the player's choice
            _selectedOpponentCountry = _availableCountries
                .Where(c => c.Name != _selectedPlayerCountry?.Name)
                .OrderBy(_ => Guid.NewGuid())
                .FirstOrDefault();

            UpdateUI();
        }

        // Navigation Button Handlers
        private void BackButton_Click(object sender, RoutedEventArgs e)
        {
            switch (_currentStep)
            {
                case Step.MapSelect:
                    CountryStepPanel.Visibility = Visibility.Visible;
                    MapStepPanel.Visibility = Visibility.Collapsed;
                    SettingsStepPanel.Visibility = Visibility.Collapsed;
                    _currentStep = Step.CountrySelect;
                    break;
                case Step.Settings:
                    CountryStepPanel.Visibility = Visibility.Collapsed;
                    MapStepPanel.Visibility = Visibility.Visible;
                    SettingsStepPanel.Visibility = Visibility.Collapsed;
                    _currentStep = Step.MapSelect;
                    break;
            }
            UpdateButtonStates();
            UpdateSummaryPanel();
        }

        private void MainMenuButton_Click(object sender, RoutedEventArgs e)
        {
            BackRequested?.Invoke(this, EventArgs.Empty);
        }

        private void NextOrStartButton_Click(object sender, RoutedEventArgs e)
        {
            switch (_currentStep)
            {
                case Step.CountrySelect:
                    // Validate country selection
                    if (_selectedPlayerCountry == null)
                    {
                        MessageBox.Show("Please select a country to continue.", "Selection Required", MessageBoxButton.OK, MessageBoxImage.Warning);
                        return;
                    }
                    CountryStepPanel.Visibility = Visibility.Collapsed;
                    MapStepPanel.Visibility = Visibility.Visible;
                    SettingsStepPanel.Visibility = Visibility.Collapsed;
                    _currentStep = Step.MapSelect;
                    break;
                case Step.MapSelect:
                    // Validate map selection
                    if (_selectedMap == null)
                    {
                        MessageBox.Show("Please select a map to continue.", "Selection Required", MessageBoxButton.OK, MessageBoxImage.Warning);
                        return;
                    }
                    CountryStepPanel.Visibility = Visibility.Collapsed;
                    MapStepPanel.Visibility = Visibility.Collapsed;
                    SettingsStepPanel.Visibility = Visibility.Visible;
                    _currentStep = Step.Settings;
                    break;
                case Step.Settings:
                    // Validate all settings before starting the battle
                    if (_selectedMap == null || _selectedPlayerCountry == null || _selectedOpponentCountry == null)
                    {
                        MessageBox.Show("Please complete all selections before starting the battle.", "Selection Required", MessageBoxButton.OK, MessageBoxImage.Warning);
                        return;
                    }

                    // Grab difficulty
                    GameDifficulty difficulty = GameDifficulty.Medium;
                    switch (DifficultyComboBox.SelectedIndex)
                    {
                        case 0: difficulty = GameDifficulty.Easy; break;
                        case 1: difficulty = GameDifficulty.Medium; break;
                        case 2: difficulty = GameDifficulty.Hard; break;
                        case 3: difficulty = GameDifficulty.Expert; break;
                        default: difficulty = GameDifficulty.Medium; break;
                    }

                    // Season
                    MapSeason season = SummerRadioButton.IsChecked == true ? MapSeason.Summer : MapSeason.Winter;
                    bool weatherEffects = WeatherEffectsCheckBox.IsChecked == true;
                    bool fogOfWarEnabled = FogOfWarCheckBox.IsChecked == true;
                    int startingFunds = 5000;
                    switch (FundsComboBox.SelectedIndex)
                    {
                        case 1: startingFunds = 3000; break;
                        case 2: startingFunds = 10000; break;
                        default: startingFunds = 5000; break;
                    }

                    var settings = new GameSettings
                    {
                        GameMode = GameMode.QuickBattle,
                        Difficulty = difficulty,
                        Season = season,
                        WeatherEffectsEnabled = weatherEffects,
                        FogOfWarEnabled = fogOfWarEnabled,
                        StartingFunds = startingFunds,
                        MapWidth = _selectedMap.Width,
                        MapHeight = _selectedMap.Height
                    };

                    BattleStartRequested?.Invoke(this, new BattleStartEventArgs(
                        _selectedMap,
                        _selectedPlayerCountry,
                        _selectedOpponentCountry,
                        settings));
                    break;
            }
            UpdateButtonStates();
            UpdateSummaryPanel();
        }

        // Handler for live update of summary panel on settings changes
        private void SettingsCombo_SelectionChanged(object sender, RoutedEventArgs e)
        {
            UpdateSummaryPanel();
        }

        /// <summary>
        /// Make the navigation buttons consistent and context-aware for each step.
        /// </summary>
        private void UpdateButtonStates()
        {
            switch (_currentStep)
            {
                case Step.CountrySelect:
                    BackButton.Visibility = Visibility.Collapsed;
                    MainMenuButton.Visibility = Visibility.Visible;
                    NextOrStartButton.Visibility = Visibility.Visible;
                    NextOrStartButton.Content = "Next";
                    NextOrStartButton.Background = new System.Windows.Media.SolidColorBrush(System.Windows.Media.Color.FromRgb(97, 218, 251)); // Blue
                    NextOrStartButton.Foreground = new System.Windows.Media.SolidColorBrush(System.Windows.Media.Color.FromRgb(22, 22, 42));
                    break;
                case Step.MapSelect:
                    BackButton.Visibility = Visibility.Visible;
                    BackButton.Content = "Back";
                    BackButton.Background = new System.Windows.Media.SolidColorBrush(System.Windows.Media.Color.FromRgb(97, 218, 251)); // Blue
                    BackButton.Foreground = new System.Windows.Media.SolidColorBrush(System.Windows.Media.Color.FromRgb(22, 22, 42));
                    MainMenuButton.Visibility = Visibility.Visible;
                    NextOrStartButton.Visibility = Visibility.Visible;
                    NextOrStartButton.Content = "Next";
                    NextOrStartButton.Background = new System.Windows.Media.SolidColorBrush(System.Windows.Media.Color.FromRgb(97, 218, 251)); // Blue
                    NextOrStartButton.Foreground = new System.Windows.Media.SolidColorBrush(System.Windows.Media.Color.FromRgb(22, 22, 42));
                    break;
                case Step.Settings:
                    BackButton.Visibility = Visibility.Visible;
                    BackButton.Content = "Back";
                    BackButton.Background = new System.Windows.Media.SolidColorBrush(System.Windows.Media.Color.FromRgb(97, 218, 251)); // Blue
                    BackButton.Foreground = new System.Windows.Media.SolidColorBrush(System.Windows.Media.Color.FromRgb(22, 22, 42));
                    MainMenuButton.Visibility = Visibility.Visible;
                    NextOrStartButton.Visibility = Visibility.Visible;
                    NextOrStartButton.Content = "Start Battle";
                    NextOrStartButton.Background = new System.Windows.Media.SolidColorBrush(System.Windows.Media.Color.FromRgb(51, 205, 66)); // Green
                    NextOrStartButton.Foreground = new System.Windows.Media.SolidColorBrush(System.Windows.Media.Color.FromRgb(22, 22, 42));
                    break;
            }
        }
    }
}