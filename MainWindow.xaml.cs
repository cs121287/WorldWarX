using System;
using System.Windows;
using System.Windows.Controls;
using WorldWarX.Views;

namespace WorldWarX
{
    public partial class MainWindow : Window
    {
        // Create instances of our screens
        private MainMenuControl _mainMenuScreen;
        private CountrySelectionControl _countrySelectionScreen;
        private GameControl _gameScreen;
        private QuickBattleControl _quickBattleScreen;
        private OptionsControl _optionsScreen;
        
        public MainWindow()
        {
            InitializeComponent();
            InitializeScreens();
            
            // Start with main menu
            NavigateToMainMenu();
        }
        
        private void InitializeScreens()
        {
            // Initialize all screens
            _mainMenuScreen = new MainMenuControl();
            _countrySelectionScreen = new CountrySelectionControl();
            _quickBattleScreen = new QuickBattleControl();
            _optionsScreen = new OptionsControl();
            
            // Wire up navigation events from main menu
            _mainMenuScreen.StartCampaignRequested += (s, e) => NavigateToCountrySelection();
            _mainMenuScreen.QuickBattleRequested += (s, e) => NavigateToQuickBattle();
            _mainMenuScreen.OptionsRequested += (s, e) => NavigateToOptions();
            _mainMenuScreen.ExitRequested += (s, e) => Application.Current.Shutdown();
            
            // Wire up navigation events from country selection
            _countrySelectionScreen.BackRequested += (s, e) => NavigateToMainMenu();
            _countrySelectionScreen.CountrySelected += (s, e) => StartGame(e.SelectedCountry);
            
            // Wire up navigation events from quick battle
            _quickBattleScreen.BackRequested += (s, e) => NavigateToMainMenu();
            _quickBattleScreen.BattleStartRequested += (s, e) => StartQuickBattle(e.Map, e.PlayerCountry, e.OpponentCountry);
            
            // Wire up navigation events from options
            _optionsScreen.BackRequested += (s, e) => NavigateToMainMenu();
        }
        
        // Navigation methods
        public void NavigateToMainMenu()
        {
            MainContentControl.Content = _mainMenuScreen;
        }
        
        public void NavigateToCountrySelection()
        {
            MainContentControl.Content = _countrySelectionScreen;
        }
        
        public void NavigateToQuickBattle()
        {
            MainContentControl.Content = _quickBattleScreen;
        }
        
        public void NavigateToOptions()
        {
            MainContentControl.Content = _optionsScreen;
        }
        
        private void StartGame(Country selectedCountry)
        {
            if (_gameScreen != null)
            {
                // Clear any previous game
                _gameScreen = null;
            }
            
            // Create new game screen with campaign mode
            _gameScreen = new GameControl(GameMode.Campaign, selectedCountry);
            _gameScreen.BackToMainMenuRequested += (s, e) => NavigateToMainMenu();
            
            // Navigate to game screen
            MainContentControl.Content = _gameScreen;
        }
        
        private void StartQuickBattle(Map map, Country playerCountry, Country opponentCountry)
        {
            if (_gameScreen != null)
            {
                // Clear any previous game
                _gameScreen = null;
            }
            
            // Create new game screen with quick battle mode
            _gameScreen = new GameControl(GameMode.QuickBattle, playerCountry, opponentCountry, map);
            _gameScreen.BackToMainMenuRequested += (s, e) => NavigateToMainMenu();
            
            // Navigate to game screen
            MainContentControl.Content = _gameScreen;
        }
    }
}