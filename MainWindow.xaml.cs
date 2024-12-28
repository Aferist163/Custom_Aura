using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Remoting.Metadata.W3cXsd2001;
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
            InitializeMatrix(); 
        }


        private System.Windows.Shapes.Rectangle[,] virtualMatrix;


        private void DrawMatrix(int rows, int columns, double cellSize, double cellSpacing)
        {
            MatrixCanvas.Children.Clear();
            virtualMatrix = new System.Windows.Shapes.Rectangle[rows, columns];

            for (int row = 0; row < rows; row++)
            {
                for (int col = 0; col < columns; col++)
                {
                    var rectangle = new System.Windows.Shapes.Rectangle
                    {
                        Width = cellSize,
                        Height = cellSize,
                        Fill = System.Windows.Media.Brushes.Black
                    };
                    double x = col * (cellSize + cellSpacing);
                    double y = row * (cellSize + cellSpacing);

                    Canvas.SetLeft(rectangle, x);
                    Canvas.SetTop(rectangle, y);

                    MatrixCanvas.Children.Add(rectangle);

                    virtualMatrix[row, col] = rectangle;
                }
            }
        }

        private void UpdateVirtualMatrix(int row, int col, uint color)
        {
            if (virtualMatrix == null) return;
            byte r = (byte)(color & 0xFF);
            byte g = (byte)((color >> 8) & 0xFF);
            byte b = (byte)((color >> 16) & 0xFF);

            var brush = new System.Windows.Media.SolidColorBrush(System.Windows.Media.Color.FromRgb(r, g, b));
            virtualMatrix[row, col].Fill = brush;
        }

        private void InitializeMatrix()
        {
            try
            {
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
                        int width = (int)dev.Width;
                        int height = (int)dev.Height;
                        DrawMatrix(height, width, 30, 5);
                    }
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Ошибка при создании эффекта звездного неба: {ex.Message}", "Ошибка", MessageBoxButton.OK, MessageBoxImage.Error);
            }
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

        private System.Windows.Media.Color InterpolateColor(System.Windows.Media.Color from, System.Windows.Media.Color to, double progress)
        {
            progress = Math.Max(0, Math.Min(1, progress)); // Ограничение значения между 0 и 1
            byte r = (byte)(from.R + (to.R - from.R) * progress);
            byte g = (byte)(from.G + (to.G - from.G) * progress);
            byte b = (byte)(from.B + (to.B - from.B) * progress);
            return System.Windows.Media.Color.FromRgb(r, g, b);
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

                    int width = (int)dev.Width;
                    int height = (int)dev.Height;
                    int totalLights = width * height;

                    for (int i = 0; i < totalLights; i++)
                    {
                        int row = i / width;
                        int col = i % width;
                        UpdateVirtualMatrix(row, col, ConvertToRgbFormat(selectedColor));
                    }

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

                        // Инициализация виртуальной матрицы
                        DrawMatrix(height, width, 30, 5);

                        var starStates = new (bool IsOn, double Progress)[totalLights];
                        double progressIncrement = 100.0 / starDuration;

                        while (!cancellationToken.IsCancellationRequested)
                        {
                            for (int i = 0; i < totalLights; i++)
                            {
                                int row = i / width;
                                int col = i % width;

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

                                        // Обновление виртуальной матрицы
                                        UpdateVirtualMatrix(row, col, ConvertToRgbFormat(backgroundColor));

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

                                        uint interpolatedColor = (uint)((r) | (g << 8) | (b << 16));
                                        dev.Lights[i].Color = interpolatedColor;

                                        // Обновление виртуальной матрицы
                                        UpdateVirtualMatrix(row, col, interpolatedColor);
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

        public async void SetRainEffect(System.Windows.Media.Color selectedColor, System.Windows.Media.Color backgroundColor)
        {
            try
            {
                if (sdk == null)
                {
                    MessageBox.Show("SDK не инициализирован!", "Ошибка", MessageBoxButton.OK, MessageBoxImage.Error);
                    return;
                }

                var devices = sdk.Enumerate(0);
                Random random = new Random();

                foreach (IAuraSyncDevice dev in devices)
                {
                    if (dev.Type == 0x80000)
                    {
                        try
                        {
                            foreach (IAuraRgbLight light in dev.Lights)
                            {
                                light.Color = ConvertToRgbFormat(backgroundColor);
                            }
                            dev.Apply();

                            int width = (int)dev.Width;
                            int height = (int)dev.Height;

                            // Инициализация виртуальной матрицы
                            DrawMatrix(height, width, 30, 5);

                            while (true)
                            {
                                int startX = random.Next(0, width);
                                int startY = random.Next(0, height);

                                for (int radius = 0; radius <= 5; radius++)
                                {
                                    for (int y = 0; y < height; y++)
                                    {
                                        for (int x = 0; x < width; x++)
                                        {
                                            int distance = Math.Abs(x - startX) + Math.Abs(y - startY);

                                            if (distance == radius)
                                            {
                                                // Обновляем устройство
                                                dev.Lights[y * width + x].Color = ConvertToRgbFormat(selectedColor);

                                                // Обновляем виртуальную матрицу
                                                UpdateVirtualMatrix(y, x, ConvertToRgbFormat(selectedColor));
                                            }
                                            else if (distance < radius)
                                            {
                                                // Обновляем устройство
                                                dev.Lights[y * width + x].Color = ConvertToRgbFormat(backgroundColor);

                                                // Обновляем виртуальную матрицу
                                                UpdateVirtualMatrix(y, x, ConvertToRgbFormat(backgroundColor));
                                            }
                                        }
                                    }

                                    dev.Apply();
                                    await Task.Delay(200); // Задержка для обновления эффекта
                                }

                                await Task.Delay(500);

                                foreach (IAuraRgbLight light in dev.Lights)
                                {
                                    light.Color = ConvertToRgbFormat(backgroundColor);
                                }
                                dev.Apply();

                                // Сброс виртуальной матрицы
                                for (int y = 0; y < height; y++)
                                {
                                    for (int x = 0; x < width; x++)
                                    {
                                        UpdateVirtualMatrix(y, x, ConvertToRgbFormat(backgroundColor));
                                    }
                                }
                            }
                        }
                        catch (Exception ex)
                        {
                            MessageBox.Show($"Ошибка при установке эффекта: {ex.Message}", "Ошибка", MessageBoxButton.OK, MessageBoxImage.Error);
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