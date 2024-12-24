using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Animation;
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

        private IAuraSdk2 sdk;

        public MainWindow()
        {
            InitializeComponent();
            InitializeAuraSdk();
        }

        private void InitializeAuraSdk()
        {
            try
            {
                sdk = new AuraSdk() as IAuraSdk2;

                if (sdk == null)
                {
                    MessageBox.Show("IAuraSdk2 initialization failed. Ensure the SDK supports IAuraSdk2.", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
                    return;
                }

                sdk.SwitchMode();
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error initializing Aura SDK: {ex.Message}", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private uint ConvertToRgbFormat(System.Windows.Media.Color color)
        {
            return (uint)((color.R) | (color.G << 8) | (color.B << 16));
        }

        private void ReleaseControl()
        {
            try
            {
                if (sdk != null)
                {
                    sdk.ReleaseControl(0);
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error releasing Aura SDK control: {ex.Message}", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }





        private CancellationTokenSource cancellationTokenSource;

        public void StopStarEffect()
        {
            if (cancellationTokenSource != null)
            {
                cancellationTokenSource.Cancel(); 
                cancellationTokenSource.Dispose(); 
                cancellationTokenSource = null;
            }
        }

        public void SetAllLightsColor(System.Windows.Media.Color selectedColor)
        {
            try
            {
                if (sdk == null)
                {
                    MessageBox.Show("SDK не инициализирован!", "Ошибка", MessageBoxButton.OK, MessageBoxImage.Error);
                    return;
                }

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

        public async void SetStarEffect(System.Windows.Media.Color backgroundColor, System.Windows.Media.Color starColor, double starDuration)
        {
            try
            {
                StopStarEffect();

                cancellationTokenSource = new CancellationTokenSource();
                var cancellationToken = cancellationTokenSource.Token;

                if (sdk == null)
                {
                    MessageBox.Show("SDK не инициализирован!", "Ошибка", MessageBoxButton.OK, MessageBoxImage.Error);
                    return;
                }

                var devices = sdk.Enumerate(0);
                var random = new Random();

                foreach (IAuraSyncDevice dev in devices)
                {
                    if (dev.Type == 0x80000)
                    {
                        foreach (IAuraRgbLight light in dev.Lights)
                        {
                            light.Color = ConvertToRgbFormat(backgroundColor);
                        }
                        dev.Apply();

                        int width = (int)dev.Width;
                        int height = (int)dev.Height;
                        int totalLights = width * height;

                        var starStates = new (bool IsOn, double Progress)[totalLights];

                        double progressIncrement = 100.0 / starDuration;

                        while (!cancellationToken.IsCancellationRequested)
                        {
                            for (int i = 0; i < totalLights; i++)
                            {
                                if (!starStates[i].IsOn)
                                {
                                    if (random.NextDouble() < 0.15)
                                    {
                                        starStates[i] = (true, 0.0);
                                    }
                                }
                                else
                                {
                                    starStates[i].Progress += progressIncrement;
                                    if (starStates[i].Progress >= 2.0)
                                    {
                                        dev.Lights[i].Color = ConvertToRgbFormat(backgroundColor);
                                        starStates[i] = (false, 0.0);
                                    }
                                    else
                                    {
                                        double t = starStates[i].Progress <= 1.0
                                            ? starStates[i].Progress
                                            : 2.0 - starStates[i].Progress;

                                        byte r = (byte)(backgroundColor.R + t * (starColor.R - backgroundColor.R));
                                        byte g = (byte)(backgroundColor.G + t * (starColor.G - backgroundColor.G));
                                        byte b = (byte)(backgroundColor.B + t * (starColor.B - backgroundColor.B));

                                        dev.Lights[i].Color = (uint)((r) | (g << 8) | (b << 16));
                                    }
                                }
                            }
                            dev.Apply();
                            await Task.Delay(100, cancellationToken);
                        }
                    }
                }
            }
            catch (TaskCanceledException)
            {

            }
            catch (Exception ex)
            {
                MessageBox.Show($"Ошибка при создании эффекта звездного неба: {ex.Message}", "Ошибка", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

    }
}