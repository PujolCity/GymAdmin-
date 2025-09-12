using System.Windows;
using System.Windows.Controls;
using System.Windows.Threading;

namespace GymAdmin.Desktop.Views
{
    /// <summary>
    /// Lógica de interacción para PlanesMembresiaView.xaml
    /// </summary>
    public partial class PlanesMembresiaView : UserControl
    {
        public PlanesMembresiaView()
        {
            InitializeComponent();
            Loaded += OnLoaded;
        }

        private void OnLoaded(object sender, RoutedEventArgs e)
        {
            // Dar foco y seleccionar todo al entrar
            Dispatcher.BeginInvoke(DispatcherPriority.Loaded, new Action(() =>
            {
                SearchTextBox.Focus();
                SearchTextBox.CaretIndex = SearchTextBox.Text.Length;
                SearchTextBox.SelectAll();
            }));
        }
    }
}
