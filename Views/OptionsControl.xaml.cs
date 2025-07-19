using System;
using System.Windows.Controls;

namespace WorldWarX.Views
{
    public partial class OptionsControl : UserControl
    {
        // Events for navigation
        public event EventHandler BackRequested;
        
        public OptionsControl()
        {
            InitializeComponent();
        }
        
        private void BtnBack_Click(object sender, System.Windows.RoutedEventArgs e)
        {
            BackRequested?.Invoke(this, EventArgs.Empty);
        }
    }
}