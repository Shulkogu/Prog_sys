using System;
using System.Collections.ObjectModel;
using System.Text.RegularExpressions;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using EasySave_GUI.JobList;
using EasySave_GUI.Settings;

namespace EasySave_GUI.Settings
{
    public partial class SettingsView : UserControl
    {
        public SettingsView()
        {
            InitializeComponent();
            DataContext = new SettingsViewModel();
            ((SettingsViewModel)DataContext).WrongNumberFormat += WrongNumberFormat;
            ((SettingsViewModel)DataContext).KeyTooSmall += KeyTooSmall;
            ((SettingsViewModel)DataContext).ExtensionAlreadyPresent += ExtensionAlreadyPresent;
            ((SettingsViewModel)DataContext).InvalidExtensionFormat += InvalidExtensionFormat;
        }

        private void ListBox_PreviewMouseWheel(object sender, MouseWheelEventArgs e)
        {
            e.Handled = true;
            ListBox listBox = (ListBox)sender;
            listBox.RaiseEvent(new MouseWheelEventArgs(e.MouseDevice, e.Timestamp, e.Delta)
            {
                RoutedEvent = UIElement.MouseWheelEvent,
                Source = listBox
            });
        }

        private void TextBox_GotFocus(object sender, RoutedEventArgs e)
        {
            TextBox textBox = sender as TextBox;
            if (textBox != null && textBox.Opacity == 0.5)
            {
                textBox.Tag = textBox.Text;
                textBox.Text = string.Empty;
                textBox.Opacity = 1;
            }
        }

        private void TextBox_LostFocus(object sender, RoutedEventArgs e)
        {
            TextBox textBox = sender as TextBox;
            if (textBox != null && string.IsNullOrWhiteSpace(textBox.Text))
            {
                textBox.Text = textBox.Tag.ToString();
                textBox.Opacity = 0.5;
            }
        }
        private void WrongNumberFormat(object sender, EventArgs e)
        {
            MessageBox.Show((string)FindResource("WrongNumberFormat"));
        }
        private void KeyTooSmall(object sender, EventArgs e)
        {
            MessageBox.Show((string)FindResource("KeyTooSmall"));
        }
        private void ExtensionAlreadyPresent(object sender, EventArgs e)
        {
            MessageBox.Show((string)FindResource("ExtensionAlreadyPresent"));
        }
        private void InvalidExtensionFormat(object sender, EventArgs e)
        {
            MessageBox.Show((string)FindResource("InvalidExtensionFormat"));
        }
    }
}