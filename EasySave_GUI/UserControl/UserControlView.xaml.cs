using System.Windows.Controls;
using EasySave_GUI.UserControlViewModel;

namespace EasySave_GUI.SaveView;

public partial class UserControlSave : UserControl
{
    public UserControlSave()
    {
        InitializeComponent();
        DataContext = new EasySave_GUI.UserControlViewModel.UserControlViewModel();
    }
    private void Savestates_SelectionChanged(object sender, SelectionChangedEventArgs e)
    //Function used to update the ViewModel's list of jobs selected by the user
    {
        var viewmodel = (EasySave_GUI.UserControlViewModel.UserControlViewModel)DataContext;
        viewmodel.SelectedJobs = Savestates.SelectedItems.Cast<Model.Savestate>().ToList();
    }
}