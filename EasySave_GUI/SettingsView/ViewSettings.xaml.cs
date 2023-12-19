using System.Collections.ObjectModel;
using System.Text.RegularExpressions;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;

namespace EasySave_GUI.SettingsView;

    public partial class ViewSettings : UserControl
    {
        public ObservableCollection<string> AddedExtensions { get; set; }
        public ICommand AddExtensionCommand { get; set; }
        public ICommand RemoveExtensionCommand { get; set; }
        public string NewExtension { get; set; }

        // Partie pour la fonction prioritaire 
        public ObservableCollection<string> AddedPrioritizedExtensions { get; set; }
        public ICommand AddPrioritizedExtensionCommand { get; set; }
        public ICommand RemovePrioritizedExtensionCommand { get; set; }
        public string NewPrioritizedExtension { get; set; }

        // Partie choix langue 

        public string SelectedItem { get; set; }
        public ViewSettings()
        {
            InitializeComponent();


            AddedExtensions = new ObservableCollection<string>();
            AddExtensionCommand = new RelayCommand(AddExtension);
            RemoveExtensionCommand = new RelayCommand<string>(RemoveExtension);

            // Partie pour la fonction prioritaire 

            AddedPrioritizedExtensions = new ObservableCollection<string>();
            AddPrioritizedExtensionCommand = new RelayCommand(AddPrioritizedExtension);
            RemovePrioritizedExtensionCommand = new RelayCommand<string>(RemovePrioritizedExtension);


            DataContext = this;
        }
        private void AddExtension()
        {
            if (!string.IsNullOrEmpty(NewExtension))
            {
                if (Regex.IsMatch(NewExtension, @"^\..+$"))
                {
                    if (!AddedExtensions.Contains(NewExtension))
                    {
                        AddedExtensions.Add(NewExtension);
                    }
                    else
                    {
                        MessageBox.Show("Extension already exists in the list.");
                    }

                    NewExtension = string.Empty;
                }
                else
                {
                    MessageBox.Show("Invalid extension format. It should start with a dot (.)");
                }
            }
        }

        private void RemoveExtension(string extension)
        {
            AddedExtensions.Remove(extension);
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

        private void AddPrioritizedExtension()
        {
            if (!string.IsNullOrEmpty(NewPrioritizedExtension))
            {
                if (Regex.IsMatch(NewPrioritizedExtension, @"^\..+$"))
                {
                    if (!AddedPrioritizedExtensions.Contains(NewPrioritizedExtension))
                    {
                        AddedPrioritizedExtensions.Add(NewPrioritizedExtension);
                    }
                    else
                    {
                        MessageBox.Show("Extension already exists in the prioritized list.");
                    }

                    NewPrioritizedExtension = string.Empty;
                }
                else
                {
                    MessageBox.Show("Invalid extension format. It should start with a dot (.)");
                }
            }
        }

        private void RemovePrioritizedExtension(string extension)
        {
            AddedPrioritizedExtensions.Remove(extension);
        }

        private void TextBox_GotFocus(object sender, RoutedEventArgs e)
        {
            TextBox textBox = sender as TextBox;
            if (textBox != null && textBox.Opacity == 0.5)
            {
                textBox.Text = string.Empty;
                textBox.Opacity = 1;
            }
        }

        private void TextBox_LostFocus(object sender, RoutedEventArgs e)
        {
            TextBox textBox = sender as TextBox;
            if (textBox != null && string.IsNullOrWhiteSpace(textBox.Text))
            {
                textBox.Text = "Placeholder Text";
                textBox.Opacity = 0.5;
            }
        }

    }