using System;
using System.Windows;

namespace WorldWarX.Views
{
    public partial class ResizeMapDialog : Window
    {
        public int NewWidth { get; private set; }
        public int NewHeight { get; private set; }

        public ResizeMapDialog(int currentWidth, int currentHeight)
        {
            InitializeComponent();
            WidthTextBox.Text = currentWidth.ToString();
            HeightTextBox.Text = currentHeight.ToString();
        }

        private void OK_Click(object sender, RoutedEventArgs e)
        {
            if (int.TryParse(WidthTextBox.Text, out int w) && int.TryParse(HeightTextBox.Text, out int h)
                && w > 0 && h > 0)
            {
                NewWidth = w;
                NewHeight = h;
                DialogResult = true;
                Close();
            }
            else
            {
                MessageBox.Show("Please enter valid positive integers for width and height.");
            }
        }

        private void Cancel_Click(object sender, RoutedEventArgs e)
        {
            DialogResult = false;
            Close();
        }
    }
}