using Microsoft.VisualBasic;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using static EasySave_GUI.Settings.SettingsView;

namespace EasySave_GUI.Settings
{
    internal class SettingsViewModel : INotifyPropertyChanged
    {
        public ICommand AddEncryptedExtensionCommand { get; set; }
        public ICommand AddPrioritizedExtensionCommand { get; set; }
        public ICommand RemoveEncryptedExtensionCommand { get; set; }
        public ICommand RemovePrioritizedExtensionCommand { get; set; }
        public SettingsViewModel()
        {
            AddEncryptedExtensionCommand = new RelayCommand(AddEncryptedExtension);
            AddPrioritizedExtensionCommand = new RelayCommand(AddPrioritizedExtension);
            RemoveEncryptedExtensionCommand = new RelayCommand<string>(RemoveEncryptedExtension);
            RemovePrioritizedExtensionCommand = new RelayCommand<string>(RemovePrioritizedExtension);
        }
        public event PropertyChangedEventHandler PropertyChanged;
        public long MaxSimultaneousFileSize
        {
            get { return Model.Constants.Settings.MaxSimultaneousFileSize; }
            set {
                try
                {
                    Model.Constants.Settings.MaxSimultaneousFileSize = value;
                    OnPropertyChanged(nameof(MaxSimultaneousFileSize));
                }
                catch
                {
                    MessageBox.Show("Please enter a proper number.");
                }
            }
        }
        public string ForbiddenSoftware
        {
            get { return Model.Constants.Settings.ForbiddenSoftware; }
            set {
                Model.Constants.Settings.ForbiddenSoftware = value;
                OnPropertyChanged(nameof(ForbiddenSoftware));
            }
        }
        public string EncryptionKey
        {
            get { return Model.Constants.Settings.EncryptionKey; }
            set
            {
                if(value.Length >= 4)
                {
                    Model.Constants.Settings.EncryptionKey = value;
                    OnPropertyChanged(nameof(EncryptionKey));
                }
                else
                {
                    MessageBox.Show("The key should be at least 4 characters long");
                }
            }
        }
        public Model.Language Language
        {
            get { return Model.Constants.Settings.Language.Value; }
            set {
                if (Model.Constants.Settings.Language.Value != value)
                {
                    Model.Constants.Settings.Language.Value = value;
                    OnPropertyChanged(nameof(Language));
                }
            }
        }
        public List<Model.Language> Languages { get; } = Enum.GetValues(typeof(Model.Language)).Cast<Model.Language>().ToList();
        public Model.LogFileType LogFileType
        {
            get { return Model.Constants.Settings.LogFileType.Value; }
            set {
                if (Model.Constants.Settings.LogFileType.Value != value)
                {
                    Model.Constants.Settings.LogFileType.Value = value;
                    OnPropertyChanged(nameof(LogFileType));
                }
            }
        }
        public List<Model.LogFileType> LogFileTypes { get; } = Enum.GetValues(typeof(Model.LogFileType)).Cast<Model.LogFileType>().ToList();
        public ObservableCollection<string> EncryptedExtensions {
            get { return new ObservableCollection<string>(Model.Constants.Settings.EncryptedExtensions); }
            set { }
        }
        public ObservableCollection<string> PrioritizedExtensions
        {
            get { return new ObservableCollection<string>(Model.Constants.Settings.PrioritizedExtensions); }
            set { }
        }
        public string NewEncryptedExtension { get; set; }
        public string NewPrioritizedExtension { get; set; }
        private void AddEncryptedExtension()
        {
            if(CanBeAdded(NewEncryptedExtension, EncryptedExtensions))
            {
                Model.Constants.Settings.EncryptedExtensions.Add(NewEncryptedExtension);
                OnPropertyChanged(nameof(EncryptedExtensions));
            }
            else
            {
                NewEncryptedExtension = string.Empty;
                OnPropertyChanged(nameof(NewEncryptedExtension));
            }
        }
        private void AddPrioritizedExtension()
        {
            if(CanBeAdded(NewPrioritizedExtension, PrioritizedExtensions))
            {
                Model.Constants.Settings.PrioritizedExtensions.Add(NewPrioritizedExtension);
                OnPropertyChanged(nameof(PrioritizedExtensions));
            }
            else
            {
                NewPrioritizedExtension = string.Empty;
                OnPropertyChanged(nameof(NewPrioritizedExtension));
            }
        }
        private bool CanBeAdded(string extension, ObservableCollection<string> list)
        {
            if (!string.IsNullOrEmpty(extension))
            {
                if (Regex.IsMatch(extension, "^"+Model.Constants.GetExtensionRegex))
                {
                    if (!list.Contains(extension))
                    {
                        return true;
                    }
                    else
                    {
                        MessageBox.Show("Extension already exists in the prioritized list.");
                    }
                }
                else
                {
                    MessageBox.Show("Invalid extension format. It should start with a dot (.)");
                }
            }
            return false;
        }
        private void RemoveEncryptedExtension(string extension)
        {
            Model.Constants.Settings.EncryptedExtensions.Remove(extension);
            OnPropertyChanged(nameof(EncryptedExtensions));
        }
        private void RemovePrioritizedExtension(string extension)
        {
            Model.Constants.Settings.PrioritizedExtensions.Remove(extension);
            OnPropertyChanged(nameof(PrioritizedExtensions));
        }
        protected virtual void OnPropertyChanged(string propertyName)
        {
            Model.Constants.Settings.SaveSettings();
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }
}