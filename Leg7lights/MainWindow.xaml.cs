using System;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;
using System.Diagnostics;
using System.IO;
using System.Collections.Generic;

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
                colorBox.Background = (SolidColorBrush)new BrushConverter().ConvertFrom($"#{text}");
            }
            else
            {
                colorBox.Background = Brushes.Transparent;
            }
        }

        private async void CopyButton_Click(object sender, RoutedEventArgs e)
        {
            // Copy the text to the clipboard
            Clipboard.SetText(textResult.Text);

            // Show the copy confirmation label
            copyStatusLabel.Visibility = Visibility.Visible;
            await Task.Delay(2000);

            // Hide the confirmation label
            copyStatusLabel.Visibility = Visibility.Collapsed;
        }

        private async void ApplyButton_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                string scriptPath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "legion7-rgb-ps-main", "legion7-rgb.ps1");
                string arguments = $"-executionPolicy bypass -file \"{scriptPath}\" {textResult.Text}";

                var startInfo = new ProcessStartInfo
                {
                    FileName = "powershell.exe",
                    Arguments = arguments,
                    UseShellExecute = false,
                    RedirectStandardOutput = true,
                    RedirectStandardError = true,
                    CreateNoWindow = true
                };

                using (var process = Process.Start(startInfo))
                {
                    if (process != null)
                    {
                        await process.WaitForExitAsync();
                        if (process.ExitCode != 0)
                        {
                            MessageBox.Show("Error executing PowerShell script", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error: {ex.Message}", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }
    }
}
