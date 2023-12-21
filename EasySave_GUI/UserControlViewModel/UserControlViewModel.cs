using Model;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Input;

namespace EasySave_GUI.UserControlViewModel
{
    internal class UserControlViewModel : INotifyPropertyChanged
    {
        public event PropertyChangedEventHandler? PropertyChanged;
        private JobOrchestrator JobOrchestrator = new JobOrchestrator();
        private JobSaver JobSaver = new JobSaver();
        public ICommand StartJobs { get; set; }
        public ICommand PauseJobs { get; set; }
        public ObservableCollection<Model.Savestate> Savestates
        {
            get
            {
                List<Model.Savestate> Savestates = new List<Model.Savestate>();
                foreach(Job job in JobSaver.LoadExistingJobs())
                {
                    if (JobOrchestrator.Logger != null && JobOrchestrator.Logger.SaveStates.ContainsKey(job.Name))
                    {
                        Savestates.Add(JobOrchestrator.Logger.SaveStates[job.Name]);
                    }
                    else
                    {
                        Savestate savestate = new Savestate(job.Name, 0, 0);
                        savestate.State = Workstate.END;
                        Savestates.Add(savestate);
                    }
                }
                return new ObservableCollection<Model.Savestate>(Savestates);
            }
        }
        public List<Model.Savestate> SelectedJobs { get; set; }
        public UserControlViewModel()
        {
            JobOrchestrator.StatesUpdated += StatesUpdatedEvent;
            JobSaver.JobsUpdated += StatesUpdatedEvent;
            StartJobs = new RelayCommand(StartSelectedJobs);
        }
        private void StatesUpdatedEvent(object sender, EventArgs e)
        {
            OnPropertyChanged(nameof(Savestates));
        }
        protected virtual void OnPropertyChanged(string propertyName)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
        private void StartSelectedJobs()
        {
            JobOrchestrator.ExecuteJobs(JobSaver.LoadExistingJobs().Where(x => SelectedJobs.Any(y => y.Name == x.Name)).ToList());
        }
    }
}
