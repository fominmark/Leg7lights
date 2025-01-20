using System;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;
using System.Diagnostics;
using System.IO;
using System.Collections.Generic;
using System.Threading.Tasks;
using System.Text.RegularExpressions;

namespace Leg7lights
{
    public partial class MainWindow : Window
    {
        private readonly LegionLightingController _controller;

        public MainWindow()
        {
            InitializeComponent();
            _controller = new LegionLightingController();

            // Add PreviewExecuted handler for all TextBoxes
            CommandManager.AddPreviewExecutedHandler(textKeys, HandlePasteCommand);
            CommandManager.AddPreviewExecutedHandler(textLogo, HandlePasteCommand);
            CommandManager.AddPreviewExecutedHandler(textVents, HandlePasteCommand);
            CommandManager.AddPreviewExecutedHandler(textNeon, HandlePasteCommand);
            CommandManager.AddPreviewExecutedHandler(textAll, HandlePasteCommand);
        }

        private void HexInput_PreviewTextInput(object sender, TextCompositionEventArgs e)
        {
            e.Handled = !Regex.IsMatch(e.Text, @"^[0-9a-fA-F]+$");
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
            // Проверяем, что окно полностью инициализировано
            if (!IsLoaded) return;
            
            // Проверяем, что элементы UI существуют
            if (allFields == null || groupFields == null) return;

            if (radioAll != null && radioAll.IsChecked == true)
            {
                allFields.Visibility = Visibility.Visible;
                groupFields.Visibility = Visibility.Collapsed;
            }
            else
            {
                allFields.Visibility = Visibility.Collapsed;
                groupFields.Visibility = Visibility.Visible;
            }
            UpdateResult();
        }

        private void UpdateResult()
        {
            if (radioGroups.IsChecked == true)
            {
                var parts = new List<string>();
                
                if (IsValidHexColor(textKeys.Text) && textKeys.Text.Length == 6)
                    parts.Add($"-keys {textKeys.Text}");
                    
                if (IsValidHexColor(textLogo.Text) && textLogo.Text.Length == 6)
                    parts.Add($"-logo {textLogo.Text}");
                    
                if (IsValidHexColor(textVents.Text) && textVents.Text.Length == 6)
                    parts.Add($"-vents {textVents.Text}");
                    
                if (IsValidHexColor(textNeon.Text) && textNeon.Text.Length == 6)
                    parts.Add($"-neon {textNeon.Text}");
                    
                textResult.Text = string.Join(" ", parts);
            }
            else if (radioAll.IsChecked == true)
            {
                textResult.Text = IsValidHexColor(textAll.Text) && textAll.Text.Length == 6 
                    ? $"-all {textAll.Text}" 
                    : string.Empty;
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
            string text = textBox.Text;

            if (!string.IsNullOrEmpty(text) && IsValidHexColor(text) && text.Length == 6)
            {
                colorBox.Background = new BrushConverter().ConvertFrom($"#{text}") as SolidColorBrush ?? Brushes.Transparent;
            }
            else
            {
                colorBox.Background = Brushes.Transparent;
            }
        }

        private async void CopyButton_Click(object sender, RoutedEventArgs e)
        {
            Clipboard.SetText(textResult.Text);
            copyStatusLabel.Visibility = Visibility.Visible;
            await Task.Delay(2000);
            copyStatusLabel.Visibility = Visibility.Collapsed;
        }

        private async void ApplyButton_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                if (radioGroups.IsChecked == true)
                {
                    if (IsValidHexColor(textKeys.Text) && textKeys.Text.Length == 6)
                        await _controller.SetColor("keys", textKeys.Text);
                        
                    if (IsValidHexColor(textLogo.Text) && textLogo.Text.Length == 6)
                        await _controller.SetColor("logo", textLogo.Text);
                        
                    if (IsValidHexColor(textVents.Text) && textVents.Text.Length == 6)
                        await _controller.SetColor("vents", textVents.Text);
                        
                    if (IsValidHexColor(textNeon.Text) && textNeon.Text.Length == 6)
                        await _controller.SetColor("neon", textNeon.Text);
                }
                else if (radioAll.IsChecked == true && IsValidHexColor(textAll.Text) && textAll.Text.Length == 6)
                {
                    await _controller.SetColor("all", textAll.Text);
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error: {ex.Message}", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }
    }
}
