using System;
using System.Windows.Controls;

namespace WorldWarX.Views
{
    public class BattleStartEventArgs : EventArgs
    {
        public Map Map { get; set; }
        public Country PlayerCountry { get; set; }
        public Country OpponentCountry { get; set; }
        
        public BattleStartEventArgs(Map map, Country playerCountry, Country opponentCountry)
        {
            Map = map;
            PlayerCountry = playerCountry;
            OpponentCountry = opponentCountry;
        }
    }
    
    public partial class QuickBattleControl : UserControl
    {
        // Events for navigation
        public event EventHandler BackRequested;
        public event EventHandler<BattleStartEventArgs> BattleStartRequested;
        
        public QuickBattleControl()
        {
            InitializeComponent();
        }
        
        private void BtnBack_Click(object sender, System.Windows.RoutedEventArgs e)
        {
            BackRequested?.Invoke(this, EventArgs.Empty);
        }
    }
}