using EasySave_GUI.ListViewModel;
using Microsoft.Win32;
using System;
using System.Collections.ObjectModel;
using System.IO;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Input;

namespace EasySave_GUI.ListView
{
    public partial class ViewList : UserControl
    {
        public ViewList()
        {
            InitializeComponent();
            DataContext = new EasySave_GUI.ListViewModel.ViewListViewModel();
        }
        private void ItemListView_SelectionChanged(object sender, SelectionChangedEventArgs e)
        //Function used to update the ViewModel's list of jobs selected by the user
        {
            var viewmodel = (ViewListViewModel)DataContext;
            viewmodel.SelectedJobs = ItemListView.SelectedItems.Cast<Model.Job>().ToList();
            viewmodel.SelectionChanged();
        }
    }
}
