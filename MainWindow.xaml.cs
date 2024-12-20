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
using AuraServiceLib;

namespace Custom_Aura
{
    public partial class MainWindow : Window
    {
        private void MinimizeWindow(object sender, RoutedEventArgs e)
        {
            this.WindowState = WindowState.Minimized;
        }
        private void MaximizeRestoreWindow(object sender, RoutedEventArgs e)
        {
            this.WindowState = this.WindowState == WindowState.Maximized
                ? WindowState.Normal
                : WindowState.Maximized;
        }
        private void CloseWindow(object sender, RoutedEventArgs e)
        {
            this.Close();
        }


        private IAuraSdk sdk;
        public MainWindow()
        {
            InitializeComponent();
            InitializeAuraSdk();
        }

        private void InitializeAuraSdk()
        {
            try
            {
                sdk = new AuraSdk();
                sdk.SwitchMode();
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Ошибка инициализации Aura SDK: {ex.Message}", "Ошибка", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private uint ConvertToRgbFormat(System.Windows.Media.Color color)
        {
            return (uint)((color.R) | (color.G << 8) | (color.B << 16));
        }

        private void SetAllLightsColor(object sender, RoutedEventArgs e)
        {
            try
            {
                if (sdk == null)
                {
                    MessageBox.Show("SDK не инициализирован!", "Ошибка", MessageBoxButton.OK, MessageBoxImage.Error);
                    return;
                }

                System.Windows.Media.Color selectedColor = ColorPickerControl.SelectedColor ?? System.Windows.Media.Colors.Red;

                var devices = sdk.Enumerate(0);
                foreach (IAuraSyncDevice dev in devices)
                {
                    if (dev.Type == 0x80000)
                    {
                        try
                        {
                            foreach (IAuraRgbLight light in dev.Lights)
                            {
                                light.Color = ConvertToRgbFormat(selectedColor);
                            }
                            dev.Apply();
                            MessageBox.Show("Все устройства подсветки окрашены в выбранный цвет.", "Успешно", MessageBoxButton.OK, MessageBoxImage.Information);
                        }
                        catch (Exception ex)
                        {
                            MessageBox.Show($"Ошибка при установке цвета всех светодиодов: {ex.Message}", "Ошибка", MessageBoxButton.OK, MessageBoxImage.Error);
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Ошибка при изменении цвета устройств: {ex.Message}", "Ошибка", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }
    }
}
