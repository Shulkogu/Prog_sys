using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Input;
using System.Diagnostics;
using Model;
using System.IO;
using System.Windows;

namespace EasySave_GUI.Logs
{
    internal class LogsViewModel : INotifyPropertyChanged
    {
        public event PropertyChangedEventHandler? PropertyChanged;
        public ICommand OpenDailyLogsDirectory { get; set; }
        public ICommand OpenStatesDirectory { get; set; }
        protected virtual void OnPropertyChanged(string propertyName)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
        public LogsViewModel()
        {
            OpenDailyLogsDirectory = new RelayCommand(OpenDailyLogsPath);
            OpenStatesDirectory = new RelayCommand(OpenStatesPath);
        }
        private void OpenDailyLogsPath()
        {
            Process.Start("explorer.exe", Constants.LogPath);
        }
        private void OpenStatesPath()
        {
            Process.Start("explorer.exe", Constants.StatePath);
        }
    }
}
