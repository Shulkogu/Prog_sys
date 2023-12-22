using EasySave_GUI.JobList;
using System.Windows.Controls;

namespace EasySave_GUI.Logs;

public partial class LogsView : UserControl
{
    public LogsView()
    {
        InitializeComponent();
        DataContext = new LogsViewModel();
    }
}