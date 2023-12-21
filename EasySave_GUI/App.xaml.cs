using System.Configuration;
using System.Data;
using System.Windows;

namespace EasySave_GUI;

/// <summary>
/// Interaction logic for App.xaml
/// </summary>
public partial class App : Application
{
    protected override void OnStartup(StartupEventArgs e)
    {
        base.OnStartup(e);

        if (e.Args.Length == 2 && e.Args[0] == "-e")
        {
            JobOrchestrator JobOrchestrator = new JobOrchestrator();
            JobSaver JobSaver = new JobSaver();
            JobOrchestrator.ExecuteJobs(JobOrchestrator.GetJobsByCriteria(e.Args[1], JobSaver.LoadExistingJobs()));
        }
        else
        {
            HomeWindow homeWindow = new HomeWindow();
            homeWindow.SwitchLanguage(Model.Constants.Settings.Language.Value);
            homeWindow.Show();
        }
    }
}
