using EasySave_GUI.JobList;
using System.Windows.Controls;

namespace EasySave_GUI.JobControl;

public partial class JobControlView : UserControl
{
    public JobControlView()
    {
        InitializeComponent();
        DataContext = new JobControlViewModel();
    }
    private void Savestates_SelectionChanged(object sender, SelectionChangedEventArgs e)
    //Function used to update the ViewModel's list of jobs selected by the user
    {
        if (ItemListView.SelectedItems.Count > 0)
        {
            var viewmodel = (JobControlViewModel)DataContext;
            viewmodel.SelectedJobs = ItemListView.SelectedItems.Cast<Model.Savestate>().ToList();
        }
    }
}