﻿using Microsoft.Win32;
using Model;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Input;

namespace EasySave_GUI.JobList
{
    internal class JobListViewModel : INotifyPropertyChanged
    {
        public event EventHandler? NameAlreadyExists;
        public event EventHandler? EmptyFields;
        public event PropertyChangedEventHandler? PropertyChanged;
        public ICommand SetSourcePath { get; set; }
        public ICommand SetTargetPath { get; set; }
        public ICommand AddJob { get; set; }
        public ICommand DeleteJob { get; set; }
        public ICommand ModifyJob { get; set; }
        public JobListViewModel()
        {
            SetSourcePath = new RelayCommand(AskSourcePath);
            SetTargetPath = new RelayCommand(AskTargetPath);
            AddJob = new RelayCommand(AddAJob);
            DeleteJob = new RelayCommand(DeleteJobs);
            ModifyJob = new RelayCommand(ModifyAJob);
        }
        private JobSaver JobSaver = new JobSaver();
        public string Name { get; set; }
        public string Source { get; set; }
        public string Target { get; set; }
        public Savetype SaveType { get; set; }
        public List<Savetype> SaveTypes { get; } = Enum.GetValues(typeof(Savetype)).Cast<Savetype>().ToList();
        public ObservableCollection<Model.Job> ItemListView { get
            {
                return new ObservableCollection<Model.Job>(JobSaver.LoadExistingJobs());
            }
        }
        public List<Model.Job> SelectedJobs { get; set; }
        public void SelectionChanged()
        {
            if(SelectedJobs != null && SelectedJobs.Count >= 1)
            {
                Name = SelectedJobs[0].Name;
                OnPropertyChanged(nameof(Name));
                Source = SelectedJobs[0].SourcePath;
                OnPropertyChanged(nameof(Source));
                Target = SelectedJobs[0].TargetPath;
                OnPropertyChanged(nameof(Target));
                SaveType = SelectedJobs[0].SaveType;
                OnPropertyChanged(nameof(SaveType));
            }
        }
        private void AddAJob()
        {
            switch(ProperProperties())
            {
                case 1:
                    Job job = new Job
                    {
                        Name = Name,
                        SourcePath = Source,
                        TargetPath = Target,
                        SaveType = SaveType,
                    };
                    JobSaver.SaveJob(job);
                    OnPropertyChanged(nameof(ItemListView));
                    break;
                case 2:
                    NameAlreadyExists?.Invoke(this, EventArgs.Empty);
                    break;
                case 0:
                    return;
            }
        }
        private void DeleteJobs()
        {
            if (SelectedJobs != null && SelectedJobs.Count >= 1)
            {
                foreach (Job job in SelectedJobs)
                {
                    JobSaver.DeleteAllJobsByName(JobSaver.LoadExistingJobs(), job.Name);
                }
                OnPropertyChanged(nameof(ItemListView));
            }
        }
        private void ModifyAJob()
        {
            if(ProperProperties() == 2)
            {
                var jobs = JobSaver.LoadExistingJobs();
                jobs[jobs.FindIndex(x => x.Name == Name)] = new Job
                {
                    Name = Name,
                    SourcePath = Source,
                    TargetPath = Target,
                    SaveType = SaveType,
                };
                JobSaver.SaveJobs(jobs);
                OnPropertyChanged(nameof(ItemListView));
            }
        }
        private int ProperProperties()
        //Verifies the properties entered by the user.
        //Returns 0 if the properties are wrong
        //Returns 1 if the properties are right and the job name doesn't exist
        //Returns 2 if the properties are right and the job name already exists
        {
            //Verify if the data is properly filled
            if (string.IsNullOrWhiteSpace(Name) || string.IsNullOrWhiteSpace(Source) ||
                string.IsNullOrWhiteSpace(Target))
            {
                EmptyFields?.Invoke(this, EventArgs.Empty);
                return 0; //Do nothing if some fields aren't properly filled in
            }
            //Verify if the name is already present in the list
            if (JobSaver.LoadExistingJobs().Any(item => item.Name == Name))
            {
                return 2;
            }
            return 1;
        }
        private void AskSourcePath()
        {
            string path = FolderSelection();
            if(path != "")
            {
                this.Source = path;
                OnPropertyChanged(nameof(Source));
            }
        }
        private void AskTargetPath()
        {
            string path = FolderSelection();
            if (path != "")
            {
                this.Target = path;
                OnPropertyChanged(nameof(Target));
            }
        }
        private string FolderSelection()
        {
            // Use OpenFileDialog to allow the user to choose a folder
            OpenFileDialog openFileDialog = new OpenFileDialog
            {
                FileName = "Select a folder",
                Filter = "Folders|*.none",
                CheckFileExists = false,
                CheckPathExists = true
            };

            // Display the dialog to the user
            bool? result = openFileDialog.ShowDialog();

            // Check if the user chose a result
            if (result == true)
            {
                // Extract the directory of the selected file (which is actually a folder)
                return(System.IO.Path.GetDirectoryName(openFileDialog.FileName));
            }
            return "";
        }
        protected virtual void OnPropertyChanged(string propertyName)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }
}