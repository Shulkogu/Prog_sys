using Microsoft.Win32;
using System;
using System.Collections.ObjectModel;
using System.IO;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Input;

namespace EasySave_GUI.JobList
{
    public partial class JobListView : UserControl
    {
        public JobListView()
        {
            InitializeComponent();
            DataContext = new JobListViewModel();
        }
        private void ItemListView_SelectionChanged(object sender, SelectionChangedEventArgs e)
        //Function used to update the ViewModel's list of jobs selected by the user
        {
            var viewmodel = (JobListViewModel)DataContext;
            viewmodel.SelectedJobs = ItemListView.SelectedItems.Cast<Model.Job>().ToList();
            viewmodel.SelectionChanged();
        }
    }
}
