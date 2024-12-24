using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;

namespace Custom_Aura.MVVM.View
{
    public partial class All_Collor : UserControl
    {
        public All_Collor()
        {
            InitializeComponent();
        }

        private void SetAllLightsButtonClick(object sender, RoutedEventArgs e)
        {
            Color selectedColor = ColorPickerControl.SelectedColor ?? Colors.Red;

            if (Application.Current.MainWindow is MainWindow mainWindow)
            {
                mainWindow.SetAllLightsColor(selectedColor);
            }
        }
    }
}
