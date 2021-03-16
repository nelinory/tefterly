using ModernWpf;
using System.Windows.Controls;

namespace Tefterly.Modules.Notebook.Views
{
    /// <summary>
    /// Interaction logic for NotebookList
    /// </summary>
    public partial class NotebookList : UserControl
    {
        public NotebookList()
        {
            InitializeComponent();
        }

        private void Button_Click(object sender, System.Windows.RoutedEventArgs e)
        {
            if (ThemeManager.Current.ApplicationTheme == ApplicationTheme.Dark)
                ThemeManager.Current.ApplicationTheme = ApplicationTheme.Light;
            else
                ThemeManager.Current.ApplicationTheme = ApplicationTheme.Dark;
        }
    }
}
