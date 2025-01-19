using System;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;

namespace Leg7lights
{
    public partial class MainWindow : Window
    {
        public MainWindow()
        {
            InitializeComponent();

            // Add PreviewExecuted handler for all TextBoxes
            CommandManager.AddPreviewExecutedHandler(textKeys, HandlePasteCommand);
            CommandManager.AddPreviewExecutedHandler(textLogo, HandlePasteCommand);
            CommandManager.AddPreviewExecutedHandler(textVents, HandlePasteCommand);
            CommandManager.AddPreviewExecutedHandler(textNeon, HandlePasteCommand);
            CommandManager.AddPreviewExecutedHandler(textAll, HandlePasteCommand);
        }

        private void HexInput_PreviewTextInput(object sender, TextCompositionEventArgs e)
        {
            e.Handled = !IsValidHexColor(e.Text);
        }

        private void HandlePasteCommand(object sender, ExecutedRoutedEventArgs e)
        {
            if (e.Command == ApplicationCommands.Paste)
            {
                string clipboardText = Clipboard.GetText();
                e.Handled = !IsValidHexColor(clipboardText);
            }
        }

        private bool IsValidHexColor(string input)
        {
            if (string.IsNullOrEmpty(input)) return false;

            foreach (char c in input)
            {
                if (!((c >= '0' && c <= '9') || (c >= 'A' && c <= 'F') || (c >= 'a' && c <= 'f')))
                {
                    return false;
                }
            }

            return true;
        }

        private void TextFields_TextChanged(object sender, TextChangedEventArgs e)
        {
            UpdateResult();
            UpdateColorIndicators();
        }

        private void TextAll_TextChanged(object sender, TextChangedEventArgs e)
        {
            UpdateResult();
            UpdateColorIndicators();
        }

        private void RadioButton_Checked(object sender, RoutedEventArgs e)
        {
            bool isGroupsMode = radioGroups.IsChecked == true;

            groupFields.Visibility = isGroupsMode ? Visibility.Visible : Visibility.Collapsed;
            allFields.Visibility = isGroupsMode ? Visibility.Collapsed : Visibility.Visible;

            UpdateResult();
            UpdateColorIndicators();
        }

        private void UpdateResult()
        {
            if (radioGroups.IsChecked == true)
            {
                textResult.Text = $"-keys {textKeys.Text} -logo {textLogo.Text} -vents {textVents.Text} -neon {textNeon.Text}";
            }
            else if (radioAll.IsChecked == true)
            {
                textResult.Text = $"-all {textAll.Text}";
            }
        }

        private void UpdateColorIndicators()
        {
            UpdateColorBox(textKeys, colorBoxKeys);
            UpdateColorBox(textLogo, colorBoxLogo);
            UpdateColorBox(textVents, colorBoxVents);
            UpdateColorBox(textNeon, colorBoxNeon);
            UpdateColorBox(textAll, colorBoxAll);
        }

        private void UpdateColorBox(TextBox textBox, Border colorBox)
        {
            if (IsValidHexColor(textBox.Text) && textBox.Text.Length == 6)
            {
                colorBox.Background = (SolidColorBrush)new BrushConverter().ConvertFrom($"#{textBox.Text}");
            }
            else
            {
                colorBox.Background = Brushes.Transparent;
            }
        }

        private void CopyButton_Click(object sender, RoutedEventArgs e)
        {
            Clipboard.SetText(textResult.Text);
            MessageBox.Show("Copied!");
        }
    }
}
