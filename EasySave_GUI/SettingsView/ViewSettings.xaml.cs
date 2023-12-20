using System;
using System.Collections.ObjectModel;
using System.Text.RegularExpressions;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using EasySave_GUI.SettingsViewModel;

namespace EasySave_GUI.SettingsView;

    public partial class ViewSettings : UserControl
    {
        public string SelectedItem { get; set; }
        public ViewSettings()
        {
            InitializeComponent();
            DataContext = new EasySave_GUI.SettingsViewModel.SettingsViewModel();
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

        public class RelayCommand : ICommand
        {
            private readonly Action _execute;

            public RelayCommand(Action execute)
            {
                _execute = execute;
            }

            public bool CanExecute(object parameter)
            {
                return true;
            }

            public void Execute(object parameter)
            {
                _execute();
            }

            public event EventHandler CanExecuteChanged;
        }

        public class RelayCommand<T> : ICommand
        {
            private readonly Action<T> _execute;

            public RelayCommand(Action<T> execute)
            {
                _execute = execute;
            }

            public bool CanExecute(object parameter)
            {
                return true;
            }

            public void Execute(object parameter)
            {
                _execute((T)parameter);
            }

            public event EventHandler CanExecuteChanged;
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
    }