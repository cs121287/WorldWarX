using System.Windows;
using WorldWarX.Models;

namespace WorldWarX.Views
{
    public partial class GameSettingsWindow : Window
    {
        public GameSettings Settings { get; private set; }

        public GameSettingsWindow()
        {
            InitializeComponent();
        }

        private void BtnStart_Click(object sender, RoutedEventArgs e)
        {
            // Determine game mode
            GameMode gameMode = GameModeComboBox.SelectedIndex == 0 ?
                                GameMode.Campaign : GameMode.QuickBattle;

            // Determine difficulty
            GameDifficulty difficulty;
            switch (DifficultyComboBox.SelectedIndex)
            {
                case 0:
                    difficulty = GameDifficulty.Easy;
                    break;
                case 2:
                    difficulty = GameDifficulty.Hard;
                    break;
                case 3:
                    difficulty = GameDifficulty.Expert;
                    break;
                default:
                    difficulty = GameDifficulty.Medium;
                    break;
            }

            // Determine map size
            int mapWidth = 12;
            int mapHeight = 10;

            switch (MapSizeComboBox.SelectedIndex)
            {
                case 0: // Small
                    mapWidth = 8;
                    mapHeight = 8;
                    break;
                case 2: // Large
                    mapWidth = 16;
                    mapHeight = 12;
                    break;
                case 3: // Huge
                    mapWidth = 20;
                    mapHeight = 16;
                    break;
                default: // Medium
                    mapWidth = 12;
                    mapHeight = 10;
                    break;
            }

            // Determine season
            MapSeason season = SummerRadioButton.IsChecked == true ?
                               MapSeason.Summer : MapSeason.Winter;

            // Weather effects
            bool weatherEffectsEnabled = WeatherEffectsCheckBox.IsChecked == true;

            // Fog of War
            bool fogOfWarEnabled = FogOfWarCheckBox.IsChecked == true;

            // Starting funds
            int startingFunds;
            switch (FundsComboBox.SelectedIndex)
            {
                case 1: // Low
                    startingFunds = 3000;
                    break;
                case 2: // High
                    startingFunds = 10000;
                    break;
                default: // Standard
                    startingFunds = 5000;
                    break;
            }

            // Create settings object
            Settings = new GameSettings
            {
                GameMode = gameMode,
                Difficulty = difficulty,
                Season = season,
                WeatherEffectsEnabled = weatherEffectsEnabled,
                FogOfWarEnabled = fogOfWarEnabled,
                StartingFunds = startingFunds,
                MapWidth = mapWidth,
                MapHeight = mapHeight
            };

            DialogResult = true;
            Close();
        }

        private void BtnCancel_Click(object sender, RoutedEventArgs e)
        {
            DialogResult = false;
            Close();
        }
    }
}