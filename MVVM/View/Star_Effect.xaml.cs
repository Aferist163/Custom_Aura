using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;

namespace Custom_Aura.MVVM.View
{
    public partial class Star_Effect : UserControl
    {
        public Star_Effect()
        {
            InitializeComponent();
        }

        private void SetStarButtonClick(object sender, RoutedEventArgs e)
        {
            Color selectedColor = ColorBackStar.SelectedColor ?? Colors.White;

            Color selectedStarColor = ColorStar.SelectedColor;

            double starDuration = StarDurationSlider.Value;

            if (Application.Current.MainWindow is MainWindow mainWindow)
            {
                mainWindow.SetStarEffect(selectedColor, selectedStarColor, starDuration);
            }
        }
    }
}
