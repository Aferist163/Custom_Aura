using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;

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
            // Получить выбранный цвет
            Color selectedColor = ColorPickerControl.SelectedColor ?? Colors.Red;

            // Отправить этот цвет в MainWindow
            if (Application.Current.MainWindow is MainWindow mainWindow)
            {
                mainWindow.SetAllLightsColor(selectedColor);
            }
        }
    }
}
