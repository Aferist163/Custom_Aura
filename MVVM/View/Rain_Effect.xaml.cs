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
    public partial class Rain_Effect : UserControl
    {
       

        public Rain_Effect()
        {
            InitializeComponent();
        }

        private void SetRainButtonClick(object sender, RoutedEventArgs e)
        {
            Color selectedStarColor = ColorRainPick.SelectedColor;

            Color selectedStarColorOne = ColorRainPickOne.SelectedColor ?? Colors.White;

            

            if (Application.Current.MainWindow is MainWindow mainWindow)
            {
                mainWindow.SetRainEffect(selectedStarColor, selectedStarColorOne);
            }
        }
    }
}
