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
        
        HomeWindow homeWindow = new HomeWindow();
        homeWindow.SwitchLanguage(Model.Constants.Settings.Language.Value);
        homeWindow.Show();
    }
}
