using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using System.Windows.Threading;

namespace Leg7lights
{
    public partial class MainWindow : Window
    {
        public MainWindow()
        {
            InitializeComponent();
            if (labelKeys == null)
            {
                MessageBox.Show("labelKeys is null");
            }
        }

        // Update the result field when individual text fields are changed
        private void TextFields_TextChanged(object sender, TextChangedEventArgs e)
        {
            // Get values from individual text fields
            string keys = textKeys.Text.Trim();
            string logo = textLogo.Text.Trim();
            string vents = textVents.Text.Trim();
            string neon = textNeon.Text.Trim();

            // Update color boxes
            UpdateColorBox(textKeys, colorBoxKeys);
            UpdateColorBox(textLogo, colorBoxLogo);
            UpdateColorBox(textVents, colorBoxVents);
            UpdateColorBox(textNeon, colorBoxNeon);

            // Format the result
            textResult.Text = $"-keys {keys} -logo {logo} -vents {vents} -neon {neon}";
        }

        private void TextAll_TextChanged(object sender, TextChangedEventArgs e)
        {
            // Get the value from the "All" text field
            string all = textAll.Text.Trim();

            // Update the color box for All
            UpdateColorBox(textAll, colorBoxAll);

            // Format the result
            textResult.Text = $"-all {all}";
        }

        // Handle the Copy button click event
        private void CopyButton_Click(object sender, RoutedEventArgs e)
        {
            string text = textResult.Text;
            if (!string.IsNullOrEmpty(text))
            {
                Clipboard.SetText(text); // Copy the text to the clipboard
                ShowCopyConfirmation(); // Show confirmation message
            }
        }

        // Show a temporary confirmation message when text is copied
        private void ShowCopyConfirmation()
        {
            copyStatusLabel.Visibility = Visibility.Visible;

            // Hide the confirmation message after 2 seconds
            DispatcherTimer timer = new DispatcherTimer
            {
                Interval = TimeSpan.FromSeconds(2)
            };
            timer.Tick += (s, e) =>
            {
                copyStatusLabel.Visibility = Visibility.Collapsed;
                timer.Stop();
            };
            timer.Start();
        }

        // Handle the Checked event for the radio buttons
        private void RadioButton_Checked(object sender, RoutedEventArgs e)
        {
            if (radioGroups.IsChecked == true)
            {
                // Show individual text fields
                SetElementVisibility(textKeys, true);
                SetElementVisibility(textLogo, true);
                SetElementVisibility(textVents, true);
                SetElementVisibility(textNeon, true);

                // Show individual field labels
                SetElementVisibility(labelKeys, true);
                SetElementVisibility(labelLogo, true);
                SetElementVisibility(labelVents, true);
                SetElementVisibility(labelNeon, true);

                // Hide the "All" text field and its label
                SetElementVisibility(textAll, false);
                SetElementVisibility(labelAll, false);

                // Update the result text
                TextFields_TextChanged(null, null);
            }
            else if (radioAll.IsChecked == true)
            {
                // Hide individual text fields
                SetElementVisibility(textKeys, false);
                SetElementVisibility(textLogo, false);
                SetElementVisibility(textVents, false);
                SetElementVisibility(textNeon, false);

                // Hide individual field labels
                SetElementVisibility(labelKeys, false);
                SetElementVisibility(labelLogo, false);
                SetElementVisibility(labelVents, false);
                SetElementVisibility(labelNeon, false);

                // Show the "All" text field and its label
                SetElementVisibility(textAll, true);
                SetElementVisibility(labelAll, true);

                // Update the result text
                TextAll_TextChanged(null, null);
            }
        }

        private void UpdateColorBox(TextBox textBox, Border colorBox)
        {
            string colorCode = textBox.Text.Trim();

            // Проверяем, является ли значение цветом
            if (IsValidHexColor(colorCode))
            {
                // Если цвет правильный, обновляем фон Border
                colorBox.Background = new SolidColorBrush((Color)ColorConverter.ConvertFromString($"#{colorCode}"));
                textBox.BorderBrush = new SolidColorBrush(Colors.Gray);  // Сбрасываем красную обводку
            }
            else
            {
                // Если цвет некорректный, обводим красным
                colorBox.Background = Brushes.Transparent;  // Убираем цвет
                textBox.BorderBrush = new SolidColorBrush(Colors.Red);  // Обводим красным
            }
        }

        private bool IsValidHexColor(string colorCode)
        {
            if (colorCode.Length == 6 && int.TryParse(colorCode, System.Globalization.NumberStyles.HexNumber, null, out _))
            {
                return true;  // Верно, если 6 символов и является шестнадцатеричным числом
            }
            return false;
        }


        // Helper method to safely show/hide UI elements
        private void SetElementVisibility(UIElement element, bool isVisible)
        {
            if (element != null)
            {
                element.Visibility = isVisible ? Visibility.Visible : Visibility.Hidden;
                element.Opacity = isVisible ? 1 : 0;
                element.IsEnabled = isVisible;
            }
        }
    }
}
