using System;
using System.Windows;
using System.Windows.Controls;

namespace WorldWarX.Views
{
    public partial class MainMenuControl : UserControl
    {
        // Events for navigation
        public event EventHandler StartCampaignRequested;
        public event EventHandler QuickBattleRequested;
        public event EventHandler MapEditorRequested;
        public event EventHandler ExitRequested;

        public MainMenuControl()
        {
            InitializeComponent();
        }

        private void BtnStart_Click(object sender, RoutedEventArgs e)
        {
            StartCampaignRequested?.Invoke(this, EventArgs.Empty);
        }

        private void BtnQuickBattle_Click(object sender, RoutedEventArgs e)
        {
            QuickBattleRequested?.Invoke(this, EventArgs.Empty);
        }

        private void BtnMapEditor_Click(object sender, RoutedEventArgs e)
        {
            MapEditorRequested?.Invoke(this, EventArgs.Empty);
        }

        private void BtnExit_Click(object sender, RoutedEventArgs e)
        {
            // Show confirmation dialog
            MessageBoxResult result = MessageBox.Show("Are you sure you want to exit World War X?",
                                                     "Exit Confirmation",
                                                     MessageBoxButton.YesNo,
                                                     MessageBoxImage.Question);

            if (result == MessageBoxResult.Yes)
            {
                ExitRequested?.Invoke(this, EventArgs.Empty);
            }
        }
    }
}