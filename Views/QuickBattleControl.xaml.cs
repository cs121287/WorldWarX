using System;
using System.Windows;
using System.Windows.Controls;
using WorldWarX.Models;

namespace WorldWarX.Views
{
    public class BattleStartEventArgs : EventArgs
    {
        public Map Map { get; set; }
        public Country PlayerCountry { get; set; }
        public Country OpponentCountry { get; set; }
        public GameSettings GameSettings { get; set; }
        
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
        // Events for navigation
        public event EventHandler BackRequested;
        public event EventHandler<BattleStartEventArgs> BattleStartRequested;
        
        private Country _selectedPlayerCountry;
        private Country _selectedOpponentCountry;
        private Map _selectedMap;
        
        public QuickBattleControl()
        {
            InitializeComponent();
            
            // For now, we'll populate with placeholder data
            _selectedPlayerCountry = new Country { Name = "Redonia" };
            _selectedOpponentCountry = new Country { Name = "Azurea" };
            _selectedMap = new Map { Name = "Test Map", Width = 12, Height = 10 };
            
            // Update UI with these selections
            UpdateUI();
        }
        
        private void UpdateUI()
        {
            // Update UI elements once they are implemented
            // For now, we'll leave this as is
        }
        
        private void BtnBack_Click(object sender, System.Windows.RoutedEventArgs e)
        {
            BackRequested?.Invoke(this, EventArgs.Empty);
        }
        
        private void BtnStartBattle_Click(object sender, RoutedEventArgs e)
        {
            // Open game settings window
            GameSettingsWindow settingsWindow = new GameSettingsWindow();
            bool? result = settingsWindow.ShowDialog();
            
            if (result == true)
            {
                // Start battle with selected settings
                BattleStartRequested?.Invoke(this, new BattleStartEventArgs(
                    _selectedMap, 
                    _selectedPlayerCountry, 
                    _selectedOpponentCountry,
                    settingsWindow.Settings));
            }
        }
    }
}