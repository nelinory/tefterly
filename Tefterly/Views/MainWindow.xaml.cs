using System.Windows;
using System.Windows.Input;

namespace Tefterly.Views
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        public MainWindow()
        {
            InitializeComponent();

            MouseDown += (sender, e) =>
            {
                if (e.LeftButton == MouseButtonState.Pressed)
                    DragMove();
            };
        }
    }
}